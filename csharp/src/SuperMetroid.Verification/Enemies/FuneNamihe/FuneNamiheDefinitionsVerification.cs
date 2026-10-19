using SuperMetroid.Core.Programs;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.FuneNamihe;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Fune and Namihe animation selectors against native words and production installs.</summary>
internal static class FuneNamiheDefinitionsVerification
{
    /// <summary>Checks all species/facing/activity selectors while forbidding migrated selector reads in production.</summary>
    internal static void VerifyFuneNamiheDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyFuneNamiheProgramSelection), () => VerifyFuneNamiheProgramSelection(rom));
        for (int variant = 0; variant < 4; variant++)
        {
            bool namihe = (variant & 2) != 0;
            bool right = (variant & 1) != 0;
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemySlot> initialize = FuneNamiheBehavior.InitializeFuneNamihe;
            Action<RoomEnemySlot, FuneNamiheEnemyState, SamusState?> runMain = FuneNamiheBehavior.RunFuneNamiheMain;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(namihe ? EnemyDefinitionId.Namihe : EnemyDefinitionId.Fune, slot.Definition);
            slot.SetSpawnParameter1((ushort)((namihe ? 1 : 0) | (right ? 0x10 : 0)));
            slot.SetSpawnParameter2(0x1000);
            slot.Position = slot.Position.WithWholeY(0x0200);
            initialize(slot);
            FuneNamiheEnemyState state = (enemies.FuneNamiheStates[0] ?? throw new InvalidOperationException("Verification requires enemies.FuneNamiheStates[0]."));
            ushort idleCursor = (ushort)((namihe
                ? FuneNamiheSelectorOracle.NamiheIdleLeftCursor
                : FuneNamiheSelectorOracle.FuneIdleLeftCursor) +
                (right ? FuneNamiheSelectorOracle.FacingRightCursorDelta : 0));
            AssertEqual(AnimationProgramCursor<FuneNamiheProgram>.Start(FuneNamiheSelectorOracle.ProgramAt(idleCursor)), state.Program,
                $"Fune/Namihe production idle list {variant}");

            runMain(slot, (state ?? throw new InvalidOperationException("Verification requires state.")), namihe ? new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0, slot.Position.Y.Whole)) } : null);
            ushort activeCursor = (ushort)(idleCursor - FuneNamiheSelectorOracle.ActiveCursorDelta);
            AssertEqual(AnimationProgramCursor<FuneNamiheProgram>.Start(FuneNamiheSelectorOracle.ProgramAt(activeCursor)), state.Program,
                $"Fune/Namihe production active list {variant}");
        }

        Console.WriteLine(
            "Fune/Namihe definitions: eight native selectors and all eight real idle/active installs pass with table reads forbidden.");
    }

    /// <summary>
    /// For each of the eight native selector entries, the production selection for that entry's species, activity and
    /// facing names the program the native pointer lists; the selector decode rejects adjacent or wrapped cursors.
    /// </summary>
    private static void VerifyFuneNamiheProgramSelection(SuperMetroidAddressSpace rom)
    {
        ushort[] cursors = [0x96d3, 0x96d5, 0x96d7, 0x96d9, 0x96db, 0x96dd, 0x96df, 0x96e1];
        foreach (ushort cursor in cursors)
        {
            ushort native = (ushort)(SnesLittleEndian.Word(FuneNamiheProgramSelectionRomByte(rom, 0xa80000 | cursor), FuneNamiheProgramSelectionRomByte(rom, 0xa80000 | (cursor + 1))));
            (bool namihe, bool active, bool right) = FuneNamiheSelectorOracle.Decode(cursor);
            var state = new FuneNamiheEnemyState { VariantIndex = namihe ? 1 : 0, ShowsActiveAnimation = active, FacingRight = right };
            AssertEqual(native, NativeEnemyProgramOracle.EntryOf(FuneNamiheNativeListAddresses.Entries, FuneNamiheBehavior.SelectFuneNamiheProgram(state)),
                "Fune/Namihe production species/activity/facing selection matches native pointer");
        }
        var valid = cursors.ToHashSet();
        for (int cursor = 0x96d1; cursor <= 0x96e4; cursor++)
            if (!valid.Contains((ushort)cursor))
                AssertThrows<InvalidDataException>(() => FuneNamiheSelectorOracle.Decode((ushort)cursor),
                    "Fune/Namihe selector decode rejects odd and adjacent cursors");
        foreach (ushort cursor in new ushort[] { 0, 1, 0x16d3, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => FuneNamiheSelectorOracle.Decode(cursor),
                "Fune/Namihe selector decode rejects distant and wrapped cursors");
    }

    /// <summary>Reads one native selector byte solely as the verification oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Fune Namihe program selection")]
    private static byte FuneNamiheProgramSelectionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}

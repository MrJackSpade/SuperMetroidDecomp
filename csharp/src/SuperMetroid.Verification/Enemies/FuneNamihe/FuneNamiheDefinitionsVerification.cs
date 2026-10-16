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
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        for (int variant = 0; variant < 4; variant++)
        {
            bool namihe = (variant & 2) != 0;
            bool right = (variant & 1) != 0;
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(new CartridgeReadGuard(new TestAddressSpace()).Forbid(
                    address => address is >= 0xa896d3 and < 0xa896e3,
                    address => $"Fune/Namihe attempted migrated selector read ${address:X6}."));
            Action<RoomEnemySlot> initialize = FuneNamiheBehavior.InitializeFuneNamihe;
            Action<RoomEnemySlot, FuneNamiheEnemyState, SamusState?> runMain = FuneNamiheBehavior.RunFuneNamiheMain;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(namihe ? EnemyDefinitionId.Namihe : EnemyDefinitionId.Fune, slot.Definition);
            slot.Parameter1 = (ushort)((namihe ? 1 : 0) | (right ? 0x10 : 0));
            slot.Parameter2 = 0x1000;
            slot.Position = slot.Position.WithWholeY(0x0200);
            initialize(slot);
            FuneNamiheEnemyState state = (enemies.FuneNamiheStates[0] ?? throw new InvalidOperationException("Verification requires enemies.FuneNamiheStates[0]."));
            ushort idleCursor = (ushort)((namihe
                ? FuneNamiheDefinitions.NamiheIdleLeftCursor
                : FuneNamiheDefinitions.FuneIdleLeftCursor) +
                (right ? FuneNamiheDefinitions.FacingRightCursorDelta : 0));
            AssertEqual(AnimationProgramCursor<FuneNamiheProgram>.Start(FuneNamiheDefinitions.InstructionList(idleCursor)), state.Program,
                $"Fune/Namihe production idle list {variant}");

            runMain(slot, (state ?? throw new InvalidOperationException("Verification requires state.")), namihe ? new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0, slot.Position.Y.Whole)) } : null);
            ushort activeCursor = (ushort)(idleCursor - FuneNamiheDefinitions.ActiveCursorDelta);
            AssertEqual(AnimationProgramCursor<FuneNamiheProgram>.Start(FuneNamiheDefinitions.InstructionList(activeCursor)), state.Program,
                $"Fune/Namihe production active list {variant}");
        }

        Console.WriteLine(
            "Fune/Namihe definitions: eight native selectors and all eight real idle/active installs pass with table reads forbidden.");
    }

    /// <summary>Compares the eight native program pointers and rejects adjacent or wrapped cursor values.</summary>
    private static void VerifyFuneNamiheProgramSelection(SuperMetroidAddressSpace rom)
    {
        ushort[] cursors = [0x96d3, 0x96d5, 0x96d7, 0x96d9, 0x96db, 0x96dd, 0x96df, 0x96e1];
        foreach (ushort cursor in cursors)
        {
            ushort native = (ushort)(SnesLittleEndian.Word(FuneNamiheProgramSelectionRomByte(rom, 0xa80000 | cursor), FuneNamiheProgramSelectionRomByte(rom, 0xa80000 | (cursor + 1))));
            AssertEqual(native, NativeEnemyProgramOracle.EntryOf(FuneNamiheNativeListAddresses.Entries, FuneNamiheDefinitions.InstructionList(cursor)),
                "Fune/Namihe named species/activity/facing case matches native pointer");
        }
        var valid = cursors.ToHashSet();
        for (int cursor = 0x96d1; cursor <= 0x96e4; cursor++)
            if (!valid.Contains((ushort)cursor))
                AssertThrows<InvalidDataException>(() => FuneNamiheDefinitions.InstructionList((ushort)cursor),
                    "Fune/Namihe selection rejects odd and adjacent cursors");
        foreach (ushort cursor in new ushort[] { 0, 1, 0x16d3, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => FuneNamiheDefinitions.InstructionList(cursor),
                "Fune/Namihe selection rejects distant and wrapped cursors");
    }

    /// <summary>Reads one native selector byte solely as the verification oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Fune Namihe program selection")]
    private static byte FuneNamiheProgramSelectionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}

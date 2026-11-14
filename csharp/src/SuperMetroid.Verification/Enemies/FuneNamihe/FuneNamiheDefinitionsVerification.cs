using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Enemies.FuneNamihe;
/// <summary>Checks Fune and Namihe animation selectors against native words and production installs.</summary>
internal static class FuneNamiheDefinitionsVerification
{
    /// <summary>Checks all species/facing/activity selectors while forbidding migrated selector reads in production.</summary>
    internal static void VerifyFuneNamiheDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyFuneNamiheProgramSelection), () => VerifyFuneNamiheProgramSelection(rom));
        for (int variant = 0; variant < 4; variant++)
        {
            bool namihe = (variant & 2) != 0;
            bool right = (variant & 1) != 0;
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemySlot> initialize = FuneNamiheBehavior.InitializeFuneNamihe;
            Action<RoomEnemySlot, FuneNamiheEnemyState, SamusState?> runMain = FuneNamiheBehavior.RunFuneNamiheMain;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(namihe ? EnemyDefinitionId.Namihe : EnemyDefinitionId.Fune, slot.Definition);
            slot.SetSpawnParameter1(EnemyDefinitionId.Fune, (ushort)((namihe ? 1 : 0) | (right ? 0x10 : 0)));
            slot.SetSpawnParameter2(EnemyDefinitionId.Fune, 0x1000);
            slot.Position = slot.Position.WithWholeY(0x0200);
            initialize(slot);
            FuneNamiheEnemyState state = (enemies.FuneNamiheStates[0] ?? throw new InvalidOperationException("Verification requires enemies.FuneNamiheStates[0]."));
            ushort idleCursor = (ushort)((namihe
                ? FuneNamiheSelectorOracle.NamiheIdleLeftCursor
                : FuneNamiheSelectorOracle.FuneIdleLeftCursor) +
                (right ? FuneNamiheSelectorOracle.FacingRightCursorDelta : 0));
            AssertEqual(new AnimationPlayback<FuneNamiheSegment>(FuneNamiheAnimations.StartingWith(FuneNamiheSelectorOracle.ProgramAt(idleCursor)), AnimationPlace.StartOfAnimation), state.Playback,
                $"Fune/Namihe production idle list {variant}");

            runMain(slot, (state), namihe ? new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0, slot.Position.Y.Whole)) } : null);
            ushort activeCursor = (ushort)(idleCursor - FuneNamiheSelectorOracle.ActiveCursorDelta);
            AssertEqual(new AnimationPlayback<FuneNamiheSegment>(FuneNamiheAnimations.StartingWith(FuneNamiheSelectorOracle.ProgramAt(activeCursor)), AnimationPlace.StartOfAnimation), state.Playback,
                $"Fune/Namihe production active list {variant}");
        }

        Console.WriteLine(
            "Fune/Namihe definitions: eight native selectors and all eight real idle/active installs pass with table reads forbidden.");
    }

    /// <summary>
    /// For each of the eight native selector entries, the production selection for that entry's species, activity and
    /// facing names the program the native pointer lists; the selector decode rejects adjacent or wrapped cursors.
    /// </summary>
    private static void VerifyFuneNamiheProgramSelection(CartridgeImportAddressSpace rom)
    {
        ushort[] cursors = [0x96d3, 0x96d5, 0x96d7, 0x96d9, 0x96db, 0x96dd, 0x96df, 0x96e1];
        foreach (ushort cursor in cursors)
        {
            ushort native = LittleEndianBytes.Word(FuneNamiheProgramSelectionRomByte(rom, 0xa80000 | cursor), FuneNamiheProgramSelectionRomByte(rom, 0xa80000 | (cursor + 1)));
            (bool namihe, bool active, bool right) = FuneNamiheSelectorOracle.Decode(cursor);
            var state = new FuneNamiheEnemyState { Variant = namihe ? FuneNamiheVariant.Namihe : FuneNamiheVariant.Fune, ShowsActiveAnimation = active, FacingRight = right };
            AssertEqual(native, NativeEnemyAnimationOracle.EntryOf(FuneNamiheNativeListAddresses.Entries, FuneNamiheBehavior.SelectFuneNamiheAnimation(state)),
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
    private static byte FuneNamiheProgramSelectionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}

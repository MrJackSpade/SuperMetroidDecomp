using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;
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
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemyPlace> initialize = namihe ? fixturePlace => FuneNamiheBehavior.InitializeNamihe(fixturePlace.HeldOccupant(), fixturePlace) : fixturePlace => FuneNamiheBehavior.InitializeFune(fixturePlace.HeldOccupant(), fixturePlace);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(namihe ? EnemyDefinitionId.Namihe : EnemyDefinitionId.Fune, place.KeptDefinition);
            place.SetSpawnParameter1(place.HeldOccupant().DefinitionId, (ushort)((namihe ? 1 : 0) | (right ? 0x10 : 0)));
            place.SetSpawnParameter2(place.HeldOccupant().DefinitionId, 0x1000);
            place.Position = place.Position.WithWholeY(0x0200);
            initialize(place);
            FuneNamiheEnemyState state = namihe
                ? enemies.NamiheStates[0] ?? throw new InvalidOperationException("Verification requires enemies.NamiheStates[0].")
                : enemies.FuneStates[0] ?? throw new InvalidOperationException("Verification requires enemies.FuneStates[0].");
            ushort idleCursor = (ushort)((namihe
                ? FuneNamiheSelectorOracle.NamiheIdleLeftCursor
                : FuneNamiheSelectorOracle.FuneIdleLeftCursor) +
                (right ? FuneNamiheSelectorOracle.FacingRightCursorDelta : 0));
            AssertEqual(new AnimationPlayback<FuneNamiheSegment>(FuneNamiheAnimations.StartingWith(FuneNamiheSelectorOracle.ProgramAt(idleCursor)), AnimationPlace.StartOfAnimation), state.Playback,
                $"Fune/Namihe production idle list {variant}");

            RunMain(place, state, namihe ? new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0, place.Position.Y.Whole)) } : RoomEnemyFrameFixtures.StandaloneSamus());
            ushort activeCursor = (ushort)(idleCursor - FuneNamiheSelectorOracle.ActiveCursorDelta);
            AssertEqual(new AnimationPlayback<FuneNamiheSegment>(FuneNamiheAnimations.StartingWith(FuneNamiheSelectorOracle.ProgramAt(activeCursor)), AnimationPlace.StartOfAnimation), state.Playback,
                $"Fune/Namihe production active list {variant}");
        }

        Console.WriteLine(
            "Fune/Namihe definitions: eight native selectors and all eight real idle/active installs pass with table reads forbidden.");
    }

    /// <summary>Runs one main-AI update of the Fune or Namihe whose state is <paramref name="state"/>.</summary>
    private static void RunMain(RoomEnemyPlace place, FuneNamiheEnemyState state, SamusState samus)
    {
        switch (state)
        {
            case FuneEnemyState fune:
                FuneNamiheBehavior.RunFuneMain(place.HeldOccupant(), place, fune);
                return;
            case NamiheEnemyState namihe:
                FuneNamiheBehavior.RunNamiheMain(place.HeldOccupant(), place, namihe, samus);
                return;
            default:
                throw new InvalidOperationException($"Undefined Fune/Namihe state {state}.");
        }
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
            FuneNamiheEnemyState state = namihe
                ? new NamiheEnemyState(active ? NamiheAction.Spitting : NamiheAction.WaitForSamus, new NamiheSpawnSettings(HorizontalDirection.Left, LaunchSpeed: SignedEightEight.Zero, YProximity: 0))
                : new FuneEnemyState(active ? FuneAction.Spitting : FuneAction.WaitForCooldown, new FuneSpawnSettings(HorizontalDirection.Left, CooldownTime: 0, LaunchSpeed: SignedEightEight.Zero));
            AssertEqual(native, NativeEnemyAnimationOracle.EntryOf(FuneNamiheNativeListAddresses.Entries, state.Animation(right ? HorizontalDirection.Right : HorizontalDirection.Left)),
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

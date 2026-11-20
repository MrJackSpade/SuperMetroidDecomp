using SuperMetroid.Core.Bosses;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.TestSupport.MovieReplay.RidleyNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// #1287: Ridley's typed body, breakup-fragment and fireball programs against the retail cartridge. Each typed
/// program must match its native list step by step, and the body's facing and low-energy branches must redirect as
/// the native gotos do. The step-by-step walk also yields each typed step's native instruction address, which
/// <see cref="RidleyNativeInstructionPointers"/> uses to compare typed cursors with recorded native pointers.
/// </summary>
internal static class RidleyAnimationsVerification
{
    /// <summary>Every body, breakup and fireball program matches its native list, and the body's branches redirect natively.</summary>
    internal static void VerifyRidleyAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = rom;
        AssertEqual(Enum.GetValues<RidleyBodySegment>().Length, BodyEntries.Length, "every typed Ridley body program has a native entry");
        for (int index = 0; index < BodyEntries.Length; index++)
            AssertEqual((RidleyBodySegment)index, BodyEntries[index].Program, "the Ridley body programs are declared in authored order");
        AssertTrue(BodyEntries.Zip(BodyEntries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends), "native Ridley body entries ascend");

        var body = RidleyNativeProgramOracle.BodyLists(cartridge);
        RidleyNativeProgramOracle.CompareBreakupLists(cartridge);
        var fireball = RidleyNativeProgramOracle.FireballLists(cartridge);

        VerifyFacingBranch();
        VerifyLowEnergyBranch();
        Console.WriteLine($"  Ridley animations: {body.Count} body segments ({body.Values.Sum(list => list.BeatAddresses.Length)} beats), " +
            $"{RidleyNativeListAddresses.BreakupEntries.Length} held breakup pictures and {fireball.Count} fireball segments match their native lists; " +
            "the facing and low-energy branches redirect natively.");
    }

    /// <summary>Creates Ridley in slot zero of a fresh enemy system, facing <paramref name="facing"/>.</summary>
    private static NewRidleyResult NewRidley(EnemyDefinitionId definition, RidleyFacing facing)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(definition, body.KeptDefinition);
        RidleyEncounterState state = definition == EnemyDefinitionId.RidleyCeres ? body.SetUpPlaced<CeresRidleyState>() : body.SetUpPlaced<NorfairRidleyState>();
        state.Flight.FacingDirection = facing;
        return new NewRidleyResult(enemies, state, body);
    }

    /// <summary>Runs one body update, expiring the current frame first.</summary>
    private static void Pass(RoomEnemyPlace body, SamusState? samus)
    {
        body.AnimationClock.StepOnNextUpdate();
        EnemyAnimationPlayer.AdvanceAnimation(body, samus ?? RoomEnemyFrameFixtures.StandaloneSamus(), level: RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, controllerInput: 0);
    }

    /// <summary>A right-facing Ridley starting a left-facing entry runs its right-facing program in the same update.</summary>
    private static void VerifyFacingBranch()
    {
        (_, RidleyEncounterState state, RoomEnemyPlace body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Right);
        RidleyFlight.StartAnimation(state.Flight, body, RidleyAnimations.OpeningRoar);
        Pass(body, samus: null);
        AssertEqual(RidleyAnimations.StartingWith(RidleyBodySegment.OpeningRoarFacingRight), state.Flight.BodyPlayback.Animation, "a right-facing Ridley roars facing right");
        AssertEqual((EnemyFrameId?)EnemyFrameId.RidleyBodyRight,
            RidleyCollisionDefinitions.BodyFrameAt(body.SpritemapPointer), "the right-facing roar shows its first frame at once");
        AssertEqual(6, body.AnimationClock.Hold, "the right-facing roar's first frame lasts six updates");

        (_, state, body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Left);
        RidleyFlight.StartAnimation(state.Flight, body, RidleyAnimations.OpeningRoar);
        Pass(body, samus: null);
        AssertEqual(RidleyAnimations.StartingWith(RidleyBodySegment.OpeningRoarFacingLeft), state.Flight.BodyPlayback.Animation, "a left-facing Ridley keeps the left-facing roar");
        AssertEqual((EnemyFrameId?)EnemyFrameId.RidleyBodyLeft, RidleyCollisionDefinitions.BodyFrameAt(body.SpritemapPointer), "the left-facing roar shows its first frame");
    }

    /// <summary>
    /// In Ceres, a Samus under 30 energy cuts the fireballing short to its closing tail and shortens the hover to eight
    /// updates; Lower Norfair Ridley ignores her energy.
    /// </summary>
    private static void VerifyLowEnergyBranch()
    {
        (_, RidleyEncounterState state, RoomEnemyPlace body) = NewRidley(EnemyDefinitionId.RidleyCeres, RidleyFacing.Left);
        state.Flight.Roaring = true;
        RidleyFlight.StartAnimation(state.Flight, body, RidleyAnimations.Fireballing);
        Pass(body, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Health = 29 });
        AssertEqual(RidleyAnimations.StartingWith(RidleyBodySegment.FireballingEndFacingLeft), state.Flight.BodyPlayback.Animation, "low energy in Ceres skips to the fireballing tail");
        AssertEqual(8, state.PhaseTimer.CartridgeCount, "low energy shortens the hover to eight updates");
        AssertTrue(!state.Flight.Roaring, "the fireballing tail clears the roaring flag");
        AssertEqual((EnemyFrameId?)EnemyFrameId.RidleyBodyLeft, RidleyCollisionDefinitions.BodyFrameAt(body.SpritemapPointer), "the tail shows the standing pose");

        // A right-facing Ridley's fireballing is cut short to its own right-facing tail (#2175).
        (_, state, body) = NewRidley(EnemyDefinitionId.RidleyCeres, RidleyFacing.Right);
        RidleyFlight.StartAnimation(state.Flight, body, RidleyAnimations.Fireballing);
        Pass(body, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Health = 29 });
        AssertEqual(RidleyAnimations.StartingWith(RidleyBodySegment.FireballingEndFacingRight), state.Flight.BodyPlayback.Animation, "low energy in Ceres skips to the right-facing fireballing tail");
        AssertEqual((EnemyFrameId?)EnemyFrameId.RidleyBodyRight, RidleyCollisionDefinitions.BodyFrameAt(body.SpritemapPointer), "the right-facing tail shows the standing pose");

        (_, state, body) = NewRidley(EnemyDefinitionId.RidleyCeres, RidleyFacing.Left);
        RidleyFlight.StartAnimation(state.Flight, body, RidleyAnimations.Fireballing);
        Pass(body, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Health = 30 });
        AssertEqual(RidleyAnimations.StartingWith(RidleyBodySegment.FireballingFacingLeft), state.Flight.BodyPlayback.Animation, "thirty energy keeps the full fireballing");

        (_, state, body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Left);
        RidleyFlight.StartAnimation(state.Flight, body, RidleyAnimations.Fireballing);
        Pass(body, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Health = 1 });
        AssertEqual(RidleyAnimations.StartingWith(RidleyBodySegment.FireballingFacingLeft), state.Flight.BodyPlayback.Animation, "Lower Norfair Ridley ignores Samus's energy");
    }

    /// <summary>Runs Ridley's program parity check against the repository cartridge.</summary>
    [VerificationCommand("--ridley-animations")]
    internal static int RidleyAnimationsCommand()
    {
        Suite(nameof(VerifyRidleyAnimations), () => VerifyRidleyAnimations(LoadRepositoryRom()));
        return 0;
    }
}

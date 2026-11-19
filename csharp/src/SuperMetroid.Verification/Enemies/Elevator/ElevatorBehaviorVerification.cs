using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Elevator;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that elevator starts and runs its typed animation (#641).</summary>
internal static class ElevatorBehaviorVerification
{
    /// <summary>The real initializer starts the Loop program, which the runtime then plays through once and back.</summary>
    internal static void VerifyElevatorBehavior()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Elevator, place.Definition);
        place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
        place.SetSpawnWords(EnemyDefinitionId.Elevator, words => words);
        enemies.ElevatorBehavior.InitializeElevator(place, RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(new AnimationPlayback<ElevatorSegment>(ElevatorAnimations.StartingWith(ElevatorSegment.Idle), AnimationPlace.StartOfAnimation), (enemies.Places.InUpdateOrder[0].EntityState<ElevatorEntity, ElevatorEnemyState>() ?? throw new InvalidOperationException("The elevator has no state.")).Playback,
            "elevator initializer animation");
        place.AnimationClock.StepOnNextUpdate();
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(ElevatorAnimations.Segments, ElevatorSegment.Idle, ElevatorFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(ElevatorAnimations.Segments, ElevatorSegment.Idle)),
            "elevator runs its loop and returns through its goto");
        Console.WriteLine("  elevator behavior: the initializer starts and runs its loop.");
    }

    /// <summary>
    /// An arriving elevator's setup names itself, wherever the room lists it, as the placement the door loader waits for
    /// (#1808) while its arrival begins, and the loader asks room loading's progress about that elevator.
    /// </summary>
    internal static void VerifyElevatorArrivalPlacement()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace elevator = enemies.Places.InUpdateOrder[3];
        elevator.Bind(EnemyDefinitionId.Elevator, elevator.Definition);
        elevator.SetSpawnParameter1(EnemyDefinitionId.Elevator, 0);
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        samus.BindBodyFixture(InstalledFixtures.SamusBody);

        enemies.ElevatorBehavior.PrepareElevatorArrival();
        enemies.ElevatorBehavior.InitializeElevator(elevator, samus);
        AssertTrue(ReferenceEquals(elevator, enemies.ElevatorBehavior.ArrivalElevator),
            "the arriving elevator is the placement the door loader waits for");

        var progress = new EvidencedRoomLoadProgress();
        progress.Expect(3);
        AssertTrue(!progress.HasInitialized(elevator), "loading that has initialized the three enemies listed before the elevator has not reached it");
        progress.Expect(4);
        AssertTrue(progress.HasInitialized(elevator), "loading that has initialized four enemies has reached the elevator listed fourth");
        Console.WriteLine("  elevator arrival: the arriving elevator is the placement, and loading progress is asked about it.");
    }

    /// <summary>Runs the elevator behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--elevator-instruction-mechanics")]
    internal static int ElevatorBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyElevatorBehavior), () => VerifyElevatorBehavior());
        Suite(nameof(VerifyElevatorArrivalPlacement), () => VerifyElevatorArrivalPlacement());
        Suite(nameof(ElevatorAnimationsVerification.VerifyElevatorAnimations), () => ElevatorAnimationsVerification.VerifyElevatorAnimations(rom));
        return 0;
    }
}

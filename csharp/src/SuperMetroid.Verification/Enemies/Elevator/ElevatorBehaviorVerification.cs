using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
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
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Elevator, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnWords(EnemyDefinitionId.Elevator, words => words);
        enemies.ElevatorBehavior.InitializeElevator(slot, null);
        AssertEqual(new AnimationPlayback<ElevatorProgram>(ElevatorAnimations.StartingWith(ElevatorProgram.Loop), AnimationPlace.StartOfAnimation), (enemies.SlotTable.Slots[0].EntityState<ElevatorEntity, ElevatorEnemyState>() ?? throw new InvalidOperationException("The elevator has no state.")).Playback,
            "elevator initializer animation");
        slot.AnimationClock.StepOnNextUpdate();
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(ElevatorAnimations.Segments, ElevatorProgram.Loop, ElevatorFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(ElevatorAnimations.Segments, ElevatorProgram.Loop)),
            "elevator runs its loop and returns through its goto");
        Console.WriteLine("  elevator behavior: the initializer starts and runs its loop.");
    }

    /// <summary>Runs the elevator behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--elevator-instruction-mechanics")]
    internal static int ElevatorBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyElevatorBehavior), () => VerifyElevatorBehavior());
        Suite(nameof(ElevatorAnimationsVerification.VerifyElevatorAnimations), () => ElevatorAnimationsVerification.VerifyElevatorAnimations(rom));
        return 0;
    }
}

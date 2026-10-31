using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Elevator;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that elevator starts and runs its typed animation (#641).</summary>
internal static class ElevatorBehaviorVerification
{
    /// <summary>The real initializer starts the Loop program, which the runtime then plays through once and back.</summary>
    internal static void VerifyElevatorBehavior()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Elevator, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        enemies.ElevatorBehavior.InitializeElevator(slot, null);
        AssertEqual(AnimationProgramCursor<ElevatorProgram>.Start(ElevatorProgram.Loop), (enemies.SlotTable.Slots[0].EntityState<ElevatorEntity, ElevatorEnemyState>() ?? throw new InvalidOperationException("The elevator has no state.")).Program,
            "elevator initializer animation");
        slot.InstructionTimer = WrappingCountdown.Start(1);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(ElevatorAnimations.Programs, ElevatorProgram.Loop, ElevatorFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(ElevatorAnimations.Programs, ElevatorProgram.Loop)),
            "elevator runs its loop and returns through its goto");
        Console.WriteLine("  elevator behavior: the initializer starts and runs its loop.");
    }

    /// <summary>Runs the elevator behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--elevator-instruction-mechanics")]
    internal static int ElevatorBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyElevatorBehavior), () => VerifyElevatorBehavior());
        Suite(nameof(ElevatorAnimationsVerification.VerifyElevatorAnimations), () => ElevatorAnimationsVerification.VerifyElevatorAnimations(rom));
        return 0;
    }
}

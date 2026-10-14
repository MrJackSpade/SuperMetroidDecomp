using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Elevator;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that elevator starts and runs its typed animation (#641).</summary>
internal static class ElevatorBehaviorVerification
{
    /// <summary>The real initializer starts the Loop program, which the runtime then plays through once and back.</summary>
    internal static void VerifyElevatorBehavior(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Elevator;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = ElevatorFrameDefinitions.Bank };
        enemies.ElevatorBehavior.InitializeElevator(slot, null);
        AssertEqual(AnimationProgramCursor<ElevatorProgram>.Start(ElevatorProgram.Loop), (enemies.ElevatorBehavior._elevatorStates[0] ?? throw new InvalidOperationException("The elevator has no state.")).Program,
            "elevator initializer animation");
        slot.InstructionTimer = NativeCountdown.Start(1);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(ElevatorAnimations.Programs, ElevatorProgram.Loop, ElevatorFrameDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(enemies, slot, EnemyInstructionRunner.LoopUpdates(ElevatorAnimations.Programs, ElevatorProgram.Loop)),
            "elevator runs its loop and returns through its goto");
        Console.WriteLine("  elevator behavior: the initializer starts and runs its loop.");
    }

    /// <summary>Runs the elevator behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--elevator-instruction-mechanics")]
    internal static int ElevatorBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyElevatorBehavior), () => VerifyElevatorBehavior(rom));
        Suite(nameof(ElevatorAnimationsVerification.VerifyElevatorAnimations), () => ElevatorAnimationsVerification.VerifyElevatorAnimations(rom));
        return 0;
    }
}

using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Boulder.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Boulder;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that Boulder starts and runs its typed animation (#641).</summary>
internal static class BoulderBehaviorVerification
{
    /// <summary>The real initializer starts the Left program, which the runtime then plays through once and back.</summary>
    internal static void VerifyBoulderBehavior(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Boulder, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = BoulderFrameToolingDefinitions.Bank });
        slot.CurrentInstruction = 8;
        slot.Parameter1 = 256;
        slot.Parameter2 = 264;
        slot.Position = slot.Position.WithWhole(new(256, 512));
        BoulderBehavior.InitializeBoulder(slot);
        AssertEqual(AnimationProgramCursor<BoulderProgram>.Start(BoulderProgram.Left), ((enemies.SlotTable.Slots[0].Entity as BoulderEntity)?.State ?? throw new InvalidOperationException("The Boulder has no state.")).Program,
            "Boulder initializer animation");
        slot.InstructionTimer = NativeCountdown.Start(1);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(BoulderAnimations.Programs, BoulderProgram.Left, BoulderFrameDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(BoulderAnimations.Programs, BoulderProgram.Left)),
            "Boulder runs its loop and returns through its goto");
        Console.WriteLine("  Boulder behavior: the initializer starts and runs its loop.");
    }

    /// <summary>Runs the Boulder behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--boulder-instruction-mechanics")]
    internal static int BoulderBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBoulderBehavior), () => VerifyBoulderBehavior(rom));
        Suite(nameof(BoulderAnimationsVerification.VerifyBoulderAnimations), () => BoulderAnimationsVerification.VerifyBoulderAnimations(rom));
        return 0;
    }
}

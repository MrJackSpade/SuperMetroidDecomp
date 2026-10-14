using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Crawlers;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that Multiviola starts and runs its typed animation (#641).</summary>
internal static class MultiviolaBehaviorVerification
{
    /// <summary>The real initializer starts the Flying program, which the runtime then plays through once and back.</summary>
    internal static void VerifyMultiviolaBehavior(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Multiviola;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = MultiviolaFrameDefinitions.Bank };
        enemies.MultiviolaBehavior.InitializeMultiviola(slot);
        AssertEqual(AnimationProgramCursor<MultiviolaProgram>.Start(MultiviolaProgram.Flying), (enemies.MultiviolaBehavior._multiviolaStates[0] ?? throw new InvalidOperationException("Multiviola has no state.")).Program,
            "Multiviola initializer animation");
        slot.InstructionTimer = NativeCountdown.Start(1);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(MultiviolaAnimations.Programs, MultiviolaProgram.Flying, MultiviolaFrameDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(enemies, slot, EnemyInstructionRunner.LoopUpdates(MultiviolaAnimations.Programs, MultiviolaProgram.Flying)),
            "Multiviola runs its loop and returns through its goto");
        Console.WriteLine("  Multiviola behavior: the initializer starts and runs its loop.");
    }

    /// <summary>Runs the Multiviola behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--multiviola-instruction-mechanics")]
    internal static int MultiviolaBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMultiviolaBehavior), () => VerifyMultiviolaBehavior(rom));
        Suite(nameof(MultiviolaAnimationsVerification.VerifyMultiviolaAnimations), () => MultiviolaAnimationsVerification.VerifyMultiviolaAnimations(rom));
        return 0;
    }
}

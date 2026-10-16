using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.Fireflea.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Fireflea;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that Fireflea starts and runs its typed animation (#641).</summary>
internal static class FirefleaBehaviorVerification
{
    /// <summary>The real initializer starts the Loop program, which the runtime then plays through once and back.</summary>
    internal static void VerifyFirefleaBehavior()
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Fireflea, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = FirefleaFrameToolingDefinitions.Bank });
        slot.Parameter1 = 0;
        slot.Parameter2 = 0;
        enemies.FirefleaBehavior.InitializeFireflea(slot);
        AssertEqual(AnimationProgramCursor<FirefleaProgram>.Start(FirefleaProgram.Loop), (enemies.SlotTable.Slots[0].EntityState<FirefleaEntity, FirefleaEnemyState>() ?? throw new InvalidOperationException("The Fireflea has no state.")).Program,
            "Fireflea initializer animation");
        slot.InstructionTimer = NativeCountdown.Start(1);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(FirefleaAnimations.Programs, FirefleaProgram.Loop, FirefleaFrameDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(FirefleaAnimations.Programs, FirefleaProgram.Loop)),
            "Fireflea runs its loop and returns through its goto");
        Console.WriteLine("  Fireflea behavior: the initializer starts and runs its loop.");
    }

    /// <summary>Runs the Fireflea behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--fireflea-instruction-program-definitions")]
    internal static int FirefleaBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyFirefleaBehavior), () => VerifyFirefleaBehavior());
        Suite(nameof(FirefleaAnimationsVerification.VerifyFirefleaAnimations), () => FirefleaAnimationsVerification.VerifyFirefleaAnimations(rom));
        return 0;
    }
}

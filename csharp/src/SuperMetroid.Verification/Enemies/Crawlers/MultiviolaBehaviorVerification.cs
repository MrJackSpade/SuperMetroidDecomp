using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Crawlers;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that Multiviola starts and runs its typed animation (#641).</summary>
internal static class MultiviolaBehaviorVerification
{
    /// <summary>The real initializer starts the Flying program, which the runtime then plays through once and back.</summary>
    internal static void VerifyMultiviolaBehavior()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Multiviola, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnWords(EnemyDefinitionId.Multiviola, words => words);
        MultiviolaBehavior.InitializeMultiviola(slot);
        AssertEqual(AnimationProgramCursor<MultiviolaProgram>.Start(MultiviolaProgram.Flying), (enemies.SlotTable.Slots[0].EntityState<MultiviolaEntity, MultiviolaEnemyState>() ?? throw new InvalidOperationException("Multiviola has no state.")).Program,
            "Multiviola initializer animation");
        slot.FrameHold = WrappingCountdown.Start(1);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(MultiviolaAnimations.Programs, MultiviolaProgram.Flying, MultiviolaFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(MultiviolaAnimations.Programs, MultiviolaProgram.Flying)),
            "Multiviola runs its loop and returns through its goto");
        Console.WriteLine("  Multiviola behavior: the initializer starts and runs its loop.");
    }

    /// <summary>Runs the Multiviola behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--multiviola-instruction-mechanics")]
    internal static int MultiviolaBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMultiviolaBehavior), () => VerifyMultiviolaBehavior());
        Suite(nameof(MultiviolaAnimationsVerification.VerifyMultiviolaAnimations), () => MultiviolaAnimationsVerification.VerifyMultiviolaAnimations(rom));
        return 0;
    }
}

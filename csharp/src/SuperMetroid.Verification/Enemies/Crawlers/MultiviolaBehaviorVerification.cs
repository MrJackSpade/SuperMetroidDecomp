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
        AssertEqual(new AnimationPlayback<MultiviolaSegment>(MultiviolaAnimations.StartingWith(MultiviolaSegment.Flying), AnimationPlace.StartOfAnimation), (enemies.SlotTable.Slots[0].EntityState<MultiviolaEntity, MultiviolaEnemyState>() ?? throw new InvalidOperationException("Multiviola has no state.")).Playback,
            "Multiviola initializer animation");
        slot.AnimationClock.StepOnNextUpdate();
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(MultiviolaAnimations.Segments, MultiviolaSegment.Flying, MultiviolaFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(MultiviolaAnimations.Segments, MultiviolaSegment.Flying)),
            "Multiviola runs its loop and returns through its goto");
        Console.WriteLine("  Multiviola behavior: the initializer starts and runs its loop.");
    }

    /// <summary>
    /// Every heading bounces as the original's toggle of its quarter-turn bits does (#1607): a quarter turn's after a
    /// wall, three quarters' after a floor or ceiling.
    /// </summary>
    internal static void VerifyMultiviolaBounce()
    {
        for (int heading = 0; heading < 256; heading++)
        {
            AssertEqual(new TrigonometryAngle(heading ^ 0x40), MultiviolaQuarterBounce.OffWall(new TrigonometryAngle(heading)), $"heading {heading} off a wall");
            AssertEqual(new TrigonometryAngle(heading ^ 0xc0), MultiviolaQuarterBounce.OffFloorOrCeiling(new TrigonometryAngle(heading)), $"heading {heading} off a floor");
        }
        Console.WriteLine("  Multiviola bounce: all 256 headings bounce as the original's quarter-turn toggle.");
    }

    /// <summary>Runs the Multiviola behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--multiviola-instruction-mechanics")]
    internal static int MultiviolaBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMultiviolaBehavior), () => VerifyMultiviolaBehavior());
        Suite(nameof(VerifyMultiviolaBounce), () => VerifyMultiviolaBounce());
        Suite(nameof(MultiviolaAnimationsVerification.VerifyMultiviolaAnimations), () => MultiviolaAnimationsVerification.VerifyMultiviolaAnimations(rom));
        return 0;
    }
}

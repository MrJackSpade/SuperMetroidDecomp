using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Skree.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Skree;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Skree's and Metaree's idle, wind-up, dive and stop animations and the cues they hand to main AI.</summary>
internal static class SkreeMetareeBehaviorVerification
{
    /// <summary>
    /// For each species the real initializer starts the idle loop, which runs on time; the wind-up shows two poses for
    /// 16 and 8 updates and then flags the attack ready; the dive switches off-screen processing on and loops; and the
    /// unused stop animation switches it off again.
    /// </summary>
    internal static void VerifySkreeMetareeBehavior()
    {
        foreach (bool metaree in new[] { false, true })
        {
            string species = metaree ? "Metaree" : "Skree";
            Func<SkreeMetareeAnimationPhase, SkreeMetareeProgram> program = metaree ? SkreeMetareePrograms.MetareeProgram : SkreeMetareePrograms.SkreeProgram;
            SkreeMetareeProgram idle = program(SkreeMetareeAnimationPhase.Idling);
            (RoomEnemySystem enemies, RoomEnemySlot slot, Func<AnimationPlayback<SkreeMetareeProgram>> current, Action<SkreeMetareeProgram> start, Func<bool> attackReady) = Create(metaree);
            AssertTrue(ReferenceEquals(SkreeMetareeAnimations.StartingWith(idle), current().Animation) &&
                current().Place == AnimationPlace.StartOfAnimation, $"{species} starts idling");
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(SkreeMetareeAnimations.Segments, idle, SkreeMetareeFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(SkreeMetareeAnimations.Segments, idle)), $"{species} idle loop");

            (enemies, slot, current, start, attackReady) = Create(metaree);
            SkreeMetareeProgram windUp = program(SkreeMetareeAnimationPhase.PreparingAttack);
            start(windUp);
            int[] holds = [16, 8];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames(SkreeMetareeAnimationsConstants.Spritemaps(windUp), holds),
                EnemyInstructionRunner.ShownFrames(slot, holds.Sum()), $"{species} wind-up poses");
            AssertTrue(!attackReady(), $"{species} is not ready while the last wind-up pose holds");
            EnemyInstructionRunner.ShownFrames(slot, 1);
            AssertTrue(attackReady(), $"{species} wind-up flags the attack ready");

            (enemies, slot, current, start, _) = Create(metaree);
            SkreeMetareeProgram dive = program(SkreeMetareeAnimationPhase.Diving);
            // The dive's cue phase runs on into the loop it then repeats.
            SkreeMetareeProgram diveLoop = SkreeMetareeAnimations.StartingWith(dive).Phases[^1].Segment;
            start(dive);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(SkreeMetareeAnimations.Segments, diveLoop, SkreeMetareeFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(SkreeMetareeAnimations.Segments, diveLoop)), $"{species} dive loop");
            AssertTrue(slot.Processing.HasAny(EnemyProcessing.OffScreen), $"{species} dive processes off screen");

            start(program(SkreeMetareeAnimationPhase.StopAnimating));
            EnemyInstructionRunner.ShownFrames(slot, 1);
            AssertTrue(!slot.Processing.HasAny(EnemyProcessing.OffScreen), $"{species} stop animation ends off-screen processing");
        }
        Console.WriteLine("  Skree/Metaree behavior: idle loops, wind-ups with their attack-ready cue, dives and the stop animation pass.");
    }

    /// <summary>A freshly initialized Skree or Metaree and accessors for its typed animation and attack-ready flag.</summary>
    private static SkreeMetareeBehaviorCreateResult Create(bool metaree)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(metaree ? EnemyDefinitionId.Metaree : EnemyDefinitionId.Skree, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        // The room spawn primes the instruction timer before the species initializer runs.
        slot.AnimationClock.StepOnNextUpdate();
        if (metaree)
        {
            MetareeBehavior.InitializeMetaree(slot);
            MetareeEnemyState state = enemies.MetareeStates[0] ?? throw new InvalidOperationException("Metaree has no state.");
            return new SkreeMetareeBehaviorCreateResult(enemies, slot, () => state.Playback, program => Restart(slot, () => state.Playback = new(SkreeMetareeAnimations.StartingWith(program), AnimationPlace.StartOfAnimation)), () => state.AttackReady);
        }
        SkreeBehavior.InitializeSkree(slot);
        SkreeEnemyState skree = enemies.SkreeStates[0] ?? throw new InvalidOperationException("Skree has no state.");
        return new SkreeMetareeBehaviorCreateResult(enemies, slot, () => skree.Playback, program => Restart(slot, () => skree.Playback = new(SkreeMetareeAnimations.StartingWith(program), AnimationPlace.StartOfAnimation)), () => skree.AttackReady);
    }

    /// <summary>Starts an animation with both instruction clocks reset, as the species' install routines do.</summary>
    private static void Restart(RoomEnemySlot slot, Action start)
    {
        start();
        slot.AnimationClock.Restart();
    }

    /// <summary>Runs the Skree/Metaree behavior check and the program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--skree-metaree-instruction-mechanics")]
    internal static int SkreeMetareeBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySkreeMetareeBehavior), () => VerifySkreeMetareeBehavior());
        Suite(nameof(SkreeMetareeAnimationsVerification.VerifySkreeMetareeAnimations), () => SkreeMetareeAnimationsVerification.VerifySkreeMetareeAnimations(rom));
        return 0;
    }
}

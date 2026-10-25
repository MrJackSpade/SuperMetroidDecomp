using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Skree.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
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
            Func<SkreeMetareeAnimationPhase, SkreeMetareeProgram> program = metaree ? SkreeMetareeAnimationDefinitions.MetareeProgram : SkreeMetareeAnimationDefinitions.SkreeProgram;
            SkreeMetareeProgram idle = program(SkreeMetareeAnimationPhase.Idling);
            (RoomEnemySystem enemies, RoomEnemySlot slot, Func<AnimationProgramCursor<SkreeMetareeProgram>> current, Action<SkreeMetareeProgram> start, Func<bool> attackReady) = Create(metaree);
            AssertEqual(AnimationProgramCursor<SkreeMetareeProgram>.Start(idle), current(), $"{species} starts idling");
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(SkreeMetareeAnimations.Programs, idle, SkreeMetareeFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(SkreeMetareeAnimations.Programs, idle)), $"{species} idle loop");

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
            SkreeMetareeProgram diveLoop = SkreeMetareeAnimations.Programs.Steps(dive).OfType<AnimationProgramStep<SkreeMetareeProgram, SkreeMetareeFrame, SkreeMetareeCue>.FallIntoStep>().Single().Target;
            start(dive);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(SkreeMetareeAnimations.Programs, diveLoop, SkreeMetareeFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(SkreeMetareeAnimations.Programs, diveLoop)), $"{species} dive loop");
            AssertTrue(slot.Properties.HasAny(EnemyProperties.ProcessOffScreen), $"{species} dive processes off screen");

            start(program(SkreeMetareeAnimationPhase.StopAnimating));
            EnemyInstructionRunner.ShownFrames(slot, 1);
            AssertTrue(!slot.Properties.HasAny(EnemyProperties.ProcessOffScreen), $"{species} stop animation ends off-screen processing");
        }
        Console.WriteLine("  Skree/Metaree behavior: idle loops, wind-ups with their attack-ready cue, dives and the stop animation pass.");
    }

    /// <summary>A freshly initialized Skree or Metaree and accessors for its typed animation and attack-ready flag.</summary>
    private static SkreeMetareeBehaviorCreateResult Create(bool metaree)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(metaree ? EnemyDefinitionId.Metaree : EnemyDefinitionId.Skree, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        // The room spawn primes the instruction timer before the species initializer runs.
        slot.InstructionTimer = WrappingCountdown.Start(1);
        if (metaree)
        {
            MetareeBehavior.InitializeMetaree(slot);
            MetareeEnemyState state = enemies.MetareeStates[0] ?? throw new InvalidOperationException("Metaree has no state.");
            return new SkreeMetareeBehaviorCreateResult(enemies, slot, () => state.Program, program => Restart(slot, () => state.Program = AnimationProgramCursor<SkreeMetareeProgram>.Start(program)), () => state.AttackReady);
        }
        SkreeBehavior.InitializeSkree(slot);
        SkreeEnemyState skree = enemies.SkreeStates[0] ?? throw new InvalidOperationException("Skree has no state.");
        return new SkreeMetareeBehaviorCreateResult(enemies, slot, () => skree.Program, program => Restart(slot, () => skree.Program = AnimationProgramCursor<SkreeMetareeProgram>.Start(program)), () => skree.AttackReady);
    }

    /// <summary>Starts an animation with both instruction clocks reset, as the species' install routines do.</summary>
    private static void Restart(RoomEnemySlot slot, Action start)
    {
        start();
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Runs the Skree/Metaree behavior check and the program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--skree-metaree-instruction-mechanics")]
    internal static int SkreeMetareeBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySkreeMetareeBehavior), () => VerifySkreeMetareeBehavior());
        Suite(nameof(SkreeMetareeAnimationsVerification.VerifySkreeMetareeAnimations), () => SkreeMetareeAnimationsVerification.VerifySkreeMetareeAnimations(rom));
        return 0;
    }
}

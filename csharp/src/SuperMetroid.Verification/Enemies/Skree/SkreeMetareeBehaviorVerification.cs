using SuperMetroid.Core.Animation;
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
            SkreeMetareeSegment idle = metaree ? SkreeMetareeSegment.MetareeIdling : SkreeMetareeSegment.SkreeIdling;
            (RoomEnemySystem enemies, RoomEnemyPlace place, Func<AnimationPlayback<SkreeMetareeSegment>> current, Action<SkreeMetareeSegment> start, Func<bool> attackReady) = Create(metaree);
            AssertTrue(ReferenceEquals(SkreeMetareeAnimations.StartingWith(idle), current().Animation) &&
                current().Place == AnimationPlace.StartOfAnimation, $"{species} starts idling");
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(SkreeMetareeAnimations.Segments, idle, SkreeMetareeFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(SkreeMetareeAnimations.Segments, idle)), $"{species} idle loop");

            (enemies, place, current, start, attackReady) = Create(metaree);
            SkreeMetareeSegment windUp = metaree ? SkreeMetareeSegment.MetareePreparingAttack : SkreeMetareeSegment.SkreePreparingAttack;
            start(windUp);
            int[] holds = [16, 8];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames(SkreeMetareeAnimationsConstants.Spritemaps(windUp), holds),
                EnemyInstructionRunner.ShownFrames(place, holds.Sum()), $"{species} wind-up poses");
            AssertTrue(!attackReady(), $"{species} is not ready while the last wind-up pose holds");
            EnemyInstructionRunner.ShownFrames(place, 1);
            AssertTrue(attackReady(), $"{species} wind-up flags the attack ready");

            (enemies, place, current, start, _) = Create(metaree);
            SkreeMetareeSegment dive = metaree ? SkreeMetareeSegment.MetareeDiving : SkreeMetareeSegment.SkreeDiving;
            // The dive's cue phase runs on into the loop it then repeats.
            SkreeMetareeSegment diveLoop = SkreeMetareeAnimations.StartingWith(dive).Phases[^1].Segment;
            start(dive);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(SkreeMetareeAnimations.Segments, diveLoop, SkreeMetareeFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(SkreeMetareeAnimations.Segments, diveLoop)), $"{species} dive loop");
            AssertTrue(place.Processing.HasAny(EnemyProcessing.OffScreen), $"{species} dive processes off screen");
        }
        Console.WriteLine("  Skree/Metaree behavior: idle loops, wind-ups with their attack-ready cue, and dives pass.");
    }

    /// <summary>A freshly initialized Skree or Metaree and accessors for its typed animation and attack-ready flag.</summary>
    private static SkreeMetareeBehaviorCreateResult Create(bool metaree)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(metaree ? EnemyDefinitionId.Metaree : EnemyDefinitionId.Skree, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        // The room spawn primes the instruction timer before the species initializer runs.
        place.AnimationClock.StepOnNextUpdate();
        if (metaree)
        {
            MetareeBehavior.InitializeMetaree();
            MetareeEnemyState state = enemies.MetareeStates[0] ?? throw new InvalidOperationException("Metaree has no state.");
            // The Metaree prepares its attack as its AI starts the wind-up, and the wind-up's cue readies it (#2439).
            return new SkreeMetareeBehaviorCreateResult(enemies, place, () => state.Playback, program => Restart(place, () =>
            {
                state.Playback = new(SkreeMetareeAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
                if (program == SkreeMetareeSegment.MetareePreparingAttack)
                    state.Action = MetareeAction.PreparingAttack;
            }), () => state.Action == MetareeAction.AttackReady);
        }
        SkreeBehavior.InitializeSkree();
        SkreeEnemyState skree = enemies.SkreeStates[0] ?? throw new InvalidOperationException("Skree has no state.");
        // The Skree winds up as its AI starts the wind-up, and the wind-up's cue readies its dive (#2450).
        return new SkreeMetareeBehaviorCreateResult(enemies, place, () => skree.Playback, program => Restart(place, () =>
        {
            skree.Playback = new(SkreeMetareeAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            if (program == SkreeMetareeSegment.SkreePreparingAttack)
                skree.Action = SkreeAction.PreparingAttack;
        }), () => skree.Action == SkreeAction.AttackReady);
    }

    /// <summary>Starts an animation with both instruction clocks reset, as the species' install routines do.</summary>
    private static void Restart(RoomEnemyPlace place, Action start)
    {
        start();
        place.AnimationClock.Restart();
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

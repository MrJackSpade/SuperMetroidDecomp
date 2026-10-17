using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassPlmProgram, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassDraw, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassPlmCue>;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's glass PLM programs (#1296), in place of its bank-$84 instruction lists. On entry the glass shows
/// cleared once the boss is dead, shattered once destroyed, and otherwise arms its hit wake and holds each of nine
/// stages until the glass has taken two more hits; later stages burst into shards, and the last sets the destroyed
/// event.
/// </summary>
internal static class MotherBrainGlassPlmProgramDefinitions
{
    /// <summary><c>$84:D1E6</c>: the pre-instruction that counts missile hits on the glass.</summary>
    internal const ushort HitPreInstruction = 0xd1e6;
    /// <summary>How many hits each stage adds to the next stage's threshold.</summary>
    private const ushort HitsPerStage = 2;
    /// <summary>How long a burst's in-between image shows.</summary>
    private const int BurstFrameDuration = 4;
    /// <summary>How long the final shattered image shows before the destroyed event.</summary>
    private const int FinalShatterDuration = 48;

    /// <summary>The stages in order.</summary>
    private static readonly MotherBrainGlassPlmProgram[] Stages =
    [
        MotherBrainGlassPlmProgram.Stage0, MotherBrainGlassPlmProgram.Stage1, MotherBrainGlassPlmProgram.Stage2,
        MotherBrainGlassPlmProgram.Stage3, MotherBrainGlassPlmProgram.Stage4, MotherBrainGlassPlmProgram.Stage5,
        MotherBrainGlassPlmProgram.Stage6, MotherBrainGlassPlmProgram.Stage7, MotherBrainGlassPlmProgram.Stage8,
    ];

    /// <summary>The glass image of each stage, then the final shatter.</summary>
    private static readonly MotherBrainGlassDraw[] StageDraws =
    [
        MotherBrainGlassDraw.Initial, MotherBrainGlassDraw.PaneDamage1, MotherBrainGlassDraw.PaneDamage2,
        MotherBrainGlassDraw.PaneTransition, MotherBrainGlassDraw.ShiftedPane1, MotherBrainGlassDraw.ShiftedPane2,
        MotherBrainGlassDraw.ShiftedPane3, MotherBrainGlassDraw.Shatter1, MotherBrainGlassDraw.Shatter2,
        MotherBrainGlassDraw.Shatter3,
    ];

    /// <summary>Every glass program.</summary>
    internal static AnimationProgramSet<MotherBrainGlassPlmProgram, MotherBrainGlassDraw, MotherBrainGlassPlmCue> Programs { get; } = new(Build());

    /// <summary>Builds the entry, the nine stages and the two already-resolved states.</summary>
    private static Dictionary<MotherBrainGlassPlmProgram, Step[]> Build()
    {
        var programs = new Dictionary<MotherBrainGlassPlmProgram, Step[]>
        {
            [MotherBrainGlassPlmProgram.Entry] =
            [
                Step.Publish(MotherBrainGlassPlmCue.Of(MotherBrainGlassPlmCueKind.SkipIfBossDead)),
                Step.Publish(MotherBrainGlassPlmCue.Of(MotherBrainGlassPlmCueKind.SkipIfGlassDestroyed)),
                Step.Publish(MotherBrainGlassPlmCue.Of(MotherBrainGlassPlmCueKind.ArmHitWake)),
                Step.FallInto(MotherBrainGlassPlmProgram.Stage0),
            ],
            [MotherBrainGlassPlmProgram.BossDead] = [Step.Show(MotherBrainGlassDraw.Cleared, 1), Step.Stop],
            [MotherBrainGlassPlmProgram.NoGlass] = [Step.Show(MotherBrainGlassDraw.Shatter3, 1), Step.Stop],
        };
        for (int stage = 0; stage < Stages.Length; stage++)
            programs[Stages[stage]] = Stage(stage);
        return programs;
    }

    /// <summary>Shows a stage and holds it until enough hits; a burst stage then spawns its shards around the next image.</summary>
    private static Step[] Stage(int stage)
    {
        List<Step> steps =
        [
            Step.Show(StageDraws[stage], 1),
            Step.Publish(MotherBrainGlassPlmCue.RepeatUntil(HitsPerStage * (stage + 1))),
        ];
        if (BurstOf(stage) is { } burst)
        {
            steps.Add(Step.Publish(burst.First));
            steps.Add(Step.Show(StageDraws[stage + 1], BurstFrameDuration));
            steps.Add(Step.Publish(burst.Second));
        }
        if (stage + 1 < Stages.Length)
        {
            steps.Add(Step.FallInto(Stages[stage + 1]));
            return [.. steps];
        }
        steps.Add(Step.Show(StageDraws[stage + 1], FinalShatterDuration));
        steps.Add(Step.Publish(MotherBrainGlassPlmCue.Of(MotherBrainGlassPlmCueKind.SetGlassDestroyed)));
        steps.Add(Step.Stop);
        return [.. steps];
    }

    /// <summary>A burst stage's two groups of four shards, or null for a stage that does not burst.</summary>
    private static (MotherBrainGlassPlmCue First, MotherBrainGlassPlmCue Second)? BurstOf(int stage)
    {
        const MotherBrainGlassShard R = MotherBrainGlassShard.Right, L = MotherBrainGlassShard.Left, C = MotherBrainGlassShard.Centre;
        return stage switch
        {
            0 or 1 or 3 or 4 => null,
            2 => (MotherBrainGlassPlmCue.Burst(R, R, R, R), MotherBrainGlassPlmCue.Burst(R, R, R, R)),
            5 => (MotherBrainGlassPlmCue.Burst(L, L, L, L), MotherBrainGlassPlmCue.Burst(L, L, L, L)),
            6 => (MotherBrainGlassPlmCue.Burst(R, R, L, L), MotherBrainGlassPlmCue.Burst(C, C, C, C)),
            7 or 8 => (MotherBrainGlassPlmCue.Burst(L, L, C, C), MotherBrainGlassPlmCue.Burst(L, L, C, C)),
            _ => throw new ArgumentOutOfRangeException(nameof(stage)),
        };
    }
}

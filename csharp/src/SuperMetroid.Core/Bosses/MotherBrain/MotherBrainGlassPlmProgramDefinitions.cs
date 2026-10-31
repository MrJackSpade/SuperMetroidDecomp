using SuperMetroid.Core.Programs;
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
    /// <summary>How many hits each stage adds to the next stage's threshold.</summary>
    private const int HitsPerStage = 2;
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

    /// <summary>Every glass program: the entry, the nine stages and the two already-resolved states.</summary>
    internal static AnimationProgramSet<MotherBrainGlassPlmProgram, MotherBrainGlassDraw, MotherBrainGlassPlmCue> Programs { get; } = new(
        new Dictionary<MotherBrainGlassPlmProgram, Step[]>
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
            [MotherBrainGlassPlmProgram.Stage0] = Stage(0),
            [MotherBrainGlassPlmProgram.Stage1] = Stage(1),
            [MotherBrainGlassPlmProgram.Stage2] = BurstStage(2, new(MotherBrainGlassPlmCue.Burst(R, R, R, R), MotherBrainGlassPlmCue.Burst(R, R, R, R))),
            [MotherBrainGlassPlmProgram.Stage3] = Stage(3),
            [MotherBrainGlassPlmProgram.Stage4] = Stage(4),
            [MotherBrainGlassPlmProgram.Stage5] = BurstStage(5, new(MotherBrainGlassPlmCue.Burst(L, L, L, L), MotherBrainGlassPlmCue.Burst(L, L, L, L))),
            [MotherBrainGlassPlmProgram.Stage6] = BurstStage(6, new(MotherBrainGlassPlmCue.Burst(R, R, L, L), MotherBrainGlassPlmCue.Burst(C, C, C, C))),
            [MotherBrainGlassPlmProgram.Stage7] = BurstStage(7, new(MotherBrainGlassPlmCue.Burst(L, L, C, C), MotherBrainGlassPlmCue.Burst(L, L, C, C))),
            [MotherBrainGlassPlmProgram.Stage8] = FinalStage(8, new(MotherBrainGlassPlmCue.Burst(L, L, C, C), MotherBrainGlassPlmCue.Burst(L, L, C, C))),
        });

    /// <summary>The right emitter.</summary>
    private const MotherBrainGlassShard R = MotherBrainGlassShard.Right;
    /// <summary>The left emitter.</summary>
    private const MotherBrainGlassShard L = MotherBrainGlassShard.Left;
    /// <summary>The centre emitter.</summary>
    private const MotherBrainGlassShard C = MotherBrainGlassShard.Centre;

    /// <summary>Shows a stage and holds it until enough hits, then falls into the next stage.</summary>
    private static Step[] Stage(int stage) =>
    [
        Step.Show(StageDraws[stage], 1),
        Step.Publish(MotherBrainGlassPlmCue.RepeatUntil(HitsPerStage * (stage + 1))),
        Step.FallInto(Stages[stage + 1]),
    ];

    /// <summary>Shows a stage, holds it until enough hits, spawns its two shard groups around the next image, then falls into the next stage.</summary>
    private static Step[] BurstStage(int stage, MotherBrainGlassBurst burst) =>
    [
        Step.Show(StageDraws[stage], 1),
        Step.Publish(MotherBrainGlassPlmCue.RepeatUntil(HitsPerStage * (stage + 1))),
        Step.Publish(burst.First),
        Step.Show(StageDraws[stage + 1], BurstFrameDuration),
        Step.Publish(burst.Second),
        Step.FallInto(Stages[stage + 1]),
    ];

    /// <summary>The last stage: its burst, then the final shatter shown before the destroyed event.</summary>
    private static Step[] FinalStage(int stage, MotherBrainGlassBurst burst) =>
    [
        Step.Show(StageDraws[stage], 1),
        Step.Publish(MotherBrainGlassPlmCue.RepeatUntil(HitsPerStage * (stage + 1))),
        Step.Publish(burst.First),
        Step.Show(StageDraws[stage + 1], BurstFrameDuration),
        Step.Publish(burst.Second),
        Step.Show(StageDraws[stage + 1], FinalShatterDuration),
        Step.Publish(MotherBrainGlassPlmCue.Of(MotherBrainGlassPlmCueKind.SetGlassDestroyed)),
        Step.Stop,
    ];
}

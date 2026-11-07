using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassObjectProgram, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassDraw, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassObjectCue>;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's glass PLM programs (#1296), in place of its instruction lists. On entry the glass shows
/// cleared once the boss is dead, shattered once destroyed, and otherwise arms its hit wake and holds each of nine
/// stages until the glass has taken two more hits; later stages burst into shards, and the last sets the destroyed
/// event.
/// </summary>
internal static class MotherBrainGlassObjectProgramDefinitions
{
    /// <summary>How many hits each stage adds to the next stage's threshold.</summary>
    private const int HitsPerStage = 2;
    /// <summary>How long a burst's in-between image shows.</summary>
    private const int BurstFrameDuration = 4;
    /// <summary>How long the final shattered image shows before the destroyed event.</summary>
    private const int FinalShatterDuration = 48;

    /// <summary>The stages in order.</summary>
    private static readonly MotherBrainGlassObjectProgram[] Stages =
    [
        MotherBrainGlassObjectProgram.Stage0, MotherBrainGlassObjectProgram.Stage1, MotherBrainGlassObjectProgram.Stage2,
        MotherBrainGlassObjectProgram.Stage3, MotherBrainGlassObjectProgram.Stage4, MotherBrainGlassObjectProgram.Stage5,
        MotherBrainGlassObjectProgram.Stage6, MotherBrainGlassObjectProgram.Stage7, MotherBrainGlassObjectProgram.Stage8,
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
    internal static AnimationProgramSet<MotherBrainGlassObjectProgram, MotherBrainGlassDraw, MotherBrainGlassObjectCue> Programs { get; } = new(
        new Dictionary<MotherBrainGlassObjectProgram, Step[]>
        {
            [MotherBrainGlassObjectProgram.Entry] =
            [
                Step.Publish(MotherBrainGlassObjectCue.Of(MotherBrainGlassObjectCueKind.SkipIfBossDead)),
                Step.Publish(MotherBrainGlassObjectCue.Of(MotherBrainGlassObjectCueKind.SkipIfGlassDestroyed)),
                Step.Publish(MotherBrainGlassObjectCue.Of(MotherBrainGlassObjectCueKind.ArmHitWake)),
                Step.FallInto(MotherBrainGlassObjectProgram.Stage0),
            ],
            [MotherBrainGlassObjectProgram.BossDead] = [Step.Show(MotherBrainGlassDraw.Cleared, 1), Step.Stop],
            [MotherBrainGlassObjectProgram.NoGlass] = [Step.Show(MotherBrainGlassDraw.Shatter3, 1), Step.Stop],
            [MotherBrainGlassObjectProgram.Stage0] = Stage(0),
            [MotherBrainGlassObjectProgram.Stage1] = Stage(1),
            [MotherBrainGlassObjectProgram.Stage2] = BurstStage(2, new(MotherBrainGlassObjectCue.Burst(R, R, R, R), MotherBrainGlassObjectCue.Burst(R, R, R, R))),
            [MotherBrainGlassObjectProgram.Stage3] = Stage(3),
            [MotherBrainGlassObjectProgram.Stage4] = Stage(4),
            [MotherBrainGlassObjectProgram.Stage5] = BurstStage(5, new(MotherBrainGlassObjectCue.Burst(L, L, L, L), MotherBrainGlassObjectCue.Burst(L, L, L, L))),
            [MotherBrainGlassObjectProgram.Stage6] = BurstStage(6, new(MotherBrainGlassObjectCue.Burst(R, R, L, L), MotherBrainGlassObjectCue.Burst(C, C, C, C))),
            [MotherBrainGlassObjectProgram.Stage7] = BurstStage(7, new(MotherBrainGlassObjectCue.Burst(L, L, C, C), MotherBrainGlassObjectCue.Burst(L, L, C, C))),
            [MotherBrainGlassObjectProgram.Stage8] = FinalStage(8, new(MotherBrainGlassObjectCue.Burst(L, L, C, C), MotherBrainGlassObjectCue.Burst(L, L, C, C))),
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
        Step.Publish(MotherBrainGlassObjectCue.RepeatUntil(HitsPerStage * (stage + 1))),
        Step.FallInto(Stages[stage + 1]),
    ];

    /// <summary>Shows a stage, holds it until enough hits, spawns its two shard groups around the next image, then falls into the next stage.</summary>
    private static Step[] BurstStage(int stage, MotherBrainGlassBurst burst) =>
    [
        Step.Show(StageDraws[stage], 1),
        Step.Publish(MotherBrainGlassObjectCue.RepeatUntil(HitsPerStage * (stage + 1))),
        Step.Publish(burst.First),
        Step.Show(StageDraws[stage + 1], BurstFrameDuration),
        Step.Publish(burst.Second),
        Step.FallInto(Stages[stage + 1]),
    ];

    /// <summary>The last stage: its burst, then the final shatter shown before the destroyed event.</summary>
    private static Step[] FinalStage(int stage, MotherBrainGlassBurst burst) =>
    [
        Step.Show(StageDraws[stage], 1),
        Step.Publish(MotherBrainGlassObjectCue.RepeatUntil(HitsPerStage * (stage + 1))),
        Step.Publish(burst.First),
        Step.Show(StageDraws[stage + 1], BurstFrameDuration),
        Step.Publish(burst.Second),
        Step.Show(StageDraws[stage + 1], FinalShatterDuration),
        Step.Publish(MotherBrainGlassObjectCue.Of(MotherBrainGlassObjectCueKind.SetGlassDestroyed)),
        Step.Stop,
    ];
}

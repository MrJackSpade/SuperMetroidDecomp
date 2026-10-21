using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHandBeamProgram, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHandBeamFrame, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHandBeamCue>;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The red hand beam's typed program (#641), in place of its bank-$86 instruction list at $86:C796.</summary>
internal static class MotherBrainHandBeamAnimations
{
    /// <summary>Stages the beam runs before deleting itself.</summary>
    internal const int StageCount = 3;

    /// <summary>
    /// Three stages, each spawning the next beam after its first frame and fading over seven frames held 3, 3, 2, 2,
    /// 1, 1 and 1 updates, then the deletion. The charging beam and every fired beam run it.
    /// </summary>
    internal static AnimationProgramSet<MotherBrainHandBeamProgram, MotherBrainHandBeamFrame, MotherBrainHandBeamCue> Programs { get; } = new(
        new Dictionary<MotherBrainHandBeamProgram, Step[]>
        {
            [MotherBrainHandBeamProgram.Beam] =
            [
                .. Enumerable.Range(0, StageCount).SelectMany(Stage),
                Step.Publish(MotherBrainHandBeamCue.Delete),
                Step.Hold,
            ],
        });

    /// <summary>The steps of stage <paramref name="stage"/>.</summary>
    private static IEnumerable<Step> Stage(int stage)
    {
        yield return Step.Show(new(stage, 0), 3);
        yield return Step.Publish(MotherBrainHandBeamCue.SpawnNextBeam);
        foreach ((int phase, int duration) in new[] { new MotherBrainHandBeamFrameHold(1, 3), new MotherBrainHandBeamFrameHold(2, 2), new MotherBrainHandBeamFrameHold(3, 2), new MotherBrainHandBeamFrameHold(4, 1), new MotherBrainHandBeamFrameHold(5, 1), new MotherBrainHandBeamFrameHold(6, 1) })
            yield return Step.Show(new(stage, phase), duration);
    }
}

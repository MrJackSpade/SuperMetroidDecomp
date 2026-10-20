using SuperMetroid.Core.Programs;

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
    internal static AnimationTimeline<MotherBrainHandBeamFrame, MotherBrainHandBeamCue> Timeline { get; } = new(
    [
        .. Enumerable.Range(0, StageCount).SelectMany(Stage),
        AnimationStep<MotherBrainHandBeamFrame, MotherBrainHandBeamCue>.Publish(MotherBrainHandBeamCue.Delete),
        AnimationStep<MotherBrainHandBeamFrame, MotherBrainHandBeamCue>.Hold,
    ]);

    /// <summary>The steps of stage <paramref name="stage"/>.</summary>
    private static IEnumerable<AnimationStep<MotherBrainHandBeamFrame, MotherBrainHandBeamCue>> Stage(int stage)
    {
        yield return AnimationStep<MotherBrainHandBeamFrame, MotherBrainHandBeamCue>.Show(new(stage, 0), 3);
        yield return AnimationStep<MotherBrainHandBeamFrame, MotherBrainHandBeamCue>.Publish(MotherBrainHandBeamCue.SpawnNextBeam);
        foreach ((int phase, int duration) in new[] { (1, 3), (2, 2), (3, 2), (4, 1), (5, 1), (6, 1) })
            yield return AnimationStep<MotherBrainHandBeamFrame, MotherBrainHandBeamCue>.Show(new(stage, phase), duration);
    }
}

using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>One frame of the red hand beam's program: its stage and its position there.</summary>
/// <param name="Stage">One of the three identical stages.</param>
/// <param name="Phase">The frame's position in its stage.</param>
public readonly record struct MotherBrainHandBeamFrame(int Stage, int Phase);

/// <summary>Gameplay cues of the hand beam's program.</summary>
internal enum MotherBrainHandBeamCue
{
    /// <summary><c>Spawn_MotherBrainRedBeam_Fired</c> ($86:C7FB): spawns the next fired beam along the aim.</summary>
    SpawnNextBeam,
    /// <summary>Deletes the beam after its third stage.</summary>
    Delete,
}

/// <summary>The red hand beam's typed program (#641), in place of its bank-$86 instruction list at $86:C796.</summary>
internal static class MotherBrainHandBeamAnimations
{
    /// <summary>Stages the beam runs before deleting itself.</summary>
    internal const int StageCount = 3;
    /// <summary>Frames in each stage.</summary>
    internal const int FramesPerStage = 7;

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

/// <summary>A hand beam's typed animation.</summary>
internal sealed class MotherBrainHandBeamAnimation : EnemyProjectileTimelineAnimation<MotherBrainHandBeamFrame, MotherBrainHandBeamCue>
{
    /// <inheritdoc/>
    private protected override AnimationTimeline<MotherBrainHandBeamFrame, MotherBrainHandBeamCue> Timeline =>
        MotherBrainHandBeamAnimations.Timeline;

    /// <inheritdoc/>
    private protected override ushort PresentationKey(MotherBrainHandBeamFrame frame) =>
        MotherBrainHandBeamVisualDefinitions.PresentationKey(frame);
}

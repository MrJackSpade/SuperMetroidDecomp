using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTopTubeSegment;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's falling ceiling-tube animations (#641, #1600): each tube's one pose, held, the beat naming the installed
/// <see cref="EnemyProjectileFrameId"/> it shows (#2249).
/// </summary>
internal static class MotherBrainTopTubeAnimations
{
    /// <summary>Every segment; each holds its own tube's picture.</summary>
    internal static UnpausedAnimationSegments<MotherBrainTopTubeSegment, EnemyProjectileFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MotherBrainTopTubeSegment, AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>.Unpaused[]>
    {
        [P.TopRight] = [Show(EnemyProjectileFrameId.MotherBrainTopTubeTopRight)],
        [P.TopLeft] = [Show(EnemyProjectileFrameId.MotherBrainTopTubeTopLeft)],
        [P.TopMiddleLeft] = [Show(EnemyProjectileFrameId.MotherBrainTopTubeTopMiddleLeft)],
        [P.TopMiddleRight] = [Show(EnemyProjectileFrameId.MotherBrainTopTubeTopMiddleRight)],
    });

    /// <summary>Each tube's animation: its pose, held.</summary>
    internal static IReadOnlyDictionary<MotherBrainTopTubeSegment, Animation<MotherBrainTopTubeSegment>> StartingAt { get; } =
        Enum.GetValues<P>().ToDictionary(tube => tube, tube => new Animation<P>([Segments.Whole(tube)], AnimationEnd.Holds));

    /// <summary>A still picture (#2250): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>.Unpaused Show(EnemyProjectileFrameId frame) =>
        new AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>.HeldFrame(frame);
}

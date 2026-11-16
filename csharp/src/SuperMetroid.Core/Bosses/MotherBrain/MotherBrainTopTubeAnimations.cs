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
    /// <summary>Every segment; each shows its own tube.</summary>
    internal static AnimationSegments<MotherBrainTopTubeSegment, EnemyProjectileFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MotherBrainTopTubeSegment, AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>[]>
    {
        [P.TopRight] = [Show(EnemyProjectileFrameId.MotherBrainTopTubeTopRight)],
        [P.TopLeft] = [Show(EnemyProjectileFrameId.MotherBrainTopTubeTopLeft)],
        [P.TopMiddleLeft] = [Show(EnemyProjectileFrameId.MotherBrainTopTubeTopMiddleLeft)],
        [P.TopMiddleRight] = [Show(EnemyProjectileFrameId.MotherBrainTopTubeTopMiddleRight)],
    });

    /// <summary>Each tube's animation: its pose, held.</summary>
    internal static IReadOnlyDictionary<MotherBrainTopTubeSegment, Animation<MotherBrainTopTubeSegment>> StartingAt { get; } =
        Enum.GetValues<P>().ToDictionary(tube => tube, tube => new Animation<P>([new(tube, 0, 1, 1)], AnimationEnd.Holds));

    /// <summary>Shows <paramref name="frame"/> for one update.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, NoAnimationCue> Show(EnemyProjectileFrameId frame) =>
        new AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>.Frame(frame, 1);
}

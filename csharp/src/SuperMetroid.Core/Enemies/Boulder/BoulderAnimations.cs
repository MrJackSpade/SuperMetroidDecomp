using P = SuperMetroid.Core.Enemies.Boulder.BoulderSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>
/// The Boulder's mirrored rolling loops (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class BoulderAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BoulderSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<BoulderSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Left] =
        [
            Show(EnemyFrameId.BoulderTurned0, 8),
            Show(EnemyFrameId.BoulderTurned45, 8),
            Show(EnemyFrameId.BoulderTurned90, 8),
            Show(EnemyFrameId.BoulderTurned135, 8),
            Show(EnemyFrameId.BoulderTurned180, 8),
            Show(EnemyFrameId.BoulderTurned225, 8),
            Show(EnemyFrameId.BoulderTurned270, 8),
            Show(EnemyFrameId.BoulderTurned315, 8),
        ],
        [P.Right] =
        [
            Show(EnemyFrameId.BoulderTurned0, 8),
            Show(EnemyFrameId.BoulderTurned315, 8),
            Show(EnemyFrameId.BoulderTurned270, 8),
            Show(EnemyFrameId.BoulderTurned225, 8),
            Show(EnemyFrameId.BoulderTurned180, 8),
            Show(EnemyFrameId.BoulderTurned135, 8),
            Show(EnemyFrameId.BoulderTurned90, 8),
            Show(EnemyFrameId.BoulderTurned45, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BoulderSegment, Animation<BoulderSegment>> StartingAt { get; } = new Dictionary<BoulderSegment, Animation<BoulderSegment>>
    {
        [P.Left] = new([Segments.Whole(P.Left)], AnimationEnd.LoopsFrom(0)),
        [P.Right] = new([Segments.Whole(P.Right)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BoulderSegment> StartingWith(BoulderSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

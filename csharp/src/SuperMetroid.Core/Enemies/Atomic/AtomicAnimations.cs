using P = SuperMetroid.Core.Enemies.Atomic.AtomicSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Atomic's four spinning animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class AtomicAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<AtomicSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<AtomicSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.UpRight] =
        [
            Show(EnemyFrameId.AtomicFacingUpRightTurned0, 8),
            Show(EnemyFrameId.AtomicFacingUpRightTurned60, 8),
            Show(EnemyFrameId.AtomicFacingUpRightTurned120, 8),
            Show(EnemyFrameId.AtomicFacingUpRightTurned180, 8),
            Show(EnemyFrameId.AtomicFacingUpRightTurned240, 8),
            Show(EnemyFrameId.AtomicFacingUpRightTurned300, 8),
        ],
        [P.UpLeft] =
        [
            Show(EnemyFrameId.AtomicFacingUpLeftTurned0, 8),
            Show(EnemyFrameId.AtomicFacingUpLeftTurned60, 8),
            Show(EnemyFrameId.AtomicFacingUpLeftTurned120, 8),
            Show(EnemyFrameId.AtomicFacingUpLeftTurned180, 8),
            Show(EnemyFrameId.AtomicFacingUpLeftTurned240, 8),
            Show(EnemyFrameId.AtomicFacingUpLeftTurned300, 8),
        ],
        [P.DownLeft] =
        [
            Show(EnemyFrameId.AtomicFacingUpRightTurned300, 8),
            Show(EnemyFrameId.AtomicFacingUpRightTurned240, 8),
            Show(EnemyFrameId.AtomicFacingUpRightTurned180, 8),
            Show(EnemyFrameId.AtomicFacingUpRightTurned120, 8),
            Show(EnemyFrameId.AtomicFacingUpRightTurned60, 8),
            Show(EnemyFrameId.AtomicFacingUpRightTurned0, 8),
        ],
        [P.DownRight] =
        [
            Show(EnemyFrameId.AtomicFacingUpLeftTurned300, 8),
            Show(EnemyFrameId.AtomicFacingUpLeftTurned240, 8),
            Show(EnemyFrameId.AtomicFacingUpLeftTurned180, 8),
            Show(EnemyFrameId.AtomicFacingUpLeftTurned120, 8),
            Show(EnemyFrameId.AtomicFacingUpLeftTurned60, 8),
            Show(EnemyFrameId.AtomicFacingUpLeftTurned0, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<AtomicSegment, Animation<AtomicSegment>> StartingAt { get; } = new Dictionary<AtomicSegment, Animation<AtomicSegment>>
    {
        [P.UpRight] = new([new(P.UpRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UpLeft] = new([new(P.UpLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownLeft] = new([new(P.DownLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownRight] = new([new(P.DownRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<AtomicSegment> StartingWith(AtomicSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

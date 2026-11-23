using P = SuperMetroid.Core.Enemies.Puyo.PuyoSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Puyo's grounded loops and airborne hop poses as typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static partial class PuyoAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PuyoSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PuyoSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.GroundedFast] = Squash(5),
        [P.GroundedMedium] = Squash(8),
        [P.GroundedSlow] = Squash(10),
        [P.TiltedFarRight] =
        [
            Hold(EnemyFrameId.PuyoAirborneClosing),
        ],
        [P.TiltedRight] =
        [
            Hold(EnemyFrameId.PuyoAirborneHalfClosed),
        ],
        [P.Upright] =
        [
            Hold(EnemyFrameId.PuyoAirborneSpread),
        ],
        [P.TiltedLeft] =
        [
            Hold(EnemyFrameId.PuyoAirborneHalfOpen),
        ],
        [P.TiltedFarLeft] =
        [
            Hold(EnemyFrameId.PuyoAirborneOpening),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static Dictionary<PuyoSegment, Animation<PuyoSegment>> StartingAt { get; } = new Dictionary<PuyoSegment, Animation<PuyoSegment>>
    {
        [P.GroundedFast] = Animation<PuyoSegment>.Looping([Segments.Whole(P.GroundedFast)]),
        [P.GroundedMedium] = Animation<PuyoSegment>.Looping([Segments.Whole(P.GroundedMedium)]),
        [P.GroundedSlow] = Animation<PuyoSegment>.Looping([Segments.Whole(P.GroundedSlow)]),
        [P.TiltedFarRight] = new([Segments.Whole(P.TiltedFarRight)], AnimationEnd.Holds),
        [P.TiltedRight] = new([Segments.Whole(P.TiltedRight)], AnimationEnd.Holds),
        [P.Upright] = new([Segments.Whole(P.Upright)], AnimationEnd.Holds),
        [P.TiltedLeft] = new([Segments.Whole(P.TiltedLeft)], AnimationEnd.Holds),
        [P.TiltedFarLeft] = new([Segments.Whole(P.TiltedFarLeft)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PuyoSegment> StartingWith(PuyoSegment segment) => StartingAt[segment];

    /// <summary>The grounded squash loop, tall, flattening, flat and flattening again, each pose shown for <paramref name="updates"/>.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] Squash(int updates) =>
    [
        Show(EnemyFrameId.PuyoGroundedTall, updates),
        Show(EnemyFrameId.PuyoGroundedFlattening, updates),
        Show(EnemyFrameId.PuyoGroundedFlat, updates),
        Show(EnemyFrameId.PuyoGroundedFlattening, updates),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2276): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);
}

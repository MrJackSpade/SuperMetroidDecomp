using P = SuperMetroid.Core.Enemies.Puyo.PuyoSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Puyo's grounded loops and airborne hop poses as typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class PuyoAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PuyoSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PuyoSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.GroundedFast] =
        [
            Show(EnemyFrameId.PuyoGroundedTall, 5),
            Show(EnemyFrameId.PuyoGroundedFlattening, 5),
            Show(EnemyFrameId.PuyoGroundedFlat, 5),
            Show(EnemyFrameId.PuyoGroundedFlattening, 5),
        ],
        [P.GroundedMedium] =
        [
            Show(EnemyFrameId.PuyoGroundedTall, 8),
            Show(EnemyFrameId.PuyoGroundedFlattening, 8),
            Show(EnemyFrameId.PuyoGroundedFlat, 8),
            Show(EnemyFrameId.PuyoGroundedFlattening, 8),
        ],
        [P.GroundedSlow] =
        [
            Show(EnemyFrameId.PuyoGroundedTall, 10),
            Show(EnemyFrameId.PuyoGroundedFlattening, 10),
            Show(EnemyFrameId.PuyoGroundedFlat, 10),
            Show(EnemyFrameId.PuyoGroundedFlattening, 10),
        ],
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
    internal static IReadOnlyDictionary<PuyoSegment, Animation<PuyoSegment>> StartingAt { get; } = new Dictionary<PuyoSegment, Animation<PuyoSegment>>
    {
        [P.GroundedFast] = new([new(P.GroundedFast, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GroundedMedium] = new([new(P.GroundedMedium, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GroundedSlow] = new([new(P.GroundedSlow, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TiltedFarRight] = new([new(P.TiltedFarRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.TiltedRight] = new([new(P.TiltedRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.Upright] = new([new(P.Upright, 0, 1, 1)], AnimationEnd.Holds),
        [P.TiltedLeft] = new([new(P.TiltedLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.TiltedFarLeft] = new([new(P.TiltedFarLeft, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PuyoSegment> StartingWith(PuyoSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2276): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);
}

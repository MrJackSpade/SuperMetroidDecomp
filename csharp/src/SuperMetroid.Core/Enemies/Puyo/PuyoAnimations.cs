using P = SuperMetroid.Core.Enemies.Puyo.PuyoSegment;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Puyo's grounded loops and airborne hop poses as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class PuyoAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PuyoSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PuyoSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.GroundedFast] =
        [
            Show(EnemyFrameId.PuyoGroundedFastFirst, 5),
            Show(EnemyFrameId.PuyoGroundedFastSecond, 5),
            Show(EnemyFrameId.PuyoGroundedFastThird, 5),
            Show(EnemyFrameId.PuyoGroundedFastSecond, 5),
        ],
        [P.GroundedMedium] =
        [
            Show(EnemyFrameId.PuyoGroundedFastFirst, 8),
            Show(EnemyFrameId.PuyoGroundedFastSecond, 8),
            Show(EnemyFrameId.PuyoGroundedFastThird, 8),
            Show(EnemyFrameId.PuyoGroundedFastSecond, 8),
        ],
        [P.GroundedSlow] =
        [
            Show(EnemyFrameId.PuyoGroundedFastFirst, 10),
            Show(EnemyFrameId.PuyoGroundedFastSecond, 10),
            Show(EnemyFrameId.PuyoGroundedFastThird, 10),
            Show(EnemyFrameId.PuyoGroundedFastSecond, 10),
        ],
        [P.RightFrame0LeftFrame4] =
        [
            Show(EnemyFrameId.PuyoAirborneFifth, 1),
        ],
        [P.RightFrame1LeftFrame3] =
        [
            Show(EnemyFrameId.PuyoAirborneFourth, 1),
        ],
        [P.Frame2] =
        [
            Show(EnemyFrameId.PuyoAirborneThird, 1),
        ],
        [P.RightFrame3LeftFrame1] =
        [
            Show(EnemyFrameId.PuyoAirborneSecond, 1),
        ],
        [P.RightFrame4LeftFrame0] =
        [
            Show(EnemyFrameId.PuyoAirborneFirst, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PuyoSegment, Animation<PuyoSegment>> StartingAt { get; } = new Dictionary<PuyoSegment, Animation<PuyoSegment>>
    {
        [P.GroundedFast] = new([new(P.GroundedFast, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GroundedMedium] = new([new(P.GroundedMedium, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GroundedSlow] = new([new(P.GroundedSlow, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RightFrame0LeftFrame4] = new([new(P.RightFrame0LeftFrame4, 0, 1, 1)], AnimationEnd.Holds),
        [P.RightFrame1LeftFrame3] = new([new(P.RightFrame1LeftFrame3, 0, 1, 1)], AnimationEnd.Holds),
        [P.Frame2] = new([new(P.Frame2, 0, 1, 1)], AnimationEnd.Holds),
        [P.RightFrame3LeftFrame1] = new([new(P.RightFrame3LeftFrame1, 0, 1, 1)], AnimationEnd.Holds),
        [P.RightFrame4LeftFrame0] = new([new(P.RightFrame4LeftFrame0, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PuyoSegment> StartingWith(PuyoSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

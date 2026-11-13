using P = SuperMetroid.Core.Enemies.Sbug.SbugSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// Sbug's eight directional animation loops (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class SbugAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SbugSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<SbugSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Right] =
        [
            Show(EnemyFrameId.SbugRightFirst, 5),
            Show(EnemyFrameId.SbugRightSecond, 5),
            Show(EnemyFrameId.SbugRightFirst, 5),
            Show(EnemyFrameId.SbugRightThird, 5),
        ],
        [P.UpRight] =
        [
            Show(EnemyFrameId.SbugUpRightFirst, 5),
            Show(EnemyFrameId.SbugUpRightSecond, 5),
            Show(EnemyFrameId.SbugUpRightFirst, 5),
            Show(EnemyFrameId.SbugUpRightThird, 5),
        ],
        [P.Up] =
        [
            Show(EnemyFrameId.SbugUpFirst, 5),
            Show(EnemyFrameId.SbugUpSecond, 5),
            Show(EnemyFrameId.SbugUpFirst, 5),
            Show(EnemyFrameId.SbugUpThird, 5),
        ],
        [P.UpLeft] =
        [
            Show(EnemyFrameId.SbugUpLeftFirst, 5),
            Show(EnemyFrameId.SbugUpLeftSecond, 5),
            Show(EnemyFrameId.SbugUpLeftFirst, 5),
            Show(EnemyFrameId.SbugUpLeftThird, 5),
        ],
        [P.Left] =
        [
            Show(EnemyFrameId.SbugLeftFirst, 5),
            Show(EnemyFrameId.SbugLeftSecond, 5),
            Show(EnemyFrameId.SbugLeftFirst, 5),
            Show(EnemyFrameId.SbugLeftThird, 5),
        ],
        [P.DownLeft] =
        [
            Show(EnemyFrameId.SbugDownLeftFirst, 5),
            Show(EnemyFrameId.SbugDownLeftSecond, 5),
            Show(EnemyFrameId.SbugDownLeftFirst, 5),
            Show(EnemyFrameId.SbugDownLeftThird, 5),
        ],
        [P.Down] =
        [
            Show(EnemyFrameId.SbugDownFirst, 5),
            Show(EnemyFrameId.SbugDownSecond, 5),
            Show(EnemyFrameId.SbugDownFirst, 5),
            Show(EnemyFrameId.SbugDownThird, 5),
        ],
        [P.DownRight] =
        [
            Show(EnemyFrameId.SbugDownRightFirst, 5),
            Show(EnemyFrameId.SbugDownRightSecond, 5),
            Show(EnemyFrameId.SbugDownRightFirst, 5),
            Show(EnemyFrameId.SbugDownRightThird, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SbugSegment, Animation<SbugSegment>> StartingAt { get; } = new Dictionary<SbugSegment, Animation<SbugSegment>>
    {
        [P.Right] = new([new(P.Right, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UpRight] = new([new(P.UpRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Up] = new([new(P.Up, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UpLeft] = new([new(P.UpLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Left] = new([new(P.Left, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownLeft] = new([new(P.DownLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Down] = new([new(P.Down, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownRight] = new([new(P.DownRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SbugSegment> StartingWith(SbugSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

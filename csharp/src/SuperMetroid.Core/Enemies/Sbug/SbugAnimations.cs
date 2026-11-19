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
            Show(EnemyFrameId.SbugFacingRightWingsUp, 5),
            Show(EnemyFrameId.SbugFacingRightWingsLevel, 5),
            Show(EnemyFrameId.SbugFacingRightWingsUp, 5),
            Show(EnemyFrameId.SbugFacingRightWingsDown, 5),
        ],
        [P.UpRight] =
        [
            Show(EnemyFrameId.SbugFacingUpRightWingsUp, 5),
            Show(EnemyFrameId.SbugFacingUpRightWingsLevel, 5),
            Show(EnemyFrameId.SbugFacingUpRightWingsUp, 5),
            Show(EnemyFrameId.SbugFacingUpRightWingsDown, 5),
        ],
        [P.Up] =
        [
            Show(EnemyFrameId.SbugFacingUpWingsUp, 5),
            Show(EnemyFrameId.SbugFacingUpWingsLevel, 5),
            Show(EnemyFrameId.SbugFacingUpWingsUp, 5),
            Show(EnemyFrameId.SbugFacingUpWingsDown, 5),
        ],
        [P.UpLeft] =
        [
            Show(EnemyFrameId.SbugFacingUpLeftWingsUp, 5),
            Show(EnemyFrameId.SbugFacingUpLeftWingsLevel, 5),
            Show(EnemyFrameId.SbugFacingUpLeftWingsUp, 5),
            Show(EnemyFrameId.SbugFacingUpLeftWingsDown, 5),
        ],
        [P.Left] =
        [
            Show(EnemyFrameId.SbugFacingLeftWingsUp, 5),
            Show(EnemyFrameId.SbugFacingLeftWingsLevel, 5),
            Show(EnemyFrameId.SbugFacingLeftWingsUp, 5),
            Show(EnemyFrameId.SbugFacingLeftWingsDown, 5),
        ],
        [P.DownLeft] =
        [
            Show(EnemyFrameId.SbugFacingDownLeftWingsUp, 5),
            Show(EnemyFrameId.SbugFacingDownLeftWingsLevel, 5),
            Show(EnemyFrameId.SbugFacingDownLeftWingsUp, 5),
            Show(EnemyFrameId.SbugFacingDownLeftWingsDown, 5),
        ],
        [P.Down] =
        [
            Show(EnemyFrameId.SbugFacingDownWingsUp, 5),
            Show(EnemyFrameId.SbugFacingDownWingsLevel, 5),
            Show(EnemyFrameId.SbugFacingDownWingsUp, 5),
            Show(EnemyFrameId.SbugFacingDownWingsDown, 5),
        ],
        [P.DownRight] =
        [
            Show(EnemyFrameId.SbugFacingDownRightWingsUp, 5),
            Show(EnemyFrameId.SbugFacingDownRightWingsLevel, 5),
            Show(EnemyFrameId.SbugFacingDownRightWingsUp, 5),
            Show(EnemyFrameId.SbugFacingDownRightWingsDown, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static Dictionary<SbugSegment, Animation<SbugSegment>> StartingAt { get; } = new Dictionary<SbugSegment, Animation<SbugSegment>>
    {
        [P.Right] = Animation<SbugSegment>.Looping([Segments.Whole(P.Right)]),
        [P.UpRight] = Animation<SbugSegment>.Looping([Segments.Whole(P.UpRight)]),
        [P.Up] = Animation<SbugSegment>.Looping([Segments.Whole(P.Up)]),
        [P.UpLeft] = Animation<SbugSegment>.Looping([Segments.Whole(P.UpLeft)]),
        [P.Left] = Animation<SbugSegment>.Looping([Segments.Whole(P.Left)]),
        [P.DownLeft] = Animation<SbugSegment>.Looping([Segments.Whole(P.DownLeft)]),
        [P.Down] = Animation<SbugSegment>.Looping([Segments.Whole(P.Down)]),
        [P.DownRight] = Animation<SbugSegment>.Looping([Segments.Whole(P.DownRight)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SbugSegment> StartingWith(SbugSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

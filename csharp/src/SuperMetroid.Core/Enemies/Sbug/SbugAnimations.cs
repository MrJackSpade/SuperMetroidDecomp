using P = SuperMetroid.Core.Spatial.CompassDirection;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// Sbug's eight directional animation loops (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static partial class SbugAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CompassDirection, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<CompassDirection, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Right] = WingBeat(EnemyFrameId.SbugFacingRightWingsUp, EnemyFrameId.SbugFacingRightWingsLevel, EnemyFrameId.SbugFacingRightWingsDown),
        [P.UpRight] = WingBeat(EnemyFrameId.SbugFacingUpRightWingsUp, EnemyFrameId.SbugFacingUpRightWingsLevel, EnemyFrameId.SbugFacingUpRightWingsDown),
        [P.Up] = WingBeat(EnemyFrameId.SbugFacingUpWingsUp, EnemyFrameId.SbugFacingUpWingsLevel, EnemyFrameId.SbugFacingUpWingsDown),
        [P.UpLeft] = WingBeat(EnemyFrameId.SbugFacingUpLeftWingsUp, EnemyFrameId.SbugFacingUpLeftWingsLevel, EnemyFrameId.SbugFacingUpLeftWingsDown),
        [P.Left] = WingBeat(EnemyFrameId.SbugFacingLeftWingsUp, EnemyFrameId.SbugFacingLeftWingsLevel, EnemyFrameId.SbugFacingLeftWingsDown),
        [P.DownLeft] = WingBeat(EnemyFrameId.SbugFacingDownLeftWingsUp, EnemyFrameId.SbugFacingDownLeftWingsLevel, EnemyFrameId.SbugFacingDownLeftWingsDown),
        [P.Down] = WingBeat(EnemyFrameId.SbugFacingDownWingsUp, EnemyFrameId.SbugFacingDownWingsLevel, EnemyFrameId.SbugFacingDownWingsDown),
        [P.DownRight] = WingBeat(EnemyFrameId.SbugFacingDownRightWingsUp, EnemyFrameId.SbugFacingDownRightWingsLevel, EnemyFrameId.SbugFacingDownRightWingsDown),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static Dictionary<CompassDirection, Animation<CompassDirection>> StartingAt { get; } = Segments.EachLooping(
        P.Right, P.UpRight, P.Up, P.UpLeft, P.Left, P.DownLeft, P.Down, P.DownRight);

    /// <summary>
    /// One wing beat over a facing's poses, five updates each: <paramref name="wingsUp"/>, <paramref name="wingsLevel"/>,
    /// up again and <paramref name="wingsDown"/>.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] WingBeat(EnemyFrameId wingsUp, EnemyFrameId wingsLevel, EnemyFrameId wingsDown) =>
        [Show(wingsUp, 5), Show(wingsLevel, 5), Show(wingsUp, 5), Show(wingsDown, 5)];

    /// <summary>The animation that starts at <paramref name="segment"/>, the loop of the facing it names.</summary>
    internal static Animation<CompassDirection> StartingWith(CompassDirection segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

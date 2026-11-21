using B = SuperMetroid.Core.Enemies.Powamp.PowampBodySegment;
using L = SuperMetroid.Core.Enemies.Powamp.PowampBalloonSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Powamp's body pulses and balloon inflate and sink animations (#641), one family each (#2553): the body's frames show
/// a <see cref="EnemyFrameId"/>, the balloon's a <see cref="PowampBalloonPose"/>, and no beat raises a cue for its owner.
/// </summary>
internal static class PowampAnimations
{
    /// <summary>The body's segments' beats.</summary>
    internal static AnimationSegments<B, EnemyFrameId, NoAnimationCue> BodySegments { get; } = new(
        new Dictionary<B, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [B.BodyFast] =
        [
            Show(EnemyFrameId.PowampBodyTailLeft, 5),
            Show(EnemyFrameId.PowampBodyTailDown, 5),
            Show(EnemyFrameId.PowampBodyTailRight, 5),
        ],
        [B.BodySlow] =
        [
            Show(EnemyFrameId.PowampBodyTailLeft, 9),
            Show(EnemyFrameId.PowampBodyTailDown, 9),
            Show(EnemyFrameId.PowampBodyTailRight, 9),
        ],
    });

    /// <summary>The balloon's segments' beats, each frame the pose the balloon stands at while it shows.</summary>
    internal static AnimationSegments<L, PowampBalloonPose, NoAnimationCue> BalloonSegments { get; } = new(
        new Dictionary<L, AnimationBeat<PowampBalloonPose, NoAnimationCue>[]>
    {
        [L.InflatingFromDeflated] =
        [
            Show(PowampBalloonPose.Deflated, 1),
        ],
        [L.InflatingFromHalfway] =
        [
            Show(PowampBalloonPose.Halfway, 6),
            Hold(PowampBalloonPose.Full),
        ],
        [L.StartSinking] =
        [
            Show(PowampBalloonPose.Full, 1),
            Show(PowampBalloonPose.Halfway, 6),
        ],
        [L.Deflated] =
        [
            Hold(PowampBalloonPose.Deflated),
        ],
    });

    /// <summary>The installed frame each balloon pose shows.</summary>
    internal static IdentityTable<PowampBalloonPose, EnemyFrameId> BalloonFrames { get; } = new(new Dictionary<PowampBalloonPose, EnemyFrameId>
    {
        [PowampBalloonPose.Deflated] = EnemyFrameId.PowampBalloonSmall,
        [PowampBalloonPose.Halfway] = EnemyFrameId.PowampBalloonHalfInflated,
        [PowampBalloonPose.Full] = EnemyFrameId.PowampBalloonInflated,
    });

    /// <summary>The body's animation that starts at each of its segments: each pulse repeats.</summary>
    internal static IdentityTable<B, Animation<B>> BodyStartingAt { get; } = new(new Dictionary<B, Animation<B>>
    {
        [B.BodyFast] = Animation<B>.Looping([BodySegments.Whole(B.BodyFast)]),
        [B.BodySlow] = Animation<B>.Looping([BodySegments.Whole(B.BodySlow)]),
    });

    /// <summary>
    /// The balloon's animation that starts at each of its segments. An inflated or deflated balloon rests on its picture
    /// until its body starts its next inflate or sink (#2439); nothing waits on the rest's length.
    /// </summary>
    internal static IdentityTable<L, Animation<L>> BalloonStartingAt { get; } = new(new Dictionary<L, Animation<L>>
    {
        [L.InflatingFromDeflated] = new([BalloonSegments.Whole(L.InflatingFromDeflated), BalloonSegments.Whole(L.InflatingFromHalfway)], AnimationEnd.Holds),
        [L.InflatingFromHalfway] = new([BalloonSegments.Whole(L.InflatingFromHalfway)], AnimationEnd.Holds),
        [L.StartSinking] = new([BalloonSegments.Whole(L.StartSinking), BalloonSegments.Whole(L.Deflated)], AnimationEnd.Holds),
        [L.Deflated] = new([BalloonSegments.Whole(L.Deflated)], AnimationEnd.Holds),
    });

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<TFrame, NoAnimationCue> Show<TFrame>(TFrame frame, int duration)
        where TFrame : struct => new AnimationBeat<TFrame, NoAnimationCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<TFrame, NoAnimationCue> Hold<TFrame>(TFrame frame)
        where TFrame : struct => new AnimationBeat<TFrame, NoAnimationCue>.HeldFrame(frame);
}

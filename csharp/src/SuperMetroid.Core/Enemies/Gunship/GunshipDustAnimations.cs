using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Enemies.Gunship.GunshipDustSegment;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship liftoff dust clouds' typed animations (#641): each cloud shows its poses once and deletes itself.
/// </summary>
internal static partial class GunshipDustAnimations
{
    /// <summary>Every dust animation.</summary>
    internal static UnpausedAnimationSegments<GunshipDustSegment, EnemyProjectileFrameId, GunshipDustCue> Segments { get; } = new(
        new Dictionary<GunshipDustSegment, AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Unpaused[]>
    {
        [P.RightInner] = Cloud(8, 9, 10, EnemyProjectileFrameId.GunshipDustRightInnerPuff, EnemyProjectileFrameId.GunshipDustRightInnerBillowing, EnemyProjectileFrameId.GunshipDustRightInnerSpreading, EnemyProjectileFrameId.GunshipDustRightInnerDrifting, EnemyProjectileFrameId.GunshipDustRightInnerThinning, EnemyProjectileFrameId.GunshipDustRightInnerFading, EnemyProjectileFrameId.GunshipDustRightInnerWisps, EnemyProjectileFrameId.GunshipDustRightInnerFaint),
        [P.RightMiddle] = Cloud(6, 7, 8, EnemyProjectileFrameId.GunshipDustRightMiddlePuff, EnemyProjectileFrameId.GunshipDustRightMiddleBillowing, EnemyProjectileFrameId.GunshipDustRightMiddleSpreading, EnemyProjectileFrameId.GunshipDustRightMiddleDrifting, EnemyProjectileFrameId.GunshipDustRightMiddleThinning, EnemyProjectileFrameId.GunshipDustRightMiddleFading, EnemyProjectileFrameId.GunshipDustRightMiddleWisps, EnemyProjectileFrameId.GunshipDustRightMiddleFaint),
        [P.RightOuter] = OuterCloud(EnemyProjectileFrameId.GunshipDustRightOuterPuff, EnemyProjectileFrameId.GunshipDustRightOuterBillowing, EnemyProjectileFrameId.GunshipDustRightOuterSpreading, EnemyProjectileFrameId.GunshipDustRightOuterDrifting, EnemyProjectileFrameId.GunshipDustRightOuterThinning, EnemyProjectileFrameId.GunshipDustRightOuterFading, EnemyProjectileFrameId.GunshipDustRightOuterFaint),
        [P.LeftInner] = Cloud(8, 9, 10, EnemyProjectileFrameId.GunshipDustLeftInnerPuff, EnemyProjectileFrameId.GunshipDustLeftInnerBillowing, EnemyProjectileFrameId.GunshipDustLeftInnerSpreading, EnemyProjectileFrameId.GunshipDustLeftInnerDrifting, EnemyProjectileFrameId.GunshipDustLeftInnerThinning, EnemyProjectileFrameId.GunshipDustLeftInnerFading, EnemyProjectileFrameId.GunshipDustLeftInnerWisps, EnemyProjectileFrameId.GunshipDustLeftInnerFaint),
        [P.LeftMiddle] = Cloud(6, 7, 8, EnemyProjectileFrameId.GunshipDustLeftMiddlePuff, EnemyProjectileFrameId.GunshipDustLeftMiddleBillowing, EnemyProjectileFrameId.GunshipDustLeftMiddleSpreading, EnemyProjectileFrameId.GunshipDustLeftMiddleDrifting, EnemyProjectileFrameId.GunshipDustLeftMiddleThinning, EnemyProjectileFrameId.GunshipDustLeftMiddleFading, EnemyProjectileFrameId.GunshipDustLeftMiddleWisps, EnemyProjectileFrameId.GunshipDustLeftMiddleFaint),
        [P.LeftOuter] = OuterCloud(EnemyProjectileFrameId.GunshipDustLeftOuterPuff, EnemyProjectileFrameId.GunshipDustLeftOuterBillowing, EnemyProjectileFrameId.GunshipDustLeftOuterSpreading, EnemyProjectileFrameId.GunshipDustLeftOuterDrifting, EnemyProjectileFrameId.GunshipDustLeftOuterThinning, EnemyProjectileFrameId.GunshipDustLeftOuterFading, EnemyProjectileFrameId.GunshipDustLeftOuterFaint),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at: each cloud's poses, once.</summary>
    internal static IReadOnlyDictionary<GunshipDustSegment, Animation<GunshipDustSegment>> StartingAt { get; } = new Dictionary<GunshipDustSegment, Animation<GunshipDustSegment>>
    {
        [P.RightInner] = Once(P.RightInner),
        [P.RightMiddle] = Once(P.RightMiddle),
        [P.RightOuter] = Once(P.RightOuter),
        [P.LeftInner] = Once(P.LeftInner),
        [P.LeftMiddle] = Once(P.LeftMiddle),
        [P.LeftOuter] = Once(P.LeftOuter),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<GunshipDustSegment> StartingWith(GunshipDustSegment segment) => StartingAt[segment];

    /// <summary>Plays every beat of <paramref name="segment"/> once, then rests: the cloud's last beat deletes it.</summary>
    private static Animation<GunshipDustSegment> Once(GunshipDustSegment segment) =>
        new([Segments.Whole(segment)], AnimationEnd.Holds);

    /// <summary>
    /// A dust cloud over its side's eight <paramref name="poses"/>, puff to faint: the first two each for
    /// <paramref name="firstUpdates"/>, the next two each for <paramref name="secondUpdates"/> and the last four each for
    /// <paramref name="thirdUpdates"/>, then the cloud deletes itself.
    /// </summary>
    private static AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Unpaused[] Cloud(int firstUpdates, int secondUpdates, int thirdUpdates,
        params ReadOnlySpan<EnemyProjectileFrameId> poses) =>
    [
        .. AnimationBeats<EnemyProjectileFrameId, GunshipDustCue>.Steady(firstUpdates, poses[..2]),
        .. AnimationBeats<EnemyProjectileFrameId, GunshipDustCue>.Steady(secondUpdates, poses[2..4]),
        .. AnimationBeats<EnemyProjectileFrameId, GunshipDustCue>.Steady(thirdUpdates, poses[4..]),
        Cue(GunshipDustCue.Delete),
    ];

    /// <summary>An outer dust cloud over its side's pictures, <paramref name="puff"/>, <paramref name="billowing"/>, <paramref name="spreading"/>, <paramref name="drifting"/>, <paramref name="thinning"/>, <paramref name="fading"/>, <paramref name="faint"/>, each pair of pictures a little longer than the last, and then the cloud is deleted.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Unpaused[] OuterCloud(EnemyProjectileFrameId puff, EnemyProjectileFrameId billowing, EnemyProjectileFrameId spreading, EnemyProjectileFrameId drifting, EnemyProjectileFrameId thinning, EnemyProjectileFrameId fading, EnemyProjectileFrameId faint) =>
    [
        Show(puff, 11),
        Show(billowing, 11),
        Show(spreading, 12),
        Show(drifting, 12),
        Show(thinning, 13),
        Show(fading, 13),
        Show(faint, 13),
        Cue(GunshipDustCue.Delete),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Unpaused Cue(GunshipDustCue cue) => new AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Event(cue);
}

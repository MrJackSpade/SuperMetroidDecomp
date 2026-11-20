using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Enemies.Gunship.GunshipDustSegment;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship liftoff dust clouds' typed animations (#641): each cloud shows its poses once and deletes itself.
/// </summary>
internal static class GunshipDustAnimations
{
    /// <summary>Every dust animation.</summary>
    internal static AnimationSegments<GunshipDustSegment, EnemyProjectileFrameId, GunshipDustCue> Segments { get; } = new(
        new Dictionary<GunshipDustSegment, AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>[]>
    {
        [P.RightInner] = Cloud(EnemyProjectileFrameId.GunshipDustRightInnerPuff, 8, EnemyProjectileFrameId.GunshipDustRightInnerBillowing, EnemyProjectileFrameId.GunshipDustRightInnerSpreading, 9, EnemyProjectileFrameId.GunshipDustRightInnerDrifting, EnemyProjectileFrameId.GunshipDustRightInnerThinning, 10, EnemyProjectileFrameId.GunshipDustRightInnerFading, EnemyProjectileFrameId.GunshipDustRightInnerWisps, EnemyProjectileFrameId.GunshipDustRightInnerFaint),
        [P.RightMiddle] = Cloud(EnemyProjectileFrameId.GunshipDustRightMiddlePuff, 6, EnemyProjectileFrameId.GunshipDustRightMiddleBillowing, EnemyProjectileFrameId.GunshipDustRightMiddleSpreading, 7, EnemyProjectileFrameId.GunshipDustRightMiddleDrifting, EnemyProjectileFrameId.GunshipDustRightMiddleThinning, 8, EnemyProjectileFrameId.GunshipDustRightMiddleFading, EnemyProjectileFrameId.GunshipDustRightMiddleWisps, EnemyProjectileFrameId.GunshipDustRightMiddleFaint),
        [P.RightOuter] = OuterCloud(EnemyProjectileFrameId.GunshipDustRightOuterPuff, EnemyProjectileFrameId.GunshipDustRightOuterBillowing, EnemyProjectileFrameId.GunshipDustRightOuterSpreading, EnemyProjectileFrameId.GunshipDustRightOuterDrifting, EnemyProjectileFrameId.GunshipDustRightOuterThinning, EnemyProjectileFrameId.GunshipDustRightOuterFading, EnemyProjectileFrameId.GunshipDustRightOuterFaint),
        [P.LeftInner] =
        [
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerPuff, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerBillowing, 8),
            Show(EnemyProjectileFr        [P.LeftInner] = Cloud(EnemyProjectileFrameId.GunshipDustLeftInnerPuff, 8, EnemyProjectileFrameId.GunshipDustLeftInnerBillowing, EnemyProjectileFrameId.GunshipDustLeftInnerSpreading, 9, EnemyProjectileFrameId.GunshipDustLeftInnerDrifting, EnemyProjectileFrameId.GunshipDustLeftInnerThinning, 10, EnemyProjectileFrameId.GunshipDustLeftInnerFading, EnemyProjectileFrameId.GunshipDustLeftInnerWisps, EnemyProjectileFrameId.GunshipDustLeftInnerFaint),FrameId.GunshipDustLeftMiddleDrifting, EnemyProjectileFrameId.GunshipDustLeftMiddleThinning, 8, EnemyProjectileFrameId.GunshipDustLeftMiddleFading, EnemyProjectileFrameId.GunshipDustLeftMiddleWisps, EnemyProjectileFrameId.GunshipDustLeftMiddleFaint),
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

    /// <summary>A dust cloud over its side's pictures, <paramref name="puff"/>, <paramref name="firstUpdates"/>, <paramref name="billowing"/>, <paramref name="spreading"/>, <paramref name="secondUpdates"/>, <paramref name="drifting"/>, <paramref name="thinning"/>, <paramref name="thirdUpdates"/>, <paramref name="fading"/>, <paramref name="wisps"/>, <paramref name="faint"/>: each pair of pictures lasts the updates given for it, and then the cloud is deleted.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>[] Cloud(EnemyProjectileFrameId puff, int firstUpdates, EnemyProjectileFrameId billowing, EnemyProjectileFrameId spreading, int secondUpdates, EnemyProjectileFrameId drifting, EnemyProjectileFrameId thinning, int thirdUpdates, EnemyProjectileFrameId fading, EnemyProjectileFrameId wisps, EnemyProjectileFrameId faint) =>
    [
        Show(puff, firstUpdates),
        Show(billowing, firstUpdates),
        Show(spreading, secondUpdates),
        Show(drifting, secondUpdates),
        Show(thinning, thirdUpdates),
        Show(fading, thirdUpdates),
        Show(wisps, thirdUpdates),
        Show(faint, thirdUpdates),
        Cue(GunshipDustCue.Delete),
    ];

    /// <summary>An outer dust cloud over its side's pictures, <paramref name="puff"/>, <paramref name="billowing"/>, <paramref name="spreading"/>, <paramref name="drifting"/>, <paramref name="thinning"/>, <paramref name="fading"/>, <paramref name="faint"/>, each pair of pictures a little longer than the last, and then the cloud is deleted.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>[] OuterCloud(EnemyProjectileFrameId puff, EnemyProjectileFrameId billowing, EnemyProjectileFrameId spreading, EnemyProjectileFrameId drifting, EnemyProjectileFrameId thinning, EnemyProjectileFrameId fading, EnemyProjectileFrameId faint) =>
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
    private static AnimationBeat<EnemyProjectileFrameId, GunshipDustCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, GunshipDustCue> Cue(GunshipDustCue cue) => new AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Event(cue);
}

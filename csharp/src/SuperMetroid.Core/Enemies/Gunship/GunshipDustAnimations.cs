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
    internal static UnpausedAnimationSegments<GunshipDustSegment, EnemyProjectileFrameId, GunshipDustCue> Segments { get; } = new(
        new Dictionary<GunshipDustSegment, AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Unpaused[]>
    {
        [P.RightInner] =
        [
            Show(EnemyProjectileFrameId.GunshipDustRightInnerPuff, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerBillowing, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerSpreading, 9),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerDrifting, 9),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerThinning, 10),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerFading, 10),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerWisps, 10),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerFaint, 10),
            Cue(GunshipDustCue.Delete),
        ],
        [P.RightMiddle] =
        [
            Show(EnemyProjectileFrameId.GunshipDustRightMiddlePuff, 6),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleBillowing, 6),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleSpreading, 7),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleDrifting, 7),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleThinning, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleFading, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleWisps, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleFaint, 8),
            Cue(GunshipDustCue.Delete),
        ],
        [P.RightOuter] =
        [
            Show(EnemyProjectileFrameId.GunshipDustRightOuterPuff, 11),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterBillowing, 11),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterSpreading, 12),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterDrifting, 12),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterThinning, 13),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterFading, 13),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterFaint, 13),
            Cue(GunshipDustCue.Delete),
        ],
        [P.LeftInner] =
        [
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerPuff, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerBillowing, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerSpreading, 9),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerDrifting, 9),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerThinning, 10),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerFading, 10),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerWisps, 10),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerFaint, 10),
            Cue(GunshipDustCue.Delete),
        ],
        [P.LeftMiddle] =
        [
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddlePuff, 6),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleBillowing, 6),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleSpreading, 7),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleDrifting, 7),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleThinning, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleFading, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleWisps, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleFaint, 8),
            Cue(GunshipDustCue.Delete),
        ],
        [P.LeftOuter] =
        [
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterPuff, 11),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterBillowing, 11),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterSpreading, 12),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterDrifting, 12),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterThinning, 13),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterFading, 13),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterFaint, 13),
            Cue(GunshipDustCue.Delete),
        ],
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Unpaused Cue(GunshipDustCue cue) => new AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Event(cue);
}

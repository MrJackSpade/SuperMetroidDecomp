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
        [P.RightInner] =
        [
            Show(EnemyProjectileFrameId.GunshipDustRightInnerFirst, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerSecond, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerThird, 9),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerFourth, 9),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerFifth, 10),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerSixth, 10),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerSeventh, 10),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerEighth, 10),
            Cue(GunshipDustCue.Delete),
        ],
        [P.RightMiddle] =
        [
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleFirst, 6),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleSecond, 6),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleThird, 7),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleFourth, 7),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleFifth, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleSixth, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleSeventh, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleEighth, 8),
            Cue(GunshipDustCue.Delete),
        ],
        [P.RightOuter] =
        [
            Show(EnemyProjectileFrameId.GunshipDustRightOuterFirst, 11),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterSecond, 11),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterThird, 12),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterFourth, 12),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterFifth, 13),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterSixth, 13),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterSeventh, 13),
            Cue(GunshipDustCue.Delete),
        ],
        [P.LeftInner] =
        [
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerFirst, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerSecond, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerThird, 9),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerFourth, 9),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerFifth, 10),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerSixth, 10),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerSeventh, 10),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerEighth, 10),
            Cue(GunshipDustCue.Delete),
        ],
        [P.LeftMiddle] =
        [
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleFirst, 6),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleSecond, 6),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleThird, 7),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleFourth, 7),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleFifth, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleSixth, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleSeventh, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleEighth, 8),
            Cue(GunshipDustCue.Delete),
        ],
        [P.LeftOuter] =
        [
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterFirst, 11),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterSecond, 11),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterThird, 12),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterFourth, 12),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterFifth, 13),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterSixth, 13),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterSeventh, 13),
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
        new([new(segment, 0, Segments.Beats(segment).Count, 1)], AnimationEnd.Holds);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, GunshipDustCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, GunshipDustCue> Cue(GunshipDustCue cue) => new AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Event(cue);
}

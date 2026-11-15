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
            Show(EnemyProjectileFrameId.GunshipDustRightInnerLoopFirst, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerLoopSecond, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerLoopThird, 9),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerLoopFourth, 9),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerLoopFifth, 10),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerLoopSixth, 10),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerLoopSeventh, 10),
            Show(EnemyProjectileFrameId.GunshipDustRightInnerLoopEighth, 10),
            Cue(GunshipDustCue.Delete),
        ],
        [P.RightMiddle] =
        [
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleLoopFirst, 6),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleLoopSecond, 6),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleLoopThird, 7),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleLoopFourth, 7),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleLoopFifth, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleLoopSixth, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleLoopSeventh, 8),
            Show(EnemyProjectileFrameId.GunshipDustRightMiddleLoopEighth, 8),
            Cue(GunshipDustCue.Delete),
        ],
        [P.RightOuter] =
        [
            Show(EnemyProjectileFrameId.GunshipDustRightOuterLoopFirst, 11),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterLoopSecond, 11),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterLoopThird, 12),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterLoopFourth, 12),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterLoopFifth, 13),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterLoopSixth, 13),
            Show(EnemyProjectileFrameId.GunshipDustRightOuterLoopSeventh, 13),
            Cue(GunshipDustCue.Delete),
        ],
        [P.LeftInner] =
        [
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerLoopFirst, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerLoopSecond, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerLoopThird, 9),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerLoopFourth, 9),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerLoopFifth, 10),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerLoopSixth, 10),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerLoopSeventh, 10),
            Show(EnemyProjectileFrameId.GunshipDustLeftInnerLoopEighth, 10),
            Cue(GunshipDustCue.Delete),
        ],
        [P.LeftMiddle] =
        [
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleLoopFirst, 6),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleLoopSecond, 6),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleLoopThird, 7),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleLoopFourth, 7),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleLoopFifth, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleLoopSixth, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleLoopSeventh, 8),
            Show(EnemyProjectileFrameId.GunshipDustLeftMiddleLoopEighth, 8),
            Cue(GunshipDustCue.Delete),
        ],
        [P.LeftOuter] =
        [
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterLoopFirst, 11),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterLoopSecond, 11),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterLoopThird, 12),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterLoopFourth, 12),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterLoopFifth, 13),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterLoopSixth, 13),
            Show(EnemyProjectileFrameId.GunshipDustLeftOuterLoopSeventh, 13),
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

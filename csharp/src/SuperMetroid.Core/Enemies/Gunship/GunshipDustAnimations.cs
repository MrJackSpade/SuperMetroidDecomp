using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Enemies.Gunship.GunshipDustSegment;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship liftoff dust clouds' typed animations (#641). Each cloud loads its general
/// timer with one, shows its poses, counts the timer down past its single repeat and deletes itself.
/// </summary>
internal static class GunshipDustAnimations
{
    /// <summary>Every dust animation.</summary>
    internal static AnimationSegments<GunshipDustSegment, EnemyProjectileFrameId, GunshipDustCue> Segments { get; } = new(
        new Dictionary<GunshipDustSegment, AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>[]>
    {
        [P.RightInner] =
        [
        ],
        [P.RightInnerLoop] =
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
        ],
        [P.RightMiddleLoop] =
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
        ],
        [P.RightOuterLoop] =
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
        ],
        [P.LeftInnerLoop] =
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
        ],
        [P.LeftMiddleLoop] =
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
        ],
        [P.LeftOuterLoop] =
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

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<GunshipDustSegment, Animation<GunshipDustSegment>> StartingAt { get; } = new Dictionary<GunshipDustSegment, Animation<GunshipDustSegment>>
    {
        [P.RightInner] = new([new(P.RightInnerLoop, 0, 8, 1, AnimationPhaseCounting.EveryPlay), new(P.RightInnerLoop, 8, 1, 1)], AnimationEnd.Holds),
        [P.RightMiddle] = new([new(P.RightMiddleLoop, 0, 8, 1, AnimationPhaseCounting.EveryPlay), new(P.RightMiddleLoop, 8, 1, 1)], AnimationEnd.Holds),
        [P.RightOuter] = new([new(P.RightOuterLoop, 0, 7, 1, AnimationPhaseCounting.EveryPlay), new(P.RightOuterLoop, 7, 1, 1)], AnimationEnd.Holds),
        [P.LeftInner] = new([new(P.LeftInnerLoop, 0, 8, 1, AnimationPhaseCounting.EveryPlay), new(P.LeftInnerLoop, 8, 1, 1)], AnimationEnd.Holds),
        [P.LeftMiddle] = new([new(P.LeftMiddleLoop, 0, 8, 1, AnimationPhaseCounting.EveryPlay), new(P.LeftMiddleLoop, 8, 1, 1)], AnimationEnd.Holds),
        [P.LeftOuter] = new([new(P.LeftOuterLoop, 0, 7, 1, AnimationPhaseCounting.EveryPlay), new(P.LeftOuterLoop, 7, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<GunshipDustSegment> StartingWith(GunshipDustSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, GunshipDustCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, GunshipDustCue> Cue(GunshipDustCue cue) => new AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Event(cue);
}

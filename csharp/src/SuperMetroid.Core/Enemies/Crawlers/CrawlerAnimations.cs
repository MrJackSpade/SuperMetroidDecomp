using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Crawlers.CrawlerSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The Sciser, Zero, Viola, orange Zoomer and shared Zoomer/Zeela/Sova surface animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue: the surface a crawler shows sets its crawl.
/// </summary>
internal static class CrawlerAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CrawlerSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<CrawlerSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.SciserUpsideRight] =
        [
        ],
        [P.SciserUpsideRightLoop] =
        [
            Show(EnemyFrameId.SciserUpsideRightLoopFirst, 8),
            Show(EnemyFrameId.SciserUpsideRightLoopSecond, 8),
            Show(EnemyFrameId.SciserUpsideRightLoopThird, 8),
            Show(EnemyFrameId.SciserUpsideRightLoopSecond, 8),
        ],
        [P.SciserUpsideLeft] =
        [
        ],
        [P.SciserUpsideLeftLoop] =
        [
            Show(EnemyFrameId.SciserUpsideLeftLoopFirst, 8),
            Show(EnemyFrameId.SciserUpsideLeftLoopSecond, 8),
            Show(EnemyFrameId.SciserUpsideLeftLoopThird, 8),
            Show(EnemyFrameId.SciserUpsideLeftLoopSecond, 8),
        ],
        [P.SciserUpsideDown] =
        [
        ],
        [P.SciserUpsideDownLoop] =
        [
            Show(EnemyFrameId.SciserUpsideDownLoopFirst, 8),
            Show(EnemyFrameId.SciserUpsideDownLoopSecond, 8),
            Show(EnemyFrameId.SciserUpsideDownLoopThird, 8),
            Show(EnemyFrameId.SciserUpsideDownLoopSecond, 8),
        ],
        [P.SciserUpsideUp] =
        [
        ],
        [P.SciserUpsideUpLoop] =
        [
            Show(EnemyFrameId.SciserUpsideUpLoopFirst, 8),
            Show(EnemyFrameId.SciserUpsideUpLoopSecond, 8),
            Show(EnemyFrameId.SciserUpsideUpLoopThird, 8),
            Show(EnemyFrameId.SciserUpsideUpLoopSecond, 8),
        ],
        [P.ZeroUpsideRight] =
        [
        ],
        [P.ZeroUpsideRightLoop] =
        [
            Show(EnemyFrameId.ZeroUpsideRightLoopFirst, 4),
            Show(EnemyFrameId.ZeroUpsideRightLoopSecond, 4),
            Show(EnemyFrameId.ZeroUpsideRightLoopThird, 4),
            Show(EnemyFrameId.ZeroUpsideRightLoopFourth, 4),
            Show(EnemyFrameId.ZeroUpsideRightLoopThird, 4),
            Show(EnemyFrameId.ZeroUpsideRightLoopSecond, 4),
        ],
        [P.ZeroUpsideLeft] =
        [
        ],
        [P.ZeroUpsideLeftLoop] =
        [
            Show(EnemyFrameId.ZeroUpsideLeftLoopFirst, 4),
            Show(EnemyFrameId.ZeroUpsideLeftLoopSecond, 4),
            Show(EnemyFrameId.ZeroUpsideLeftLoopThird, 4),
            Show(EnemyFrameId.ZeroUpsideLeftLoopFourth, 4),
            Show(EnemyFrameId.ZeroUpsideLeftLoopThird, 4),
            Show(EnemyFrameId.ZeroUpsideLeftLoopSecond, 4),
        ],
        [P.ZeroUpsideDown] =
        [
        ],
        [P.ZeroUpsideDownLoop] =
        [
            Show(EnemyFrameId.ZeroUpsideDownLoopFirst, 4),
            Show(EnemyFrameId.ZeroUpsideDownLoopSecond, 4),
            Show(EnemyFrameId.ZeroUpsideDownLoopThird, 4),
            Show(EnemyFrameId.ZeroUpsideDownLoopFourth, 4),
            Show(EnemyFrameId.ZeroUpsideDownLoopThird, 4),
            Show(EnemyFrameId.ZeroUpsideDownLoopSecond, 4),
        ],
        [P.ZeroUpsideUp] =
        [
        ],
        [P.ZeroUpsideUpLoop] =
        [
            Show(EnemyFrameId.ZeroUpsideUpLoopFirst, 4),
            Show(EnemyFrameId.ZeroUpsideUpLoopSecond, 4),
            Show(EnemyFrameId.ZeroUpsideUpLoopThird, 4),
            Show(EnemyFrameId.ZeroUpsideUpLoopFourth, 4),
            Show(EnemyFrameId.ZeroUpsideUpLoopThird, 4),
            Show(EnemyFrameId.ZeroUpsideUpLoopSecond, 4),
        ],
        [P.ViolaUpsideDown] =
        [
        ],
        [P.ViolaUpsideUp] =
        [
        ],
        [P.ViolaUpsideRight] =
        [
        ],
        [P.ViolaUpsideLeft] =
        [
        ],
        [P.ViolaNormal] =
        [
            Show(EnemyFrameId.ViolaNormalFirst, 10),
            Show(EnemyFrameId.ViolaNormalSecond, 10),
            Show(EnemyFrameId.ViolaNormalThird, 10),
            Show(EnemyFrameId.ViolaNormalFourth, 10),
            Show(EnemyFrameId.ViolaNormalFifth, 10),
            Show(EnemyFrameId.ViolaNormalSixth, 10),
            Show(EnemyFrameId.ViolaNormalSeventh, 10),
            Show(EnemyFrameId.ViolaNormalEighth, 10),
            Show(EnemyFrameId.ViolaNormalSeventh, 10),
            Show(EnemyFrameId.ViolaNormalSixth, 10),
            Show(EnemyFrameId.ViolaNormalFifth, 10),
            Show(EnemyFrameId.ViolaNormalFourth, 10),
            Show(EnemyFrameId.ViolaNormalThird, 10),
            Show(EnemyFrameId.ViolaNormalSecond, 10),
        ],
        [P.OrangeZoomerUpsideRight] =
        [
        ],
        [P.OrangeZoomerUpsideRightLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideRightLoopFirst, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightLoopSecond, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightLoopThird, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightLoopFourth, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightLoopFifth, 3),
        ],
        [P.OrangeZoomerUpsideLeft] =
        [
        ],
        [P.OrangeZoomerUpsideLeftLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideLeftLoopFirst, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftLoopSecond, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftLoopThird, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftLoopFourth, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftLoopFifth, 3),
        ],
        [P.OrangeZoomerUpsideDown] =
        [
        ],
        [P.OrangeZoomerUpsideDownLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideDownLoopFirst, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownLoopSecond, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownLoopThird, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownLoopFourth, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownLoopFifth, 3),
        ],
        [P.OrangeZoomerUpsideUp] =
        [
        ],
        [P.OrangeZoomerUpsideUpLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideUpLoopFirst, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpLoopSecond, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpLoopThird, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpLoopFourth, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpLoopFifth, 3),
        ],
        [P.SharedUpsideRight] =
        [
        ],
        [P.SharedUpsideRightLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideRightLoopFirst, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightLoopSecond, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightLoopThird, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightLoopFourth, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightLoopFifth, 3),
        ],
        [P.SharedUpsideLeft] =
        [
        ],
        [P.SharedUpsideLeftLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideLeftLoopFirst, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftLoopSecond, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftLoopThird, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftLoopFourth, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftLoopFifth, 3),
        ],
        [P.SharedUpsideDown] =
        [
        ],
        [P.SharedUpsideDownLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideDownLoopFirst, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownLoopSecond, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownLoopThird, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownLoopFourth, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownLoopFifth, 3),
        ],
        [P.SharedUpsideUp] =
        [
        ],
        [P.SharedUpsideUpLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideUpLoopFirst, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpLoopSecond, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpLoopThird, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpLoopFourth, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpLoopFifth, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CrawlerSegment, Animation<CrawlerSegment>> StartingAt { get; } = new Dictionary<CrawlerSegment, Animation<CrawlerSegment>>
    {
        [P.SciserUpsideRight] = new([new(P.SciserUpsideRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideRightLoop] = new([new(P.SciserUpsideRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideLeft] = new([new(P.SciserUpsideLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideLeftLoop] = new([new(P.SciserUpsideLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideDown] = new([new(P.SciserUpsideDownLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideDownLoop] = new([new(P.SciserUpsideDownLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideUp] = new([new(P.SciserUpsideUpLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideUpLoop] = new([new(P.SciserUpsideUpLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideRight] = new([new(P.ZeroUpsideRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideRightLoop] = new([new(P.ZeroUpsideRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideLeft] = new([new(P.ZeroUpsideLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideLeftLoop] = new([new(P.ZeroUpsideLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideDown] = new([new(P.ZeroUpsideDownLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideDownLoop] = new([new(P.ZeroUpsideDownLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideUp] = new([new(P.ZeroUpsideUpLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideUpLoop] = new([new(P.ZeroUpsideUpLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ViolaUpsideDown] = new([new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ViolaUpsideUp] = new([new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ViolaUpsideRight] = new([new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ViolaUpsideLeft] = new([new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ViolaNormal] = new([new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideRight] = new([new(P.OrangeZoomerUpsideRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideRightLoop] = new([new(P.OrangeZoomerUpsideRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideLeft] = new([new(P.OrangeZoomerUpsideLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideLeftLoop] = new([new(P.OrangeZoomerUpsideLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideDown] = new([new(P.OrangeZoomerUpsideDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideDownLoop] = new([new(P.OrangeZoomerUpsideDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideUp] = new([new(P.OrangeZoomerUpsideUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideUpLoop] = new([new(P.OrangeZoomerUpsideUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideRight] = new([new(P.SharedUpsideRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideRightLoop] = new([new(P.SharedUpsideRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideLeft] = new([new(P.SharedUpsideLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideLeftLoop] = new([new(P.SharedUpsideLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideDown] = new([new(P.SharedUpsideDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideDownLoop] = new([new(P.SharedUpsideDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideUp] = new([new(P.SharedUpsideUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideUpLoop] = new([new(P.SharedUpsideUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CrawlerSegment> StartingWith(CrawlerSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

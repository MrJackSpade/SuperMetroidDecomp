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
        [P.HZoomerUpsideRight] =
        [
        ],
        [P.HZoomerUpsideRightLoop] =
        [
            Show(EnemyFrameId.HZoomerUpsideRightLoopFirst, 3),
            Show(EnemyFrameId.HZoomerUpsideRightLoopSecond, 3),
            Show(EnemyFrameId.HZoomerUpsideRightLoopThird, 3),
            Show(EnemyFrameId.HZoomerUpsideRightLoopFourth, 3),
            Show(EnemyFrameId.HZoomerUpsideRightLoopFifth, 3),
        ],
        [P.HZoomerUpsideLeft] =
        [
        ],
        [P.HZoomerUpsideLeftLoop] =
        [
            Show(EnemyFrameId.HZoomerUpsideLeftLoopFirst, 3),
            Show(EnemyFrameId.HZoomerUpsideLeftLoopSecond, 3),
            Show(EnemyFrameId.HZoomerUpsideLeftLoopThird, 3),
            Show(EnemyFrameId.HZoomerUpsideLeftLoopFourth, 3),
            Show(EnemyFrameId.HZoomerUpsideLeftLoopFifth, 3),
        ],
        [P.HZoomerUpsideDown] =
        [
        ],
        [P.HZoomerUpsideDownLoop] =
        [
            Show(EnemyFrameId.HZoomerUpsideDownLoopFirst, 3),
            Show(EnemyFrameId.HZoomerUpsideDownLoopSecond, 3),
            Show(EnemyFrameId.HZoomerUpsideDownLoopThird, 3),
            Show(EnemyFrameId.HZoomerUpsideDownLoopFourth, 3),
            Show(EnemyFrameId.HZoomerUpsideDownLoopFifth, 3),
        ],
        [P.HZoomerUpsideUp] =
        [
        ],
        [P.HZoomerUpsideUpLoop] =
        [
            Show(EnemyFrameId.HZoomerUpsideUpLoopFirst, 3),
            Show(EnemyFrameId.HZoomerUpsideUpLoopSecond, 3),
            Show(EnemyFrameId.HZoomerUpsideUpLoopThird, 3),
            Show(EnemyFrameId.HZoomerUpsideUpLoopFourth, 3),
            Show(EnemyFrameId.HZoomerUpsideUpLoopFifth, 3),
        ],
        [P.SharedUpsideRight] =
        [
        ],
        [P.SharedUpsideRightLoop] =
        [
            Show(EnemyFrameId.HZoomerUpsideRightLoopFirst, 3),
            Show(EnemyFrameId.HZoomerUpsideRightLoopSecond, 3),
            Show(EnemyFrameId.HZoomerUpsideRightLoopThird, 3),
            Show(EnemyFrameId.HZoomerUpsideRightLoopFourth, 3),
            Show(EnemyFrameId.HZoomerUpsideRightLoopFifth, 3),
        ],
        [P.SharedUpsideLeft] =
        [
        ],
        [P.SharedUpsideLeftLoop] =
        [
            Show(EnemyFrameId.HZoomerUpsideLeftLoopFirst, 3),
            Show(EnemyFrameId.HZoomerUpsideLeftLoopSecond, 3),
            Show(EnemyFrameId.HZoomerUpsideLeftLoopThird, 3),
            Show(EnemyFrameId.HZoomerUpsideLeftLoopFourth, 3),
            Show(EnemyFrameId.HZoomerUpsideLeftLoopFifth, 3),
        ],
        [P.SharedUpsideDown] =
        [
        ],
        [P.SharedUpsideDownLoop] =
        [
            Show(EnemyFrameId.HZoomerUpsideDownLoopFirst, 3),
            Show(EnemyFrameId.HZoomerUpsideDownLoopSecond, 3),
            Show(EnemyFrameId.HZoomerUpsideDownLoopThird, 3),
            Show(EnemyFrameId.HZoomerUpsideDownLoopFourth, 3),
            Show(EnemyFrameId.HZoomerUpsideDownLoopFifth, 3),
        ],
        [P.SharedUpsideUp] =
        [
        ],
        [P.SharedUpsideUpLoop] =
        [
            Show(EnemyFrameId.HZoomerUpsideUpLoopFirst, 3),
            Show(EnemyFrameId.HZoomerUpsideUpLoopSecond, 3),
            Show(EnemyFrameId.HZoomerUpsideUpLoopThird, 3),
            Show(EnemyFrameId.HZoomerUpsideUpLoopFourth, 3),
            Show(EnemyFrameId.HZoomerUpsideUpLoopFifth, 3),
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
        [P.HZoomerUpsideRight] = new([new(P.HZoomerUpsideRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HZoomerUpsideRightLoop] = new([new(P.HZoomerUpsideRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HZoomerUpsideLeft] = new([new(P.HZoomerUpsideLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HZoomerUpsideLeftLoop] = new([new(P.HZoomerUpsideLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HZoomerUpsideDown] = new([new(P.HZoomerUpsideDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HZoomerUpsideDownLoop] = new([new(P.HZoomerUpsideDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HZoomerUpsideUp] = new([new(P.HZoomerUpsideUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HZoomerUpsideUpLoop] = new([new(P.HZoomerUpsideUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
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

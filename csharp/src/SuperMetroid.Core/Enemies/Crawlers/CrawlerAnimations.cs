using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Crawlers.CrawlerCueKind;
using P = SuperMetroid.Core.Enemies.Crawlers.CrawlerSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The Sciser, Zero, Viola, orange Zoomer and shared Zoomer/Zeela/Sova surface animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="CrawlerCue"/> with the values it acts on.
/// </summary>
internal static class CrawlerAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CrawlerSegment, EnemyFrameId, CrawlerCue> Segments { get; } = new(
        new Dictionary<CrawlerSegment, AnimationBeat<EnemyFrameId, CrawlerCue>[]>
    {
        [P.SciserUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
        ],
        [P.ViolaUpsideUp] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
        ],
        [P.ViolaUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
        ],
        [P.ViolaUpsideLeft] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingVertically)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingVertically)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingHorizontally)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingHorizontally)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
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
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
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
        [P.SciserUpsideRight] = new([new(P.SciserUpsideRight, 0, 1, 1), new(P.SciserUpsideRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SciserUpsideRightLoop] = new([new(P.SciserUpsideRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideLeft] = new([new(P.SciserUpsideLeft, 0, 1, 1), new(P.SciserUpsideLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SciserUpsideLeftLoop] = new([new(P.SciserUpsideLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideDown] = new([new(P.SciserUpsideDown, 0, 1, 1), new(P.SciserUpsideDownLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SciserUpsideDownLoop] = new([new(P.SciserUpsideDownLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideUp] = new([new(P.SciserUpsideUp, 0, 1, 1), new(P.SciserUpsideUpLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SciserUpsideUpLoop] = new([new(P.SciserUpsideUpLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideRight] = new([new(P.ZeroUpsideRight, 0, 1, 1), new(P.ZeroUpsideRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ZeroUpsideRightLoop] = new([new(P.ZeroUpsideRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideLeft] = new([new(P.ZeroUpsideLeft, 0, 1, 1), new(P.ZeroUpsideLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ZeroUpsideLeftLoop] = new([new(P.ZeroUpsideLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideDown] = new([new(P.ZeroUpsideDown, 0, 1, 1), new(P.ZeroUpsideDownLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ZeroUpsideDownLoop] = new([new(P.ZeroUpsideDownLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideUp] = new([new(P.ZeroUpsideUp, 0, 1, 1), new(P.ZeroUpsideUpLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ZeroUpsideUpLoop] = new([new(P.ZeroUpsideUpLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ViolaUpsideDown] = new([new(P.ViolaUpsideDown, 0, 1, 1), new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ViolaUpsideUp] = new([new(P.ViolaUpsideUp, 0, 1, 1), new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ViolaUpsideRight] = new([new(P.ViolaUpsideRight, 0, 1, 1), new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ViolaUpsideLeft] = new([new(P.ViolaUpsideLeft, 0, 1, 1), new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ViolaNormal] = new([new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HZoomerUpsideRight] = new([new(P.HZoomerUpsideRight, 0, 1, 1), new(P.HZoomerUpsideRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HZoomerUpsideRightLoop] = new([new(P.HZoomerUpsideRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HZoomerUpsideLeft] = new([new(P.HZoomerUpsideLeft, 0, 1, 1), new(P.HZoomerUpsideLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HZoomerUpsideLeftLoop] = new([new(P.HZoomerUpsideLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HZoomerUpsideDown] = new([new(P.HZoomerUpsideDown, 0, 1, 1), new(P.HZoomerUpsideDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HZoomerUpsideDownLoop] = new([new(P.HZoomerUpsideDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HZoomerUpsideUp] = new([new(P.HZoomerUpsideUp, 0, 1, 1), new(P.HZoomerUpsideUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HZoomerUpsideUpLoop] = new([new(P.HZoomerUpsideUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideRight] = new([new(P.SharedUpsideRight, 0, 1, 1), new(P.SharedUpsideRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SharedUpsideRightLoop] = new([new(P.SharedUpsideRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideLeft] = new([new(P.SharedUpsideLeft, 0, 1, 1), new(P.SharedUpsideLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SharedUpsideLeftLoop] = new([new(P.SharedUpsideLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideDown] = new([new(P.SharedUpsideDown, 0, 1, 1), new(P.SharedUpsideDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SharedUpsideDownLoop] = new([new(P.SharedUpsideDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideUp] = new([new(P.SharedUpsideUp, 0, 1, 1), new(P.SharedUpsideUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SharedUpsideUpLoop] = new([new(P.SharedUpsideUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CrawlerSegment> StartingWith(CrawlerSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, CrawlerCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, CrawlerCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CrawlerCue> Cue(CrawlerCue cue) => new AnimationBeat<EnemyFrameId, CrawlerCue>.Event(cue);
}

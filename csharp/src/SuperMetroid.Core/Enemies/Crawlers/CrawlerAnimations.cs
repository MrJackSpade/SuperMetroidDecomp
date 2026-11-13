using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Crawlers.CrawlerCueKind;
using P = SuperMetroid.Core.Enemies.Crawlers.CrawlerProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The Sciser, Zero, Viola, orange Zoomer and shared Zoomer/Zeela/Sova surface animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="CrawlerCue"/> with its typed operands.
/// </summary>
internal static class CrawlerAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CrawlerProgram, EnemyFrameId, CrawlerCue> Segments { get; } = new(
        new Dictionary<CrawlerProgram, AnimationBeat<EnemyFrameId, CrawlerCue>[]>
    {
        [P.SciserUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
        ],
        [P.SciserUpsideRightLoop] =
        [
            Show(EnemyFrameId.SciserUpsideRight0, 8),
            Show(EnemyFrameId.SciserUpsideRight1, 8),
            Show(EnemyFrameId.SciserUpsideRight2, 8),
            Show(EnemyFrameId.SciserUpsideRight1, 8),
        ],
        [P.SciserUpsideLeft] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
        ],
        [P.SciserUpsideLeftLoop] =
        [
            Show(EnemyFrameId.SciserUpsideLeft0, 8),
            Show(EnemyFrameId.SciserUpsideLeft1, 8),
            Show(EnemyFrameId.SciserUpsideLeft2, 8),
            Show(EnemyFrameId.SciserUpsideLeft1, 8),
        ],
        [P.SciserUpsideDown] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
        ],
        [P.SciserUpsideDownLoop] =
        [
            Show(EnemyFrameId.SciserUpsideDown0, 8),
            Show(EnemyFrameId.SciserUpsideDown1, 8),
            Show(EnemyFrameId.SciserUpsideDown2, 8),
            Show(EnemyFrameId.SciserUpsideDown1, 8),
        ],
        [P.SciserUpsideUp] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
        ],
        [P.SciserUpsideUpLoop] =
        [
            Show(EnemyFrameId.SciserUpsideUp0, 8),
            Show(EnemyFrameId.SciserUpsideUp1, 8),
            Show(EnemyFrameId.SciserUpsideUp2, 8),
            Show(EnemyFrameId.SciserUpsideUp1, 8),
        ],
        [P.ZeroUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
        ],
        [P.ZeroUpsideRightLoop] =
        [
            Show(EnemyFrameId.ZeroCrawler00, 4),
            Show(EnemyFrameId.ZeroCrawler01, 4),
            Show(EnemyFrameId.ZeroCrawler02, 4),
            Show(EnemyFrameId.ZeroCrawler03, 4),
            Show(EnemyFrameId.ZeroCrawler02, 4),
            Show(EnemyFrameId.ZeroCrawler01, 4),
        ],
        [P.ZeroUpsideLeft] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
        ],
        [P.ZeroUpsideLeftLoop] =
        [
            Show(EnemyFrameId.ZeroCrawler04, 4),
            Show(EnemyFrameId.ZeroCrawler05, 4),
            Show(EnemyFrameId.ZeroCrawler06, 4),
            Show(EnemyFrameId.ZeroCrawler07, 4),
            Show(EnemyFrameId.ZeroCrawler06, 4),
            Show(EnemyFrameId.ZeroCrawler05, 4),
        ],
        [P.ZeroUpsideDown] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
        ],
        [P.ZeroUpsideDownLoop] =
        [
            Show(EnemyFrameId.ZeroCrawler08, 4),
            Show(EnemyFrameId.ZeroCrawler09, 4),
            Show(EnemyFrameId.ZeroCrawler10, 4),
            Show(EnemyFrameId.ZeroCrawler11, 4),
            Show(EnemyFrameId.ZeroCrawler10, 4),
            Show(EnemyFrameId.ZeroCrawler09, 4),
        ],
        [P.ZeroUpsideUp] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
        ],
        [P.ZeroUpsideUpLoop] =
        [
            Show(EnemyFrameId.ZeroCrawler12, 4),
            Show(EnemyFrameId.ZeroCrawler13, 4),
            Show(EnemyFrameId.ZeroCrawler14, 4),
            Show(EnemyFrameId.ZeroCrawler15, 4),
            Show(EnemyFrameId.ZeroCrawler14, 4),
            Show(EnemyFrameId.ZeroCrawler13, 4),
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
            Show(EnemyFrameId.ViolaSpin0, 10),
            Show(EnemyFrameId.ViolaSpin1, 10),
            Show(EnemyFrameId.ViolaSpin2, 10),
            Show(EnemyFrameId.ViolaSpin3, 10),
            Show(EnemyFrameId.ViolaSpin4, 10),
            Show(EnemyFrameId.ViolaSpin5, 10),
            Show(EnemyFrameId.ViolaSpin6, 10),
            Show(EnemyFrameId.ViolaSpin7, 10),
            Show(EnemyFrameId.ViolaSpin6, 10),
            Show(EnemyFrameId.ViolaSpin5, 10),
            Show(EnemyFrameId.ViolaSpin4, 10),
            Show(EnemyFrameId.ViolaSpin3, 10),
            Show(EnemyFrameId.ViolaSpin2, 10),
            Show(EnemyFrameId.ViolaSpin1, 10),
        ],
        [P.HZoomerUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingVertically)),
        ],
        [P.HZoomerUpsideRightLoop] =
        [
            Show(EnemyFrameId.HzoomerUpsideRight0, 3),
            Show(EnemyFrameId.HzoomerUpsideRight1, 3),
            Show(EnemyFrameId.HzoomerUpsideRight2, 3),
            Show(EnemyFrameId.HzoomerUpsideRight3, 3),
            Show(EnemyFrameId.HzoomerUpsideRight4, 3),
        ],
        [P.HZoomerUpsideLeft] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingVertically)),
        ],
        [P.HZoomerUpsideLeftLoop] =
        [
            Show(EnemyFrameId.HzoomerUpsideLeft0, 3),
            Show(EnemyFrameId.HzoomerUpsideLeft1, 3),
            Show(EnemyFrameId.HzoomerUpsideLeft2, 3),
            Show(EnemyFrameId.HzoomerUpsideLeft3, 3),
            Show(EnemyFrameId.HzoomerUpsideLeft4, 3),
        ],
        [P.HZoomerUpsideDown] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingHorizontally)),
        ],
        [P.HZoomerUpsideDownLoop] =
        [
            Show(EnemyFrameId.HzoomerUpsideDown0, 3),
            Show(EnemyFrameId.HzoomerUpsideDown1, 3),
            Show(EnemyFrameId.HzoomerUpsideDown2, 3),
            Show(EnemyFrameId.HzoomerUpsideDown3, 3),
            Show(EnemyFrameId.HzoomerUpsideDown4, 3),
        ],
        [P.HZoomerUpsideUp] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingHorizontally)),
        ],
        [P.HZoomerUpsideUpLoop] =
        [
            Show(EnemyFrameId.HzoomerUpsideUp0, 3),
            Show(EnemyFrameId.HzoomerUpsideUp1, 3),
            Show(EnemyFrameId.HzoomerUpsideUp2, 3),
            Show(EnemyFrameId.HzoomerUpsideUp3, 3),
            Show(EnemyFrameId.HzoomerUpsideUp4, 3),
        ],
        [P.SharedUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
        ],
        [P.SharedUpsideRightLoop] =
        [
            Show(EnemyFrameId.HzoomerUpsideRight0, 3),
            Show(EnemyFrameId.HzoomerUpsideRight1, 3),
            Show(EnemyFrameId.HzoomerUpsideRight2, 3),
            Show(EnemyFrameId.HzoomerUpsideRight3, 3),
            Show(EnemyFrameId.HzoomerUpsideRight4, 3),
        ],
        [P.SharedUpsideLeft] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
        ],
        [P.SharedUpsideLeftLoop] =
        [
            Show(EnemyFrameId.HzoomerUpsideLeft0, 3),
            Show(EnemyFrameId.HzoomerUpsideLeft1, 3),
            Show(EnemyFrameId.HzoomerUpsideLeft2, 3),
            Show(EnemyFrameId.HzoomerUpsideLeft3, 3),
            Show(EnemyFrameId.HzoomerUpsideLeft4, 3),
        ],
        [P.SharedUpsideDown] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
        ],
        [P.SharedUpsideDownLoop] =
        [
            Show(EnemyFrameId.HzoomerUpsideDown0, 3),
            Show(EnemyFrameId.HzoomerUpsideDown1, 3),
            Show(EnemyFrameId.HzoomerUpsideDown2, 3),
            Show(EnemyFrameId.HzoomerUpsideDown3, 3),
            Show(EnemyFrameId.HzoomerUpsideDown4, 3),
        ],
        [P.SharedUpsideUp] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
        ],
        [P.SharedUpsideUpLoop] =
        [
            Show(EnemyFrameId.HzoomerUpsideUp0, 3),
            Show(EnemyFrameId.HzoomerUpsideUp1, 3),
            Show(EnemyFrameId.HzoomerUpsideUp2, 3),
            Show(EnemyFrameId.HzoomerUpsideUp3, 3),
            Show(EnemyFrameId.HzoomerUpsideUp4, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CrawlerProgram, Animation<CrawlerProgram>> StartingAt { get; } = new Dictionary<CrawlerProgram, Animation<CrawlerProgram>>
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
    internal static Animation<CrawlerProgram> StartingWith(CrawlerProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, CrawlerCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, CrawlerCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CrawlerCue> Cue(CrawlerCue cue) => new AnimationBeat<EnemyFrameId, CrawlerCue>.Event(cue);
}

using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Crawlers.CrawlerCueKind;
using P = SuperMetroid.Core.Enemies.Crawlers.CrawlerProgram;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The Sciser, Zero, Viola, orange Zoomer and shared Zoomer/Zeela/Sova surface animations as typed programs (#641). Each
/// frame shows a <see cref="CrawlerFrame"/> pose and each callback becomes a <see cref="CrawlerCue"/> with its typed operands.
/// </summary>
internal static class CrawlerAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CrawlerProgram, CrawlerFrame, CrawlerCue> Segments { get; } = new(
        new Dictionary<CrawlerProgram, AnimationBeat<CrawlerFrame, CrawlerCue>[]>
    {
        [P.SciserUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
        ],
        [P.SciserUpsideRightLoop] =
        [
            Show(3, 8),
            Show(4, 8),
            Show(5, 8),
            Show(4, 8),
        ],
        [P.SciserUpsideLeft] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
        ],
        [P.SciserUpsideLeftLoop] =
        [
            Show(9, 8),
            Show(10, 8),
            Show(11, 8),
            Show(10, 8),
        ],
        [P.SciserUpsideDown] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
        ],
        [P.SciserUpsideDownLoop] =
        [
            Show(6, 8),
            Show(7, 8),
            Show(8, 8),
            Show(7, 8),
        ],
        [P.SciserUpsideUp] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
        ],
        [P.SciserUpsideUpLoop] =
        [
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            Show(1, 8),
        ],
        [P.ZeroUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
        ],
        [P.ZeroUpsideRightLoop] =
        [
            Show(24, 4),
            Show(25, 4),
            Show(26, 4),
            Show(27, 4),
            Show(26, 4),
            Show(25, 4),
        ],
        [P.ZeroUpsideLeft] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
        ],
        [P.ZeroUpsideLeftLoop] =
        [
            Show(16, 4),
            Show(17, 4),
            Show(18, 4),
            Show(19, 4),
            Show(18, 4),
            Show(17, 4),
        ],
        [P.ZeroUpsideDown] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
        ],
        [P.ZeroUpsideDownLoop] =
        [
            Show(20, 4),
            Show(21, 4),
            Show(22, 4),
            Show(23, 4),
            Show(22, 4),
            Show(21, 4),
        ],
        [P.ZeroUpsideUp] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
        ],
        [P.ZeroUpsideUpLoop] =
        [
            Show(12, 4),
            Show(13, 4),
            Show(14, 4),
            Show(15, 4),
            Show(14, 4),
            Show(13, 4),
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
            Show(28, 10),
            Show(29, 10),
            Show(30, 10),
            Show(31, 10),
            Show(32, 10),
            Show(33, 10),
            Show(34, 10),
            Show(35, 10),
            Show(34, 10),
            Show(33, 10),
            Show(32, 10),
            Show(31, 10),
            Show(30, 10),
            Show(29, 10),
        ],
        [P.HZoomerUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingVertically)),
        ],
        [P.HZoomerUpsideRightLoop] =
        [
            Show(51, 3),
            Show(52, 3),
            Show(53, 3),
            Show(54, 3),
            Show(55, 3),
        ],
        [P.HZoomerUpsideLeft] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingVertically)),
        ],
        [P.HZoomerUpsideLeftLoop] =
        [
            Show(41, 3),
            Show(42, 3),
            Show(43, 3),
            Show(44, 3),
            Show(45, 3),
        ],
        [P.HZoomerUpsideDown] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingHorizontally)),
        ],
        [P.HZoomerUpsideDownLoop] =
        [
            Show(46, 3),
            Show(47, 3),
            Show(48, 3),
            Show(49, 3),
            Show(50, 3),
        ],
        [P.HZoomerUpsideUp] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingHorizontally)),
        ],
        [P.HZoomerUpsideUpLoop] =
        [
            Show(36, 3),
            Show(37, 3),
            Show(38, 3),
            Show(39, 3),
            Show(40, 3),
        ],
        [P.SharedUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
        ],
        [P.SharedUpsideRightLoop] =
        [
            Show(51, 3),
            Show(52, 3),
            Show(53, 3),
            Show(54, 3),
            Show(55, 3),
        ],
        [P.SharedUpsideLeft] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
        ],
        [P.SharedUpsideLeftLoop] =
        [
            Show(41, 3),
            Show(42, 3),
            Show(43, 3),
            Show(44, 3),
            Show(45, 3),
        ],
        [P.SharedUpsideDown] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
        ],
        [P.SharedUpsideDownLoop] =
        [
            Show(46, 3),
            Show(47, 3),
            Show(48, 3),
            Show(49, 3),
            Show(50, 3),
        ],
        [P.SharedUpsideUp] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
        ],
        [P.SharedUpsideUpLoop] =
        [
            Show(36, 3),
            Show(37, 3),
            Show(38, 3),
            Show(39, 3),
            Show(40, 3),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CrawlerFrame, CrawlerCue> Show(int pose, int duration) => new AnimationBeat<CrawlerFrame, CrawlerCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<CrawlerFrame, CrawlerCue> Cue(CrawlerCue cue) => new AnimationBeat<CrawlerFrame, CrawlerCue>.Event(cue);
}

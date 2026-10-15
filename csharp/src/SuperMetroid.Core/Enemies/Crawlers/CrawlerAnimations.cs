using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Crawlers.CrawlerCueKind;
using P = SuperMetroid.Core.Enemies.Crawlers.CrawlerProgram;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The Sciser, Zero, Viola, orange Zoomer and shared Zoomer/Zeela/Sova surface animations as typed programs (#641), in place of their bank-$A3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="CrawlerFrame"/> pose and each callback becomes a <see cref="CrawlerCue"/> with its typed operands.
/// </summary>
internal static class CrawlerAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<CrawlerProgram, CrawlerFrame, CrawlerCue> Programs { get; } = new(
        new Dictionary<CrawlerProgram, AnimationProgramStep<CrawlerProgram, CrawlerFrame, CrawlerCue>[]>
    {
        // $A3:967B
        [P.SciserUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
            FallInto(P.SciserUpsideRightLoop),
        ],
        // $A3:967F
        [P.SciserUpsideRightLoop] =
        [
            Show(3, 8),
            Show(4, 8),
            Show(5, 8),
            Show(4, 8),
            GoTo(P.SciserUpsideRightLoop),
        ],
        // $A3:9693
        [P.SciserUpsideLeft] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
            FallInto(P.SciserUpsideLeftLoop),
        ],
        // $A3:9697
        [P.SciserUpsideLeftLoop] =
        [
            Show(9, 8),
            Show(10, 8),
            Show(11, 8),
            Show(10, 8),
            GoTo(P.SciserUpsideLeftLoop),
        ],
        // $A3:96AB
        [P.SciserUpsideDown] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
            FallInto(P.SciserUpsideDownLoop),
        ],
        // $A3:96AF
        [P.SciserUpsideDownLoop] =
        [
            Show(6, 8),
            Show(7, 8),
            Show(8, 8),
            Show(7, 8),
            GoTo(P.SciserUpsideDownLoop),
        ],
        // $A3:96C3
        [P.SciserUpsideUp] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
            FallInto(P.SciserUpsideUpLoop),
        ],
        // $A3:96C7
        [P.SciserUpsideUpLoop] =
        [
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            Show(1, 8),
            GoTo(P.SciserUpsideUpLoop),
        ],
        // $A3:984B
        [P.ZeroUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
            FallInto(P.ZeroUpsideRightLoop),
        ],
        // $A3:984F
        [P.ZeroUpsideRightLoop] =
        [
            Show(24, 4),
            Show(25, 4),
            Show(26, 4),
            Show(27, 4),
            Show(26, 4),
            Show(25, 4),
            GoTo(P.ZeroUpsideRightLoop),
        ],
        // $A3:988B
        [P.ZeroUpsideLeft] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
            FallInto(P.ZeroUpsideLeftLoop),
        ],
        // $A3:988F
        [P.ZeroUpsideLeftLoop] =
        [
            Show(16, 4),
            Show(17, 4),
            Show(18, 4),
            Show(19, 4),
            Show(18, 4),
            Show(17, 4),
            GoTo(P.ZeroUpsideLeftLoop),
        ],
        // $A3:98AB
        [P.ZeroUpsideDown] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
            FallInto(P.ZeroUpsideDownLoop),
        ],
        // $A3:98AF
        [P.ZeroUpsideDownLoop] =
        [
            Show(20, 4),
            Show(21, 4),
            Show(22, 4),
            Show(23, 4),
            Show(22, 4),
            Show(21, 4),
            GoTo(P.ZeroUpsideDownLoop),
        ],
        // $A3:990B
        [P.ZeroUpsideUp] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
            FallInto(P.ZeroUpsideUpLoop),
        ],
        // $A3:990F
        [P.ZeroUpsideUpLoop] =
        [
            Show(12, 4),
            Show(13, 4),
            Show(14, 4),
            Show(15, 4),
            Show(14, 4),
            Show(13, 4),
            GoTo(P.ZeroUpsideUpLoop),
        ],
        // $A3:B5D3
        [P.ViolaUpsideDown] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
            GoTo(P.ViolaNormal),
        ],
        // $A3:B5DB
        [P.ViolaUpsideUp] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
            GoTo(P.ViolaNormal),
        ],
        // $A3:B5E3
        [P.ViolaUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
            GoTo(P.ViolaNormal),
        ],
        // $A3:B5EB
        [P.ViolaUpsideLeft] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
            FallInto(P.ViolaNormal),
        ],
        // $A3:B5EF
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
            GoTo(P.ViolaNormal),
        ],
        // $A3:DFCB
        [P.HZoomerUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingVertically)),
            FallInto(P.HZoomerUpsideRightLoop),
        ],
        // $A3:DFCF
        [P.HZoomerUpsideRightLoop] =
        [
            Show(51, 3),
            Show(52, 3),
            Show(53, 3),
            Show(54, 3),
            Show(55, 3),
            GoTo(P.HZoomerUpsideRightLoop),
        ],
        // $A3:DFE7
        [P.HZoomerUpsideLeft] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingVertically)),
            FallInto(P.HZoomerUpsideLeftLoop),
        ],
        // $A3:DFEB
        [P.HZoomerUpsideLeftLoop] =
        [
            Show(41, 3),
            Show(42, 3),
            Show(43, 3),
            Show(44, 3),
            Show(45, 3),
            GoTo(P.HZoomerUpsideLeftLoop),
        ],
        // $A3:E003
        [P.HZoomerUpsideDown] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingHorizontally)),
            FallInto(P.HZoomerUpsideDownLoop),
        ],
        // $A3:E007
        [P.HZoomerUpsideDownLoop] =
        [
            Show(46, 3),
            Show(47, 3),
            Show(48, 3),
            Show(49, 3),
            Show(50, 3),
            GoTo(P.HZoomerUpsideDownLoop),
        ],
        // $A3:E01F
        [P.HZoomerUpsideUp] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.HZoomerCrawlingHorizontally)),
            FallInto(P.HZoomerUpsideUpLoop),
        ],
        // $A3:E023
        [P.HZoomerUpsideUpLoop] =
        [
            Show(36, 3),
            Show(37, 3),
            Show(38, 3),
            Show(39, 3),
            Show(40, 3),
            GoTo(P.HZoomerUpsideUpLoop),
        ],
        // $A3:E25C
        [P.SharedUpsideRight] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
            FallInto(P.SharedUpsideRightLoop),
        ],
        // $A3:E260
        [P.SharedUpsideRightLoop] =
        [
            Show(51, 3),
            Show(52, 3),
            Show(53, 3),
            Show(54, 3),
            Show(55, 3),
            GoTo(P.SharedUpsideRightLoop),
        ],
        // $A3:E278
        [P.SharedUpsideLeft] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingVertically)),
            FallInto(P.SharedUpsideLeftLoop),
        ],
        // $A3:E27C
        [P.SharedUpsideLeftLoop] =
        [
            Show(41, 3),
            Show(42, 3),
            Show(43, 3),
            Show(44, 3),
            Show(45, 3),
            GoTo(P.SharedUpsideLeftLoop),
        ],
        // $A3:E294
        [P.SharedUpsideDown] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
            FallInto(P.SharedUpsideDownLoop),
        ],
        // $A3:E298
        [P.SharedUpsideDownLoop] =
        [
            Show(46, 3),
            Show(47, 3),
            Show(48, 3),
            Show(49, 3),
            Show(50, 3),
            GoTo(P.SharedUpsideDownLoop),
        ],
        // $A3:E2B0
        [P.SharedUpsideUp] =
        [
            Cue(new(K.SetFunction, Function: CrawlerEnemyFunction.CrawlingHorizontally)),
            FallInto(P.SharedUpsideUpLoop),
        ],
        // $A3:E2B4
        [P.SharedUpsideUpLoop] =
        [
            Show(36, 3),
            Show(37, 3),
            Show(38, 3),
            Show(39, 3),
            Show(40, 3),
            GoTo(P.SharedUpsideUpLoop),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params CrawlerProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => CrawlerFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<CrawlerProgram, CrawlerFrame, CrawlerCue> Show(int pose, int duration) => AnimationProgramStep<CrawlerProgram, CrawlerFrame, CrawlerCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<CrawlerProgram, CrawlerFrame, CrawlerCue> Cue(CrawlerCue cue) => AnimationProgramStep<CrawlerProgram, CrawlerFrame, CrawlerCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<CrawlerProgram, CrawlerFrame, CrawlerCue> GoTo(CrawlerProgram program) => AnimationProgramStep<CrawlerProgram, CrawlerFrame, CrawlerCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<CrawlerProgram, CrawlerFrame, CrawlerCue> FallInto(CrawlerProgram program) => AnimationProgramStep<CrawlerProgram, CrawlerFrame, CrawlerCue>.FallInto(program);

}

using P = SuperMetroid.Core.Enemies.PipeBugs.PipeBugProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// The Zeb, Zebbo, Gamet and Geega Pipe Bugs' rising, shooting, flying and arcing loops as typed programs (#641), in place of their bank-$B3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="PipeBugFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class PipeBugAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<PipeBugProgram, PipeBugFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<PipeBugProgram, AnimationProgramStep<PipeBugProgram, PipeBugFrame, NoAnimationCue>[]>
    {
        [P.ZebRisingLeft] =
        [
            Show(0, 2),
            Show(1, 2),
            Show(2, 2),
            Show(3, 2),
            Show(4, 2),
            Show(3, 2),
            Show(2, 2),
            Show(1, 2),
            GoTo(P.ZebRisingLeft),
        ],
        [P.ZebShootingLeft] =
        [
            Show(0, 1),
            Show(1, 1),
            Show(3, 1),
            Show(4, 1),
            Show(3, 1),
            Show(1, 1),
            GoTo(P.ZebShootingLeft),
        ],
        [P.ZebRisingRight] =
        [
            Show(5, 2),
            Show(6, 2),
            Show(7, 2),
            Show(8, 2),
            Show(9, 2),
            Show(8, 2),
            Show(7, 2),
            Show(6, 2),
            GoTo(P.ZebRisingRight),
        ],
        [P.ZebShootingRight] =
        [
            Show(5, 1),
            Show(6, 1),
            Show(8, 1),
            Show(9, 1),
            Show(8, 1),
            Show(6, 1),
            GoTo(P.ZebShootingRight),
        ],
        [P.ZebboRisingLeft] =
        [
            Show(13, 2),
            Show(14, 1),
            Show(15, 2),
            Show(14, 1),
            GoTo(P.ZebboRisingLeft),
        ],
        [P.ZebboShootingLeft] =
        [
            Show(10, 3),
            Show(11, 3),
            Show(12, 3),
            Show(11, 3),
            GoTo(P.ZebboShootingLeft),
        ],
        [P.ZebboRisingRight] =
        [
            Show(19, 2),
            Show(20, 1),
            Show(21, 2),
            Show(20, 1),
            GoTo(P.ZebboRisingRight),
        ],
        [P.ZebboShootingRight] =
        [
            Show(16, 3),
            Show(17, 3),
            Show(18, 3),
            Show(17, 3),
            GoTo(P.ZebboShootingRight),
        ],
        [P.GametRisingLeft] =
        [
            Show(22, 2),
            Show(23, 2),
            Show(24, 2),
            Show(25, 2),
            Show(26, 2),
            Show(25, 2),
            Show(24, 2),
            Show(23, 2),
            GoTo(P.GametRisingLeft),
        ],
        [P.GametFlyingLeft] =
        [
            Show(22, 1),
            Show(23, 1),
            Show(25, 1),
            Show(26, 1),
            Show(25, 1),
            Show(23, 1),
            GoTo(P.GametFlyingLeft),
        ],
        [P.GametRisingRight] =
        [
            Show(27, 2),
            Show(28, 2),
            Show(29, 2),
            Show(30, 2),
            Show(31, 2),
            Show(30, 2),
            Show(29, 2),
            Show(28, 2),
            GoTo(P.GametRisingRight),
        ],
        [P.GametFlyingRight] =
        [
            Show(27, 1),
            Show(28, 1),
            Show(30, 1),
            Show(31, 1),
            Show(30, 1),
            Show(28, 1),
            GoTo(P.GametFlyingRight),
        ],
        [P.GeegaFlyingLeft] =
        [
            Show(32, 4),
            Show(33, 4),
            Show(34, 4),
            Show(33, 4),
            GoTo(P.GeegaFlyingLeft),
        ],
        [P.GeegaArcingLeft] =
        [
            Show(35, 1),
            Show(36, 1),
            Show(37, 1),
            Show(36, 1),
            GoTo(P.GeegaArcingLeft),
        ],
        [P.GeegaFlyingRight] =
        [
            Show(38, 4),
            Show(39, 4),
            Show(40, 4),
            Show(39, 4),
            GoTo(P.GeegaFlyingRight),
        ],
        [P.GeegaArcingRight] =
        [
            Show(41, 1),
            Show(42, 1),
            Show(43, 1),
            Show(42, 1),
            GoTo(P.GeegaArcingRight),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<PipeBugProgram, PipeBugFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<PipeBugProgram, PipeBugFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<PipeBugProgram, PipeBugFrame, NoAnimationCue> GoTo(PipeBugProgram program) => AnimationProgramStep<PipeBugProgram, PipeBugFrame, NoAnimationCue>.GoTo(program);

}

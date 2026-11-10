using P = SuperMetroid.Core.Enemies.PipeBugs.PipeBugProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// The Zeb, Zebbo, Gamet and Geega Pipe Bugs' rising, shooting, flying and arcing loops as typed programs (#641). Each
/// frame shows a <see cref="PipeBugFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class PipeBugAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PipeBugProgram, PipeBugFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PipeBugProgram, AnimationBeat<PipeBugFrame, NoAnimationCue>[]>
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
        ],
        [P.ZebShootingLeft] =
        [
            Show(0, 1),
            Show(1, 1),
            Show(3, 1),
            Show(4, 1),
            Show(3, 1),
            Show(1, 1),
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
        ],
        [P.ZebShootingRight] =
        [
            Show(5, 1),
            Show(6, 1),
            Show(8, 1),
            Show(9, 1),
            Show(8, 1),
            Show(6, 1),
        ],
        [P.ZebboRisingLeft] =
        [
            Show(13, 2),
            Show(14, 1),
            Show(15, 2),
            Show(14, 1),
        ],
        [P.ZebboShootingLeft] =
        [
            Show(10, 3),
            Show(11, 3),
            Show(12, 3),
            Show(11, 3),
        ],
        [P.ZebboRisingRight] =
        [
            Show(19, 2),
            Show(20, 1),
            Show(21, 2),
            Show(20, 1),
        ],
        [P.ZebboShootingRight] =
        [
            Show(16, 3),
            Show(17, 3),
            Show(18, 3),
            Show(17, 3),
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
        ],
        [P.GametFlyingLeft] =
        [
            Show(22, 1),
            Show(23, 1),
            Show(25, 1),
            Show(26, 1),
            Show(25, 1),
            Show(23, 1),
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
        ],
        [P.GametFlyingRight] =
        [
            Show(27, 1),
            Show(28, 1),
            Show(30, 1),
            Show(31, 1),
            Show(30, 1),
            Show(28, 1),
        ],
        [P.GeegaFlyingLeft] =
        [
            Show(32, 4),
            Show(33, 4),
            Show(34, 4),
            Show(33, 4),
        ],
        [P.GeegaArcingLeft] =
        [
            Show(35, 1),
            Show(36, 1),
            Show(37, 1),
            Show(36, 1),
        ],
        [P.GeegaFlyingRight] =
        [
            Show(38, 4),
            Show(39, 4),
            Show(40, 4),
            Show(39, 4),
        ],
        [P.GeegaArcingRight] =
        [
            Show(41, 1),
            Show(42, 1),
            Show(43, 1),
            Show(42, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PipeBugProgram, Animation<PipeBugProgram>> StartingAt { get; } = new Dictionary<PipeBugProgram, Animation<PipeBugProgram>>
    {
        [P.ZebRisingLeft] = new([new(P.ZebRisingLeft, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebShootingLeft] = new([new(P.ZebShootingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebRisingRight] = new([new(P.ZebRisingRight, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebShootingRight] = new([new(P.ZebShootingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebboRisingLeft] = new([new(P.ZebboRisingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebboShootingLeft] = new([new(P.ZebboShootingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebboRisingRight] = new([new(P.ZebboRisingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebboShootingRight] = new([new(P.ZebboShootingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GametRisingLeft] = new([new(P.GametRisingLeft, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GametFlyingLeft] = new([new(P.GametFlyingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GametRisingRight] = new([new(P.GametRisingRight, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GametFlyingRight] = new([new(P.GametFlyingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GeegaFlyingLeft] = new([new(P.GeegaFlyingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GeegaArcingLeft] = new([new(P.GeegaArcingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GeegaFlyingRight] = new([new(P.GeegaFlyingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GeegaArcingRight] = new([new(P.GeegaArcingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PipeBugProgram> StartingWith(PipeBugProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<PipeBugFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<PipeBugFrame, NoAnimationCue>.Frame(new(pose), duration);
}

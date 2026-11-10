using P = SuperMetroid.Core.Enemies.Sbug.SbugProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// Sbug's eight directional animation loops as typed programs (#641). Each
/// frame shows a <see cref="SbugFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class SbugAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SbugProgram, SbugFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<SbugProgram, AnimationBeat<SbugFrame, NoAnimationCue>[]>
    {
        [P.Right] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(0, 5),
            Show(2, 5),
        ],
        [P.UpRight] =
        [
            Show(3, 5),
            Show(4, 5),
            Show(3, 5),
            Show(5, 5),
        ],
        [P.Up] =
        [
            Show(6, 5),
            Show(7, 5),
            Show(6, 5),
            Show(8, 5),
        ],
        [P.UpLeft] =
        [
            Show(9, 5),
            Show(10, 5),
            Show(9, 5),
            Show(11, 5),
        ],
        [P.Left] =
        [
            Show(12, 5),
            Show(13, 5),
            Show(12, 5),
            Show(14, 5),
        ],
        [P.DownLeft] =
        [
            Show(15, 5),
            Show(16, 5),
            Show(15, 5),
            Show(17, 5),
        ],
        [P.Down] =
        [
            Show(18, 5),
            Show(19, 5),
            Show(18, 5),
            Show(20, 5),
        ],
        [P.DownRight] =
        [
            Show(21, 5),
            Show(22, 5),
            Show(21, 5),
            Show(23, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SbugProgram, Animation<SbugProgram>> StartingAt { get; } = new Dictionary<SbugProgram, Animation<SbugProgram>>
    {
        [P.Right] = new([new(P.Right, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UpRight] = new([new(P.UpRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Up] = new([new(P.Up, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UpLeft] = new([new(P.UpLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Left] = new([new(P.Left, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownLeft] = new([new(P.DownLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Down] = new([new(P.Down, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownRight] = new([new(P.DownRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SbugProgram> StartingWith(SbugProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<SbugFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<SbugFrame, NoAnimationCue>.Frame(new(pose), duration);
}

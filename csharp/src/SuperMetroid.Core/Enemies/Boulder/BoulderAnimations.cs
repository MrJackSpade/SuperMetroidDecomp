using P = SuperMetroid.Core.Enemies.Boulder.BoulderProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>
/// The Boulder's mirrored rolling loops as typed programs (#641). Each
/// frame shows a <see cref="BoulderFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class BoulderAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BoulderProgram, BoulderFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<BoulderProgram, AnimationBeat<BoulderFrame, NoAnimationCue>[]>
    {
        [P.Left] =
        [
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            Show(3, 8),
            Show(4, 8),
            Show(5, 8),
            Show(6, 8),
            Show(7, 8),
        ],
        [P.Right] =
        [
            Show(0, 8),
            Show(7, 8),
            Show(6, 8),
            Show(5, 8),
            Show(4, 8),
            Show(3, 8),
            Show(2, 8),
            Show(1, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BoulderProgram, Animation<BoulderProgram>> StartingAt { get; } = new Dictionary<BoulderProgram, Animation<BoulderProgram>>
    {
        [P.Left] = new([new(P.Left, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Right] = new([new(P.Right, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BoulderProgram> StartingWith(BoulderProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<BoulderFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<BoulderFrame, NoAnimationCue>.Frame(new(pose), duration);
}

using P = SuperMetroid.Core.Enemies.Flies.FlyProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>
/// The shared Mellow, Mella and Memu flight loop as a typed program (#641). Each
/// frame shows a <see cref="FlyFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class FlyAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FlyProgram, FlyFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<FlyProgram, AnimationBeat<FlyFrame, NoAnimationCue>[]>
    {
        [P.Flight] =
        [
            Show(0, 2),
            Show(1, 2),
            Show(2, 2),
            Show(3, 2),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FlyProgram, Animation<FlyProgram>> StartingAt { get; } = new Dictionary<FlyProgram, Animation<FlyProgram>>
    {
        [P.Flight] = new([new(P.Flight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FlyProgram> StartingWith(FlyProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<FlyFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<FlyFrame, NoAnimationCue>.Frame(new(pose), duration);
}

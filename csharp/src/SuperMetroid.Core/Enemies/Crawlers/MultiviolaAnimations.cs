using P = SuperMetroid.Core.Enemies.Crawlers.MultiviolaProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// Multiviola's spinning flight loop as a typed program (#641). Each
/// frame shows a <see cref="MultiviolaFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class MultiviolaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MultiviolaProgram, MultiviolaFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MultiviolaProgram, AnimationBeat<MultiviolaFrame, NoAnimationCue>[]>
    {
        [P.Flying] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(3, 10),
            Show(4, 10),
            Show(5, 10),
            Show(6, 10),
            Show(7, 10),
            Show(6, 10),
            Show(5, 10),
            Show(4, 10),
            Show(3, 10),
            Show(2, 10),
            Show(1, 10),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MultiviolaProgram, Animation<MultiviolaProgram>> StartingAt { get; } = new Dictionary<MultiviolaProgram, Animation<MultiviolaProgram>>
    {
        [P.Flying] = new([new(P.Flying, 0, 14, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MultiviolaProgram> StartingWith(MultiviolaProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<MultiviolaFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<MultiviolaFrame, NoAnimationCue>.Frame(new(pose), duration);
}

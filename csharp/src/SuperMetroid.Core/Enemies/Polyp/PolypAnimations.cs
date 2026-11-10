using P = SuperMetroid.Core.Enemies.Polyp.PolypProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Polyp's stationary pose as a typed program (#641). Each
/// frame shows a <see cref="PolypFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class PolypAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PolypProgram, PolypFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PolypProgram, AnimationBeat<PolypFrame, NoAnimationCue>[]>
    {
        [P.Stationary] =
        [
            Show(0, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PolypProgram, Animation<PolypProgram>> StartingAt { get; } = new Dictionary<PolypProgram, Animation<PolypProgram>>
    {
        [P.Stationary] = new([new(P.Stationary, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PolypProgram> StartingWith(PolypProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<PolypFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<PolypFrame, NoAnimationCue>.Frame(new(pose), duration);
}

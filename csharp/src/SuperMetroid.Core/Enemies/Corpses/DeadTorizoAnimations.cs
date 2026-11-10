using P = SuperMetroid.Core.Enemies.Corpses.DeadTorizoProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Dead Torizo's stationary corpse pose as a typed program (#641). Each
/// frame shows a <see cref="DeadTorizoFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class DeadTorizoAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DeadTorizoProgram, DeadTorizoFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<DeadTorizoProgram, AnimationBeat<DeadTorizoFrame, NoAnimationCue>[]>
    {
        [P.Stationary] =
        [
            Show(0, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DeadTorizoProgram, Animation<DeadTorizoProgram>> StartingAt { get; } = new Dictionary<DeadTorizoProgram, Animation<DeadTorizoProgram>>
    {
        [P.Stationary] = new([new(P.Stationary, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DeadTorizoProgram> StartingWith(DeadTorizoProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<DeadTorizoFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<DeadTorizoFrame, NoAnimationCue>.Frame(new(pose), duration);
}

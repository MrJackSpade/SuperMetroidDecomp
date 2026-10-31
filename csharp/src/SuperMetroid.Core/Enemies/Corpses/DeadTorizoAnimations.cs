using P = SuperMetroid.Core.Enemies.Corpses.DeadTorizoProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Dead Torizo's stationary corpse pose as a typed program (#641). Each
/// frame shows a <see cref="DeadTorizoFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class DeadTorizoAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<DeadTorizoProgram, DeadTorizoFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<DeadTorizoProgram, AnimationProgramStep<DeadTorizoProgram, DeadTorizoFrame, NoAnimationCue>[]>
    {
        [P.Stationary] =
        [
            Show(0, 1),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<DeadTorizoProgram, DeadTorizoFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<DeadTorizoProgram, DeadTorizoFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Holds the last frame, as the sleep does.</summary>
    private static AnimationProgramStep<DeadTorizoProgram, DeadTorizoFrame, NoAnimationCue> Hold => AnimationProgramStep<DeadTorizoProgram, DeadTorizoFrame, NoAnimationCue>.Hold;

}

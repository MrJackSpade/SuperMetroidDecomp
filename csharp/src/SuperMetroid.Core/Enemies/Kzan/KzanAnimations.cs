using P = SuperMetroid.Core.Enemies.Kzan.KzanProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// Kzan's idle pose as a typed program (#641), in place of their bank-$A6 lists. Generated from the verified native lists: each
/// frame shows a <see cref="KzanFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class KzanAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<KzanProgram, KzanFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<KzanProgram, AnimationProgramStep<KzanProgram, KzanFrame, NoAnimationCue>[]>
    {
        [P.Idle] =
        [
            Show(0, 1),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<KzanProgram, KzanFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<KzanProgram, KzanFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<KzanProgram, KzanFrame, NoAnimationCue> Hold => AnimationProgramStep<KzanProgram, KzanFrame, NoAnimationCue>.Hold;

}

using P = SuperMetroid.Core.Enemies.Polyp.PolypProgram;
using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Polyp's stationary pose as a typed program (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="PolypFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class PolypAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<PolypProgram, PolypFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<PolypProgram, AnimationProgramStep<PolypProgram, PolypFrame, NoAnimationCue>[]>
    {
        // $A2:B51A
        [P.Stationary] =
        [
            Show(0, 1),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<PolypProgram, PolypFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<PolypProgram, PolypFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<PolypProgram, PolypFrame, NoAnimationCue> Hold => AnimationProgramStep<PolypProgram, PolypFrame, NoAnimationCue>.Hold;

}

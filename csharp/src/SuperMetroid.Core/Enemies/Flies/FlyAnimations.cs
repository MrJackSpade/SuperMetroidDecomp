using P = SuperMetroid.Core.Enemies.Flies.FlyProgram;
using SuperMetroid.Core.Enemies.Flies.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>
/// The shared Mellow, Mella and Memu flight loop as a typed program (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="FlyFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class FlyAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<FlyProgram, FlyFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<FlyProgram, AnimationProgramStep<FlyProgram, FlyFrame, NoAnimationCue>[]>
    {
        // $A2:B013
        [P.Flight] =
        [
            Show(0, 2),
            Show(1, 2),
            Show(2, 2),
            Show(3, 2),
            GoTo(P.Flight),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<FlyProgram, FlyFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<FlyProgram, FlyFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<FlyProgram, FlyFrame, NoAnimationCue> GoTo(FlyProgram program) => AnimationProgramStep<FlyProgram, FlyFrame, NoAnimationCue>.GoTo(program);

}

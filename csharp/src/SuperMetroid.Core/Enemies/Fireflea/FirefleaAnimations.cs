using P = SuperMetroid.Core.Enemies.Fireflea.FirefleaProgram;
using SuperMetroid.Core.Enemies.Fireflea.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// Fireflea's 52-frame glow loop as a typed program (#641), in place of their bank-$A3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="FirefleaFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class FirefleaAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<FirefleaProgram, FirefleaFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<FirefleaProgram, AnimationProgramStep<FirefleaProgram, FirefleaFrame, NoAnimationCue>[]>
    {
        // $A3:8C2F
        [P.Loop] =
        [
            Show(0, 2),
            Show(1, 1),
            Show(2, 2),
            Show(1, 1),
            Show(0, 2),
            Show(1, 1),
            Show(2, 2),
            Show(1, 1),
            Show(3, 2),
            Show(4, 1),
            Show(5, 2),
            Show(4, 1),
            Show(6, 2),
            Show(7, 1),
            Show(8, 2),
            Show(7, 1),
            Show(9, 2),
            Show(10, 1),
            Show(11, 2),
            Show(10, 1),
            Show(12, 2),
            Show(13, 1),
            Show(14, 2),
            Show(13, 1),
            Show(15, 2),
            Show(16, 1),
            Show(17, 2),
            Show(16, 1),
            Show(18, 2),
            Show(19, 1),
            Show(20, 2),
            Show(19, 1),
            Show(15, 2),
            Show(16, 1),
            Show(17, 2),
            Show(16, 1),
            Show(12, 2),
            Show(13, 1),
            Show(14, 2),
            Show(13, 1),
            Show(9, 2),
            Show(10, 1),
            Show(11, 2),
            Show(10, 1),
            Show(6, 2),
            Show(7, 1),
            Show(8, 2),
            Show(7, 1),
            Show(3, 2),
            Show(4, 1),
            Show(5, 2),
            Show(4, 1),
            GoTo(P.Loop),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<FirefleaProgram, FirefleaFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<FirefleaProgram, FirefleaFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<FirefleaProgram, FirefleaFrame, NoAnimationCue> GoTo(FirefleaProgram program) => AnimationProgramStep<FirefleaProgram, FirefleaFrame, NoAnimationCue>.GoTo(program);

}

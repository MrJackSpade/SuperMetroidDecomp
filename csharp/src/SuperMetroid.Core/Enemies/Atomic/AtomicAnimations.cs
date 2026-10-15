using P = SuperMetroid.Core.Enemies.Atomic.AtomicProgram;
using SuperMetroid.Core.Enemies.Atomic.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Atomic's four spinning animations as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="AtomicFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class AtomicAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<AtomicProgram, AtomicFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<AtomicProgram, AnimationProgramStep<AtomicProgram, AtomicFrame, NoAnimationCue>[]>
    {
        // $A8:E310
        [P.UpRight] =
        [
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            Show(3, 8),
            Show(4, 8),
            Show(5, 8),
            GoTo(P.UpRight),
        ],
        // $A8:E32C
        [P.UpLeft] =
        [
            Show(6, 8),
            Show(7, 8),
            Show(8, 8),
            Show(9, 8),
            Show(10, 8),
            Show(11, 8),
            GoTo(P.UpLeft),
        ],
        // $A8:E348
        [P.DownLeft] =
        [
            Show(5, 8),
            Show(4, 8),
            Show(3, 8),
            Show(2, 8),
            Show(1, 8),
            Show(0, 8),
            GoTo(P.DownLeft),
        ],
        // $A8:E364
        [P.DownRight] =
        [
            Show(11, 8),
            Show(10, 8),
            Show(9, 8),
            Show(8, 8),
            Show(7, 8),
            Show(6, 8),
            GoTo(P.DownRight),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<AtomicProgram, AtomicFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<AtomicProgram, AtomicFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<AtomicProgram, AtomicFrame, NoAnimationCue> GoTo(AtomicProgram program) => AnimationProgramStep<AtomicProgram, AtomicFrame, NoAnimationCue>.GoTo(program);

}

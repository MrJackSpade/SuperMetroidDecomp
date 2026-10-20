using P = SuperMetroid.Core.Enemies.Bull.BullProgram;
using SuperMetroid.Core.Enemies.Bull.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// Bull's normal and immune-shot animations as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="BullFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class BullAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<BullProgram, BullFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<BullProgram, AnimationProgramStep<BullProgram, BullFrame, NoAnimationCue>[]>
    {
        [P.Normal] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(0, 10),
            Show(2, 10),
            GoTo(P.Normal),
        ],
        [P.Shot] =
        [
            SetCounter(5),
            FallInto(P.ShotLoop),
        ],
        [P.ShotLoop] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(0, 3),
            Show(2, 3),
            RepeatWhileCounting(P.ShotLoop),
            GoTo(P.Normal),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<BullProgram, BullFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<BullProgram, BullFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<BullProgram, BullFrame, NoAnimationCue> SetCounter(int count) => AnimationProgramStep<BullProgram, BullFrame, NoAnimationCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<BullProgram, BullFrame, NoAnimationCue> RepeatWhileCounting(BullProgram program) => AnimationProgramStep<BullProgram, BullFrame, NoAnimationCue>.RepeatWhileCounting(program);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<BullProgram, BullFrame, NoAnimationCue> GoTo(BullProgram program) => AnimationProgramStep<BullProgram, BullFrame, NoAnimationCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<BullProgram, BullFrame, NoAnimationCue> FallInto(BullProgram program) => AnimationProgramStep<BullProgram, BullFrame, NoAnimationCue>.FallInto(program);

}

using P = SuperMetroid.Core.Enemies.Ripper.RipperProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The GRipper, Ripper II and Ripper flying loops as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="RipperFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class RipperAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<RipperProgram, RipperFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<RipperProgram, AnimationProgramStep<RipperProgram, RipperFrame, NoAnimationCue>[]>
    {
        [P.GRipperMovingLeft] =
        [
            Show(0, 8),
            Show(1, 7),
            Show(0, 8),
            Show(2, 7),
            GoTo(P.GRipperMovingLeft),
        ],
        [P.GRipperMovingRight] =
        [
            Show(3, 8),
            Show(4, 7),
            Show(3, 8),
            Show(5, 7),
            GoTo(P.GRipperMovingRight),
        ],
        [P.Ripper2MovingRight] =
        [
            Show(0, 8),
            Show(1, 7),
            Show(0, 8),
            Show(2, 7),
            GoTo(P.Ripper2MovingRight),
        ],
        [P.Ripper2MovingLeft] =
        [
            Show(3, 8),
            Show(4, 7),
            Show(3, 8),
            Show(5, 7),
            GoTo(P.Ripper2MovingLeft),
        ],
        [P.RipperMovingRight] =
        [
            Show(9, 8),
            Show(10, 7),
            Show(9, 8),
            Show(11, 7),
            GoTo(P.RipperMovingRight),
        ],
        [P.RipperMovingLeft] =
        [
            Show(6, 8),
            Show(7, 7),
            Show(6, 8),
            Show(8, 7),
            GoTo(P.RipperMovingLeft),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<RipperProgram, RipperFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<RipperProgram, RipperFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<RipperProgram, RipperFrame, NoAnimationCue> GoTo(RipperProgram program) => AnimationProgramStep<RipperProgram, RipperFrame, NoAnimationCue>.GoTo(program);

}

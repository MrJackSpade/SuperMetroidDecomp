using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Zoa.ZoaCueKind;
using P = SuperMetroid.Core.Enemies.Zoa.ZoaProgram;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// Zoa's shooting and rising animations as typed programs (#641), in place of their bank-$A3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="ZoaFrame"/> pose and each callback becomes a <see cref="ZoaCue"/> with its typed operands.
/// </summary>
internal static class ZoaAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<ZoaProgram, ZoaFrame, ZoaCue> Programs { get; } = new(
        new Dictionary<ZoaProgram, AnimationProgramStep<ZoaProgram, ZoaFrame, ZoaCue>[]>
    {
        [P.FacingLeftShooting] =
        [
            Cue(new(K.SetSpeedStage1)),
            Show(0, 64),
            Cue(new(K.SetSpeedStage2)),
            Show(1, 8),
            Cue(new(K.SetSpeedStage3)),
            Show(2, 48),
            GoTo(P.FacingLeftShooting),
        ],
        [P.FacingLeftRising] =
        [
            Show(4, 4),
            Show(3, 4),
            Show(5, 4),
            GoTo(P.FacingLeftRising),
        ],
        [P.FacingRightShooting] =
        [
            Cue(new(K.SetSpeedStage1)),
            Show(6, 64),
            Cue(new(K.SetSpeedStage2)),
            Show(7, 8),
            Cue(new(K.SetSpeedStage3)),
            Show(8, 48),
            GoTo(P.FacingRightShooting),
        ],
        [P.FacingRightRising] =
        [
            Show(10, 4),
            Show(9, 4),
            Show(11, 4),
            GoTo(P.FacingRightRising),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<ZoaProgram, ZoaFrame, ZoaCue> Show(int pose, int duration) => AnimationProgramStep<ZoaProgram, ZoaFrame, ZoaCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<ZoaProgram, ZoaFrame, ZoaCue> Cue(ZoaCue cue) => AnimationProgramStep<ZoaProgram, ZoaFrame, ZoaCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<ZoaProgram, ZoaFrame, ZoaCue> GoTo(ZoaProgram program) => AnimationProgramStep<ZoaProgram, ZoaFrame, ZoaCue>.GoTo(program);

}

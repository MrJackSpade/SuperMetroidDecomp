using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Stoke.StokeCueKind;
using P = SuperMetroid.Core.Enemies.Stoke.StokeProgram;

namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// Stoke's walking and fire-spitting animations as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="StokeFrame"/> pose and each callback becomes a <see cref="StokeCue"/> with its typed operands.
/// </summary>
internal static class StokeAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<StokeProgram, StokeFrame, StokeCue> Programs { get; } = new(
        new Dictionary<StokeProgram, AnimationProgramStep<StokeProgram, StokeFrame, StokeCue>[]>
    {
        [P.MovingLeft] =
        [
            Cue(new(K.SetMovingLeft)),
            FallInto(P.MovingLeftLoop),
        ],
        [P.MovingLeftLoop] =
        [
            Show(0, 8),
            Show(1, 16),
            Show(2, 8),
            Show(3, 8),
            GoTo(P.MovingLeftLoop),
        ],
        [P.AttackingLeft] =
        [
            Show(2, 16),
            Cue(new(K.SpawnFireball, FireballDirection: StokeDirection.Left)),
            Show(4, 16),
            GoTo(P.MovingLeft),
        ],
        [P.MovingRight] =
        [
            Cue(new(K.SetMovingRight)),
            FallInto(P.MovingRightLoop),
        ],
        [P.MovingRightLoop] =
        [
            Show(5, 8),
            Show(6, 16),
            Show(7, 8),
            Show(8, 8),
            GoTo(P.MovingRightLoop),
        ],
        [P.AttackingRight] =
        [
            Show(7, 16),
            Cue(new(K.SpawnFireball, FireballDirection: StokeDirection.Right)),
            Show(9, 16),
            GoTo(P.MovingRight),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<StokeProgram, StokeFrame, StokeCue> Show(int pose, int duration) => AnimationProgramStep<StokeProgram, StokeFrame, StokeCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<StokeProgram, StokeFrame, StokeCue> Cue(StokeCue cue) => AnimationProgramStep<StokeProgram, StokeFrame, StokeCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<StokeProgram, StokeFrame, StokeCue> GoTo(StokeProgram program) => AnimationProgramStep<StokeProgram, StokeFrame, StokeCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<StokeProgram, StokeFrame, StokeCue> FallInto(StokeProgram program) => AnimationProgramStep<StokeProgram, StokeFrame, StokeCue>.FallInto(program);

}

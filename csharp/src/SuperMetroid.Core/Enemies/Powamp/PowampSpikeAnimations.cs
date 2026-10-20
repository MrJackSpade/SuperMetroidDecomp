using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.Powamp.PowampSpikeProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// The Powamp spike's programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="PowampSpikeFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class PowampSpikeAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<PowampSpikeProgram, PowampSpikeFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<PowampSpikeProgram, AnimationProgramStep<PowampSpikeProgram, PowampSpikeFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Initial] =
        [
            Show(0, 6),
            Show(1, 6),
            Show(2, 6),
            GoTo(P.Initial),
        ],
        [P.Delete] =
        [
            Cue(new(CK.Delete)),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<PowampSpikeProgram, PowampSpikeFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<PowampSpikeProgram, PowampSpikeFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<PowampSpikeProgram, PowampSpikeFrame, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => AnimationProgramStep<PowampSpikeProgram, PowampSpikeFrame, EnemyProjectileCommonCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<PowampSpikeProgram, PowampSpikeFrame, EnemyProjectileCommonCue> GoTo(PowampSpikeProgram program) => AnimationProgramStep<PowampSpikeProgram, PowampSpikeFrame, EnemyProjectileCommonCue>.GoTo(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<PowampSpikeProgram, PowampSpikeFrame, EnemyProjectileCommonCue> Hold => AnimationProgramStep<PowampSpikeProgram, PowampSpikeFrame, EnemyProjectileCommonCue>.Hold;
}

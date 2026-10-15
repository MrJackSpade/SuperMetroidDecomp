using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.Shaktool.ShaktoolProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Shaktool's attack-circle programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="ShaktoolProjectileFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class ShaktoolProjectileAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<ShaktoolProjectileProgram, ShaktoolProjectileFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<ShaktoolProjectileProgram, AnimationProgramStep<ShaktoolProjectileProgram, ShaktoolProjectileFrame, EnemyProjectileCommonCue>[]>
    {
        // $86:BD68
        [P.Front] =
        [
            Show(0, 4),
            Show(1, 4),
            FallInto(P.FrontLoop),
        ],
        // $86:BD70
        [P.FrontLoop] =
        [
            Show(2, 119),
            GoTo(P.FrontLoop),
        ],
        // $86:BD78
        [P.Middle] =
        [
            Show(3, 6),
            Cue(new(CK.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.ShaktoolsAttack_MiddleBack_Moving)),
            Show(4, 4),
            FallInto(P.MiddleLoop),
        ],
        // $86:BD84
        [P.MiddleLoop] =
        [
            Show(5, 119),
            GoTo(P.MiddleLoop),
        ],
        // $86:BD8C
        [P.Back] =
        [
            Show(6, 10),
            Cue(new(CK.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.ShaktoolsAttack_MiddleBack_Moving)),
            FallInto(P.BackLoop),
        ],
        // $86:BD94
        [P.BackLoop] =
        [
            Show(7, 119),
            GoTo(P.BackLoop),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<ShaktoolProjectileProgram, ShaktoolProjectileFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<ShaktoolProjectileProgram, ShaktoolProjectileFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<ShaktoolProjectileProgram, ShaktoolProjectileFrame, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => AnimationProgramStep<ShaktoolProjectileProgram, ShaktoolProjectileFrame, EnemyProjectileCommonCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<ShaktoolProjectileProgram, ShaktoolProjectileFrame, EnemyProjectileCommonCue> GoTo(ShaktoolProjectileProgram program) => AnimationProgramStep<ShaktoolProjectileProgram, ShaktoolProjectileFrame, EnemyProjectileCommonCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<ShaktoolProjectileProgram, ShaktoolProjectileFrame, EnemyProjectileCommonCue> FallInto(ShaktoolProjectileProgram program) => AnimationProgramStep<ShaktoolProjectileProgram, ShaktoolProjectileFrame, EnemyProjectileCommonCue>.FallInto(program);
}

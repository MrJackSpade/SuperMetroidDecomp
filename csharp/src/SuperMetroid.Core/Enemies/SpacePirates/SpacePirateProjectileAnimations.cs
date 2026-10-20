using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Enemies.SpacePirates.SpacePirateProjectileProgram;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Pirate/Mother Brain laser and Ninja Pirate claw programs (#641), in place of their bank-$86 lists.</summary>
internal static class SpacePirateProjectileAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue> Programs { get; } = new(
        new Dictionary<SpacePirateProjectileProgram, AnimationProgramStep<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue>[]>
    {
        [P.LaserLeft] =
        [
            Show(0, 2),
            Show(1, 2),
            Show(2, 2),
            Cue(SpacePirateProjectileCueKind.StartMovingNow, EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Left),
            Show(3, 1),
            Show(4, 1),
            Show(5, 1),
            Show(6, 1),
            Show(7, 1),
            Show(8, 1),
            Show(9, 1),
            Show(10, 1),
            FallInto(P.LaserLeftLoop),
        ],
        [P.LaserLeftLoop] =
        [
            Show(11, 1),
            Show(12, 1),
            GoTo(P.LaserLeftLoop),
        ],
        [P.LaserRight] =
        [
            Show(13, 2),
            Show(14, 2),
            Show(15, 2),
            Cue(SpacePirateProjectileCueKind.StartMovingNow, EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Right),
            Show(16, 1),
            Show(17, 1),
            Show(18, 1),
            Show(19, 1),
            Show(20, 1),
            Show(21, 1),
            Show(22, 1),
            Show(23, 1),
            FallInto(P.LaserRightLoop),
        ],
        [P.LaserRightLoop] =
        [
            Show(24, 1),
            Show(25, 1),
            GoTo(P.LaserRightLoop),
        ],
        [P.ClawLeft] =
        [
            Cue(SpacePirateProjectileCueKind.StartMoving, EnemyProjectilePreInstruction.PirateClaw_Left),
            FallInto(P.ClawLeftLoop),
        ],
        [P.ClawLeftLoop] =
        [
            Show(26, 1),
            Show(27, 1),
            Show(28, 1),
            Show(29, 1),
            Show(30, 1),
            Show(31, 1),
            Show(32, 1),
            Show(33, 1),
            GoTo(P.ClawLeftLoop),
        ],
        [P.ClawRight] =
        [
            Cue(SpacePirateProjectileCueKind.StartMoving, EnemyProjectilePreInstruction.PirateClaw_Right),
            FallInto(P.ClawRightLoop),
        ],
        [P.ClawRightLoop] =
        [
            Show(34, 1),
            Show(35, 1),
            Show(36, 1),
            Show(37, 1),
            Show(38, 1),
            Show(39, 1),
            Show(40, 1),
            Show(41, 1),
            GoTo(P.ClawRightLoop),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue> Show(int pose, int duration) =>
        AnimationProgramStep<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue>.Show(new(pose), duration);

    /// <summary>Publishes a cue of <paramref name="kind"/> installing <paramref name="movement"/>.</summary>
    private static AnimationProgramStep<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue> Cue(
        SpacePirateProjectileCueKind kind, EnemyProjectilePreInstruction movement) =>
        AnimationProgramStep<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue>.Publish(new(kind, movement));

    /// <summary>Runs on into <paramref name="program"/>.</summary>
    private static AnimationProgramStep<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue> FallInto(SpacePirateProjectileProgram program) =>
        AnimationProgramStep<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue>.FallInto(program);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue> GoTo(SpacePirateProjectileProgram program) =>
        AnimationProgramStep<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue>.GoTo(program);
}

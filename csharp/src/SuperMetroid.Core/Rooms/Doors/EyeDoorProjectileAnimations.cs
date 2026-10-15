using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Rooms.Doors.EyeDoorProjectileCueKind;
using P = SuperMetroid.Core.Rooms.Doors.EyeDoorProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The eye door projectile's flight, impact and shot programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="EyeDoorProjectileFrame"/> pose and each command becomes a <see cref="EyeDoorProjectileCue"/> with its typed operands.
/// </summary>
internal static class EyeDoorProjectileAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue> Programs { get; } = new(
        new Dictionary<EyeDoorProjectileProgram, AnimationProgramStep<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue>[]>
    {
        // $86:B5D9
        [P.Initial] =
        [
            Show(0, 4),
            Show(1, 3),
            Show(2, 2),
            Cue(new(K.AimAtSamus)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.EyeDoorProjectilePreInstruction)),
            FallInto(P.FlyingLoop),
        ],
        // $86:B5EB
        [P.FlyingLoop] =
        [
            Show(3, 16),
            GoTo(P.FlyingLoop),
        ],
        // $86:B5F3
        [P.Impact] =
        [
            Cue(new(K.ClearPreInstruction)),
            Show(4, 2),
            Show(5, 3),
            Show(6, 4),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:B603
        [P.Shot] =
        [
            Show(7, 4),
            Show(8, 4),
            Show(9, 4),
            Show(10, 4),
            Cue(new(K.Delete)),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue> Show(int pose, int duration) => AnimationProgramStep<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue> Cue(EyeDoorProjectileCue cue) => AnimationProgramStep<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue> GoTo(EyeDoorProjectileProgram program) => AnimationProgramStep<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue> FallInto(EyeDoorProjectileProgram program) => AnimationProgramStep<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue> Hold => AnimationProgramStep<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue>.Hold;
}

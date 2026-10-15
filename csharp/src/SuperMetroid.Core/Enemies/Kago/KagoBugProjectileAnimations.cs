using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Kago.KagoBugProjectileCueKind;
using P = SuperMetroid.Core.Enemies.Kago.KagoBugProjectileProgram;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// The Kago bugs' landing, falling, jumping and shot programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="KagoBugProjectileFrame"/> pose and each command becomes a <see cref="KagoBugProjectileCue"/> with its typed operands.
/// </summary>
internal static class KagoBugProjectileAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue> Programs { get; } = new(
        new Dictionary<KagoBugProjectileProgram, AnimationProgramStep<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue>[]>
    {
        // $86:84FC
        [P.SharedDelete] =
        [
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:D03C
        [P.Landed] =
        [
            Show(0, 5),
            Cue(new(K.StartIdle)),
            Show(1, 32767),
            GoTo(P.Landed),
        ],
        // $86:D04A
        [P.Falling] =
        [
            Show(2, 32767),
            GoTo(P.Falling),
        ],
        // $86:D052
        [P.JumpStart] =
        [
            Show(3, 16),
            Show(4, 5),
            Cue(new(K.StartJump)),
            FallInto(P.JumpLoop),
        ],
        // $86:D05C
        [P.JumpLoop] =
        [
            Show(5, 32767),
            GoTo(P.JumpLoop),
        ],
        // $86:D064
        [P.Shot] =
        [
            Cue(new(K.UsePaletteZero)),
            Show(6, 4),
            Show(7, 4),
            Show(8, 4),
            Show(9, 4),
            Show(10, 4),
            Cue(new(K.SpawnDrop)),
            GoTo(P.SharedDelete),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue> Show(int pose, int duration) => AnimationProgramStep<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue> Cue(KagoBugProjectileCue cue) => AnimationProgramStep<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue> GoTo(KagoBugProjectileProgram program) => AnimationProgramStep<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue> FallInto(KagoBugProjectileProgram program) => AnimationProgramStep<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue> Hold => AnimationProgramStep<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue>.Hold;
}

using K = SuperMetroid.Core.Rooms.Plms.DownwardGateProjectileCueKind;
using P = SuperMetroid.Core.Rooms.Plms.DownwardGateProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The downward shot gate's moving and closed programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="DownwardGateProjectileFrame"/> pose and each command becomes a <see cref="DownwardGateProjectileCue"/> with its typed operands.
/// </summary>
internal static class DownwardGateProjectileAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue> Programs { get; } = new(
        new Dictionary<DownwardGateProjectileProgram, AnimationProgramStep<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue>[]>
    {
        // $86:E53C
        [P.Moving] =
        [
            Cue(new(K.SetYVelocity, Value: 256)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.DownwardGateMovementPreInstruction)),
            Show(0, 1),
            Hold,
        ],
        // $86:E54A
        [P.MovingStep2] =
        [
            Show(1, 1),
            Hold,
        ],
        // $86:E550
        [P.MovingStep3] =
        [
            Show(2, 1),
            Hold,
        ],
        // $86:E556
        [P.MovingStep4] =
        [
            Show(3, 1),
            Hold,
        ],
        // $86:E55C
        [P.MovingStep5] =
        [
            Cue(new(K.ClearPreInstruction)),
            FallInto(P.Closed),
        ],
        // $86:E55E
        [P.Closed] =
        [
            Cue(new(K.SetYVelocity, Value: 65280)),
            Show(4, 1),
            Hold,
        ],
        // $86:E568
        [P.ClosedStep2] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.DownwardGateMovementPreInstruction)),
            Show(5, 1),
            Hold,
        ],
        // $86:E572
        [P.ClosedStep3] =
        [
            Show(6, 1),
            Hold,
        ],
        // $86:E578
        [P.ClosedStep4] =
        [
            Show(7, 1),
            Hold,
        ],
        // $86:E57E
        [P.ClosedStep5] =
        [
            Show(8, 1),
            Hold,
        ],
        // $86:E584
        [P.ClosedStep6] =
        [
            Cue(new(K.Delete)),
            Hold,
        ],
    });

    /// <summary>
    /// The program each sleeping program resumes with when its owner steps the native list past the sleep, keyed by
    /// the program that slept.
    /// </summary>
    internal static IReadOnlyDictionary<DownwardGateProjectileProgram, DownwardGateProjectileProgram> Resumption { get; } = new Dictionary<DownwardGateProjectileProgram, DownwardGateProjectileProgram>
    {
        [P.Moving] = P.MovingStep2,
        [P.MovingStep2] = P.MovingStep3,
        [P.MovingStep3] = P.MovingStep4,
        [P.MovingStep4] = P.MovingStep5,
        [P.Closed] = P.ClosedStep2,
        [P.ClosedStep2] = P.ClosedStep3,
        [P.ClosedStep3] = P.ClosedStep4,
        [P.ClosedStep4] = P.ClosedStep5,
        [P.ClosedStep5] = P.ClosedStep6,
    };

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue> Show(int pose, int duration) => AnimationProgramStep<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue> Cue(DownwardGateProjectileCue cue) => AnimationProgramStep<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue>.Publish(cue);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue> FallInto(DownwardGateProjectileProgram program) => AnimationProgramStep<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue> Hold => AnimationProgramStep<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue>.Hold;
}

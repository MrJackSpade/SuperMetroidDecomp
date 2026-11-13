using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Rooms.RoomObjects.DownwardGateProjectileCueKind;
using P = SuperMetroid.Core.Rooms.RoomObjects.DownwardGateProjectileProgram;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The downward shot gate's moving and closed programs (#641). Each
/// frame shows a <see cref="DownwardGateProjectileFrame"/> pose and each command becomes a <see cref="DownwardGateProjectileCue"/> with its typed operands.
/// </summary>
internal static class DownwardGateProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue> Segments { get; } = new(
        new Dictionary<DownwardGateProjectileProgram, AnimationBeat<DownwardGateProjectileFrame, DownwardGateProjectileCue>[]>
    {
        [P.Moving] =
        [
            Cue(new DownwardGateProjectileCue.SetYVelocity(YVelocity: SignedEightEight.FromParts(1, 0))),
            Cue(new DownwardGateProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.DownwardGateDescent)),
            Show(0, 1),
        ],
        [P.MovingStep2] =
        [
            Show(1, 1),
        ],
        [P.MovingStep3] =
        [
            Show(2, 1),
        ],
        [P.MovingStep4] =
        [
            Show(3, 1),
        ],
        [P.MovingStep5] =
        [
            Cue(new DownwardGateProjectileCue.Command(K.ClearFrameBehavior)),
        ],
        [P.Closed] =
        [
            Cue(new DownwardGateProjectileCue.SetYVelocity(YVelocity: -SignedEightEight.FromParts(1, 0))),
            Show(4, 1),
        ],
        [P.ClosedStep2] =
        [
            Cue(new DownwardGateProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.DownwardGateDescent)),
            Show(5, 1),
        ],
        [P.ClosedStep3] =
        [
            Show(6, 1),
        ],
        [P.ClosedStep4] =
        [
            Show(7, 1),
        ],
        [P.ClosedStep5] =
        [
            Show(8, 1),
        ],
        [P.ClosedStep6] =
        [
            Cue(new DownwardGateProjectileCue.Command(K.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DownwardGateProjectileProgram, Animation<DownwardGateProjectileProgram>> StartingAt { get; } = new Dictionary<DownwardGateProjectileProgram, Animation<DownwardGateProjectileProgram>>
    {
        [P.Moving] = new([new(P.Moving, 0, 3, 1)], AnimationEnd.Holds),
        [P.MovingStep2] = new([new(P.MovingStep2, 0, 1, 1)], AnimationEnd.Holds),
        [P.MovingStep3] = new([new(P.MovingStep3, 0, 1, 1)], AnimationEnd.Holds),
        [P.MovingStep4] = new([new(P.MovingStep4, 0, 1, 1)], AnimationEnd.Holds),
        [P.MovingStep5] = new([new(P.MovingStep5, 0, 1, 1), new(P.Closed, 0, 2, 1)], AnimationEnd.Holds),
        [P.Closed] = new([new(P.Closed, 0, 2, 1)], AnimationEnd.Holds),
        [P.ClosedStep2] = new([new(P.ClosedStep2, 0, 2, 1)], AnimationEnd.Holds),
        [P.ClosedStep3] = new([new(P.ClosedStep3, 0, 1, 1)], AnimationEnd.Holds),
        [P.ClosedStep4] = new([new(P.ClosedStep4, 0, 1, 1)], AnimationEnd.Holds),
        [P.ClosedStep5] = new([new(P.ClosedStep5, 0, 1, 1)], AnimationEnd.Holds),
        [P.ClosedStep6] = new([new(P.ClosedStep6, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DownwardGateProjectileProgram> StartingWith(DownwardGateProjectileProgram segment) => StartingAt[segment];

    /// <summary>
    /// The program each sleeping program resumes with when its owner steps the list past the sleep, keyed by
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
    private static AnimationBeat<DownwardGateProjectileFrame, DownwardGateProjectileCue> Show(int pose, int duration) => new AnimationBeat<DownwardGateProjectileFrame, DownwardGateProjectileCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<DownwardGateProjectileFrame, DownwardGateProjectileCue> Cue(DownwardGateProjectileCue cue) => new AnimationBeat<DownwardGateProjectileFrame, DownwardGateProjectileCue>.Event(cue);
}

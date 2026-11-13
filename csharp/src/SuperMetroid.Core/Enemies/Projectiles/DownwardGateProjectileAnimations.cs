using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Rooms.RoomObjects.DownwardGateProjectileCueKind;
using P = SuperMetroid.Core.Rooms.RoomObjects.DownwardGateProjectileSegment;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The downward shot gate's moving and closed programs (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each command becomes a <see cref="DownwardGateProjectileCue"/> with its typed operands.
/// </summary>
internal static class DownwardGateProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DownwardGateProjectileSegment, EnemyProjectileFrameId, DownwardGateProjectileCue> Segments { get; } = new(
        new Dictionary<DownwardGateProjectileSegment, AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue>[]>
    {
        [P.Moving] =
        [
            Cue(new DownwardGateProjectileCue.SetYVelocity(YVelocity: SignedEightEight.FromParts(1, 0))),
            Cue(new DownwardGateProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.DownwardGateDescent)),
            Show(EnemyProjectileFrameId.DownwardGateMoving, 1),
        ],
        [P.MovingStep2] =
        [
            Show(EnemyProjectileFrameId.DownwardGateThirdMovingStep, 1),
        ],
        [P.MovingStep3] =
        [
            Show(EnemyProjectileFrameId.DownwardGateFourthMovingStep, 1),
        ],
        [P.MovingStep4] =
        [
            Show(EnemyProjectileFrameId.DownwardGateFifthMovingStep, 1),
        ],
        [P.MovingStep5] =
        [
            Cue(new DownwardGateProjectileCue.Command(K.ClearFrameBehavior)),
        ],
        [P.Closed] =
        [
            Cue(new DownwardGateProjectileCue.SetYVelocity(YVelocity: -SignedEightEight.FromParts(1, 0))),
            Show(EnemyProjectileFrameId.DownwardGateClosed, 1),
        ],
        [P.ClosedStep2] =
        [
            Cue(new DownwardGateProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.DownwardGateDescent)),
            Show(EnemyProjectileFrameId.DownwardGateThirdClosedStep, 1),
        ],
        [P.ClosedStep3] =
        [
            Show(EnemyProjectileFrameId.DownwardGateFourthClosedStep, 1),
        ],
        [P.ClosedStep4] =
        [
            Show(EnemyProjectileFrameId.DownwardGateFifthClosedStep, 1),
        ],
        [P.ClosedStep5] =
        [
            Show(EnemyProjectileFrameId.DownwardGateSixthClosedStep, 1),
        ],
        [P.ClosedStep6] =
        [
            Cue(new DownwardGateProjectileCue.Command(K.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DownwardGateProjectileSegment, Animation<DownwardGateProjectileSegment>> StartingAt { get; } = new Dictionary<DownwardGateProjectileSegment, Animation<DownwardGateProjectileSegment>>
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
    internal static Animation<DownwardGateProjectileSegment> StartingWith(DownwardGateProjectileSegment segment) => StartingAt[segment];

    /// <summary>
    /// The program each sleeping program resumes with when its owner steps the list past the sleep, keyed by
    /// the program that slept.
    /// </summary>
    internal static IReadOnlyDictionary<DownwardGateProjectileSegment, DownwardGateProjectileSegment> Resumption { get; } = new Dictionary<DownwardGateProjectileSegment, DownwardGateProjectileSegment>
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue> Cue(DownwardGateProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue>.Event(cue);
}

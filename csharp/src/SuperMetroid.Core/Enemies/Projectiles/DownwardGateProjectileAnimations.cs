using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Rooms.RoomObjects.DownwardGateProjectileCueKind;
using P = SuperMetroid.Core.Rooms.RoomObjects.DownwardGateProjectileSegment;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The downward shot gate's moving and closed animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="DownwardGateProjectileCue"/> with the values it acts on.
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
        [P.MovedOneBlock] =
        [
            Show(EnemyProjectileFrameId.DownwardGateThirdMovingStep, 1),
        ],
        [P.MovedTwoBlocks] =
        [
            Show(EnemyProjectileFrameId.DownwardGateFourthMovingStep, 1),
        ],
        [P.MovedThreeBlocks] =
        [
            Show(EnemyProjectileFrameId.DownwardGateFifthMovingStep, 1),
        ],
        [P.Stopping] =
        [
            Cue(new DownwardGateProjectileCue.Signal(K.ClearFrameBehavior)),
        ],
        [P.Closed] =
        [
            Cue(new DownwardGateProjectileCue.SetYVelocity(YVelocity: -SignedEightEight.FromParts(1, 0))),
            Show(EnemyProjectileFrameId.DownwardGateClosed, 1),
        ],
        [P.Rising] =
        [
            Cue(new DownwardGateProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.DownwardGateDescent)),
            Show(EnemyProjectileFrameId.DownwardGateThirdClosedStep, 1),
        ],
        [P.RisenOneBlock] =
        [
            Show(EnemyProjectileFrameId.DownwardGateFourthClosedStep, 1),
        ],
        [P.RisenTwoBlocks] =
        [
            Show(EnemyProjectileFrameId.DownwardGateFifthClosedStep, 1),
        ],
        [P.RisenThreeBlocks] =
        [
            Show(EnemyProjectileFrameId.DownwardGateSixthClosedStep, 1),
        ],
        [P.Vanishing] =
        [
            Cue(new DownwardGateProjectileCue.Signal(K.Delete)),
        ],
    });

    /// <summary>The animation each place of the gate's travel starts; the stopping gate shows itself closed as it stops.</summary>
    internal static IReadOnlyDictionary<DownwardGateProjectileSegment, Animation<DownwardGateProjectileSegment>> StartingAt { get; } = new Dictionary<DownwardGateProjectileSegment, Animation<DownwardGateProjectileSegment>>
    {
        [P.Moving] = new([new(P.Moving, 0, 3, 1)], AnimationEnd.Holds),
        [P.MovedOneBlock] = new([new(P.MovedOneBlock, 0, 1, 1)], AnimationEnd.Holds),
        [P.MovedTwoBlocks] = new([new(P.MovedTwoBlocks, 0, 1, 1)], AnimationEnd.Holds),
        [P.MovedThreeBlocks] = new([new(P.MovedThreeBlocks, 0, 1, 1)], AnimationEnd.Holds),
        [P.Stopping] = new([new(P.Stopping, 0, 1, 1), new(P.Closed, 0, 2, 1)], AnimationEnd.Holds),
        [P.Closed] = new([new(P.Closed, 0, 2, 1)], AnimationEnd.Holds),
        [P.Rising] = new([new(P.Rising, 0, 2, 1)], AnimationEnd.Holds),
        [P.RisenOneBlock] = new([new(P.RisenOneBlock, 0, 1, 1)], AnimationEnd.Holds),
        [P.RisenTwoBlocks] = new([new(P.RisenTwoBlocks, 0, 1, 1)], AnimationEnd.Holds),
        [P.RisenThreeBlocks] = new([new(P.RisenThreeBlocks, 0, 1, 1)], AnimationEnd.Holds),
        [P.Vanishing] = new([new(P.Vanishing, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DownwardGateProjectileSegment> StartingWith(DownwardGateProjectileSegment segment) => StartingAt[segment];

    /// <summary>
    /// The animation the gate plays at its next place once it has travelled a block, or once a closed gate is woken, keyed
    /// by the animation it rests in.
    /// </summary>
    internal static IReadOnlyDictionary<DownwardGateProjectileSegment, DownwardGateProjectileSegment> Resumption { get; } = new Dictionary<DownwardGateProjectileSegment, DownwardGateProjectileSegment>
    {
        [P.Moving] = P.MovedOneBlock,
        [P.MovedOneBlock] = P.MovedTwoBlocks,
        [P.MovedTwoBlocks] = P.MovedThreeBlocks,
        [P.MovedThreeBlocks] = P.Stopping,
        [P.Closed] = P.Rising,
        [P.Rising] = P.RisenOneBlock,
        [P.RisenOneBlock] = P.RisenTwoBlocks,
        [P.RisenTwoBlocks] = P.RisenThreeBlocks,
        [P.RisenThreeBlocks] = P.Vanishing,
    };

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue> Cue(DownwardGateProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue>.Event(cue);
}

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
            Cue(new DownwardGateProjectileCue.Signal(K.StartSliding)),
            Hold(EnemyProjectileFrameId.DownwardGateOneBlockTall),
        ],
        [P.MovedOneBlock] =
        [
            Hold(EnemyProjectileFrameId.DownwardGateTwoBlocksTall),
        ],
        [P.MovedTwoBlocks] =
        [
            Hold(EnemyProjectileFrameId.DownwardGateThreeBlocksTall),
        ],
        [P.MovedThreeBlocks] =
        [
            Hold(EnemyProjectileFrameId.DownwardGateFourBlocksTall),
        ],
        [P.Stopping] =
        [
            Cue(new DownwardGateProjectileCue.Signal(K.StopSliding)),
        ],
        [P.Closed] =
        [
            Cue(new DownwardGateProjectileCue.SetYVelocity(YVelocity: -SignedEightEight.FromParts(1, 0))),
            Hold(EnemyProjectileFrameId.DownwardGateFourBlocksTall),
        ],
        [P.Rising] =
        [
            Cue(new DownwardGateProjectileCue.Signal(K.StartSliding)),
            Hold(EnemyProjectileFrameId.DownwardGateFourBlocksTall),
        ],
        [P.RisenOneBlock] =
        [
            Hold(EnemyProjectileFrameId.DownwardGateThreeBlocksTall),
        ],
        [P.RisenTwoBlocks] =
        [
            Hold(EnemyProjectileFrameId.DownwardGateTwoBlocksTall),
        ],
        [P.RisenThreeBlocks] =
        [
            Hold(EnemyProjectileFrameId.DownwardGateOneBlockTall),
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
    /// The gate's travel, each stage named by the animation that starts it, and the stage each takes the gate on to
    /// (#2284): the moving gate slides down three blocks and stops closed; once woken, the closed gate rises three blocks
    /// and vanishes. Each block the gate passes, and the wake, takes it on to the next stage; the vanishing gate has none.
    /// </summary>
    internal static IReadOnlyDictionary<DownwardGateProjectileSegment, DownwardGateProjectileSegment> NextStage { get; } =
        new Dictionary<DownwardGateProjectileSegment, DownwardGateProjectileSegment>
        {
            [P.Moving] = P.MovedOneBlock,
            [P.MovedOneBlock] = P.MovedTwoBlocks,
            [P.MovedTwoBlocks] = P.MovedThreeBlocks,
            [P.MovedThreeBlocks] = P.Stopping,
            [P.Stopping] = P.Rising,
            [P.Rising] = P.RisenOneBlock,
            [P.RisenOneBlock] = P.RisenTwoBlocks,
            [P.RisenTwoBlocks] = P.RisenThreeBlocks,
            [P.RisenThreeBlocks] = P.Vanishing,
        };

    /// <summary>The stage a moving gate starts at, sliding down.</summary>
    internal const DownwardGateProjectileSegment SlidingDownStage = P.Moving;

    /// <summary>
    /// The stage a gate stands at closed: where a moving gate stops, and where a gate spawned closed starts, showing itself
    /// closed.
    /// </summary>
    internal const DownwardGateProjectileSegment ClosedStage = P.Stopping;

    /// <summary>Shows <paramref name="frame"/> for good (#2276): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue> Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue> Cue(DownwardGateProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue>.Event(cue);
}

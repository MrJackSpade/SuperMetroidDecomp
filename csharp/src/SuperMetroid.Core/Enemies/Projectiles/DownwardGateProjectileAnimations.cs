using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Rooms.RoomObjects.DownwardGateProjectileCueKind;
using P = SuperMetroid.Core.Rooms.RoomObjects.DownwardGateProjectileSegment;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The downward shot gate's moving and closed animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="DownwardGateProjectileCue"/> with the values it acts on.
/// </summary>
internal static partial class DownwardGateProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static UnpausedAnimationSegments<DownwardGateProjectileSegment, EnemyProjectileFrameId, DownwardGateProjectileCue> Segments { get; } = new(
        new Dictionary<DownwardGateProjectileSegment, AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue>.Unpaused[]>
    {
        [P.Moving] =
        [
            Cue(new DownwardGateProjectileCue.SetYVelocity(YVelocity: SignedEightEight.FromPixels(1))),
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
            Cue(new DownwardGateProjectileCue.SetYVelocity(YVelocity: -SignedEightEight.FromPixels(1))),
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
    internal static IReadOnlyDictionary<DownwardGateProjectileSegment, Animation<DownwardGateProjectileSegment>> StartingAt { get; } = Segments.StartsEnding(new Dictionary<DownwardGateProjectileSegment, Animation<DownwardGateProjectileSegment>>
    {
        [P.Stopping] = new([Segments.Whole(P.Stopping), Segments.Whole(P.Closed)], AnimationEnd.Holds),
    }, AnimationEnd.Holds,
        P.Moving, P.MovedOneBlock, P.MovedTwoBlocks, P.MovedThreeBlocks, P.Closed, P.Rising, P.RisenOneBlock,
        P.RisenTwoBlocks, P.RisenThreeBlocks, P.Vanishing);

    /// <summary>
    /// The gate's travel, each stage named by the animation that starts it, and the stage each takes the gate on to
    /// (#2284): the moving gate slides down three blocks and stops closed; once woken, the closed gate rises three blocks
    /// and vanishes. Each block the gate passes, and the wake, takes it on to the next stage. The vanishing gate deletes
    /// itself as its stage starts, so it goes on to no other: it stays vanishing.
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
            [P.Vanishing] = P.Vanishing,
        };

    /// <summary>The stage a moving gate starts at, sliding down.</summary>
    internal const DownwardGateProjectileSegment SlidingDownStage = P.Moving;

    /// <summary>
    /// The stage a gate stands at closed: where a moving gate stops, and where a gate spawned closed starts, showing itself
    /// closed.
    /// </summary>
    internal const DownwardGateProjectileSegment ClosedStage = P.Stopping;

    /// <summary>Shows <paramref name="frame"/> for good (#2276): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue>.Unpaused Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue>.Unpaused Cue(DownwardGateProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, DownwardGateProjectileCue>.Event(cue);
}

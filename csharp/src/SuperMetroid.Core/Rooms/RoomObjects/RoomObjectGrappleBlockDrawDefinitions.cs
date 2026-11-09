using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The five one-block draws used by breakable-Grapple-block PLMs: the initial frame installs Grapple collision and the
/// later frames install air collision. What each shows is its installed picture (#1557).
/// </summary>
internal static class RoomObjectGrappleBlockDrawDefinitions
{
    /// <summary>A single Grapple-block draw with how the block it writes collides.</summary>
    internal readonly record struct DrawList(GrappleBlockDraw Frame, RoomCollisionType Collision);

    /// <summary>How the block each draw installs collides.</summary>
    internal static readonly IReadOnlyDictionary<GrappleBlockDraw, RoomCollisionType> Collisions = new Dictionary<GrappleBlockDraw, RoomCollisionType>
    {
        [GrappleBlockDraw.Intact] = RoomCollisionType.GrappleBlock,
        [GrappleBlockDraw.Break0] = RoomCollisionType.Air,
        [GrappleBlockDraw.Break1] = RoomCollisionType.Air,
        [GrappleBlockDraw.Break2] = RoomCollisionType.Air,
        [GrappleBlockDraw.Blank] = RoomCollisionType.Air,
    };
}

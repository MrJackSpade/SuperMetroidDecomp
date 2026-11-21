using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The five one-block draws used by breakable-Grapple-block room objects: the intact block installs Grapple collision and
/// the breaking and cleared blocks install air collision. What each shows is its installed picture (#1557).
/// </summary>
internal static partial class RoomObjectGrappleBlockDrawDefinitions
{
    /// <summary>A single Grapple-block draw with how the block it writes collides.</summary>
    /// <param name="Draw">The draw.</param>
    /// <param name="Collision">How the block it writes collides.</param>
    internal readonly partial record struct DrawnBlock(GrappleBlockDraw Draw, RoomCollisionType Collision);

    /// <summary>How the block each draw installs collides.</summary>
    internal static readonly IReadOnlyDictionary<GrappleBlockDraw, RoomCollisionType> Collisions = new Dictionary<GrappleBlockDraw, RoomCollisionType>
    {
        [GrappleBlockDraw.Intact] = RoomCollisionType.GrappleBlock,
        [GrappleBlockDraw.Rock] = RoomCollisionType.Air,
        [GrappleBlockDraw.Pieces] = RoomCollisionType.Air,
        [GrappleBlockDraw.Dust] = RoomCollisionType.Air,
        [GrappleBlockDraw.Blank] = RoomCollisionType.Air,
    };
}

using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The five one-block draws used by breakable-Grapple-block PLMs. These are complete terrain blocks, not merely
/// appearances: the initial frame installs Grapple collision and the later frames install air collision.
/// </summary>
internal static class RoomObjectGrappleBlockDrawDefinitions
{
    /// <summary>A single Grapple-block draw with the terrain it installs.</summary>
    internal readonly record struct DrawList(GrappleBlockDraw Frame, RoomTerrainBlock Terrain);

    /// <summary>The terrain each draw installs.</summary>
    internal static readonly IReadOnlyDictionary<GrappleBlockDraw, RoomTerrainBlock> Blocks = new Dictionary<GrappleBlockDraw, RoomTerrainBlock>
    {
        [GrappleBlockDraw.Intact] = RoomTerrainBlock.Of(183, BlockMirroring.None, RoomCollisionType.GrappleBlock),
        [GrappleBlockDraw.Break0] = RoomTerrainBlock.Of(83, BlockMirroring.None, RoomCollisionType.Air),
        [GrappleBlockDraw.Break1] = RoomTerrainBlock.Of(84, BlockMirroring.None, RoomCollisionType.Air),
        [GrappleBlockDraw.Break2] = RoomTerrainBlock.Of(85, BlockMirroring.None, RoomCollisionType.Air),
        [GrappleBlockDraw.Blank] = RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
    };
}

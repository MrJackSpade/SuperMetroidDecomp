using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The five one-block draws used by breakable-Grapple-block PLMs. These are complete terrain blocks, not merely
/// appearances: the initial frame installs Grapple collision and the later frames install air collision.
/// </summary>
internal static class RoomPlmGrappleBlockDrawDefinitions
{
    /// <summary>A single Grapple-block draw with the terrain it installs.</summary>
    internal readonly record struct DrawList(GrappleBlockDraw Frame, RoomTerrainBlock Terrain)
    {
        /// <summary>The one-block run writes along its row.</summary>
        internal const PlmRunDirection Direction = PlmRunDirection.Horizontal;
        /// <summary>The run writes its one block, followed by signed zero X/Y termination.</summary>
        internal const int Count = 1;
        /// <summary>The one-block record leaves the next draw position in its current column.</summary>
        internal const int NextX = 0;
        /// <summary>The one-block record leaves the next draw position in its current row.</summary>
        internal const int NextY = 0;
    }

    /// <summary>The terrain each draw installs.</summary>
    internal static readonly IReadOnlyDictionary<GrappleBlockDraw, RoomTerrainBlock> Blocks = new Dictionary<GrappleBlockDraw, RoomTerrainBlock>
    {
        [GrappleBlockDraw.Intact] = RoomTerrainBlock.Of(0x0b7, BlockMirroring.None, RoomCollisionType.GrappleBlock),
        [GrappleBlockDraw.Break0] = RoomTerrainBlock.Of(0x053, BlockMirroring.None, RoomCollisionType.Air),
        [GrappleBlockDraw.Break1] = RoomTerrainBlock.Of(0x054, BlockMirroring.None, RoomCollisionType.Air),
        [GrappleBlockDraw.Break2] = RoomTerrainBlock.Of(0x055, BlockMirroring.None, RoomCollisionType.Air),
        [GrappleBlockDraw.Blank] = RoomTerrainBlock.Of(0x0ff, BlockMirroring.None, RoomCollisionType.Air),
    };
}

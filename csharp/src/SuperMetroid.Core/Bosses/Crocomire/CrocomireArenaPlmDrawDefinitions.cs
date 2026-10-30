using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Five bounded physical block-draw layouts for Crocomire's bridge and
/// three-column arena wall at $84:9B5B..9BF6. Bridge runs repeat one block;
/// wall columns use consecutive tiles, two alternating rows and a final base row.
/// Clear/create share artwork and differ only in whether the blocks are solid. Native
/// continuation offsets are relative to the origin, not the preceding column.
/// </summary>
internal static class CrocomireArenaPlmDrawDefinitions
{
    /// <summary>The bridge's or wall's shape for each of the five arena draws.</summary>
    internal static readonly Dictionary<CrocomireArenaDraw, CrocomireArenaShape> Shapes = new()
    {
        [CrocomireArenaDraw.ClearBridge] = CrocomireArenaShape.Bridge,
        [CrocomireArenaDraw.CrumbleBridgeBlock] = CrocomireArenaShape.Bridge,
        [CrocomireArenaDraw.ClearBridgeBlock] = CrocomireArenaShape.Bridge,
        [CrocomireArenaDraw.ClearInvisibleWall] = CrocomireArenaShape.ClearWall,
        [CrocomireArenaDraw.CreateInvisibleWall] = CrocomireArenaShape.SolidWall,
    };

    /// <summary>
    /// Each arena draw's cells, one row per run. A bridge is one run of repeated cells; a wall is three columns of
    /// consecutive tiles with two alternating rows and a final base row between empty cells, solid only when the
    /// create-wall state writes it.
    /// </summary>
    internal static readonly Dictionary<CrocomireArenaDraw, RoomTerrainBlock[][]> Cells = new()
    {
        [CrocomireArenaDraw.ClearBridge] =
        [
            [
                RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.Air),
            ],
        ],
        [CrocomireArenaDraw.CrumbleBridgeBlock] =
        [
            [RoomTerrainBlock.Of(0x10b, BlockMirroring.None, RoomCollisionType.SolidBlock)],
        ],
        [CrocomireArenaDraw.ClearBridgeBlock] =
        [
            [RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.Air)],
        ],
        [CrocomireArenaDraw.ClearInvisibleWall] =
        [
            WallColumn(0x107, 0x127, 0x147, RoomCollisionType.Air),
            WallColumn(0x108, 0x128, 0x148, RoomCollisionType.Air),
            WallColumn(0x109, 0x129, 0x149, RoomCollisionType.Air),
        ],
        [CrocomireArenaDraw.CreateInvisibleWall] =
        [
            WallColumn(0x107, 0x127, 0x147, RoomCollisionType.SolidBlock),
            WallColumn(0x108, 0x128, 0x148, RoomCollisionType.SolidBlock),
            WallColumn(0x109, 0x129, 0x149, RoomCollisionType.SolidBlock),
        ],
    };

    /// <summary>Each arena draw's editable artwork key.</summary>
    internal static readonly Dictionary<CrocomireArenaDraw, string> VisualIds = new()
    {
        [CrocomireArenaDraw.ClearBridge] = "clear-bridge",
        [CrocomireArenaDraw.CrumbleBridgeBlock] = "crumble-bridge-block",
        [CrocomireArenaDraw.ClearBridgeBlock] = "clear-bridge-block",
        [CrocomireArenaDraw.ClearInvisibleWall] = "clear-invisible-wall",
        [CrocomireArenaDraw.CreateInvisibleWall] = "create-invisible-wall",
    };

    /// <summary>
    /// One wall column: an empty cell, the top block, the middle block (the block one tileset row below the top), the
    /// two again, the base block (two tileset rows below the top), then two empty cells.
    /// </summary>
    private static RoomTerrainBlock[] WallColumn(int top, int middle, int bottom, RoomCollisionType collision) =>
    [
        RoomTerrainBlock.Of(0x80, BlockMirroring.None, collision),
        RoomTerrainBlock.Of(top, BlockMirroring.None, collision),
        RoomTerrainBlock.Of(middle, BlockMirroring.None, collision),
        RoomTerrainBlock.Of(top, BlockMirroring.None, collision),
        RoomTerrainBlock.Of(middle, BlockMirroring.None, collision),
        RoomTerrainBlock.Of(bottom, BlockMirroring.None, collision),
        RoomTerrainBlock.Of(0x80, BlockMirroring.None, collision),
        RoomTerrainBlock.Of(0x80, BlockMirroring.None, collision),
    ];
}

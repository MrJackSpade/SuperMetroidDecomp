using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Five bounded physical block-draw layouts for Crocomire's bridge and
/// three-column arena wall at $84:9B5B..9BF6. Bridge runs repeat one word;
/// wall columns use consecutive tiles, two alternating rows and a final base row.
/// Clear/create share artwork and differ only in the solid block bit. Native
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
    internal static readonly Dictionary<CrocomireArenaDraw, RoomLevelWord[][]> Cells = new()
    {
        [CrocomireArenaDraw.ClearBridge] =
        [
            [
                RoomLevelWord.Create(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x80, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x80, BlockMirroring.None, RoomCollisionType.Air),
            ],
        ],
        [CrocomireArenaDraw.CrumbleBridgeBlock] =
        [
            [RoomLevelWord.Create(0x10b, BlockMirroring.None, RoomCollisionType.SolidBlock)],
        ],
        [CrocomireArenaDraw.ClearBridgeBlock] =
        [
            [RoomLevelWord.Create(0x80, BlockMirroring.None, RoomCollisionType.Air)],
        ],
        [CrocomireArenaDraw.ClearInvisibleWall] =
        [
            WallColumn(0x107, RoomCollisionType.Air),
            WallColumn(0x108, RoomCollisionType.Air),
            WallColumn(0x109, RoomCollisionType.Air),
        ],
        [CrocomireArenaDraw.CreateInvisibleWall] =
        [
            WallColumn(0x107, RoomCollisionType.SolidBlock),
            WallColumn(0x108, RoomCollisionType.SolidBlock),
            WallColumn(0x109, RoomCollisionType.SolidBlock),
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
    /// One wall column from its first tile: an empty cell, the tile, the tile one tileset row below, the two again,
    /// the base two rows below the tile, then two empty cells.
    /// </summary>
    private static RoomLevelWord[] WallColumn(int tile, RoomCollisionType collision) =>
    [
        RoomLevelWord.Create(0x80, BlockMirroring.None, collision),
        RoomLevelWord.Create(tile, BlockMirroring.None, collision),
        RoomLevelWord.Create(tile + 0x20, BlockMirroring.None, collision),
        RoomLevelWord.Create(tile, BlockMirroring.None, collision),
        RoomLevelWord.Create(tile + 0x20, BlockMirroring.None, collision),
        RoomLevelWord.Create(tile + 0x40, BlockMirroring.None, collision),
        RoomLevelWord.Create(0x80, BlockMirroring.None, collision),
        RoomLevelWord.Create(0x80, BlockMirroring.None, collision),
    ];
}

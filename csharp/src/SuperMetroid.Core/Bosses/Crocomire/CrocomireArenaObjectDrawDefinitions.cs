using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Five bounded block-draw layouts for Crocomire's bridge and three-column arena wall: a bridge is one run, a wall
/// three eight-cell columns. Clear and create differ only in whether the wall's cells are solid. The continuation
/// offsets are relative to the origin, not the preceding column. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class CrocomireArenaObjectDrawDefinitions
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
    /// How each arena draw's cells collide, one row per run. The cleared bridge is a run of ten air cells, the crumbling
    /// bridge block one solid cell and the cleared bridge block one air cell; a wall is three columns of eight cells, solid
    /// only when the create-wall state writes it.
    /// </summary>
    internal static readonly Dictionary<CrocomireArenaDraw, RoomCollisionType[][]> Cells = new()
    {
        [CrocomireArenaDraw.ClearBridge] =
        [
            [
                RoomCollisionType.Air, RoomCollisionType.Air, RoomCollisionType.Air, RoomCollisionType.Air, RoomCollisionType.Air,
                RoomCollisionType.Air, RoomCollisionType.Air, RoomCollisionType.Air, RoomCollisionType.Air, RoomCollisionType.Air,
            ],
        ],
        [CrocomireArenaDraw.CrumbleBridgeBlock] =
        [
            [RoomCollisionType.SolidBlock],
        ],
        [CrocomireArenaDraw.ClearBridgeBlock] =
        [
            [RoomCollisionType.Air],
        ],
        [CrocomireArenaDraw.ClearInvisibleWall] =
        [
            WallColumn(RoomCollisionType.Air),
            WallColumn(RoomCollisionType.Air),
            WallColumn(RoomCollisionType.Air),
        ],
        [CrocomireArenaDraw.CreateInvisibleWall] =
        [
            WallColumn(RoomCollisionType.SolidBlock),
            WallColumn(RoomCollisionType.SolidBlock),
            WallColumn(RoomCollisionType.SolidBlock),
        ],
    };

    /// <summary>One wall column's eight cells, every one colliding as <paramref name="collision"/>.</summary>
    private static RoomCollisionType[] WallColumn(RoomCollisionType collision) =>
        [collision, collision, collision, collision, collision, collision, collision, collision];
}

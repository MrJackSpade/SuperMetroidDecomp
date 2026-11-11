using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The block shape of each of Crocomire's five bridge and invisible-wall frames (#1608): the bridge's ten blocks along
/// its row from the object's block, a single bridge block at it, and the invisible wall's three columns of eight blocks
/// hanging from it. Clearing and creating the wall cover the same blocks and differ only in whether they are solid. What
/// each frame shows is its installed picture (#1557).
/// </summary>
internal static class CrocomireArenaObjectDrawDefinitions
{
    /// <summary>The cleared bridge: ten open blocks along the row from the object's block.</summary>
    private static readonly RoomBlockShape ClearedBridge = new(
    [
        new(new(0, 0), RoomCollisionType.Air), new(new(1, 0), RoomCollisionType.Air), new(new(2, 0), RoomCollisionType.Air),
        new(new(3, 0), RoomCollisionType.Air), new(new(4, 0), RoomCollisionType.Air), new(new(5, 0), RoomCollisionType.Air),
        new(new(6, 0), RoomCollisionType.Air), new(new(7, 0), RoomCollisionType.Air), new(new(8, 0), RoomCollisionType.Air),
        new(new(9, 0), RoomCollisionType.Air),
    ]);

    /// <summary>A crumbling bridge block: the object's block, still solid.</summary>
    private static readonly RoomBlockShape CrumblingBridgeBlock = new([new(new(0, 0), RoomCollisionType.SolidBlock)]);

    /// <summary>A cleared bridge block: the object's block, open.</summary>
    private static readonly RoomBlockShape ClearedBridgeBlock = new([new(new(0, 0), RoomCollisionType.Air)]);

    /// <summary>The cleared invisible wall: three columns of eight open blocks from the object's block rightward.</summary>
    private static readonly RoomBlockShape ClearedWall = new(
    [
        .. WallColumn(0, RoomCollisionType.Air), .. WallColumn(1, RoomCollisionType.Air), .. WallColumn(2, RoomCollisionType.Air),
    ]);

    /// <summary>The created invisible wall: the cleared wall's blocks, every one solid.</summary>
    private static readonly RoomBlockShape SolidWall = new(
    [
        .. WallColumn(0, RoomCollisionType.SolidBlock), .. WallColumn(1, RoomCollisionType.SolidBlock), .. WallColumn(2, RoomCollisionType.SolidBlock),
    ]);

    /// <summary>The block shape each of the five arena frames draws.</summary>
    internal static readonly Dictionary<CrocomireArenaDraw, RoomBlockShape> Shapes = new()
    {
        [CrocomireArenaDraw.ClearBridge] = ClearedBridge,
        [CrocomireArenaDraw.CrumbleBridgeBlock] = CrumblingBridgeBlock,
        [CrocomireArenaDraw.ClearBridgeBlock] = ClearedBridgeBlock,
        [CrocomireArenaDraw.ClearInvisibleWall] = ClearedWall,
        [CrocomireArenaDraw.CreateInvisibleWall] = SolidWall,
    };

    /// <summary>
    /// One invisible-wall column: the eight blocks from the wall's top row down, <paramref name="column"/> blocks right of
    /// the object's block, each colliding as <paramref name="collision"/>.
    /// </summary>
    private static RoomBlockShapeCell[] WallColumn(int column, RoomCollisionType collision) =>
    [
        new(new(column, 0), collision), new(new(column, 1), collision), new(new(column, 2), collision), new(new(column, 3), collision),
        new(new(column, 4), collision), new(new(column, 5), collision), new(new(column, 6), collision), new(new(column, 7), collision),
    ];
}

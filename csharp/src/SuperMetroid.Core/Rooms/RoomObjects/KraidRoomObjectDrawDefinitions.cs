using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The block shape of each of the ten reachable Kraid ceiling and spike frames (#1608). The first crumble frame is also
/// drawn by the Maridia elevatube, and covers the same block for either owner. What each frame shows is its installed
/// picture (#1557).
/// </summary>
internal static class KraidRoomObjectDrawDefinitions
{
    /// <summary>A still-solid block at the object's block: the first two crumble frames.</summary>
    private static readonly RoomBlockShape SolidBlock = new([new(new(0, 0), RoomCollisionType.SolidBlock)]);

    /// <summary>An open block at the object's block: the last crumble frame, each background a ceiling block leaves, and each spike column's cleared block.</summary>
    private static readonly RoomBlockShape OpenBlock = new([new(new(0, 0), RoomCollisionType.Air)]);

    /// <summary>The cleared ceiling of an already-defeated room: fifteen open blocks along the row from the object's block.</summary>
    private static readonly RoomBlockShape ClearedCeiling = new(
    [
        new(new(0, 0), RoomCollisionType.Air), new(new(1, 0), RoomCollisionType.Air), new(new(2, 0), RoomCollisionType.Air),
        new(new(3, 0), RoomCollisionType.Air), new(new(4, 0), RoomCollisionType.Air), new(new(5, 0), RoomCollisionType.Air),
        new(new(6, 0), RoomCollisionType.Air), new(new(7, 0), RoomCollisionType.Air), new(new(8, 0), RoomCollisionType.Air),
        new(new(9, 0), RoomCollisionType.Air), new(new(10, 0), RoomCollisionType.Air), new(new(11, 0), RoomCollisionType.Air),
        new(new(12, 0), RoomCollisionType.Air), new(new(13, 0), RoomCollisionType.Air), new(new(14, 0), RoomCollisionType.Air),
    ]);

    /// <summary>The cleared spikes of an already-defeated room: twenty-two open blocks along the row from the object's block.</summary>
    private static readonly RoomBlockShape ClearedSpikes = new(
    [
        new(new(0, 0), RoomCollisionType.Air), new(new(1, 0), RoomCollisionType.Air), new(new(2, 0), RoomCollisionType.Air),
        new(new(3, 0), RoomCollisionType.Air), new(new(4, 0), RoomCollisionType.Air), new(new(5, 0), RoomCollisionType.Air),
        new(new(6, 0), RoomCollisionType.Air), new(new(7, 0), RoomCollisionType.Air), new(new(8, 0), RoomCollisionType.Air),
        new(new(9, 0), RoomCollisionType.Air), new(new(10, 0), RoomCollisionType.Air), new(new(11, 0), RoomCollisionType.Air),
        new(new(12, 0), RoomCollisionType.Air), new(new(13, 0), RoomCollisionType.Air), new(new(14, 0), RoomCollisionType.Air),
        new(new(15, 0), RoomCollisionType.Air), new(new(16, 0), RoomCollisionType.Air), new(new(17, 0), RoomCollisionType.Air),
        new(new(18, 0), RoomCollisionType.Air), new(new(19, 0), RoomCollisionType.Air), new(new(20, 0), RoomCollisionType.Air),
        new(new(21, 0), RoomCollisionType.Air),
    ]);

    /// <summary>The block shape each of the ten Kraid room frames draws.</summary>
    internal static readonly Dictionary<KraidRoomDraw, RoomBlockShape> Shapes = new()
    {
        [KraidRoomDraw.CrumbleFirst] = SolidBlock,
        [KraidRoomDraw.CrumbleSecond] = SolidBlock,
        [KraidRoomDraw.CrumbleThird] = OpenBlock,
        [KraidRoomDraw.CeilingBackground1] = OpenBlock,
        [KraidRoomDraw.CeilingBackground2] = OpenBlock,
        [KraidRoomDraw.CeilingBackground3] = OpenBlock,
        [KraidRoomDraw.SpikeFirst] = OpenBlock,
        [KraidRoomDraw.SpikeSecond] = OpenBlock,
        [KraidRoomDraw.ClearCeiling] = ClearedCeiling,
        [KraidRoomDraw.ClearSpikes] = ClearedSpikes,
    };
}

using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The block shape of each of Kraid's ceiling and spike frames (#1608): the three stages of a crumbling block's breakup,
/// and the pictures and clears its draws leave. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class KraidRoomObjectDrawDefinitions
{
    /// <summary>A still-solid block at the object's block: a crumbling block's rock and its pieces.</summary>
    private static readonly RoomBlockShape SolidBlock = new([new(new(0, 0), RoomCollisionType.SolidBlock)]);

    /// <summary>An open block at the object's block: a crumbling block's dust, each background a ceiling block leaves, and each spike column's cleared block.</summary>
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

    /// <summary>The block shape each stage of a crumbling block's breakup draws; its last stage is the picture its draw leaves.</summary>
    internal static readonly Dictionary<BlockBreakupStage, RoomBlockShape> CrumbleShapes = new()
    {
        [BlockBreakupStage.Rock] = SolidBlock,
        [BlockBreakupStage.Pieces] = SolidBlock,
        [BlockBreakupStage.Dust] = OpenBlock,
    };

    /// <summary>The block shape each picture a crumbled block leaves, and each clear, draws.</summary>
    internal static readonly Dictionary<KraidRoomDraw, RoomBlockShape> Shapes = new()
    {
        [KraidRoomDraw.CeilingBackgroundLeftEdge] = OpenBlock,
        [KraidRoomDraw.CeilingBackgroundOddColumn] = OpenBlock,
        [KraidRoomDraw.CeilingBackgroundEvenColumn] = OpenBlock,
        [KraidRoomDraw.SpikeBackgroundLeftColumn] = OpenBlock,
        [KraidRoomDraw.SpikeBackgroundRightColumn] = OpenBlock,
        [KraidRoomDraw.ClearCeiling] = ClearedCeiling,
        [KraidRoomDraw.ClearSpikes] = ClearedSpikes,
    };
}

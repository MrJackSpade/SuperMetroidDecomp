using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The block shape of each of Kraid's ceiling and spike frames (#1608): the three stages of a crumbling block's breakup,
/// and the pictures and clears its draws leave. What each frame shows is its installed picture (#1557).
/// </summary>
internal static partial class KraidRoomObjectDrawDefinitions
{
    /// <summary>A still-solid block at the object's block: a crumbling block's rock and its pieces.</summary>
    private static readonly RoomBlockShape SolidBlock = new([new(new(0, 0), RoomCollisionType.SolidBlock)]);

    /// <summary>An open block at the object's block: a crumbling block's dust, each background a ceiling block leaves, and each spike column's cleared block.</summary>
    private static readonly RoomBlockShape OpenBlock = new([new(new(0, 0), RoomCollisionType.Air)]);

    /// <summary>The cleared ceiling of an already-defeated room: fifteen open blocks along the row from the object's block.</summary>
    private static readonly RoomBlockShape ClearedCeiling = RoomBlockShapes.Row(15, RoomCollisionType.Air);

    /// <summary>The cleared spikes of an already-defeated room: twenty-two open blocks along the row from the object's block.</summary>
    private static readonly RoomBlockShape ClearedSpikes = RoomBlockShapes.Row(22, RoomCollisionType.Air);

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

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The block shapes of the four horizontal crumble frames and six-row clear draw selected by the Tourian access-floor room
/// objects (#1862): rows four blocks wide stacked down from the object's block, all air. What each frame shows is its
/// installed picture (#1557).
/// </summary>
internal static class TourianAccessObjectDrawDefinitions
{
    /// <summary>One opened row of the floor: four air blocks from the object's block.</summary>
    private static readonly RoomBlockShape OpenRow = RoomBlockShapes.Rectangle(new(4, 1), RoomCollisionType.Air);

    /// <summary>The cleared floor: the access's rows opened down from the object's block.</summary>
    private static readonly RoomBlockShape ClearedFloor = RoomBlockShapes.Rectangle(new(4, 6), RoomCollisionType.Air);

    /// <summary>The block shape each Tourian access draw draws: clear opens the access's rows; the empty row one.</summary>
    internal static readonly IReadOnlyDictionary<TourianAccessDraw, RoomBlockShape> Shapes =
        new Dictionary<TourianAccessDraw, RoomBlockShape>
        {
            [TourianAccessDraw.Clear] = ClearedFloor,
            [TourianAccessDraw.EmptyRow] = OpenRow,
        };

    /// <summary>The block shape each drawn stage of a row's crumble draws (#2291): one opened row.</summary>
    internal static readonly IReadOnlyDictionary<BlockBreakupStage, RoomBlockShape> CrumbleShapes =
        new Dictionary<BlockBreakupStage, RoomBlockShape>
        {
            [BlockBreakupStage.Rock] = OpenRow,
            [BlockBreakupStage.Pieces] = OpenRow,
            [BlockBreakupStage.Dust] = OpenRow,
        };
}

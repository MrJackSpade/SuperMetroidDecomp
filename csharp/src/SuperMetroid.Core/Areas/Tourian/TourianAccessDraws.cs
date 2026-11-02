using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Describes the Tourian access floor's draws (<see cref="TourianAccessPlmDefinitions"/>, <see cref="TourianAccessPlmDrawDefinitions"/>):
/// each draw's rows of four air cells.
/// </summary>
internal static class TourianAccessDraws
{
    /// <summary>The rows and air block of one Tourian access draw (<see cref="TourianAccessPlmDrawDefinitions.Fills"/>).</summary>
    internal static void Describe(TourianAccessDraw list, out int rows, out RoomTerrainBlock block)
    {
        TourianAccessDrawFill fill = TourianAccessPlmDrawDefinitions.Fills.TryGetValue(list, out TourianAccessDrawFill listed)
            ? listed : throw new InvalidOperationException($"Undefined TourianAccessDraw {list}.");
        rows = fill.Rows;
        block = fill.Block;
    }
}

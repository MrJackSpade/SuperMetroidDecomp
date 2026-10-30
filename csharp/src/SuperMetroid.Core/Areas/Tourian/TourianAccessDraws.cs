using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Builds the Tourian access floor's draws (<see cref="TourianAccessPlmDefinitions"/>, <see cref="TourianAccessPlmDrawDefinitions"/>):
/// each draw's rows of four air cells, and the visual identity artwork files name it by.
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

    /// <summary>Calculates temporary DTOs for artwork import/export without retaining a draw cache.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (TourianAccessDraw draw in Enum.GetValues<TourianAccessDraw>())
                yield return Compile(draw);
        }
    }

    /// <summary>Gets the stable artwork identity of a Tourian access draw.</summary>
    internal static string VisualId(PlmDraw draw) => VisualId(PlmDraw.FrameOf<TourianAccessDraw>(draw, "Tourian access"));

    /// <summary>Names the empty, three crumble, or six-row clear frame for visual files.</summary>
    internal static string VisualId(TourianAccessDraw draw) =>
        TourianAccessPlmDrawDefinitions.VisualIds.TryGetValue(draw, out string? id)
            ? id : throw new InvalidOperationException($"Undefined TourianAccessDraw {draw}.");

    /// <summary>Finds a compiled floor draw by its case-sensitive visual identity.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        ShotBlockPlmDraws.FindByVisualId(All, VisualId, id, out list);

    /// <summary>Builds the four-air-cell runs of a Tourian access draw.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList Compile(TourianAccessDraw draw)
    {
        Describe(draw, out int rows, out RoomTerrainBlock block);
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[rows];
        for (int row = 0; row < rows; row++)
            runs[row] = new(PlmRunDirection.Horizontal, Enumerable.Repeat(block, 4).ToArray(),
                new(0, (sbyte)(row + 1 < rows ? row + 1 : 0)));
        return new(PlmDraw.Of(draw), runs);
    }
}

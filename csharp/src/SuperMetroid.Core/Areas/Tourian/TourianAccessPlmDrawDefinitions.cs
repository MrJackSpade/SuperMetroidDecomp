using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The four horizontal crumble frames and six-row clear draw selected by the
/// Tourian access-floor PLMs. These words are physical level mutations;
/// replaceable block appearance is a separate concern. Uniform row words and
/// origin-relative continuation geometry are calculated without stored layouts.
/// </summary>
internal static class TourianAccessPlmDrawDefinitions
{
    /// <summary>
    /// The rows and air word of one Tourian access draw. Clear selects six blank rows; empty selects one. The three
    /// crumble records select successive tiles $53..55. Every row is four air cells.
    /// </summary>
    internal static void Describe(TourianAccessDraw list, out int rows, out int word)
    {
        TourianAccessDrawFill fill = list switch
        {
            TourianAccessDraw.Clear => new TourianAccessDrawFill(6, 0x00ff),
            TourianAccessDraw.EmptyRow => new TourianAccessDrawFill(1, 0x00ff),
            TourianAccessDraw.CrumbleFirst => new TourianAccessDrawFill(1, 0x53),
            TourianAccessDraw.CrumbleSecond => new TourianAccessDrawFill(1, 0x54),
            TourianAccessDraw.CrumbleThird => new TourianAccessDrawFill(1, 0x55),
            _ => throw new InvalidOperationException($"Undefined TourianAccessDraw {list}."),
        };
        rows = fill.Rows;
        word = checked((ushort)fill.Word);
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
    internal static string VisualId(TourianAccessDraw draw) => draw switch
    {
        TourianAccessDraw.EmptyRow => "crumble-empty-row",
        TourianAccessDraw.CrumbleFirst => "crumble-frame-0",
        TourianAccessDraw.CrumbleSecond => "crumble-frame-1",
        TourianAccessDraw.CrumbleThird => "crumble-frame-2",
        TourianAccessDraw.Clear => "clear-six-rows",
        _ => throw new InvalidOperationException($"Undefined TourianAccessDraw {draw}."),
    };

    /// <summary>Finds a compiled floor draw by its case-sensitive visual identity.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId(All, VisualId, id, out list);

    /// <summary>Builds the four-air-cell runs of a Tourian access draw.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList Compile(TourianAccessDraw draw)
    {
        Describe(draw, out int rows, out int word);
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[rows];
        for (int row = 0; row < rows; row++)
            runs[row] = new(4, Enumerable.Repeat(checked((ushort)word), 4).ToArray(),
                new(0, (sbyte)(row + 1 < rows ? row + 1 : 0)));
        return new(PlmDraw.Of(draw), runs);
    }
}

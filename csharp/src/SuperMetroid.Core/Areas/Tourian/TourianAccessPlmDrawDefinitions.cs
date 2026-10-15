using SuperMetroid.Core.Programs;
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
    /// Clear selects six blank rows; empty selects one. The three crumble records
    /// select successive tiles $53..55. Every row is four air cells.
    /// </summary>
    internal static bool TryDescribe(ushort pointer, out int rows, out ushort word)
    {
        if (!Enum.IsDefined((TourianAccessDraw)pointer))
        {
            rows = 0;
            word = 0;
            return false;
        }
        var list = (TourianAccessDraw)pointer;
        (rows, word) = list switch
        {
            TourianAccessDraw.Clear => (6, (ushort)0x00ff),
            TourianAccessDraw.EmptyRow => (1, (ushort)0x00ff),
            TourianAccessDraw.CrumbleFirst => (1, (ushort)0x53),
            TourianAccessDraw.CrumbleSecond => (1, (ushort)0x54),
            TourianAccessDraw.CrumbleThird => (1, (ushort)0x55),
            _ => throw new InvalidOperationException($"Undefined TourianAccessDraw {list}."),
        };
        return true;
    }

    /// <summary>Calculates temporary DTOs for artwork import/export without retaining a draw cache.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            for (int pointer = (int)TourianAccessDraw.EmptyRow; pointer <= (int)TourianAccessDraw.Clear; pointer += 12)
            {
                TryGet((ushort)pointer, out var draw);
                yield return draw;
            }
        }
    }

    /// <summary>Gets a stable artwork identity after validating a native draw pointer.</summary>
    internal static string VisualId(ushort pointer) =>
        VisualId(ClosedNativeWords.Decode<TourianAccessDraw>(pointer, "Tourian access draw with a visual ID"));

    /// <summary>Names the empty, three crumble, or six-row clear frame for visual files.</summary>
    internal static string VisualId(TourianAccessDraw pointer) => pointer switch
    {
        TourianAccessDraw.EmptyRow => "crumble-empty-row",
        TourianAccessDraw.CrumbleFirst => "crumble-frame-0",
        TourianAccessDraw.CrumbleSecond => "crumble-frame-1",
        TourianAccessDraw.CrumbleThird => "crumble-frame-2",
        TourianAccessDraw.Clear => "clear-six-rows",
        _ => throw new InvalidOperationException($"Undefined TourianAccessDraw {pointer}."),
    };

    /// <summary>Finds a compiled floor draw by its case-sensitive visual identity.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in All)
        {
            if (!string.Equals(VisualId(candidate.Pointer), id,
                    StringComparison.Ordinal))
                continue;
            list = candidate;
            return true;
        }
        list = default;
        return false;
    }

    /// <summary>Builds four-air-cell runs for a supported draw, including all six rows of the final clear.</summary>
    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        if (!TryDescribe(pointer, out int rows, out ushort word))
        {
            list = default;
            return false;
        }
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[rows];
        for (int row = 0; row < rows; row++)
            runs[row] = new(4, new ushort[] {word, word, word, word},
                new(0, (sbyte)(row + 1 < rows ? row + 1 : 0)));
        list = new(pointer, runs);
        return true;
    }
}

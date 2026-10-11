using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>The five bank-$84 Tourian access-floor draw lists, valued by native address.</summary>
internal enum TourianAccessDraw : ushort
{
    /// <summary><c>$84:9297</c>: clear one row after its crumble frames.</summary>
    EmptyRow = 0x9297,
    /// <summary><c>$84:92A3</c>: first visible crumble row.</summary>
    CrumbleFirst = 0x92a3,
    /// <summary><c>$84:92AF</c>: second visible crumble row.</summary>
    CrumbleSecond = 0x92af,
    /// <summary><c>$84:92BB</c>: third visible crumble row.</summary>
    CrumbleThird = 0x92bb,
    /// <summary><c>$84:92C7</c>: clear all six rows at once.</summary>
    Clear = 0x92c7,
}

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

    // Temporary DTOs retain the artwork import/export contract; no draw cache remains.
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

    internal static string VisualId(ushort pointer) =>
        VisualId(ClosedNativeWords.Decode<TourianAccessDraw>(pointer, "Tourian access draw with a visual ID"));

    internal static string VisualId(TourianAccessDraw pointer) => pointer switch
    {
        TourianAccessDraw.EmptyRow => "crumble-empty-row",
        TourianAccessDraw.CrumbleFirst => "crumble-frame-0",
        TourianAccessDraw.CrumbleSecond => "crumble-frame-1",
        TourianAccessDraw.CrumbleThird => "crumble-frame-2",
        TourianAccessDraw.Clear => "clear-six-rows",
        _ => throw new InvalidOperationException($"Undefined TourianAccessDraw {pointer}."),
    };

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

    internal static ushort CrumbleFramePointer(int frame) => frame switch
    {
        0 => (ushort)TourianAccessDraw.CrumbleFirst,
        1 => (ushort)TourianAccessDraw.CrumbleSecond,
        2 => (ushort)TourianAccessDraw.CrumbleThird,
        3 => (ushort)TourianAccessDraw.EmptyRow,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

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
                0, (sbyte)(row + 1 < rows ? row + 1 : 0));
        list = new(pointer, runs);
        return true;
    }
}

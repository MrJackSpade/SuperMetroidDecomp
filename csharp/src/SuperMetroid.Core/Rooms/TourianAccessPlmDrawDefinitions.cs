namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The four horizontal crumble frames and six-row clear draw selected by the
/// Tourian access-floor PLMs. These words are physical level mutations;
/// replaceable block appearance is a separate concern. Uniform row words and
/// origin-relative continuation geometry are calculated without stored layouts.
/// </summary>
internal static class TourianAccessPlmDrawDefinitions
{
    /// <summary><c>$84:9297</c>: clear one row after its crumble frames.</summary>
    internal const ushort EmptyRowPointer = 0x9297;
    /// <summary><c>$84:92A3</c>: first visible crumble row.</summary>
    internal const ushort CrumbleFirstPointer = 0x92a3;
    /// <summary><c>$84:92AF</c>: second visible crumble row.</summary>
    internal const ushort CrumbleSecondPointer = 0x92af;
    /// <summary><c>$84:92BB</c>: third visible crumble row.</summary>
    internal const ushort CrumbleThirdPointer = 0x92bb;
    /// <summary><c>$84:92C7</c>: clear all six rows at once.</summary>
    internal const ushort ClearPointer = 0x92c7;

    /// <summary>
    /// Clear selects six blank rows; empty selects one. Three aligned crumble records
    /// select successive tiles $53..55 at a twelve-byte stride. Every row is four air cells.
    /// </summary>
    internal static bool TryDescribe(ushort pointer, out int rows, out ushort word)
    {
        if (pointer is ClearPointer or EmptyRowPointer)
        {
            rows = pointer == ClearPointer ? 6 : 1;
            word = 0x00ff;
            return true;
        }
        int relative = pointer - CrumbleFirstPointer;
        if (relative >= 0 && relative <= CrumbleThirdPointer - CrumbleFirstPointer && relative % 12 == 0)
        {
            rows = 1;
            word = (ushort)(0x53 + relative / 12);
            return true;
        }
        rows = 0;
        word = 0;
        return false;
    }

    // Temporary DTOs retain the artwork import/export contract; no draw cache remains.
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            for (int pointer = EmptyRowPointer; pointer <= ClearPointer; pointer += 12)
            {
                TryGet((ushort)pointer, out var draw);
                yield return draw;
            }
        }
    }

    internal static string VisualId(ushort pointer) => pointer switch
    {
        EmptyRowPointer => "crumble-empty-row",
        CrumbleFirstPointer => "crumble-frame-0",
        CrumbleSecondPointer => "crumble-frame-1",
        CrumbleThirdPointer => "crumble-frame-2",
        ClearPointer => "clear-six-rows",
        _ => throw new InvalidDataException(
            $"Tourian access draw ${pointer:X4} has no visual ID."),
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
        0 => CrumbleFirstPointer,
        1 => CrumbleSecondPointer,
        2 => CrumbleThirdPointer,
        3 => EmptyRowPointer,
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

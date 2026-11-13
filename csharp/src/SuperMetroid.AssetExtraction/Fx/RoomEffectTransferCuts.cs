namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// Cuts a room effect's tilemap transfer (<see cref="RoomFxLayer3Pages"/>) into the pictures the effect overlay shows
/// (#1706). The cartridge scrolls its overlay so that a liquid's surface lies on the bottom line of the transfer's first
/// row and the rows below repeat down without end, and so that an atmosphere never shows the transfer's first row. A
/// liquid is therefore installed as its surface strip, the rows above the shortest run the rest of the transfer repeats
/// to its end, and its body, one such run; an atmosphere as the transfer below its first row, which repeats down. Rows are
/// <see cref="RoomFxLayer3TilemapFormat.WidthInTiles"/> words each.
/// </summary>
internal static class RoomEffectTransferCuts
{
    /// <summary>Words in one row of the transfer.</summary>
    private const int RowWords = RoomFxLayer3TilemapFormat.WidthInTiles;

    /// <summary>The rows at the top of an atmosphere's transfer it never shows.</summary>
    private const int AtmosphereHiddenRows = 1;

    /// <summary>Pixels down one row of the transfer.</summary>
    private const int RowPixels = 8;

    /// <summary>The strip's pixel row that lies on the liquid's surface line: the bottom line of the transfer's first row.</summary>
    internal const int SurfaceRow = RowPixels - 1;

    /// <summary>
    /// The rows of a liquid's transfer <paramref name="words"/> its surface strip and its body show: the strip from the
    /// first row to the body, the body the shortest run of rows from the earliest row from which the transfer repeats that
    /// run, at least twice, to its end.
    /// </summary>
    /// <exception cref="InvalidDataException">The transfer is not whole rows, or no run of its rows repeats to its end.</exception>
    internal static LiquidTransferRows Liquid(ReadOnlySpan<ushort> words)
    {
        int rows = Rows(words);
        for (int first = 1; first < rows; first++)
        {
            for (int period = 1; 2 * period <= rows - first; period++)
            {
                if (RepeatsDown(words, first, period, rows))
                    return new LiquidTransferRows(first, period);
            }
        }
        throw new InvalidDataException("A liquid's transfer has no body: no run of its rows repeats to its end.");
    }

    /// <summary>The first row of an atmosphere's transfer <paramref name="words"/> that it shows; it shows every row from there.</summary>
    /// <exception cref="InvalidDataException">The transfer is not whole rows, or holds no row it shows.</exception>
    internal static int AtmosphereFirstRow(ReadOnlySpan<ushort> words) =>
        Rows(words) > AtmosphereHiddenRows ? AtmosphereHiddenRows
            : throw new InvalidDataException("An atmosphere's transfer holds no row it shows.");

    /// <summary>The rows <paramref name="words"/> hold.</summary>
    /// <exception cref="InvalidDataException">The words are not whole rows.</exception>
    private static int Rows(ReadOnlySpan<ushort> words) =>
        !words.IsEmpty && words.Length % RowWords == 0 ? words.Length / RowWords
            : throw new InvalidDataException($"A room effect's transfer is whole rows of {RowWords} words.");

    /// <summary>Whether every row of <paramref name="words"/> from <paramref name="first"/> plus <paramref name="period"/> repeats the row <paramref name="period"/> above it.</summary>
    private static bool RepeatsDown(ReadOnlySpan<ushort> words, int first, int period, int rows)
    {
        for (int row = first + period; row < rows; row++)
        {
            if (!words.Slice(row * RowWords, RowWords).SequenceEqual(words.Slice((row - period) * RowWords, RowWords)))
                return false;
        }
        return true;
    }
}

/// <summary>Where a liquid's transfer is cut (#1706): its surface strip is rows 0 up to <paramref name="BodyFirstRow"/>, its body the <paramref name="BodyRows"/> rows from there.</summary>
/// <param name="BodyFirstRow">The first row of the body, the rows above it the surface strip.</param>
/// <param name="BodyRows">The rows of the body, which the transfer repeats down to its end.</param>
internal readonly record struct LiquidTransferRows(int BodyFirstRow, int BodyRows);

using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The two selected Crocomire dissolve illustrations at $A4:9C79 and $A4:9E7B.
/// Native BG rows have 32 cells; the editable 16x16 document preserves that linear
/// order. Irregular fragment placement is the drawing itself. Blank margins,
/// adjacent atlas columns and repeated tiles calculate rather than being stored.
/// This owns composition only: pixels, dissolve timing and collision are separate.
/// </summary>
internal sealed class CrocomireMeltingTilemap
{
    /// <summary>$A4:9C79..A07A: the blank glyph, character 824 with plain attributes; also clears Crocomire's working BG2 rows.</summary>
    internal static SnesBgTilemapWord BlankGlyph { get; } = SnesBgTilemapWord.Create(824, 0, priority: false);
    /// <summary>Both native illustrations use BG palette 7 with priority and no reflections.</summary>
    private const int IllustrationPalette = 7;
    /// <summary>Horizontal stride between vertically adjacent source tiles in the editable atlas.</summary>
    private const int AtlasColumns = 16;
    /// <summary>Native BG tilemap row width used to decode linear cell positions.</summary>
    private const int BgColumns = 32;
    /// <summary>Selects the eroded second illustration instead of the initial closed-jaw image.</summary>
    private readonly bool second;
    /// <summary>Supplied cells that differ from the reviewed illustration composition.</summary>
    internal readonly Dictionary<int, ushort> edits = [];

    /// <summary>Retains custom cells while calculating unchanged composition and blank margins.</summary>
    internal CrocomireMeltingTilemap(CrocomireMeltingIllustration pass, ReadOnlySpan<ushort> supplied)
    {
        this.second = pass == CrocomireMeltingIllustration.Second;
        if (supplied.Length != CrocomireMeltingArtworkFormat.TilemapCellCount)
            throw new InvalidDataException("Crocomire melting layout requires 256 words.");
        for (int index = 0; index < supplied.Length; index++)
            if (supplied[index] != Calculate(index)) edits.Add(index, supplied[index]);
    }

    /// <summary>Builds a fresh native-order tilemap from calculated cells and retained edits.</summary>
    internal ushort[] Words()
    {
        var words = new ushort[CrocomireMeltingArtworkFormat.TilemapCellCount];
        for (int index = 0; index < words.Length; index++)
            words[index] = edits.ContainsKey(index) ? edits[index] : Calculate(index);
        return words;
    }

    /// <summary>The atlas tile at <paramref name="row"/> and <paramref name="column"/> of the sixteen-wide editable atlas.</summary>
    private static int At(int row, int column) => row * AtlasColumns + column;

    /// <summary>Increasing atlas rows are contiguous source strips, sixteen tiles apart.</summary>
    private static int Strip(int first, int column, int start) => first + (column - start) * AtlasColumns;

    /// <summary>Applies the common illustration palette and priority to a selected fragment or emits the blank glyph.</summary>
    private ushort Calculate(int index)
    {
        int row = index / BgColumns, column = index % BgColumns;
        int tile = second ? Second(row, column) : First(row, column);
        return tile < 0 ? BlankGlyph.Raw : SnesBgTilemapWord.Create(tile, IllustrationPalette, priority: true).Raw;
    }

    /// <summary>$A4:9C79: closed upper jaw, eye ridge, mouth and lower neck fragments.
    /// The repeated tile29 in the lower row is preserved as selected composition;
    /// no historical mistake or implied tile39 is invented.</summary>
    private static int First(int row, int x) => row switch
    {
        0 => x switch { 0 => At(0, 0), >= 1 and <= 4 => Strip(At(2, 0), x, 1), 5 => At(0, 1), _ => -1 },
        1 => x switch { 0 or 1 => At(1, 0) + x, 2 => At(0, 4), 3 or 4 => Strip(At(3, 3), x, 3),
            5 => At(2, 3), 6 or 7 => Strip(At(4, 1), x, 6), _ => -1 },
        2 => x switch { 1 => At(0, 2), 2 => At(1, 4), 3 => At(0, 5), 4 or 5 => Strip(At(4, 4), x, 4),
            6 or 7 => Strip(At(2, 4), x, 6), 8 or 9 => Strip(At(1, 2), x, 8), _ => -1 },
        3 => x switch { 2 => At(5, 3), 3 => At(1, 5), >= 4 and <= 8 => Strip(row + 3, x, 4),
            9 => At(1, 3), 10 => At(5, 2), _ => -1 },
        4 => x switch { 3 => At(2, 5), 4 => At(5, 6), >= 5 and <= 9 => Strip(row + 3, x, 5), 10 => At(0, 3), _ => -1 },
        5 => x switch { 4 => At(5, 7), >= 5 and <= 9 => Strip(row + 3, x, 5), 10 => At(3, 5), _ => -1 },
        6 => x switch { 1 => At(3, 2), 2 => At(2, 1), 3 => At(4, 5), 4 => At(5, 8),
            >= 5 and <= 7 => Strip(row + 3, x, 5), 8 => Strip(row + 3, 7, 5),
            9 or 10 => Strip(At(4, 9), x, 9), _ => -1 },
        7 => x switch { 1 => At(4, 2), 2 => At(3, 1), 3 => At(5, 5),
            >= 4 and <= 9 => Strip(row + 3, x, 4), 10 => At(0, 11), _ => -1 },
        _ => -1,
    };

    /// <summary>$A4:9E7B: exposed teeth/jaw and eroded face/neck. The disconnected
    /// upper tile00 at column9 is the native selected fragment, not an inferred correction.</summary>
    private static int Second(int row, int x) => row switch
    {
        0 => x switch { >= 0 and <= 3 => Strip(At(0, 0), x, 0), 9 => At(0, 0), _ => -1 },
        1 => x switch { 0 => At(4, 0), 1 or 2 => Strip(At(1, 1), x, 1), 3 => At(5, 0), 4 => At(2, 3), _ => -1 },
        2 => x switch { 1 or 2 => Strip(At(3, 2), x, 1), 3 => At(0, 4), 4 => At(5, 4),
            5 => At(0, 3), 8 => At(3, 3), 9 => At(0, 1), 10 => At(5, 3), _ => -1 },
        3 => x switch { 3 or 4 => Strip(At(3, 4), x, 3), 5 => At(0, 5), 6 => At(2, 4),
            7 => At(1, 4), >= 8 and <= 10 => Strip(At(0, 2), x, 8), _ => -1 },
        4 => x switch { >= 4 and <= 6 => Strip(At(3, 5), x, 4),
            >= 7 and <= 10 => Strip(row + 2, x, 7), 11 => At(3, 10), _ => -1 },
        5 => x switch { 5 or 6 => Strip(At(4, 6), x, 5), >= 7 and <= 11 => Strip(row + 2, x, 7), _ => -1 },
        6 => x switch { >= 1 and <= 3 => Strip(At(3, 1), x, 1), 4 => At(5, 7),
            >= 5 and <= 10 => Strip(row + 2, x, 5), 11 => At(0, 9), _ => -1 },
        7 => x switch { 1 => At(4, 3), 2 => At(5, 2), 3 => At(1, 9), 4 => At(1, 3),
            >= 5 and <= 7 => Strip(At(3, 9), x, 5), >= 8 and <= 11 => Strip(At(0, 10), x, 8), _ => -1 },
        _ => -1,
    };
}


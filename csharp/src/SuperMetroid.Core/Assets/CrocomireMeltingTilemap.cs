namespace SuperMetroid.Core.Assets;

/// <summary>
/// The two selected Crocomire dissolve illustrations at $A4:9C79 and $A4:9E7B.
/// Native BG rows have 32 cells; the editable 16x16 document preserves that linear
/// order. Irregular fragment placement is the drawing itself. Blank margins,
/// adjacent atlas columns and repeated tiles calculate rather than being stored.
/// This owns composition only: pixels, dissolve timing and collision are separate.
/// </summary>
internal sealed class CrocomireMeltingTilemap
{
    /// <summary>$A4:9C79..A07A: selected blank glyph, including its original plain attributes.</summary>
    private const ushort Blank = 0x0338;
    /// <summary>Both native illustrations use BG palette7 and priority1, with no reflections.</summary>
    private const ushort IllustrationStyle = 0x3c00;
    /// <summary>
    /// Number of 8-pixel tile columns in the source atlas used to calculate contiguous strips.
    /// </summary>
    private const int AtlasColumns = 16;
    /// <summary>
    /// Native background row width used to map each linear tilemap index to a row and column.
    /// </summary>
    private const int BgColumns = 32;
    /// <summary>
    /// Selects the exposed-jaw composition when true and the closed-jaw composition otherwise.
    /// </summary>
    private readonly bool second;
    /// <summary>
    /// Stores supplied cells only when they differ from the selected composition's calculated tile.
    /// </summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>
    /// Creates a tilemap composition, retaining only authored cells that differ from its
    /// cartridge-derived default layout.
    /// </summary>
    /// <param name="second">Selects the exposed-jaw illustration instead of the closed-jaw illustration.</param>
    /// <param name="supplied">The 16-by-16 authoring tilemap whose nondefault cells are preserved.</param>
    internal CrocomireMeltingTilemap(bool second, ReadOnlySpan<ushort> supplied)
    {
        this.second = second;
        if (supplied.Length != CrocomireMeltingArtworkFormat.TilemapCellCount)
            throw new InvalidDataException("Crocomire melting layout requires 256 words.");
        for (int index = 0; index < supplied.Length; index++)
            if (supplied[index] != Calculate(index)) edits.Add(index, supplied[index]);
    }

    /// <summary>
    /// Builds the complete 16-by-16 tilemap by combining calculated defaults with authored cell edits.
    /// </summary>
    internal ushort[] Words()
    {
        var words = new ushort[CrocomireMeltingArtworkFormat.TilemapCellCount];
        for (int index = 0; index < words.Length; index++)
            words[index] = edits.TryGetValue(index, out ushort edit) ? edit : Calculate(index);
        return words;
    }

    /// <summary>Increasing atlas rows are contiguous source strips, sixteen tiles apart.</summary>
    private static int Strip(int first, int column, int start) => first + (column - start) * AtlasColumns;

    /// <summary>
    /// Calculates the native default tile word for one linear cell in the 16-by-16 authoring layout.
    /// </summary>
    private ushort Calculate(int index)
    {
        int row = index / BgColumns, column = index % BgColumns;
        int tile = second ? Second(row, column) : First(row, column);
        return tile < 0 ? Blank : (ushort)(IllustrationStyle | tile);
    }

    /// <summary>$A4:9C79: closed upper jaw, eye ridge, mouth and lower neck fragments.
    /// The repeated tile29 in the lower row is preserved as selected composition;
    /// no historical mistake or implied tile39 is invented.</summary>
    private static int First(int row, int x) => row switch
    {
        0 => x switch { 0 => 0x00, >= 1 and <= 4 => Strip(0x20, x, 1), 5 => 0x01, _ => -1 },
        1 => x switch { 0 or 1 => 0x10 + x, 2 => 0x04, 3 or 4 => Strip(0x33, x, 3),
            5 => 0x23, 6 or 7 => Strip(0x41, x, 6), _ => -1 },
        2 => x switch { 1 => 0x02, 2 => 0x14, 3 => 0x05, 4 or 5 => Strip(0x44, x, 4),
            6 or 7 => Strip(0x24, x, 6), 8 or 9 => Strip(0x12, x, 8), _ => -1 },
        3 => x switch { 2 => 0x53, 3 => 0x15, >= 4 and <= 8 => Strip(row + 3, x, 4),
            9 => 0x13, 10 => 0x52, _ => -1 },
        4 => x switch { 3 => 0x25, 4 => 0x56, >= 5 and <= 9 => Strip(row + 3, x, 5), 10 => 0x03, _ => -1 },
        5 => x switch { 4 => 0x57, >= 5 and <= 9 => Strip(row + 3, x, 5), 10 => 0x35, _ => -1 },
        6 => x switch { 1 => 0x32, 2 => 0x21, 3 => 0x45, 4 => 0x58,
            >= 5 and <= 7 => Strip(row + 3, x, 5), 8 => Strip(row + 3, 7, 5),
            9 or 10 => Strip(0x49, x, 9), _ => -1 },
        7 => x switch { 1 => 0x42, 2 => 0x31, 3 => 0x55,
            >= 4 and <= 9 => Strip(row + 3, x, 4), 10 => 0x0b, _ => -1 },
        _ => -1,
    };

    /// <summary>$A4:9E7B: exposed teeth/jaw and eroded face/neck. The disconnected
    /// upper tile00 at column9 is the native selected fragment, not an inferred correction.</summary>
    private static int Second(int row, int x) => row switch
    {
        0 => x switch { >= 0 and <= 3 => Strip(0x00, x, 0), 9 => 0x00, _ => -1 },
        1 => x switch { 0 => 0x40, 1 or 2 => Strip(0x11, x, 1), 3 => 0x50, 4 => 0x23, _ => -1 },
        2 => x switch { 1 or 2 => Strip(0x32, x, 1), 3 => 0x04, 4 => 0x54,
            5 => 0x03, 8 => 0x33, 9 => 0x01, 10 => 0x53, _ => -1 },
        3 => x switch { 3 or 4 => Strip(0x34, x, 3), 5 => 0x05, 6 => 0x24,
            7 => 0x14, >= 8 and <= 10 => Strip(0x02, x, 8), _ => -1 },
        4 => x switch { >= 4 and <= 6 => Strip(0x35, x, 4),
            >= 7 and <= 10 => Strip(row + 2, x, 7), 11 => 0x3a, _ => -1 },
        5 => x switch { 5 or 6 => Strip(0x46, x, 5), >= 7 and <= 11 => Strip(row + 2, x, 7), _ => -1 },
        6 => x switch { >= 1 and <= 3 => Strip(0x31, x, 1), 4 => 0x57,
            >= 5 and <= 10 => Strip(row + 2, x, 5), 11 => 0x09, _ => -1 },
        7 => x switch { 1 => 0x43, 2 => 0x52, 3 => 0x19, 4 => 0x13,
            >= 5 and <= 7 => Strip(0x39, x, 5), >= 8 and <= 11 => Strip(0x0a, x, 8), _ => -1 },
        _ => -1,
    };
}



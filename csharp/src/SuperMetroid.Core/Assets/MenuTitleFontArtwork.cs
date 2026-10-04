namespace SuperMetroid.Core.Assets;

/// <summary>PLANET ZEBES title glyph halves. Calculate index13 shadows one pixel down/right
/// from authored index14 silhouettes, including the shared upper/lower boundary established
/// by82:CBFB. Literal font silhouettes and independently painted deviations remain artwork;
/// tracing their chosen strokes in numerical cases would merely re-encode the typeface.
/// Stock retouches add shade at T-stem cell11(3,6) and clear N cell37(1,6); these
/// are the particular terminal/edge treatments, not samples of the cast-shadow rule.</summary>
internal sealed class MenuTitleFontArtwork
{
    internal const int TileCount = 18;
    private readonly byte[] faces = new byte[TileCount * 8];
    private readonly Dictionary<int, byte>? edits;
    internal int StoredFaceByteCount => faces.Length;
    internal int StoredEditCount => edits?.Count ?? 0;

    internal MenuTitleFontArtwork(IndexedPngImage image)
    {
        foreach (int tile in Tiles())
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
            if (Source(tile, x, y) == 14) faces[Index(tile) * 8 + y] |= (byte)(1 << x);
        foreach (int tile in Tiles())
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            byte pixel = Source(tile, x, y);
            if (pixel != Basis(tile, x, y)) (edits ??= []).Add(Index(tile) * 64 + y * 8 + x, pixel);
        }
        byte Source(int tile, int x, int y) => image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];
    }

    // Upper/lower cells for A, B, P, E, T, L, N, S and Z in the native title composition.
    internal static bool Contains(int tile) => tile is 0x0a or 0x0b or 0x0d or 0x0e or 0x11 or
        0x1a or 0x1b or 0x1e or 0x25 or 0x27 or 0x2b or 0x2c or 0x35 or 0x37 or 0x38 or 0x3b or 0x42 or 0x52;
    private static IEnumerable<int> Tiles()
    {
        for (int tile = 0; tile <= 0x52; tile++) if (Contains(tile)) yield return tile;
    }
    private static int Index(int tile)
    {
        int index = 0;
        for (int candidate = 0; candidate < tile; candidate++) if (Contains(candidate)) index++;
        return index;
    }
    private static int UpperHalf(int tile) => tile switch
    {
        0x1a => 0x0a, // A
        0x1b => 0x0b, // B
        0x1e => 0x0e, // E
        0x35 => 0x25, // L
        0x37 => 0x27, // N
        0x38 => 0x0d, // P
        0x3b => 0x2b, // S
        0x11 => 0x2c, // T
        0x52 => 0x42, // Z
        _ => -1,
    };
    internal byte Pixel(int tile, int x, int y)
    {
        if (!Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(Index(tile) * 64 + y * 8 + x, out byte pixel)
            ? pixel : Basis(tile, x, y);
    }

    private byte Basis(int tile, int x, int y)
    {
        int row = Index(tile) * 8 + y;
        if ((faces[row] & (1 << x)) != 0) return 14;
        if (x == 0) return 0;
        int upper = UpperHalf(tile);
        int precedingRow = y > 0 ? row - 1 : upper >= 0 ? Index(upper) * 8 + 7 : -1;
        return precedingRow >= 0 && (faces[precedingRow] & (1 << (x - 1))) != 0 ? (byte)13 : (byte)0;
    }
}

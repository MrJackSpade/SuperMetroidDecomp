namespace SuperMetroid.Core.Assets;

/// <summary>Reviewed large menu glyph halves and transparent tile0F. Calculate index13 shadows one pixel down/right
/// from authored index14 silhouettes, including the shared upper/lower boundary established
/// by82:CBFB. Literal font silhouettes and independently painted deviations remain artwork;
/// tracing their chosen strokes in numerical cases would merely re-encode the typeface.
/// Ten stock retouches shade T-stem/M/Q/X joins or clear N/W edges. These particular
/// terminal, counter and diagonal treatments are authored design, not cast-shadow samples.</summary>
internal sealed class MenuLargeFontArtwork
{
    internal const int TileCount = 40;
    private readonly byte[] faces = new byte[(TileCount - 1) * 8];
    private readonly Dictionary<int, byte>? edits;
    internal int StoredFaceByteCount => faces.Length;
    internal int StoredEditCount => edits?.Count ?? 0;

    internal MenuLargeFontArtwork(IndexedPngImage image)
    {
        foreach (int tile in Tiles())
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
            if (tile != 0x0f && Source(tile, x, y) == 14) faces[Index(tile) * 8 + y] |= (byte)(1 << x);
        foreach (int tile in Tiles())
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            byte pixel = Source(tile, x, y);
            if (pixel != Basis(tile, x, y)) (edits ??= []).Add(tile * 64 + y * 8 + x, pixel);
        }
        byte Source(int tile, int x, int y) => image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];
    }

    /// <summary>Reviewed large-letter cells and blank0F. Alternate stem17 and Y cells3E/41 remain unreviewed.</summary>
    internal static bool Contains(int tile) => tile is >= 0x0a and <= 0x0f or 0x11 or >= 0x1a and <= 0x1f or
        >= 0x21 and <= 0x27 or >= 0x2b and <= 0x2d or 0x2f or 0x30 or 0x31 or
        >= 0x33 and <= 0x3b or 0x3f or 0x40 or 0x42 or 0x50 or 0x52;
    private static IEnumerable<int> Tiles()
    {
        for (int tile = 0; tile <= 0x52; tile++) if (Contains(tile)) yield return tile;
    }
    private static int Index(int tile)
    {
        int index = 0;
        for (int candidate = 0; candidate < tile; candidate++) if (candidate != 0x0f && Contains(candidate)) index++;
        return index;
    }
    private static int UpperHalf(int tile) => tile switch
    {
        0x1a => 0x0a, // A
        0x1b => 0x0b, // B
        0x1c => 0x0c, // C
        0x1d => 0x0d, // D/O
        0x1f => 0x0e, // F
        0x30 => 0x0c, // G
        0x31 => 0x21, // H
        0x33 => 0x23, // J
        0x34 => 0x24, // K
        0x36 => 0x26, // M
        0x39 => 0x0d, // Q
        0x3a => 0x0d, // R
        0x3f => 0x2f, // W
        0x50 => 0x40, // X
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
        return edits is not null && edits.TryGetValue(tile * 64 + y * 8 + x, out byte pixel)
            ? pixel : Basis(tile, x, y);
    }

    private byte Basis(int tile, int x, int y)
    {
        if (tile == 0x0f) return 0;
        int row = Index(tile) * 8 + y;
        if ((faces[row] & (1 << x)) != 0) return 14;
        if (x == 0) return 0;
        int upper = UpperHalf(tile);
        int precedingRow = y > 0 ? row - 1 : upper >= 0 ? Index(upper) * 8 + 7 : -1;
        return precedingRow >= 0 && (faces[precedingRow] & (1 << (x - 1))) != 0 ? (byte)13 : (byte)0;
    }
}

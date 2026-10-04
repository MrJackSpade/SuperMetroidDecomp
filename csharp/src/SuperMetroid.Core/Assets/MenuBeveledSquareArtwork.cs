namespace SuperMetroid.Core.Assets;

/// <summary>Menu atlas98/99 atB6:D300..D33F are two colorways of a6x6 beveled
/// square inset one pixel in its8x8 cell. Bottom/right shading wins at their corners,
/// top/left highlight covers the remaining edges, and the interior is flat face color.
/// Tile98 uses face11/highlight14/shadow12; tile99 uses10/9/7. The role-based color
/// selection is explicit; no per-pixel table or generated tile cache remains.</summary>
internal sealed class MenuBeveledSquareArtwork
{
    internal const int TileCount = 2;
    private readonly Dictionary<int, byte>? edits;
    internal int StoredEditCount => edits?.Count ?? 0;
    internal static bool Contains(int tile) => tile is 0x98 or 0x99;

    internal MenuBeveledSquareArtwork(IndexedPngImage image)
    {
        for (int tile = 0x98; tile <= 0x99; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            byte pixel = image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];
            if (pixel != Basis(tile, x, y)) (edits ??= []).Add(tile * 64 + y * 8 + x, pixel);
        }
    }

    internal byte Pixel(int tile, int x, int y)
    {
        if (!Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + y * 8 + x, out byte pixel) ? pixel : Basis(tile, x, y);
    }

    private static byte Basis(int tile, int x, int y)
    {
        if (x is 0 or 7 || y is 0 or 7) return 0;
        (byte face, byte highlight, byte shadow) = tile == 0x98 ? ((byte)11, (byte)14, (byte)12) : ((byte)10, (byte)9, (byte)7);
        if (x == 6 || y == 6) return shadow;
        return x == 1 || y == 1 ? highlight : face;
    }
}

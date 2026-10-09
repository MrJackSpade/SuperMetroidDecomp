namespace SuperMetroid.Core.Assets;

/// <summary>Menu atlas98/99 atB6:D300..D33F are two colorways of a6x6 beveled
/// square inset one pixel in its8x8 cell. Bottom/right shading wins at their corners,
/// top/left highlight covers the remaining edges, and the interior is flat face color.
/// Tile98 uses face11/highlight14/shadow12; tile99 uses10/9/7. The role-based color
/// selection is explicit; no per-pixel table or generated tile cache remains.</summary>
internal sealed class MenuBeveledSquareArtwork
{
    /// <summary>Number of beveled-square colorway tiles represented by this artwork.</summary>
    internal const int TileCount = 2;

    /// <summary>Pixels that differ from the generated beveled basis, keyed by tile and cell position.</summary>
    private readonly Dictionary<int, byte>? edits;

    /// <summary>Reports whether a tile index selects either beveled-square colorway.</summary>
    /// <param name="tile">Menu atlas tile index to check.</param>
    /// <returns><see langword="true"/> for tile 0x98 or 0x99.</returns>
    internal static bool Contains(int tile) => tile is 0x98 or 0x99;

    /// <summary>Stores only source-image pixels that differ from the role-colored beveled basis.</summary>
    /// <param name="image">Menu atlas image containing the two beveled-square tiles.</param>
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

    /// <summary>Returns a tile pixel, using a sparse source edit when present and the beveled basis otherwise.</summary>
    /// <param name="tile">Menu atlas tile index, either 0x98 or 0x99.</param>
    /// <param name="x">Horizontal pixel coordinate within the eight-pixel tile.</param>
    /// <param name="y">Vertical pixel coordinate within the eight-pixel tile.</param>
    /// <returns>The source-edited pixel or the corresponding face, highlight, shadow, or transparent basis color.</returns>
    internal byte Pixel(int tile, int x, int y)
    {
        if (!Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + y * 8 + x, out byte pixel) ? pixel : Basis(tile, x, y);
    }

    /// <summary>Calculates the transparent border, shaded lower/right edge, and highlighted upper/left edge.</summary>
    /// <param name="tile">Colorway tile index selecting its face, highlight, and shadow palette roles.</param>
    /// <param name="x">Horizontal coordinate within the eight-pixel tile.</param>
    /// <param name="y">Vertical coordinate within the eight-pixel tile.</param>
    /// <returns>The palette index assigned to this position in the beveled-square basis.</returns>
    private static byte Basis(int tile, int x, int y)
    {
        if (x is 0 or 7 || y is 0 or 7) return 0;
        (byte face, byte highlight, byte shadow) = tile == 0x98 ? ((byte)11, (byte)14, (byte)12) : ((byte)10, (byte)9, (byte)7);
        if (x == 6 || y == 6) return shadow;
        return x == 1 || y == 1 ? highlight : face;
    }
}

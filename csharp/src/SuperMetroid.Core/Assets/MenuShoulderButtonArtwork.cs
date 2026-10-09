namespace SuperMetroid.Core.Assets;

/// <summary>Pressed L/R buttons at82:C48F/C4A0 use left28, middle29/2E and right2A.
/// The24x7 pill has radius-three caps rounded to nearest pixels. Index4 shades the
/// right edge and bottom; its lower endpoint extends to meet the preceding edge
/// diagonally. Only the two particular four-row L/R glyph drawings remain artwork.
/// Replacing those strokes with coordinate cases would merely disguise the font.</summary>
internal sealed class MenuShoulderButtonArtwork
{
    /// <summary>Number of nonblank tiles used to compose the left and right shoulder-button pills.</summary>
    internal const int TileCount = 4;

    /// <summary>Four-by-four palette-4 glyph masks captured from the L and R artwork for their respective button labels.</summary>
    private readonly ushort leftLetter, rightLetter;

    /// <summary>Sparse source pixels that differ from the procedural pill and glyph basis.</summary>
    private readonly Dictionary<int, byte>? edits;

    /// <summary>Identifies the four tiles that contribute pixels to the shoulder-button graphic.</summary>
    internal static bool Contains(int tile) => tile is >= 0x28 and <= 0x2a or 0x2e;

    /// <summary>Captures the L/R glyph masks and records authored pixels that the procedural button shape does not reproduce.</summary>
    /// <param name="image">Indexed source atlas containing the shoulder-button tiles and their letter artwork.</param>
    internal MenuShoulderButtonArtwork(IndexedPngImage image)
    {
        leftLetter = ReadLetter(0x29);
        rightLetter = ReadLetter(0x2e);
        for (int tile = 0x28; tile <= 0x2e; tile++)
        {
            if (!Contains(tile)) continue;
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                byte pixel = Source(tile, x, y);
                if (pixel != Basis(tile, x, y)) (edits ??= []).Add(tile * 64 + y * 8 + x, pixel);
            }
        }
        ushort ReadLetter(int tile)
        {
            ushort bits = 0;
            for (int y = 1; y <= 4; y++)
            for (int x = 2; x <= 5; x++)
                if (Source(tile, x, y) == 4) bits |= (ushort)(1 << ((y - 1) * 4 + x - 2));
            return bits;
        }
        byte Source(int tile, int x, int y) => image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];
    }

    /// <summary>Gets the final palette index for one button tile pixel, preserving recorded artwork over the generated basis.</summary>
    /// <param name="tile">Tile ID belonging to the shoulder-button graphic.</param>
    /// <param name="x">Horizontal coordinate within the 8-by-8 tile.</param>
    /// <param name="y">Vertical coordinate within the 8-by-8 tile.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tile is outside the button set or either coordinate is outside the tile.</exception>
    internal byte Pixel(int tile, int x, int y)
    {
        if (!Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + y * 8 + x, out byte pixel) ? pixel : Basis(tile, x, y);
    }

    /// <summary>Calculates the rounded pill fill, edge shading, and captured L/R glyph from the tile-local coordinates.</summary>
    /// <param name="tile">Button tile whose portion of the pill is being generated.</param>
    /// <param name="x">Horizontal pixel coordinate within the tile.</param>
    /// <param name="y">Vertical pixel coordinate within the tile.</param>
    /// <returns>Palette index 3 for the pill fill, 4 for its edge or letter, or 0 for transparency.</returns>
    private byte Basis(int tile, int x, int y)
    {
        if (y == 7) return 0;
        if (tile is 0x29 or 0x2e && x is >= 2 and <= 5 && y is >= 1 and <= 4)
        {
            ushort glyph = tile == 0x29 ? leftLetter : rightLetter;
            if ((glyph & (1 << ((y - 1) * 4 + x - 2))) != 0) return 4;
        }
        int position = x + (tile == 0x28 ? 0 : tile == 0x2a ? 16 : 8);
        int left = Inset(y), right = 23 - left;
        if (y == 6) right = Math.Max(right, 23 - Inset(y - 1) - 1);
        if (position < left || position > right) return 0;
        return y == 6 || y > 0 && position == right ? (byte)4 : (byte)3;
    }

    /// <summary>3-round(sqrt(9-(y-3)^2)), for rows0..6. Integer half-pixel
    /// thresholds give exact nearest rounding without floating-point evaluation.</summary>
    private static int Inset(int y)
    {
        int dy = y - 3, squaredRadius = 9 - dy * dy, extent = 0;
        while (extent < 3 && (2 * extent + 1) * (2 * extent + 1) <= 4 * squaredRadius) extent++;
        return 3 - extent;
    }
}

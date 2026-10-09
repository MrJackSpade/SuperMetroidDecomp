namespace SuperMetroid.Core.Assets;

/// <summary>Native82:C465 builds the32x16 L/R highlight from corner3C/51 and
/// body3D/43, mirroring the right corners. The outer frame occupies X1..30,Y1..14.
/// Its symmetric cutout spans rows4..12 with left edge3+floor((Y-8)^2/4), a
/// quadratic cap four pixels wide/high; mirror it for the right edge. Index6 fills
/// the remaining frame. No original pixel data or generated contour cache is kept.</summary>
internal sealed class MenuShoulderHighlightArtwork
{
    /// <summary>Number of authored tiles participating in the shoulder-button highlight composition.</summary>
    internal const int TileCount = 4;
    /// <summary>Sparse pixel overrides where the editable PNG differs from the generated geometric basis.</summary>
    private readonly Dictionary<int, byte>? edits;

    /// <summary>Checks whether a tile index is one of the four shoulder-highlight corner or body tiles.</summary>
    /// <param name="tile">Tile index within the menu artwork atlas.</param>
    /// <returns>True for the two corner tiles and their vertically mirrored body tiles.</returns>
    internal static bool Contains(int tile) => tile is 0x3c or 0x3d or 0x43 or 0x51;

    /// <summary>Captures authored pixels that differ from the procedural highlight shape.</summary>
    /// <param name="image">Indexed artwork image containing the four source tiles in the menu atlas layout.</param>
    internal MenuShoulderHighlightArtwork(IndexedPngImage image)
    {
        for (int tile = 0x3c; tile <= 0x51; tile++)
        {
            if (!Contains(tile)) continue;
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                byte pixel = image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];
                if (pixel != Basis(tile, x, y)) (edits ??= []).Add(tile * 64 + y * 8 + x, pixel);
            }
        }
    }

    /// <summary>Returns an authored pixel override or the generated basis pixel for a tile coordinate.</summary>
    /// <param name="tile">One of the four supported shoulder-highlight tile indexes.</param>
    /// <param name="x">Horizontal pixel coordinate within the 8-by-8 tile.</param>
    /// <param name="y">Vertical pixel coordinate within the 8-by-8 tile.</param>
    /// <returns>The palette index selected for that pixel.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The tile is unsupported or either coordinate is outside the tile.</exception>
    internal byte Pixel(int tile, int x, int y)
    {
        if (!Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + y * 8 + x, out byte pixel) ? pixel : Basis(tile, x, y);
    }

    /// <summary>Calculates the mirrored frame and quadratic cutout used when no authored pixel override is present.</summary>
    /// <param name="tile">Supported source tile whose corner/body position determines the mirrored basis region.</param>
    /// <param name="x">Horizontal pixel coordinate within the tile.</param>
    /// <param name="y">Vertical pixel coordinate within the tile.</param>
    /// <returns>Palette index 6 for the highlight fill or 0 for the transparent cutout.</returns>
    private static byte Basis(int tile, int x, int y)
    {
        int row = y + (tile is 0x43 or 0x51 ? 8 : 0);
        int column = x + (tile is 0x3d or 0x43 ? 8 : 0);
        if (row is 0 or 15 || column == 0) return 0;
        if (row < 4 || row > 12) return 6;
        int distance = row - 8;
        return column < 3 + distance * distance / 4 ? (byte)6 : (byte)0;
    }
}

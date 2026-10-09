namespace SuperMetroid.Core.Assets;

/// <summary>Menu OBJ cells57..5A atB6:CAE0..CB5F form a sixteen-row panel edge:
/// upper corner/body then lower corner/body. Calculate outer index3, inner index6
/// and lower highlight8 bands directly; only independently supplied pixel edits remain.</summary>
internal sealed class MenuPanelTileArtwork
{
    /// <summary>Number of menu-panel tiles represented by this artwork view.</summary>
    internal const int TileCount = 4;
    /// <summary>Pixel overrides for cells that differ from the procedurally calculated panel basis.</summary>
    private readonly Dictionary<int, byte>? edits;
    /// <summary>Checks whether a tile index belongs to the four-cell menu-panel strip.</summary>
    /// <param name="tile">OBJ tile index to test.</param>
    /// <returns><see langword="true"/> for tile indices 0x57 through 0x5A.</returns>
    internal static bool Contains(int tile) => tile is >= 0x57 and <= 0x5a;

    /// <summary>Captures only source pixels that differ from the generated panel pattern.</summary>
    /// <param name="image">Indexed artwork atlas containing the menu-panel tiles.</param>
    internal MenuPanelTileArtwork(IndexedPngImage image)
    {
        for (int tile = 0x57; tile <= 0x5a; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            byte pixel = image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];
            if (pixel != Basis(tile, x, y)) (edits ??= []).Add(tile * 64 + y * 8 + x, pixel);
        }
    }

    /// <summary>Gets a panel pixel from its stored override or the generated basis pattern.</summary>
    /// <param name="tile">OBJ tile index in the supported panel strip.</param>
    /// <param name="x">Zero-based horizontal pixel coordinate within the tile.</param>
    /// <param name="y">Zero-based vertical pixel coordinate within the tile.</param>
    /// <returns>The indexed color at the requested tile pixel.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The tile is outside the panel strip or either pixel coordinate is outside the 8-by-8 tile.</exception>
    internal byte Pixel(int tile, int x, int y)
    {
        if (!Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + y * 8 + x, out byte pixel) ? pixel : Basis(tile, x, y);
    }

    /// <summary>Calculates the shared corner-and-edge pattern used when no source pixel override exists.</summary>
    /// <param name="tile">OBJ tile index in the panel strip.</param>
    /// <param name="x">Zero-based horizontal pixel coordinate within the tile.</param>
    /// <param name="y">Zero-based vertical pixel coordinate within the tile.</param>
    /// <returns>The basis palette index for that pixel.</returns>
    private static byte Basis(int tile, int x, int y)
    {
        int row = y + (tile >= 0x59 ? 8 : 0);
        if (row is 0 or 15) return 0;
        if (row is 1 or 2 or 14) return 3;
        bool leftCorner = ((tile - 0x57) & 1) == 0;
        if (leftCorner && x == 0) return 3;
        if (leftCorner && x == 1 || row is 3 or 13) return 6;
        return row == 12 ? (byte)8 : (byte)0;
    }
}

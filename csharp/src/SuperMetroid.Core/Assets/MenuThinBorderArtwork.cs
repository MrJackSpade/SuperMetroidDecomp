namespace SuperMetroid.Core.Assets;

/// <summary>Menu atlas cellsAD/AE atB6:D5A0..D5DF are a horizontal line and its
/// left corner. Index6 covers row1; the corner also covers column0 below that row.
/// All other pixels are transparent. Calculate these lines directly, retaining only
/// independently supplied artwork differences, never a generated tile cache.</summary>
internal sealed class MenuThinBorderArtwork
{
    /// <summary>Number of menu atlas tiles whose pixels are interpreted as the thin border line and corner.</summary>
    internal const int TileCount = 2;
    /// <summary>Sparse replacement pixels retained only when supplied artwork differs from the calculated border shapes.</summary>
    private readonly Dictionary<int, byte>? edits;

    /// <summary>Reports whether an atlas tile is one of the two menu thin-border pieces.</summary>
    internal static bool Contains(int tile) => tile is 0xad or 0xae;

    /// <summary>Captures differences from the calculated line and corner pixels for the two selected atlas tiles.</summary>
    /// <param name="image">Indexed atlas image containing menu tiles $AD and $AE.</param>
    internal MenuThinBorderArtwork(IndexedPngImage image)
    {
        for (int tile = 0xad; tile <= 0xae; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            byte pixel = image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];
            if (pixel != Basis(tile, x, y)) (edits ??= []).Add(tile * 64 + y * 8 + x, pixel);
        }
    }

    /// <summary>Returns the supplied replacement pixel when present, otherwise the calculated thin-border pixel.</summary>
    /// <param name="tile">Menu atlas tile $AD for the horizontal line or $AE for its left corner.</param>
    /// <param name="x">Pixel column within the eight-pixel tile.</param>
    /// <param name="y">Pixel row within the eight-pixel tile.</param>
    /// <returns>The indexed pixel value at the requested tile coordinate.</returns>
    internal byte Pixel(int tile, int x, int y)
    {
        if (!Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + y * 8 + x, out byte pixel) ? pixel : Basis(tile, x, y);
    }

    /// <summary>Calculates the unedited border mask: palette index six across row one and down the corner's left edge.</summary>
    private static byte Basis(int tile, int x, int y) =>
        y == 1 || tile == 0xae && x == 0 && y > 1 ? (byte)6 : (byte)0;
}

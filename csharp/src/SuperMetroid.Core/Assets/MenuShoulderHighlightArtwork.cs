namespace SuperMetroid.Core.Assets;

/// <summary>Native82:C465 builds the32x16 L/R highlight from corner3C/51 and
/// body3D/43, mirroring the right corners. The outer frame occupies X1..30,Y1..14.
/// Its symmetric cutout spans rows4..12 with left edge3+floor((Y-8)^2/4), a
/// quadratic cap four pixels wide/high; mirror it for the right edge. Index6 fills
/// the remaining frame. No original pixel data or generated contour cache is kept.</summary>
internal sealed class MenuShoulderHighlightArtwork
{
    internal const int TileCount = 4;
    private readonly Dictionary<int, byte>? edits;
    internal static bool Contains(int tile) => tile is 0x3c or 0x3d or 0x43 or 0x51;

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

    internal byte Pixel(int tile, int x, int y)
    {
        if (!Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + y * 8 + x, out byte pixel) ? pixel : Basis(tile, x, y);
    }

    private static byte Basis(int tile, int x, int y)
    {
        int row = y + (tile is 0x43 or 0x51 ? 8 : 0);
        int column = x + (tile is 0x3d or 0x43 ? 8 : 0);
        if (row is 0 or 15 || column == 0) return 0;
        if (row is < 4 or > 12) return 6;
        int distance = row - 8;
        return column < 3 + distance * distance / 4 ? (byte)6 : (byte)0;
    }
}

namespace SuperMetroid.Core.Assets;

/// <summary>Menu atlas cellsAD/AE atB6:D5A0..D5DF are a horizontal line and its
/// left corner. Index6 covers row1; the corner also covers column0 below that row.
/// All other pixels are transparent. Calculate these lines directly, retaining only
/// independently supplied artwork differences, never a generated tile cache.</summary>
internal sealed class MenuThinBorderArtwork
{
    internal const int TileCount = 2;
    private readonly Dictionary<int, byte>? edits;
    internal int StoredEditCount => edits?.Count ?? 0;
    internal static bool Contains(int tile) => tile is 0xad or 0xae;

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

    internal byte Pixel(int tile, int x, int y)
    {
        if (!Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + y * 8 + x, out byte pixel) ? pixel : Basis(tile, x, y);
    }

    private static byte Basis(int tile, int x, int y) =>
        y == 1 || tile == 0xae && x == 0 && y > 1 ? (byte)6 : (byte)0;
}

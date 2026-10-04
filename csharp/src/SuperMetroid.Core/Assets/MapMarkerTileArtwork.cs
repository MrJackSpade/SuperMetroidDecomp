namespace SuperMetroid.Core.Assets;

/// <summary>Menu atlas9D/9E are transposed triangular arrow halves,9F is an
/// outlined defeated-boss diagonal/frame corner, andAF is a three-line pulse corner.
/// Native map compositions mirror these halves/corners (82:C232..C262,C298,C38C..C3A8,
/// CF7C..CFE0). X/Y are0..7. Arrow9E fills X&lt;=Y with11, using14 at X0 or X=Y;
/// 9D transposes X/Y. Pulse uses min(X,Y):2/4 black15,3 bright14, otherwise0.
/// Boss uses black outer X0/Y0, red10 at X1/Y1 and diagonals X=Y or Y-1,
/// then black inner X2/Y2 and diagonals X=Y+1 or Y-2; other pixels are0.
/// Calculate every original pixel directly; keep only supplied edits.</summary>
internal sealed class MapMarkerTileArtwork
{
    internal const int TileCount = 4;
    private readonly Dictionary<int, byte>? edits;
    internal int StoredEditCount => edits?.Count ?? 0;
    internal static bool Contains(int tile) => tile is >= 0x9d and <= 0x9f or 0xaf;

    internal MapMarkerTileArtwork(IndexedPngImage image)
    {
        for (int tile = 0x9d; tile <= 0xaf; tile++)
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
        if (tile == 0xaf)
        {
            // Nested right angles: outer black, bright center, inner black.
            int inset = Math.Min(x, y);
            return inset is 2 or 4 ? (byte)15 : inset == 3 ? (byte)14 : (byte)0;
        }
        if (tile == 0x9f)
        {
            if (x == 0 || y == 0) return 15;
            if (x == 1 || y == 1 || x == y || x == y - 1) return 10;
            return x == 2 || y == 2 || x == y + 1 || x == y - 2 ? (byte)15 : (byte)0;
        }
        if (tile == 0x9d) (x, y) = (y, x);
        if (x > y) return 0;
        return x == 0 || x == y ? (byte)14 : (byte)11;
    }
}

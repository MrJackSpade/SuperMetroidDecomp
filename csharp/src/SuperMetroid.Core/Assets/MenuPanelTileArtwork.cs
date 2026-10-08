namespace SuperMetroid.Core.Assets;

/// <summary>Menu OBJ cells57..5A atB6:CAE0..CB5F form a sixteen-row panel edge:
/// upper corner/body then lower corner/body. Calculate outer index3, inner index6
/// and lower highlight8 bands directly; only independently supplied pixel edits remain.</summary>
internal sealed class MenuPanelTileArtwork
{
    internal const int TileCount = 4;
    private readonly Dictionary<int, byte>? edits;
    internal static bool Contains(int tile) => tile is >= 0x57 and <= 0x5a;

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

    internal byte Pixel(int tile, int x, int y)
    {
        if (!Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + y * 8 + x, out byte pixel) ? pixel : Basis(tile, x, y);
    }

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

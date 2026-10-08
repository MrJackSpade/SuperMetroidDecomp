namespace SuperMetroid.Core.Assets;

/// <summary>
/// Menu font tiles60..88 atB6:CC00..D11F. Authored letter silhouettes use index14;
/// index13 shadows those silhouettes one pixel down/right, behind foreground pixels.
/// Join touching upper/left ink corners with13. Continue an existing upper-left diagonal
/// shadow into a notch bounded by left/right/lower ink with both upper diagonals open;
/// a bare notch or a vertical-sided gap is not filled.
/// These rules calculate all six extra0/M/Q/V shading pixels. Preserve three particular
/// B/K drawing choices: B bevels its lower-left shadow where D's identical lower ink
/// contour does not; K trims its descending arm where Z keeps shadows on matching local
/// ink. A glyph/coordinate switch for these choices would only recite the artwork.
/// The chosen silhouettes and these particular trims are art; supplied differences remain editable.
/// </summary>
internal sealed class MenuSmallFontArtwork
{
    internal const int FirstTile = 0x60, TileCount = 41;
    private readonly byte[] faces = new byte[TileCount * 8];
    private readonly Dictionary<int, byte>? edits;

    internal MenuSmallFontArtwork(IndexedPngImage image)
    {
        for (int tile = FirstTile; tile < FirstTile + TileCount; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
            if (Source(tile, x, y) == 14) faces[(tile - FirstTile) * 8 + y] |= (byte)(1 << x);
        for (int tile = FirstTile; tile < FirstTile + TileCount; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            byte pixel = Source(tile, x, y);
            if (pixel != Basis(tile, x, y)) (edits ??= []).Add((tile - FirstTile) * 64 + y * 8 + x, pixel);
        }
        byte Source(int tile, int x, int y) => image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];
    }

    internal static bool Contains(int tile) => tile >= FirstTile && tile < FirstTile + TileCount;
    internal byte Pixel(int tile, int x, int y)
    {
        if (!Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue((tile - FirstTile) * 64 + y * 8 + x, out byte pixel)
            ? pixel : Basis(tile, x, y);
    }

    private byte Basis(int tile, int x, int y)
    {
        int row = (tile - FirstTile) * 8 + y;
        if ((faces[row] & (1 << x)) != 0) return 14;
        if (Ink(x - 1, y - 1)) return 13;
        if (Ink(x - 1, y) && (Ink(x, y - 1) ||
            Ink(x + 1, y) && Ink(x, y + 1) && !Ink(x + 1, y - 1) && Ink(x - 2, y - 2))) return 13;
        return 0;

        // The two-step ink source establishes an incoming diagonal shadow at the
        // upper-left neighbor; ordinary one-step shadow/foreground already won above.
        bool Ink(int px, int py) => (uint)px < 8 && (uint)py < 8 &&
            (faces[(tile - FirstTile) * 8 + py] & (1 << px)) != 0;
    }
}

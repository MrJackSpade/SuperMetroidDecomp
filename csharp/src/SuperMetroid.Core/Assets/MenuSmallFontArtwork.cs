namespace SuperMetroid.Core.Assets;

/// <summary>
/// Menu font tiles60..88 atB6:CC00..D11F. Authored letter silhouettes use index14;
/// index13 shadows those silhouettes one pixel down/right, behind foreground pixels.
/// Nine original retouches add shading inside0/M/Q/V or remove edge shading fromB/K.
/// Silhouettes and those particular retouches define the chosen font design: reciting
/// their strokes in numerical cases would disguise the artwork, not derive a font.
/// All other shadow/transparent pixels are calculated, with independent edits preserved.
/// </summary>
internal sealed class MenuSmallFontArtwork
{
    internal const int FirstTile = 0x60, TileCount = 41;
    private readonly byte[] faces = new byte[TileCount * 8];
    private readonly Dictionary<int, byte>? edits;
    internal int StoredFaceByteCount => faces.Length;
    internal int StoredEditCount => edits?.Count ?? 0;

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
        return x > 0 && y > 0 && (faces[row - 1] & (1 << (x - 1))) != 0 ? (byte)13 : (byte)0;
    }
}

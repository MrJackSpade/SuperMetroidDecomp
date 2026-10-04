using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Map OBJ sheet with procedural reserve bars and selector borders and independently authored pixel edits.</summary>
internal sealed class MapObjectTileArtwork
{
    /// <summary>$B6:C8E0..C9FF, menu tiles47..4F: six partial bars, empty, full and cap.</summary>
    internal const int FirstReserveTile = 0x47, ReserveTileCount = 9;
    /// <summary>$B6:CB60..CBDf, tiles5B..5E: selector left corner, horizontal line and right ends.</summary>
    internal const int FirstHighlightTile = 0x5b, HighlightTileCount = 4;
    /// <summary>Shared reserve cursor46 atB6:C8C0, reused in the left selector corner with swapped colors1/2.</summary>
    private const int CursorTile = 0x46;
    private readonly byte[] otherCharacters;
    private readonly Dictionary<int, byte>? reserveEdits;
    private readonly Dictionary<int, byte>? highlightEdits;
    internal int StoredHighlightPixelCount => highlightEdits?.Count ?? 0;
    internal int StoredReservePixelCount => reserveEdits?.Count ?? 0;
    internal int StoredOtherByteCount => otherCharacters.Length;

    internal MapObjectTileArtwork(IndexedPngImage image)
    {
        byte[] encoded = SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4);
        otherCharacters = new byte[encoded.Length - (ReserveTileCount + HighlightTileCount) * 32];
        int stored = 0;
        for (int tile = 0; tile < encoded.Length / 32; tile++)
        {
            if (IsReserve(tile) || IsHighlight(tile)) continue;
            encoded.AsSpan(tile * 32, 32).CopyTo(otherCharacters.AsSpan(stored, 32));
            stored += 32;
        }
        for (int tile = FirstReserveTile; tile < FirstReserveTile + ReserveTileCount; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            byte pixel = image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];
            if (pixel != ReservePixel(tile, x, y))
                (reserveEdits ??= new()).Add((tile - FirstReserveTile) * 64 + y * 8 + x, pixel);
        }
        for (int tile = FirstHighlightTile; tile < FirstHighlightTile + HighlightTileCount; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            byte pixel = image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];
            if (pixel != HighlightPixel(tile, x, y))
                (highlightEdits ??= new()).Add((tile - FirstHighlightTile) * 64 + y * 8 + x, pixel);
        }
    }

    private static bool IsReserve(int tile) => tile >= FirstReserveTile && tile < FirstReserveTile + ReserveTileCount;
    private static bool IsHighlight(int tile) => tile >= FirstHighlightTile && tile < FirstHighlightTile + HighlightTileCount;

    /// <summary>Border index12; left corner reuses cursor46 shifted right with colors1/2 exchanged.
    /// Source cursor edits are compensated by independent corner-pixel edits captured at import.</summary>
    internal byte HighlightPixel(int tile, int x, int y)
    {
        if (!IsHighlight(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        if (tile == FirstHighlightTile)
        {
            if (x == 0 || y == 0) return 12;
            int pixel = 0;
            for (int plane = 0; plane < 4; plane++)
                pixel |= ((otherCharacters[CursorTile * 32 + plane / 2 * 16 + y * 2 + plane % 2] >> (8 - x)) & 1) << plane;
            return (byte)(pixel == 1 ? 2 : pixel == 2 ? 1 : pixel);
        }
        if (tile == FirstHighlightTile + 1) return y == 0 ? (byte)12 : (byte)0;
        return (y == 0 && x < 2) || (tile == FirstHighlightTile + 2 && x == 1) ? (byte)12 : (byte)0;
    }
    /// <summary>Calculates the native border, fill and empty-interior indices of one reserve pixel.</summary>
    internal static byte ReservePixel(int tile, int x, int y)
    {
        if ((uint)(tile - FirstReserveTile) >= ReserveTileCount || (uint)x >= 8 || (uint)y >= 8)
            throw new ArgumentOutOfRangeException(nameof(tile));
        if (y < 2) return 0;
        if (tile == FirstReserveTile + 8) return x == 0 ? (byte)5 : (byte)0;
        if (y is 2 or 7 || x == 0) return 5;
        int fill = tile < FirstReserveTile + 6 ? tile - FirstReserveTile + 1 : tile == FirstReserveTile + 6 ? 0 : 7;
        return x <= fill ? (byte)9 : (byte)14;
    }

    internal void LoadTo(SnesVram vram, int destinationByte)
    {
        // This is the immediate transfer payload, never a retained calculated lookup.
        Span<byte> transfer = stackalloc byte[MapSpriteFormat.ByteCount];
        int stored = 0;
        for (int tile = 0; tile < MapSpriteFormat.ByteCount / 32; tile++)
        {
            bool reserve = IsReserve(tile);
            if (!reserve && !IsHighlight(tile))
            {
                otherCharacters.AsSpan(stored, 32).CopyTo(transfer.Slice(tile * 32, 32));
                stored += 32;
                continue;
            }
            var edits = reserve ? reserveEdits : highlightEdits;
            int first = reserve ? FirstReserveTile : FirstHighlightTile;
            for (int y = 0; y < 8; y++)
            for (int plane = 0; plane < 4; plane++)
            {
                byte encoded = 0;
                for (int x = 0; x < 8; x++)
                {
                    int key = (tile - first) * 64 + y * 8 + x;
                    byte pixel = edits is not null && edits.TryGetValue(key, out byte authored)
                        ? authored : reserve ? ReservePixel(tile, x, y) : HighlightPixel(tile, x, y);
                    encoded |= (byte)(((pixel >> plane) & 1) << (7 - x));
                }
                transfer[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] = encoded;
            }
        }
        vram.LoadBytes(destinationByte, transfer);
    }
}

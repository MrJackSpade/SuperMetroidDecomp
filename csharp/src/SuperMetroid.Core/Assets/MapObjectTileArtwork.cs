using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Map OBJ sheet with procedural reserve bars and independently authored pixel edits.</summary>
internal sealed class MapObjectTileArtwork
{
    /// <summary>$B6:C8E0..C9FF, menu tiles47..4F: six partial bars, empty, full and cap.</summary>
    internal const int FirstReserveTile = 0x47, ReserveTileCount = 9;
    private const int ReserveByteStart = FirstReserveTile * 32, ReserveByteCount = ReserveTileCount * 32;
    private readonly byte[] otherCharacters;
    private readonly Dictionary<int, byte>? reserveEdits;
    internal int StoredReservePixelCount => reserveEdits?.Count ?? 0;
    internal int StoredOtherByteCount => otherCharacters.Length;

    internal MapObjectTileArtwork(IndexedPngImage image)
    {
        byte[] encoded = SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4);
        otherCharacters = new byte[encoded.Length - ReserveByteCount];
        encoded.AsSpan(0, ReserveByteStart).CopyTo(otherCharacters);
        encoded.AsSpan(ReserveByteStart + ReserveByteCount).CopyTo(otherCharacters.AsSpan(ReserveByteStart));
        for (int tile = FirstReserveTile; tile < FirstReserveTile + ReserveTileCount; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            byte pixel = image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];
            if (pixel != ReservePixel(tile, x, y))
                (reserveEdits ??= new()).Add((tile - FirstReserveTile) * 64 + y * 8 + x, pixel);
        }
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
        otherCharacters.AsSpan(0, ReserveByteStart).CopyTo(transfer);
        otherCharacters.AsSpan(ReserveByteStart).CopyTo(transfer[(ReserveByteStart + ReserveByteCount)..]);
        for (int tile = FirstReserveTile; tile < FirstReserveTile + ReserveTileCount; tile++)
        for (int y = 0; y < 8; y++)
        for (int plane = 0; plane < 4; plane++)
        {
            byte encoded = 0;
            for (int x = 0; x < 8; x++)
            {
                int key = (tile - FirstReserveTile) * 64 + y * 8 + x;
                byte pixel = reserveEdits is not null && reserveEdits.TryGetValue(key, out byte authored)
                    ? authored : ReservePixel(tile, x, y);
                encoded |= (byte)(((pixel >> plane) & 1) << (7 - x));
            }
            transfer[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] = encoded;
        }
        vram.LoadBytes(destinationByte, transfer);
    }
}

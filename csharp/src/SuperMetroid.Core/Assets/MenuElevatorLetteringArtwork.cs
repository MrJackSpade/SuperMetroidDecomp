namespace SuperMetroid.Core.Assets;

/// <summary>
/// Six elevator-label strips, selected by82:C4DB..C568. Authored ink index11 casts
/// a one-pixel eight-neighbor black outline15 across adjacent cells of the same strip.
/// Authored ink silhouettes select the lettering design; tracing their chosen strokes
/// in numerical cases would disguise that art. Thirty transparent corner/notch trims
/// remain explicit inputs pending their own geometric review, not an accepted exception.
/// </summary>
internal sealed class MenuElevatorLetteringArtwork
{
    internal const int TileCount = 22;
    private readonly byte[] ink = new byte[TileCount * 8];
    private readonly Dictionary<int, byte>? edits;
    internal int StoredInkByteCount => ink.Length;
    internal int StoredEditCount => edits?.Count ?? 0;

    internal static bool Contains(int tile) => tile is >= 0 and <= 7 or 0x10 or >= 0x12 and <= 0x16 or
        0x18 or 0x19 or 0x44 or 0x45 or >= 0x53 and <= 0x56;

    /// <summary>Physical strip layout: Crateria, Brinstar, Norfair, Maridia, Ship and Wrecked.
    /// Norfair and Maridia skip the intervening large-font stem cells11 and17.</summary>
    private static (int Start, int Count, int Gap, int Offset) Layout(int tile) => tile switch
    {
        >= 0 and <= 3 => (0, 4, -1, 0),
        >= 4 and <= 7 => (4, 4, -1, 4),
        0x10 or >= 0x12 and <= 0x14 => (0x10, 4, 0x11, 8),
        0x15 or 0x16 or 0x18 or 0x19 => (0x15, 4, 0x17, 12),
        0x44 or 0x45 => (0x44, 2, -1, 16),
        >= 0x53 and <= 0x56 => (0x53, 4, -1, 18),
        _ => throw new ArgumentOutOfRangeException(nameof(tile)),
    };

    internal MenuElevatorLetteringArtwork(IndexedPngImage image)
    {
        for (int tile = 0; tile <= 0x56; tile++)
        {
            if (!Contains(tile)) continue;
            var row = Layout(tile);
            int column = tile - row.Start - (row.Gap >= 0 && tile > row.Gap ? 1 : 0);
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                if (Source(tile, x, y) == 11) ink[(row.Offset + column) * 8 + y] |= (byte)(1 << x);
        }
        for (int tile = 0; tile <= 0x56; tile++)
        {
            if (!Contains(tile)) continue;
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                byte pixel = Source(tile, x, y);
                if (pixel != Basis(tile, x, y)) (edits ??= []).Add(tile * 64 + y * 8 + x, pixel);
            }
        }
        byte Source(int tile, int x, int y) => image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];
    }

    internal byte Pixel(int tile, int x, int y)
    {
        if (!Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + y * 8 + x, out byte pixel) ? pixel : Basis(tile, x, y);
    }

    private byte Basis(int tile, int x, int y)
    {
        var row = Layout(tile);
        int column = tile - row.Start - (row.Gap >= 0 && tile > row.Gap ? 1 : 0);
        int position = column * 8 + x;
        if (IsInk(position, y)) return 11;
        for (int dy = -1; dy <= 1; dy++)
        for (int dx = -1; dx <= 1; dx++)
            if (IsInk(position + dx, y + dy)) return 15;
        return 0;

        bool IsInk(int px, int py) => (uint)px < row.Count * 8 && (uint)py < 8 &&
            (ink[(row.Offset + px / 8) * 8 + py] & (1 << (px % 8))) != 0;
    }
}

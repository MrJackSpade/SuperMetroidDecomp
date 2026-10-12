using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Assets;

/// <summary>Menu atlas cellsAD/AE atB6:D5A0..D5DF are a horizontal line and its
/// left corner. Index6 covers row1; the corner also covers column0 below that row.
/// All other pixels are transparent. Calculate these lines directly, retaining only
/// independently supplied artwork differences, never a generated tile cache.</summary>
internal sealed class MenuThinBorderArtwork
{
    /// <summary>Stores tile count.</summary>
    internal const int TileCount = 2;
    /// <summary>Stores edits.</summary>
    private readonly Dictionary<int, byte>? edits;
    /// <summary>Contains.</summary>
    internal static bool Contains(int tile) => tile is 0xad or 0xae;

    /// <summary>Initializes a new menu thin border artwork instance.</summary>
    internal MenuThinBorderArtwork(IndexedPngImage image)
    {
        for (int tile = 0xad; tile <= 0xae; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            byte pixel = image.Pixels[(tile / 16 * 8 + y) * image.Size.X + tile % 16 * 8 + x];
            if (pixel != Basis(tile, new(checked((ushort)x), checked((ushort)y)))) (edits ??= []).Add(tile * 64 + y * 8 + x, pixel);
        }
    }

    /// <summary>Pixel.</summary>
    internal byte Pixel(int tile, Position16<CharacterPixels> point)
    {
        if (!Contains(tile) || (uint)point.X >= 8 || (uint)point.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + point.Y * 8 + point.X, out byte pixel) ? pixel : Basis(tile, point);
    }

    /// <summary>Basis.</summary>
    private static byte Basis(int tile, Position16<CharacterPixels> point) =>
        point.Y == 1 || tile == 0xae && point.X == 0 && point.Y > 1 ? (byte)6 : (byte)0;
}

using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>Menu atlas98/99 atB6:D300..D33F are two colorways of a6x6 beveled
/// square inset one pixel in its8x8 cell. Bottom/right shading wins at their corners,
/// top/left highlight covers the remaining edges, and the interior is flat face color.
/// Tile98 uses face11/highlight14/shadow12; tile99 uses10/9/7. The role-based color
/// selection is explicit; no per-pixel table or generated tile cache remains.</summary>
internal sealed class MenuBeveledSquareArtwork
{
    /// <summary>Two menu cells carry the light and dark colorways of the same beveled square.</summary>
    internal const int TileCount = 2;
    /// <summary>Only source pixels differing from the calculated square are retained.</summary>
    internal readonly Dictionary<int, byte>? edits;
    /// <summary>Restricts lookup to the menu's $98 and $99 square cells.</summary>
    internal static bool Contains(int tile) => Enum.IsDefined((MenuBeveledSquareCell)tile);

    /// <summary>Scans both source cells and keeps independent pixel edits outside the stock role-based colors.</summary>
    internal MenuBeveledSquareArtwork(IndexedPngImage image)
    {
        foreach (int tile in Enum.GetValues<MenuBeveledSquareCell>().Select(cell => (int)cell))
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            byte pixel = image.Pixels[(tile / 16 * 8 + y) * image.Size.X + tile % 16 * 8 + x];
            if (pixel != Basis((MenuBeveledSquareCell)tile, new(checked((ushort)x), checked((ushort)y)))) (edits ??= []).Add(tile * 64 + y * 8 + x, pixel);
        }
    }

    /// <summary>Returns an edited cell pixel when present, otherwise its calculated face, highlight, shadow, or transparency.</summary>
    internal byte Pixel(int tile, Position16<CharacterPixels> point)
    {
        if (!Contains(tile) || (uint)point.X >= 8 || (uint)point.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + point.Y * 8 + point.X, out byte pixel) ? pixel : Basis((MenuBeveledSquareCell)tile, point);
    }

    /// <summary>Calculates the six-by-six inset with bottom/right shadow precedence at the corners.</summary>
    private static byte Basis(MenuBeveledSquareCell cell, Position16<CharacterPixels> point)
    {
        if (point.X is 0 or 7 || point.Y is 0 or 7) return 0;
        (byte face, byte highlight, byte shadow) = cell == MenuBeveledSquareCell.Light ? ((byte)11, (byte)14, (byte)12) : ((byte)10, (byte)9, (byte)7);
        if (point.X == 6 || point.Y == 6) return shadow;
        return point.X == 1 || point.Y == 1 ? highlight : face;
    }
}

using SuperMetroid.Core.Frontend.Menus.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Menus;

/// <summary>Native82:C465 builds the32x16 L/R highlight from corner3C/51 and
/// body3D/43, mirroring the right corners. The outer frame occupies X1..30,Y1..14.
/// Its symmetric cutout spans rows4..12 with left edge3+floor((Y-8)^2/4), a
/// quadratic cap four pixels wide/high; mirror it for the right edge. Index6 fills
/// the remaining frame. No original pixel data or generated contour cache is kept.</summary>
internal sealed class MenuShoulderHighlightArtwork
{
    /// <summary>Four native tiles compose both halves of the L/R shoulder-button highlight.</summary>
    internal const int TileCount = 4;
    /// <summary>Only pixels differing from the calculated symmetric contour, keyed by tile and local pixel index.</summary>
    internal readonly Dictionary<int, byte>? edits;
    /// <summary>Admits only the four native shoulder-highlight tile identities.</summary>
    internal static bool Contains(int tile) => Enum.IsDefined((MenuShoulderHighlightCell)tile);

    /// <summary>Compares supplied tile pixels with the calculated contour and retains independently edited exceptions.</summary>
    internal MenuShoulderHighlightArtwork(IndexedPngImage image)
    {
        foreach (int tile in Enum.GetValues<MenuShoulderHighlightCell>().Select(cell => (int)cell))
        {
            if (!Contains(tile)) continue;
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                byte pixel = image.Pixels[(tile / 16 * 8 + y) * image.Size.X + tile % 16 * 8 + x];
                if (pixel != Basis((MenuShoulderHighlightCell)tile, new(checked((ushort)x), checked((ushort)y)))) (edits ??= []).Add(tile * 64 + y * 8 + x, pixel);
            }
        }
    }

    /// <summary>Returns an edited pixel when supplied, otherwise the native 32-by-16 shoulder-highlight contour.</summary>
    internal byte Pixel(int tile, Position16<CharacterPixels> point)
    {
        if (!Contains(tile) || (uint)point.X >= 8 || (uint)point.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + point.Y * 8 + point.X, out byte pixel) ? pixel : Basis((MenuShoulderHighlightCell)tile, point);
    }

    /// <summary>Maps each tile-local pixel into the combined shoulder frame and computes its quadratic cutout edge.</summary>
    private static byte Basis(MenuShoulderHighlightCell cell, Position16<CharacterPixels> point)
    {
        int row = point.Y + (cell is MenuShoulderHighlightCell.BottomRight or MenuShoulderHighlightCell.BottomLeft ? 8 : 0);
        int column = point.X + (cell is MenuShoulderHighlightCell.TopRight or MenuShoulderHighlightCell.BottomRight ? 8 : 0);
        if (row is 0 or 15 || column == 0) return 0;
        if (row is < 4 or > 12) return 6;
        int distance = row - 8;
        return column < 3 + distance * distance / 4 ? (byte)6 : (byte)0;
    }
}

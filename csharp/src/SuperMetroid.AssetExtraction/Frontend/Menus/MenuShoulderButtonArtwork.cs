using SuperMetroid.Core.Frontend.Menus.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Menus;

/// <summary>Pressed L/R buttons at82:C48F/C4A0 use left28, middle29/2E and right2A.
/// The24x7 pill has radius-three caps rounded to nearest pixels. Index4 shades the
/// right edge and bottom; its lower endpoint extends to meet the preceding edge
/// diagonally. Only the two particular four-row L/R glyph drawings remain artwork.
/// Replacing those strokes with coordinate cases would merely disguise the font.</summary>
internal sealed class MenuShoulderButtonArtwork
{
    /// <summary>Two rounded edge tiles and two letter-bearing middle tiles form the pressed buttons.</summary>
    internal const int TileCount = 4;
    /// <summary>Side length, in pixels, of the square L and R glyph drawings.</summary>
    internal const int GlyphSize = 4;
    /// <summary>Row-major four-by-four L glyph: true where the glyph draws in the shading colour.</summary>
    private readonly bool[] leftLetter;
    /// <summary>Row-major four-by-four R glyph: true where the glyph draws in the shading colour.</summary>
    private readonly bool[] rightLetter;
    /// <summary>Imported pixels that differ from the calculated button pill and selected glyphs.</summary>
    internal readonly Dictionary<int, byte>? edits;
    /// <summary>Recognizes the four menu-atlas tiles used by pressed L and R buttons.</summary>
    internal static bool Contains(int tile) => Enum.IsDefined((MenuShoulderButtonCell)tile);

    /// <summary>Reads the authored L/R glyphs, then keeps only differences from the rounded pill calculation.</summary>
    internal MenuShoulderButtonArtwork(IndexedPngImage image)
    {
        leftLetter = ReadLetter((int)MenuShoulderButtonCell.L);
        rightLetter = ReadLetter((int)MenuShoulderButtonCell.R);
        foreach (int tile in Enum.GetValues<MenuShoulderButtonCell>().Select(cell => (int)cell))
        {
            if (!Contains(tile)) continue;
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                byte pixel = Source(tile, x, y);
                if (pixel != Basis((MenuShoulderButtonCell)tile, new(checked((ushort)x), checked((ushort)y)))) (edits ??= []).Add(tile * 64 + y * 8 + x, pixel);
            }
        }
        bool[] ReadLetter(int tile)
        {
            var glyph = new bool[GlyphSize * GlyphSize];
            for (int y = 1; y <= GlyphSize; y++)
            for (int x = 2; x < 2 + GlyphSize; x++)
                glyph[(y - 1) * GlyphSize + x - 2] = Source(tile, x, y) == 4;
            return glyph;
        }
        byte Source(int tile, int x, int y) => image.Pixels[(tile / 16 * 8 + y) * image.Size.X + tile % 16 * 8 + x];
    }

    /// <summary>Uses an imported pixel edit when present, otherwise calculates the pressed-button pixel.</summary>
    internal byte Pixel(int tile, Position16<CharacterPixels> point)
    {
        if (!Contains(tile) || (uint)point.X >= 8 || (uint)point.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + point.Y * 8 + point.X, out byte pixel) ? pixel : Basis((MenuShoulderButtonCell)tile, point);
    }

    /// <summary>Composes a glyph mask with the shared 24-by-seven pill outline and shaded edge.</summary>
    private byte Basis(MenuShoulderButtonCell cell, Position16<CharacterPixels> point)
    {
        if (point.Y == 7) return 0;
        if (cell is MenuShoulderButtonCell.L or MenuShoulderButtonCell.R && point.X is >= 2 and <= 5 && point.Y is >= 1 and <= 4)
        {
            bool[] glyph = cell == MenuShoulderButtonCell.L ? leftLetter : rightLetter;
            if (glyph[(point.Y - 1) * GlyphSize + point.X - 2]) return 4;
        }
        int position = point.X + cell switch
        {
            MenuShoulderButtonCell.LeftEdge => 0,
            MenuShoulderButtonCell.RightEdge => 16,
            MenuShoulderButtonCell.L or MenuShoulderButtonCell.R => 8,
            _ => throw new ArgumentOutOfRangeException(nameof(cell), cell, "Not a pressed-button cell."),
        };
        int left = Inset(point.Y), right = 23 - left;
        if (point.Y == 6) right = Math.Max(right, 23 - Inset(point.Y - 1) - 1);
        if (position < left || position > right) return 0;
        return point.Y == 6 || point.Y > 0 && position == right ? (byte)4 : (byte)3;
    }

    /// <summary>3-round(sqrt(9-(y-3)^2)), for rows0..6. Integer half-pixel
    /// thresholds give exact nearest rounding without floating-point evaluation.</summary>
    private static int Inset(int y)
    {
        int dy = y - 3, squaredRadius = 9 - dy * dy, extent = 0;
        while (extent < 3 && (2 * extent + 1) * (2 * extent + 1) <= 4 * squaredRadius) extent++;
        return 3 - extent;
    }
}

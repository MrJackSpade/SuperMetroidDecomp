using System.Numerics;
using SuperMetroid.Core.Frontend.Menus.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Menus;

/// <summary>
/// Seven elevator-label strips, selected by82:C4DB..C568, EXIT cells5F/B7,
/// and SELECT/START cellsA5..AA share a six-row alphabet and one-pixel glyph gaps.
/// SELECT/START use ink14 with shadow13 displaced one pixel down/right, ink winning.
/// Fill isolated four-neighbor gaps within glyph columns with shadow13, preserving
/// the one-pixel spaces between glyphs;
/// this closes the single-pixel gap at R's bowl/leg junction without a glyph exception.
/// Authored ink index11 casts
/// a one-pixel eight-neighbor black outline15 across adjacent cells of the same strip.
/// Bevel each row's outer diagonal-only corner when its ink is inset from a neighboring
/// row. Retain Tourian's particular left T-cap outline pixel as drawing content:
/// its complete five-column/eight-row T region matches reflected EXIT except for
/// that black pixel. Neither the shared ink nor a symmetric bevel selects the change;
/// a Tourian-coordinate case would merely recite this independently drawn outline.
/// Twenty authored six-row glyph silhouettes select the lettering design; each label
/// places these glyphs with a one-pixel gap. Tracing the chosen glyph strokes in numerical
/// cases would disguise that art. Capture supplied differences, including that one
/// original outline choice; this exception does not cover other atlas mappings.
/// </summary>
internal sealed class MenuCompactLetteringArtwork
{
    /// <summary>The compact lettering artwork spans 34 selected eight-pixel tile cells across the menu atlas.</summary>
    internal const int TileCount = 34;
    /// <summary>Six-row ink masks captured once per distinct letter from the supplied source atlas.</summary>
    internal readonly Dictionary<char, uint> glyphs = [];
    /// <summary>Source pixels that differ from the shared glyph, shadow, and outline drawing rules.</summary>
    internal readonly Dictionary<int, byte>? edits;

    /// <summary>Identifies atlas cells occupied by the elevator labels, EXIT, SELECT, START, or ship lettering.</summary>
    internal static bool Contains(int tile) => MenuCompactLetteringDefinitions.Strips.Any(strip => strip.Contains(tile));

    /// <summary>The label strip occupying <paramref name="tile"/> (<see cref="MenuCompactLetteringDefinitions.Strips"/>).</summary>
    private static MenuLetteringStrip Layout(int tile) =>
        MenuCompactLetteringDefinitions.Strips.FirstOrDefault(strip => strip.Contains(tile)) ??
        throw new ArgumentOutOfRangeException(nameof(tile), tile, "No compact-lettering label occupies this cell.");

    /// <summary>Six-row font metrics: I is a single stem, N spans four columns,
    /// M/W span five, and the other authored letters span three.</summary>
    private static int Width(char letter) => letter switch { 'I' => 1, 'N' => 4, 'M' or 'W' => 5, _ => 3 };

    /// <summary>Extracts each distinct glyph from the image, then records only pixels that the shared lettering rules cannot reconstruct.</summary>
    internal MenuCompactLetteringArtwork(IndexedPngImage image)
    {
        foreach (MenuLetteringStrip row in MenuCompactLetteringDefinitions.Strips)
        {
            int position = row.Origin.X;
            foreach (char letter in row.Text)
            {
                int width = Width(letter);
                if (!glyphs.ContainsKey(letter))
                {
                    uint ink = 0;
                    for (int y = 0; y < 6; y++)
                    for (int x = 0; x < width; x++)
                    {
                        int sourceX = position + x;
                        int sourceTile = row.OrderedCells[sourceX / 8];
                        if (Source(sourceTile, sourceX % 8, row.Origin.Y + y) == (row.Shadow ? 14 : 11)) ink |= 1u << (y * width + x);
                    }
                    glyphs.Add(letter, ink);
                }
                position += width + 1;
            }
        }
        foreach (int tile in MenuCompactLetteringDefinitions.Strips.SelectMany(strip => strip.OrderedCells).Order())
        {
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                byte pixel = Source(tile, x, y);
                if (pixel != Basis(tile, new(checked((ushort)x), checked((ushort)y)))) (edits ??= []).Add(tile * 64 + y * 8 + x, pixel);
            }
        }
        byte Source(int tile, int x, int y) => image.Pixels[(tile / 16 * 8 + y) * image.Size.X + tile % 16 * 8 + x];
    }

    /// <summary>Reconstructs a lettering pixel from glyph ink and styling, applying a captured source edit where one exists.</summary>
    internal byte Pixel(int tile, Position16<CharacterPixels> point)
    {
        if (!Contains(tile) || (uint)point.X >= 8 || (uint)point.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + point.Y * 8 + point.X, out byte pixel) ? pixel : Basis(tile, point);
    }

    /// <summary>Draws ink, SELECT/START shadow and enclosed gaps, or a beveled eight-neighbor outline for an unedited atlas pixel.</summary>
    private byte Basis(int tile, Position16<CharacterPixels> point)
    {
        MenuLetteringStrip row = Layout(tile);
        int column = row.Column(tile);
        int position = column * 8 + point.X;
        uint current = RowInk(point.Y), bit = 1u << position;
        if ((current & bit) != 0) return row.Shadow ? (byte)14 : (byte)11;
        if (row.Shadow)
        {
            uint painted = current | (RowInk(point.Y - 1) << 1);
            if ((painted & bit) != 0) return 13;
            uint above = RowInk(point.Y - 1) | (RowInk(point.Y - 2) << 1);
            uint below = RowInk(point.Y + 1) | (current << 1);
            uint enclosed = (painted << 1) & (painted >> 1) & above & below;
            return (enclosed & bit) != 0 && InsideGlyphColumn() ? (byte)13 : (byte)0;
        }
        uint previous = RowInk(point.Y - 1), next = RowInk(point.Y + 1);
        uint neighboring = current | previous | next;
        if (((neighboring | (neighboring << 1) | (neighboring >> 1)) & bit) == 0) return 0;

        int left = BitOperations.TrailingZeroCount(neighboring);
        int right = 31 - BitOperations.LeadingZeroCount(neighboring);
        if (position == left - 1 && BitOperations.TrailingZeroCount(current) > left) return 0;
        if (position == right + 1 && 31 - BitOperations.LeadingZeroCount(current) < right) return 0;
        return 15;

        bool InsideGlyphColumn()
        {
            int start = row.Origin.X;
            foreach (char letter in row.Text)
            {
                int width = Width(letter);
                if (position >= start && position < start + width) return true;
                start += width + 1;
            }
            return false;
        }

        uint RowInk(int py)
        {
            int glyphY = py - row.Origin.Y;
            if ((uint)glyphY >= 6) return 0;
            uint bits = 0;
            int position = row.Origin.X;
            foreach (char letter in row.Text)
            {
                int width = Width(letter);
                bits |= ((glyphs[letter] >> (glyphY * width)) & ((1u << width) - 1)) << position;
                position += width + 1;
            }
            return bits;
        }
    }
}

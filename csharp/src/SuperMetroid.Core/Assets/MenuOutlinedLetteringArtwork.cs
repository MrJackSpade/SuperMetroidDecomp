using System.Numerics;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Seven elevator-label strips, selected by82:C4DB..C568, and EXIT cells5F/B7.
/// Authored ink index11 casts
/// a one-pixel eight-neighbor black outline15 across adjacent cells of the same strip.
/// Bevel each row's outer diagonal-only corner when its ink is inset from a neighboring
/// row. Tourian's extra left T-cap pixel remains unresolved required review; EXIT's
/// mirrored T cap disproves the former symmetric stroke-junction explanation.
/// Nineteen authored six-row glyph silhouettes select the lettering design; each label
/// places these glyphs with a one-pixel gap. Tracing the chosen glyph strokes in numerical
/// cases would disguise that art. Capture supplied differences, including that one
/// pending original pixel; it has no accepted artistic-retention disposition.
/// </summary>
internal sealed class MenuOutlinedLetteringArtwork
{
    internal const int TileCount = 28;
    private readonly Dictionary<char, uint> glyphs = [];
    private readonly Dictionary<int, byte>? edits;
    internal int StoredInkByteCount => glyphs.Count * sizeof(uint);
    internal int StoredEditCount => edits?.Count ?? 0;
    internal bool HasPixelOverride(int tile, int x, int y)
    {
        if (!Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.ContainsKey(tile * 64 + y * 8 + x);
    }

    internal static bool Contains(int tile) => tile is >= 0 and <= 9 or 0x10 or >= 0x12 and <= 0x16 or
        0x18 or 0x19 or 0x20 or 0x32 or 0x44 or 0x45 or >= 0x53 and <= 0x56 or 0x5f or 0xb7;

    /// <summary>Physical strip layout: Crateria, Brinstar, Norfair, Maridia, Ship and Wrecked.
    /// Norfair and Maridia skip the intervening large-font stem cells11 and17.</summary>
    private static (int Start, int Gap, string Text, int X, int Y) Layout(int tile) => tile switch
    {
        >= 0 and <= 3 => (0, -1, "CRATERIA", 1, 1),
        >= 4 and <= 7 => (4, -1, "BRINSTAR", 1, 1),
        8 or 9 or 0x20 or 0x32 => (8, -1, "TOURIAN", 3, 1),
        0x10 or >= 0x12 and <= 0x14 => (0x10, 0x11, "NORFAIR", 3, 1),
        0x15 or 0x16 or 0x18 or 0x19 => (0x15, 0x17, "MARIDIA", 3, 1),
        0x44 or 0x45 => (0x44, -1, "SHIP", 1, 0),
        >= 0x53 and <= 0x56 => (0x53, -1, "WRECKED", 1, 1),
        0x5f or 0xb7 => (0x5f, -1, "EXIT", 1, 1),
        _ => throw new ArgumentOutOfRangeException(nameof(tile)),
    };

    /// <summary>Six-row font metrics: I is a single stem, N spans four columns,
    /// M/W span five, and the other authored letters span three.</summary>
    private static int Width(char letter) => letter switch { 'I' => 1, 'N' => 4, 'M' or 'W' => 5, _ => 3 };

    /// <summary>Native82:C51D places Tourian cells08,09,20,32 left to right.
    /// Other strips use consecutive cells except the documented stem gaps.</summary>
    private static int Column(int tile, int start, int gap) => tile switch
    {
        0x20 => 2,
        0x32 => 3,
        0xb7 => 1,
        _ => tile - start - (gap >= 0 && tile > gap ? 1 : 0),
    };

    private static int SourceTile(int column, int start, int gap)
    {
        if (start == 8) return column switch { 0 => 8, 1 => 9, 2 => 0x20, 3 => 0x32, _ => throw new ArgumentOutOfRangeException(nameof(column)) };
        if (start == 0x5f) return column switch { 0 => 0x5f, 1 => 0xb7, _ => throw new ArgumentOutOfRangeException(nameof(column)) };
        int tile = start + column;
        return gap >= 0 && tile >= gap ? tile + 1 : tile;
    }

    internal MenuOutlinedLetteringArtwork(IndexedPngImage image)
    {
        for (int tile = 0; tile <= 0xb7; tile++)
        {
            if (!Contains(tile)) continue;
            var row = Layout(tile);
            if (tile != row.Start) continue;
            int position = row.X;
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
                        int sourceTile = SourceTile(sourceX / 8, row.Start, row.Gap);
                        if (Source(sourceTile, sourceX % 8, row.Y + y) == 11) ink |= 1u << (y * width + x);
                    }
                    glyphs.Add(letter, ink);
                }
                position += width + 1;
            }
        }
        for (int tile = 0; tile <= 0xb7; tile++)
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
        int column = Column(tile, row.Start, row.Gap);
        int position = column * 8 + x;
        uint current = RowInk(y), bit = 1u << position;
        if ((current & bit) != 0) return 11;
        uint previous = RowInk(y - 1), next = RowInk(y + 1);
        uint neighboring = current | previous | next;
        if (((neighboring | (neighboring << 1) | (neighboring >> 1)) & bit) == 0) return 0;

        int left = BitOperations.TrailingZeroCount(neighboring);
        int right = 31 - BitOperations.LeadingZeroCount(neighboring);
        if (position == left - 1 && BitOperations.TrailingZeroCount(current) > left) return 0;
        if (position == right + 1 && 31 - BitOperations.LeadingZeroCount(current) < right) return 0;
        return 15;

        uint RowInk(int py)
        {
            int glyphY = py - row.Y;
            if ((uint)glyphY >= 6) return 0;
            uint bits = 0;
            int position = row.X;
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

using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Selector14 at82:C29F is one cursor at(-4,-4). Selectors15/16 atC2B7/C2F5 are
/// two-row highlight grids, six/ten cells wide, with X origins-16/-36 and Y=-4/4.
/// Grid geometry and edge tiles are calculated. The original OAM visitation order is
/// retained as authored composition order: it jumps between rows and columns rather
/// than following a spatial traversal, and changing it changes partial-capacity output.
/// Encoding those arbitrary cell permutations as numeric cases would disguise the data.
/// </summary>
internal sealed class PauseSelectorVisual
{
    /// <summary>Identifies the selector layout whose native geometry and visitation order are calculated.</summary>
    private readonly int group;
    /// <summary>Holds a compiled author-supplied composition when the parts do not match a native selector layout.</summary>
    private readonly SpriteComposition? authored;

    /// <summary>Creates a selector renderer backed by either a recognized native layout or a compiled authored composition.</summary>
    /// <param name="group">Native layout identifier, or a negative value when <paramref name="authored"/> supplies the artwork.</param>
    /// <param name="authored">Compiled fallback composition for custom parts; null when native selector geometry is used.</param>
    private PauseSelectorVisual(int group, SpriteComposition? authored) { this.group = group; this.authored = authored; }

    /// <summary>Original beam highlight paint order expressed as row-major grid cells, not coordinates or tiles.</summary>
    private static ReadOnlySpan<byte> BeamOrder => [10, 4, 6, 7, 8, 9, 11, 5, 3, 2, 1, 0];
    /// <summary>Original equipment highlight paint order; the middle cells have a distinct non-spatial order.</summary>
    private static ReadOnlySpan<byte> EquipmentOrder => [18, 8, 15, 17, 16, 7, 6, 5, 14, 4, 10, 11, 12, 13, 19, 9, 3, 2, 1, 0];

    /// <summary>Returns the number of sprite parts in a cursor, beam-highlight, or equipment-highlight layout.</summary>
    private static int Count(int group) => group switch
    { 0 => 1, 1 => 12, 2 => 20, _ => throw new ArgumentOutOfRangeException(nameof(group)) };

    /// <summary>Calculates one selector sprite part at its native OAM visitation index.</summary>
    private static CompiledSpritePart Part(int group, int index)
    {
        if ((uint)index >= (uint)Count(group)) throw new ArgumentOutOfRangeException(nameof(index));
        if (group == 0) return new(SnesSpritemapXWord.Create(-4, false), 0xfc, SnesObjAttributeWord.Create(0x46, 0, 3), true);
        int width = group == 1 ? 6 : 10;
        int cell = group == 1 ? BeamOrder[index] : EquipmentOrder[index];
        int column = cell % width, row = cell / width;
        int x = (group == 1 ? -16 : -36) + 8 * column;
        int tile = column == width - 1 ? (row == 0 ? 0x5d : 0x5e) : column == 0 && row == 0 ? 0x5b : 0x5c;
        return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)(-4 + 8 * row)),
            SnesObjAttributeWord.Create(tile, 0, 3), true);
    }

    /// <summary>
    /// Recognizes the native selector layouts from their ordered parts, compiling any other
    /// supplied arrangement as a general sprite composition.
    /// </summary>
    internal static PauseSelectorVisual Compile(SpriteVisualPart[] parts, string name)
    {
        for (int group = 0; group < 3; group++)
        {
            if (parts.Length != Count(group)) continue;
            bool match = true;
            for (int index = 0; match && index < parts.Length; index++)
            {
                var basis = Part(group, index); var part = parts[index];
                match = part is not null && part.OffsetX == basis.X.SignedOffset && part.OffsetY == unchecked((sbyte)basis.Y) &&
                    part.TileColumn == basis.Attributes.TileNumber % 16 && part.TileRow == basis.Attributes.TileNumber / 16 &&
                    part.Size == 8 && part.Priority == 3 && part.Palette is null && !part.FlipX && !part.FlipY;
            }
            if (match) return new(group, null);
        }
        return new(-1, MenuSpriteCompiler.Compile(parts, name));
    }

    /// <summary>Draws the selector at the requested screen position using the caller's palette bits.</summary>
    internal void DrawOnScreen(OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        if (authored is not null) { authored.DrawOnScreen(oam, x, y, paletteBits); return; }
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        for (int index = 0; index < Count(group); index++)
        {
            var part = Part(group, index);
            oam.AddOnScreenSpritePart(part.X, part.Y, part.Attributes.WithPaletteBits(paletteBits), x, y);
        }
    }
}

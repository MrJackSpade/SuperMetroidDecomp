using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus.Artwork;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

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
    /// <summary>Recognized cursor, beam, or equipment selector group; minus one selects preserved authored OAM.</summary>
    private readonly int group;
    /// <summary>Caller-supplied composition retained when it differs from every calculated native selector.</summary>
    internal readonly SpriteComposition? authored;
    /// <summary>Stores either a recognized calculated selector group or an independently supplied composition.</summary>
    private PauseSelectorVisual(int group, SpriteComposition? authored) { this.group = group; this.authored = authored; }

    /// <summary>Original beam highlight paint order expressed as row-major grid cells, not coordinates or tiles.</summary>
    private static ReadOnlySpan<byte> BeamOrder => [10, 4, 6, 7, 8, 9, 11, 5, 3, 2, 1, 0];
    /// <summary>Original equipment highlight paint order; the middle cells have a distinct non-spatial order.</summary>
    private static ReadOnlySpan<byte> EquipmentOrder => [18, 8, 15, 17, 16, 7, 6, 5, 14, 4, 10, 11, 12, 13, 19, 9, 3, 2, 1, 0];

    /// <summary>Returns the native part count for the cursor, beam grid, or equipment grid.</summary>
    private static int Count(int group) => group switch
    { 0 => 1, 1 => 12, 2 => 20, _ => throw new ArgumentOutOfRangeException(nameof(group)) };

    /// <summary>Calculates one selector cell's geometry and edge tile while preserving the authored visitation order.</summary>
    private static CompiledSpritePart Part(int group, int index)
    {
        if ((uint)index >= (uint)Count(group)) throw new ArgumentOutOfRangeException(nameof(index));
        if (group == 0) return new(SnesSpritemapXWord.Create(-4, false), SnesSpritemapYByte.Create(-4), SnesObjAttributeWord.Create(PauseSelectorDefinitions.CursorCell, 0, 3), true);
        int width = group == 1 ? 6 : 10;
        int cell = group == 1 ? BeamOrder[index] : EquipmentOrder[index];
        int column = cell % width, row = cell / width;
        int x = (group == 1 ? -16 : -36) + 8 * column;
        int tile = column == width - 1 ? (row == 0 ? PauseSelectorDefinitions.GridTopRightCell : PauseSelectorDefinitions.GridRightCell)
            : column == 0 && row == 0 ? PauseSelectorDefinitions.GridTopLeftCell : PauseSelectorDefinitions.GridCell;
        return new(SnesSpritemapXWord.Create(x, false), SnesSpritemapYByte.Create(-4 + 8 * row),
            SnesObjAttributeWord.Create(tile, 0, 3), true);
    }

    /// <summary>Recognizes an exact native selector or compiles and retains edited OAM when its paint or order differs.</summary>
    internal static PauseSelectorVisual Compile(SpriteVisualPart[] parts, string name)
    {
        for (int group = 0; group < 3; group++)
        {
            if (parts.Length != Count(group)) continue;
            bool match = true;
            for (int index = 0; match && index < parts.Length; index++)
            {
                var basis = Part(group, index); var part = parts[index];
                match = part is not null && part.OffsetX == basis.X.SignedOffset && part.OffsetY == basis.Y.SignedOffset &&
                    part.TileColumn == basis.Attributes.TileNumber % 16 && part.TileRow == basis.Attributes.TileNumber / 16 &&
                    part.Size == 8 && part.Priority == 3 && part.Palette is null && !part.FlipX && !part.FlipY;
            }
            if (match) return new(group, null);
        }
        return new(-1, MenuSpriteCompiler.Compile(parts, name));
    }

    /// <summary>Draws the recognized selector in native OAM order or delegates an edited composition, applying caller palette bits.</summary>
    internal void DrawOnScreen(OamBuffer oam, Position16<OamPixels> origin, ushort paletteBits)
    {
        if (authored is not null) { authored.DrawOnScreen(oam, origin, paletteBits); return; }
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        for (int index = 0; index < Count(group); index++)
        {
            var part = Part(group, index);
            oam.AddOnScreenSpritePart(part.X, part.Y, part.Attributes.WithPaletteBits(paletteBits), origin);
        }
    }
}

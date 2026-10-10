using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// World-area lettering at82:CC6B..CD66. Tiles use the contiguous alphabet6A+(letter-'A'),
/// with one small, unflipped, priority3 sprite per letter and live caller palette.
/// Single lines use Y=-4; Wrecked Ship draws SHIP at Y=0 before WRECKED at Y=-8.
/// Horizontal placement advances by the eight-pixel glyph cell, with authored line origins
/// and individual nonstandard advances. These are optical typography inputs: the same I/A
/// glyph pair advances7 in Crateria/Tourian and8 in Maridia, while Wrecked Ship's lower line
/// is deliberately staggered. A uniform font-metric rule would change those chosen layouts;
/// reciting their per-word choices in code would disguise the same authored composition.
/// </summary>
internal sealed class WorldMapLabelComposition
{
    private readonly MapSpriteId identity;
    private readonly int upperOrigin;
    private readonly int lowerOrigin;
    private readonly Dictionary<int, int>? letterAdvances;
    private readonly SpriteComposition? authored;

    private WorldMapLabelComposition(MapSpriteId identity, int upperOrigin, int lowerOrigin,
        Dictionary<int, int>? letterAdvances, SpriteComposition? authored)
    { this.identity = identity; this.upperOrigin = upperOrigin; this.lowerOrigin = lowerOrigin;
        this.letterAdvances = letterAdvances; this.authored = authored; }

    /// <summary>Area names are semantic text content; named cases select the map-label wording.</summary>
    private static string Text(MapSpriteId id) => id switch
    {
        MapSpriteId.WorldCrateria => "CRATERIA",
        MapSpriteId.WorldBrinstar => "BRINSTAR",
        MapSpriteId.WorldNorfair => "NORFAIR",
        MapSpriteId.WorldWreckedShip => "WRECKEDSHIP",
        MapSpriteId.WorldMaridia => "MARIDIA",
        MapSpriteId.WorldTourian => "TOURIAN",
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    private static int VerticalOffset(MapSpriteId id, int index) =>
        id == MapSpriteId.WorldWreckedShip ? (index < 4 ? 0 : -8) : -4;

    internal static WorldMapLabelComposition Compile(MapSpriteId id, SpriteVisualPart[] parts, string name)
    {
        string text = Text(id);
        bool regular = parts.Length == text.Length;
        for (int index = 0; regular && index < parts.Length; index++)
        {
            var part = parts[index];
            int tile = 0x6a + text[text.Length - 1 - index] - 'A';
            regular = part is not null && part.OffsetX is >= -256 and <= 255 &&
                part.OffsetY == VerticalOffset(id, index) && part.TileColumn == tile % 16 && part.TileRow == tile / 16 &&
                part.Size == 8 && part.Priority == 3 && part.Palette is null && !part.FlipX && !part.FlipY;
        }
        if (!regular) return new(id, 0, 0, null, MenuSpriteCompiler.Compile(parts, name));
        Dictionary<int, int>? advances = null;
        bool twoLines = id == MapSpriteId.WorldWreckedShip;
        for (int index = 0; index < parts.Length - 1; index++)
        {
            if (twoLines && index == 3) continue;
            int advance = parts[index].OffsetX - parts[index + 1].OffsetX;
            if (advance != 8) (advances ??= []).Add(index, advance);
        }
        return new(id, parts[^1].OffsetX, twoLines ? parts[3].OffsetX : 0, advances, null);
    }

    internal void Draw(OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        if (authored is not null) { authored.DrawOnScreen(oam, x, y, paletteBits); return; }
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        string text = Text(identity);
        for (int index = 0; index < text.Length; index++)
        {
            int tile = 0x6a + text[text.Length - 1 - index] - 'A';
            oam.AddOnScreenSpritePart(SnesSpritemapXWord.Create(HorizontalOffset(index, text.Length), false),
                unchecked((byte)VerticalOffset(identity, index)),
                SnesObjAttributeWord.Create(tile, 0, 3).WithPaletteBits(paletteBits), x, y);
        }
    }

    private int HorizontalOffset(int index, int count)
    {
        bool bottom = identity == MapSpriteId.WorldWreckedShip && index < 4;
        int leftmostIndex = bottom ? 3 : count - 1;
        int x = bottom ? lowerOrigin : upperOrigin;
        for (int next = leftmostIndex - 1; next >= index; next--)
            x += letterAdvances is not null && letterAdvances.TryGetValue(next, out int advance) ? advance : 8;
        return x;
    }
}

using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// World-area lettering at82:CC6B..CD66. Tiles use the contiguous alphabet6A+(letter-'A'),
/// with one small, unflipped, priority3 sprite per letter and live caller palette.
/// Single lines use Y=-4; Wrecked Ship draws SHIP at Y=0 before WRECKED at Y=-8.
/// Horizontal positions remain loaded inputs pending their independent spacing review;
/// this conversion does not classify those positions as an accepted retention exception.
/// </summary>
internal sealed class WorldMapLabelComposition
{
    private readonly ushort identity;
    private readonly short[]? horizontalPositions;
    private readonly SpriteComposition? authored;

    private WorldMapLabelComposition(ushort identity, short[]? horizontalPositions, SpriteComposition? authored)
    { this.identity = identity; this.horizontalPositions = horizontalPositions; this.authored = authored; }

    internal bool StoresComposition => authored is not null;
    internal int StoredHorizontalCount => horizontalPositions?.Length ?? 0;

    /// <summary>Area names are semantic text content; named cases select the map-label wording.</summary>
    private static string Text(ushort id) => id switch
    {
        MapSpriteDefinitions.WorldCrateria => "CRATERIA",
        MapSpriteDefinitions.WorldBrinstar => "BRINSTAR",
        MapSpriteDefinitions.WorldNorfair => "NORFAIR",
        MapSpriteDefinitions.WorldWreckedShip => "WRECKEDSHIP",
        MapSpriteDefinitions.WorldMaridia => "MARIDIA",
        MapSpriteDefinitions.WorldTourian => "TOURIAN",
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    private static int VerticalOffset(ushort id, int index) =>
        id == MapSpriteDefinitions.WorldWreckedShip ? (index < 4 ? 0 : -8) : -4;

    internal static WorldMapLabelComposition Compile(ushort id, SpriteVisualPart[] parts, string name)
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
        if (!regular) return new(id, null, MenuSpriteCompiler.Compile(parts, name));
        var positions = new short[parts.Length];
        for (int index = 0; index < parts.Length; index++) positions[index] = (short)parts[index].OffsetX;
        return new(id, positions, null);
    }

    internal void Draw(OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        if (authored is not null) { authored.DrawOnScreen(oam, x, y, paletteBits); return; }
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        string text = Text(identity);
        for (int index = 0; index < text.Length; index++)
        {
            int tile = 0x6a + text[text.Length - 1 - index] - 'A';
            oam.AddOnScreenSpritePart(SnesSpritemapXWord.Create(horizontalPositions![index], false),
                unchecked((byte)VerticalOffset(identity, index)),
                SnesObjAttributeWord.Create(tile, 0, 3).WithPaletteBits(paletteBits), x, y);
        }
    }
}

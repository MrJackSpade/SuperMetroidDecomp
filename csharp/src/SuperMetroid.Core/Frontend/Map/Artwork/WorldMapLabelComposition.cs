using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus.Artwork;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

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
    /// <summary>Area label identity selecting the lettering and one- or two-line placement.</summary>
    internal readonly MapSpriteId identity;
    /// <summary>Authored horizontal start for the main or upper text line.</summary>
    private readonly int upperOrigin;
    /// <summary>Separate start for Wrecked Ship's lower SHIP line.</summary>
    private readonly int lowerOrigin;
    /// <summary>Nonstandard optical advances retained by glyph position.</summary>
    internal readonly Dictionary<int, int>? letterAdvances;
    /// <summary>Full compiled parts when supplied artwork does not fit the regular lettering composition.</summary>
    internal readonly SpriteComposition? authored;

    /// <summary>Retains selected text origins or independent authored parts after composition analysis.</summary>
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
        MapSpriteId.ArrowRight or
            MapSpriteId.ArrowLeft or
            MapSpriteId.ArrowUp or
            MapSpriteId.ArrowDown or
            MapSpriteId.MarkerBoss or
            MapSpriteId.StationEnergy or
            MapSpriteId.StationMissile or
            MapSpriteId.StationMap or
            MapSpriteId.MarkerDefeatedBoss or
            MapSpriteId.MarkerGunship or
            MapSpriteId.IndicatorBacking or
            MapSpriteId.IndicatorFrame0 or
            MapSpriteId.IndicatorFrame1 or
            MapSpriteId.IndicatorFrame2 or
            MapSpriteId.ElevatorCrateria or
            MapSpriteId.ElevatorBrinstar or
            MapSpriteId.ElevatorNorfair or
            MapSpriteId.ElevatorWreckedShip or
            MapSpriteId.ElevatorMaridia or
            MapSpriteId.WorldTitle => throw new ArgumentOutOfRangeException(nameof(id)),
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    /// <summary>Places Wrecked Ship's SHIP line below WRECKED and centers single-line area labels.</summary>
    private static int VerticalOffset(MapSpriteId id, int index) =>
        id == MapSpriteId.WorldWreckedShip ? (index < 4 ? 0 : -8) : -4;

    /// <summary>Recognizes regular alphabet glyphs and retains only selected origins and optical advances; preserves other art verbatim.</summary>
    internal static WorldMapLabelComposition Compile(MapSpriteId id, SpriteVisualPart[] parts, string name)
    {
        string text = Text(id);
        bool regular = parts.Length == text.Length;
        for (int index = 0; regular && index < parts.Length; index++)
        {
            var part = parts[index];
            int tile = WorldMapTileDefinitions.LabelLetterACell + text[text.Length - 1 - index] - 'A';
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

    /// <summary>
    /// The authored parts or the area's reconstructed glyph sequence as a whole frame (#1392); the glyphs take their
    /// draw's palette.
    /// </summary>
    internal SpriteFrame ToFrame(string name)
    {
        if (authored is not null)
            return authored.ToFrame(name, 0);
        string text = Text(identity);
        return new(name, [.. Enumerable.Range(0, text.Length).Select(index =>
        {
            int tile = WorldMapTileDefinitions.LabelLetterACell + text[text.Length - 1 - index] - 'A';
            return SpritePart.Create(Offset16<ScreenPixels>.Of(HorizontalOffset(index, text.Length), VerticalOffset(identity, index)), false, tile, 0, 3)
                .ToFramePart(0, SpritePartFrames.StandardSizes, inheritPalette: true);
        })]);
    }

    /// <summary>Accumulates eight-pixel cells and selected optical advances from the line's authored start.</summary>
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

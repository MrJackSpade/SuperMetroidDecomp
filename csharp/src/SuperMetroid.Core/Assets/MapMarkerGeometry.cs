using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Regular map marker compositions from bank82:C232..C262,C298,C38C..C3A8,CF7C..CFE0.
/// Arrows join mirrored halves seven pixels apart; pulse corners expand at radius4..6.
/// Boss corners use the native seven-pixel horizontal/eight-pixel vertical spacing;
/// gunship halves are eight pixels apart. Native draw order is preserved before clipping.
/// All parts are small, inherit the caller palette, and use priority2 (arrows and title priority3).
/// Elevator labels at82:C4DB/C4F1/C507/C533/C549 draw contiguous text strips right-to-left.
/// Norfair and Maridia skip atlas cells11/17 containing vertical-bar art, not label text.
/// Wrecked Ship draws its centered two-tile bottom line before its four-tile top line.
/// World title82:CBFB spells PLANET ZEBES on an eight-pixel grid using two tiles per letter.
/// Letters draw right-to-left, bottom before top except the leading P, which draws top first.
/// </summary>
internal static class MapMarkerGeometry
{
    /// <summary>The authored map title at82:CBFB; its wording is text content, not a numerical mapping.</summary>
    private const string WorldTitleText = "PLANET ZEBES";

    internal static int PartCount(MapSpriteId id) => id switch
    {
        MapSpriteId.ArrowRight or MapSpriteId.ArrowLeft or
        MapSpriteId.ArrowUp or MapSpriteId.ArrowDown or MapSpriteId.MarkerGunship => 2,
        MapSpriteId.MarkerDefeatedBoss or MapSpriteId.IndicatorFrame0 or
        MapSpriteId.IndicatorFrame1 or MapSpriteId.IndicatorFrame2 => 4,
        MapSpriteId.MarkerBoss or MapSpriteId.StationEnergy or MapSpriteId.StationMissile or
        MapSpriteId.StationMap or MapSpriteId.IndicatorBacking => 1,
        MapSpriteId.ElevatorCrateria or MapSpriteId.ElevatorBrinstar or
        MapSpriteId.ElevatorNorfair or MapSpriteId.ElevatorMaridia => 4,
        MapSpriteId.ElevatorWreckedShip => 6,
        MapSpriteId.WorldTitle => (WorldTitleText.Length - 1) * 2,
        MapSpriteId.WorldCrateria or MapSpriteId.WorldBrinstar or MapSpriteId.WorldNorfair or
            MapSpriteId.WorldWreckedShip or MapSpriteId.WorldMaridia or MapSpriteId.WorldTourian => 0,
        _ => throw new InvalidOperationException($"Undefined MapSpriteId {id}."),
    };

    internal static CompiledSpritePart Part(MapSpriteId id, int index)
    {
        if ((uint)index >= (uint)PartCount(id)) throw new ArgumentOutOfRangeException(nameof(index));
        int x, y, tile, priority = 2;
        bool flipX = false, flipY = false;
        switch (id)
        {
            case MapSpriteId.WorldTitle:
                int letter = WorldTitleText.Length - 2 - index / 2;
                int textColumn = letter < 6 ? letter : letter + 1;
                bool top = letter == 0 ? (index & 1) == 0 : (index & 1) != 0;
                x = 8 * (textColumn - 6); y = top ? -8 : 0; priority = 3;
                tile = TitleGlyphTile(WorldTitleText[textColumn], top);
                break;
            case MapSpriteId.ElevatorCrateria:
            case MapSpriteId.ElevatorBrinstar:
            case MapSpriteId.ElevatorNorfair:
            case MapSpriteId.ElevatorMaridia:
                int column = 3 - index;
                x = -8 + 8 * column; y = -8;
                tile = id switch
                {
                    MapSpriteId.ElevatorCrateria => column,
                    MapSpriteId.ElevatorBrinstar => 4 + column,
                    MapSpriteId.ElevatorNorfair => 0x10 + column + (column >= 1 ? 1 : 0),
                    MapSpriteId.ElevatorMaridia => 0x15 + column + (column >= 2 ? 1 : 0),
                    _ => throw new ArgumentOutOfRangeException(nameof(id)),
                };
                break;
            case MapSpriteId.ElevatorWreckedShip:
                bool bottom = index < 2;
                int rowColumn = bottom ? 1 - index : 5 - index;
                x = (bottom ? -4 : -12) + 8 * rowColumn; y = bottom ? 0 : -8;
                tile = (bottom ? 0x44 : 0x53) + rowColumn;
                break;
            case MapSpriteId.ArrowRight:
            case MapSpriteId.ArrowLeft:
                x = -4; y = -7 * index; tile = 0x9e; priority = 3;
                flipX = id == MapSpriteId.ArrowLeft; flipY = index == 0;
                break;
            case MapSpriteId.ArrowUp:
            case MapSpriteId.ArrowDown:
                x = -1 - 7 * index; y = -4; tile = 0x9d; priority = 3;
                flipX = index == 0; flipY = id == MapSpriteId.ArrowUp;
                break;
            case MapSpriteId.MarkerGunship:
                x = 4 - 8 * index; y = -2; tile = 0x8f; flipX = index == 0;
                break;
            case MapSpriteId.MarkerDefeatedBoss:
                flipX = index < 2; flipY = (index & 1) == 0;
                x = flipX ? 3 : -4; y = flipY ? 4 : -4; tile = 0x9f;
                break;
            case MapSpriteId.IndicatorFrame0:
            case MapSpriteId.IndicatorFrame1:
            case MapSpriteId.IndicatorFrame2:
                int radius = 4 + (id - MapSpriteId.IndicatorFrame0);
                flipX = (index & 1) == 0; flipY = index < 2;
                x = flipX ? radius : -radius; y = flipY ? radius : -radius; tile = 0xaf;
                break;
            case MapSpriteId.MarkerBoss:
            case MapSpriteId.StationEnergy:
            case MapSpriteId.StationMissile:
            case MapSpriteId.StationMap:
            case MapSpriteId.IndicatorBacking:
            case MapSpriteId.WorldCrateria:
            case MapSpriteId.WorldBrinstar:
            case MapSpriteId.WorldNorfair:
            case MapSpriteId.WorldWreckedShip:
            case MapSpriteId.WorldMaridia:
            case MapSpriteId.WorldTourian:
                x = 1; y = 0;
                tile = id switch
                {
                    MapSpriteId.MarkerBoss => 0x8a,
                    MapSpriteId.StationEnergy => 0x8c,
                    MapSpriteId.StationMissile => 0x8b,
                    MapSpriteId.StationMap => 0x8e,
                    MapSpriteId.IndicatorBacking => 0x89,
                    _ => throw new ArgumentOutOfRangeException(nameof(id)),
                };
                break;
            default:
                throw new InvalidOperationException($"Undefined MapSpriteId {id}.");
        }
        var flips = (flipX ? SnesTileFlipFlags.Horizontal : 0) | (flipY ? SnesTileFlipFlags.Vertical : 0);
        return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
            SnesObjAttributeWord.Create(tile, 0, priority, flips), true);
    }

    /// <summary>
    /// Named glyph selection in the shared menu alphabet. Lower halves are one atlas row below
    /// upper halves except P, whose top shares D's cell0D and bottom uses38, and T, whose stem
    /// shares cell11. These are glyph-part aliases established from the original indexed pixels.
    /// </summary>
    private static int TitleGlyphTile(char letter, bool top)
    {
        if (!top && letter == 'P') return 0x38;
        if (!top && letter == 'T') return 0x11;
        int upper = letter switch
        {
            'A' => 0x0a,
            'B' => 0x0b,
            'E' => 0x0e,
            'L' => 0x25,
            'N' => 0x27,
            'P' => 0x0d,
            'S' => 0x2b,
            'T' => 0x2c,
            'Z' => 0x42,
            _ => throw new ArgumentOutOfRangeException(nameof(letter)),
        };
        return upper + (top ? 0 : 16);
    }

    internal static bool Matches(MapSpriteId id, SpriteVisualPart[] parts)
    {
        int count = PartCount(id);
        if (count == 0 || parts.Length != count) return false;
        for (int index = 0; index < count; index++)
        {
            var basis = Part(id, index);
            var visual = parts[index];
            if (visual is null || visual.OffsetX != basis.X.SignedOffset || visual.OffsetY != unchecked((sbyte)basis.Y) ||
                visual.Size != 8 || visual.Palette is not null || visual.Priority != basis.Attributes.Priority ||
                visual.TileColumn != basis.Attributes.TileNumber % 16 || visual.TileRow != basis.Attributes.TileNumber / 16 ||
                visual.FlipX != basis.Attributes.FlipHorizontally || visual.FlipY != basis.Attributes.FlipVertically) return false;
        }
        return true;
    }
}

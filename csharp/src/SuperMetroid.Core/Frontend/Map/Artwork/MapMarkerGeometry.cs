using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus.Artwork;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

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

    /// <summary>Counts native OAM parts for arrows, markers, elevator labels, and the world title.</summary>
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

    /// <summary>Calculates one map object's native offset, tile, flips, and priority in draw order.</summary>
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
                tile = MenuCompactLetteringDefinitions.Strip(id switch
                {
                    MapSpriteId.ElevatorCrateria => "CRATERIA",
                    MapSpriteId.ElevatorBrinstar => "BRINSTAR",
                    MapSpriteId.ElevatorNorfair => "NORFAIR",
                    MapSpriteId.ElevatorMaridia => "MARIDIA",
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
                        MapSpriteId.ElevatorWreckedShip or
                        MapSpriteId.WorldTitle or
                        MapSpriteId.WorldCrateria or
                        MapSpriteId.WorldBrinstar or
                        MapSpriteId.WorldNorfair or
                        MapSpriteId.WorldWreckedShip or
                        MapSpriteId.WorldMaridia or
                        MapSpriteId.WorldTourian => throw new ArgumentOutOfRangeException(nameof(id)),
                    _ => throw new ArgumentOutOfRangeException(nameof(id)),
                }).OrderedCells[column];
                break;
            case MapSpriteId.ElevatorWreckedShip:
                bool bottom = index < 2;
                int rowColumn = bottom ? 1 - index : 5 - index;
                x = (bottom ? -4 : -12) + 8 * rowColumn; y = bottom ? 0 : -8;
                tile = MenuCompactLetteringDefinitions.Strip(bottom ? "SHIP" : "WRECKED").OrderedCells[rowColumn];
                break;
            case MapSpriteId.ArrowRight:
            case MapSpriteId.ArrowLeft:
                x = -4; y = -7 * index; tile = MapMarkerCellDefinitions.Tiles[MapMarkerCell.HorizontalArrowHalf]; priority = 3;
                flipX = id == MapSpriteId.ArrowLeft; flipY = index == 0;
                break;
            case MapSpriteId.ArrowUp:
            case MapSpriteId.ArrowDown:
                x = -1 - 7 * index; y = -4; tile = MapMarkerCellDefinitions.Tiles[MapMarkerCell.VerticalArrowHalf]; priority = 3;
                flipX = index == 0; flipY = id == MapSpriteId.ArrowUp;
                break;
            case MapSpriteId.MarkerGunship:
                x = 4 - 8 * index; y = -2; tile = MapMarkerCellDefinitions.Tiles[MapMarkerCell.GunshipHalf]; flipX = index == 0;
                break;
            case MapSpriteId.MarkerDefeatedBoss:
                flipX = index < 2; flipY = (index & 1) == 0;
                x = flipX ? 3 : -4; y = flipY ? 4 : -4; tile = MapMarkerCellDefinitions.Tiles[MapMarkerCell.DefeatedBossCorner];
                break;
            case MapSpriteId.IndicatorFrame0:
            case MapSpriteId.IndicatorFrame1:
            case MapSpriteId.IndicatorFrame2:
                int radius = 4 + (id - MapSpriteId.IndicatorFrame0);
                flipX = (index & 1) == 0; flipY = index < 2;
                x = flipX ? radius : -radius; y = flipY ? radius : -radius; tile = MapMarkerCellDefinitions.Tiles[MapMarkerCell.IndicatorCorner];
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
                    MapSpriteId.MarkerBoss => MapMarkerCellDefinitions.Tiles[MapMarkerCell.Boss],
                    MapSpriteId.StationEnergy => MapMarkerCellDefinitions.Tiles[MapMarkerCell.EnergyStation],
                    MapSpriteId.StationMissile => MapMarkerCellDefinitions.Tiles[MapMarkerCell.MissileStation],
                    MapSpriteId.StationMap => MapMarkerCellDefinitions.Tiles[MapMarkerCell.MapStation],
                    MapSpriteId.IndicatorBacking => MapMarkerCellDefinitions.Tiles[MapMarkerCell.IndicatorBacking],
                    MapSpriteId.ArrowRight or
                        MapSpriteId.ArrowLeft or
                        MapSpriteId.ArrowUp or
                        MapSpriteId.ArrowDown or
                        MapSpriteId.MarkerDefeatedBoss or
                        MapSpriteId.MarkerGunship or
                        MapSpriteId.IndicatorFrame0 or
                        MapSpriteId.IndicatorFrame1 or
                        MapSpriteId.IndicatorFrame2 or
                        MapSpriteId.ElevatorCrateria or
                        MapSpriteId.ElevatorBrinstar or
                        MapSpriteId.ElevatorNorfair or
                        MapSpriteId.ElevatorWreckedShip or
                        MapSpriteId.ElevatorMaridia or
                        MapSpriteId.WorldTitle or
                        MapSpriteId.WorldCrateria or
                        MapSpriteId.WorldBrinstar or
                        MapSpriteId.WorldNorfair or
                        MapSpriteId.WorldWreckedShip or
                        MapSpriteId.WorldMaridia or
                        MapSpriteId.WorldTourian => throw new ArgumentOutOfRangeException(nameof(id)),
                    _ => throw new ArgumentOutOfRangeException(nameof(id)),
                };
                break;
            default:
                throw new InvalidOperationException($"Undefined MapSpriteId {id}.");
        }
        var flips = (flipX ? TileFlip.Horizontal : 0) | (flipY ? TileFlip.Vertical : 0);
        return new(SpritePart.Create(x, y, false,
            tile, 0, priority, flips), true);
    }

    /// <summary>A title letter's half in the shared large menu font (<see cref="MenuLargeFontCellDefinitions.Letters"/>).</summary>
    private static int TitleGlyphTile(char letter, bool top)
    {
        MenuLargeFontLetter glyph = MenuLargeFontCellDefinitions.Letters.TryGetValue(letter, out MenuLargeFontLetter found)
            ? found : throw new ArgumentOutOfRangeException(nameof(letter), letter, "The large menu font has no such letter.");
        return top ? glyph.Upper : glyph.Lower;
    }

    /// <summary>Checks whether editable parts retain the calculated native map-object geometry.</summary>
    internal static bool Matches(MapSpriteId id, SpriteVisualPart[] parts)
    {
        int count = PartCount(id);
        if (count == 0 || parts.Length != count) return false;
        for (int index = 0; index < count; index++)
        {
            var basis = Part(id, index);
            var visual = parts[index];
            if (visual is null || visual.OffsetX != basis.Part.Offset.X || visual.OffsetY != basis.Part.Offset.Y ||
                visual.Size != 8 || visual.Palette is not null || visual.Priority != basis.Part.Priority ||
                visual.TileColumn != basis.Part.Character % 16 || visual.TileRow != basis.Part.Character / 16 ||
                visual.FlipX != basis.Part.FlipsHorizontally || visual.FlipY != basis.Part.FlipsVertically) return false;
        }
        return true;
    }
}

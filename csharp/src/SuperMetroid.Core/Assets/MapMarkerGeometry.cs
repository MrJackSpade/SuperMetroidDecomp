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

    /// <summary>Returns the number of ordered OBJ parts used by a supported world-map marker identifier.</summary>
    /// <param name="id">Compiled map-sprite identifier whose composition is being measured.</param>
    /// <returns>The part count, or zero when the identifier has no regular marker composition here.</returns>
    internal static int PartCount(ushort id) => id switch
    {
        MapSpriteDefinitions.ArrowRight or MapSpriteDefinitions.ArrowLeft or
        MapSpriteDefinitions.ArrowUp or MapSpriteDefinitions.ArrowDown or MapSpriteDefinitions.MarkerGunship => 2,
        MapSpriteDefinitions.MarkerDefeatedBoss or MapSpriteDefinitions.IndicatorFrame0 or
        MapSpriteDefinitions.IndicatorFrame1 or MapSpriteDefinitions.IndicatorFrame2 => 4,
        MapSpriteDefinitions.MarkerBoss or MapSpriteDefinitions.StationEnergy or MapSpriteDefinitions.StationMissile or
        MapSpriteDefinitions.StationMap or MapSpriteDefinitions.IndicatorBacking => 1,
        MapSpriteDefinitions.ElevatorCrateria or MapSpriteDefinitions.ElevatorBrinstar or
        MapSpriteDefinitions.ElevatorNorfair or MapSpriteDefinitions.ElevatorMaridia => 4,
        MapSpriteDefinitions.ElevatorWreckedShip => 6,
        MapSpriteDefinitions.WorldTitle => (WorldTitleText.Length - 1) * 2,
        _ => 0,
    };

    /// <summary>Calculates one marker part's relative position, tile, priority, and flips in its native composition order.</summary>
    /// <param name="id">Compiled map-sprite identifier selecting the marker layout.</param>
    /// <param name="index">Zero-based part position below <see cref="PartCount(ushort)"/>.</param>
    /// <returns>The compiled OBJ attributes and offsets for the selected part.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the selected marker's part range.</exception>
    internal static CompiledSpritePart Part(ushort id, int index)
    {
        if ((uint)index >= (uint)PartCount(id)) throw new ArgumentOutOfRangeException(nameof(index));
        int x, y, tile, priority = 2;
        bool flipX = false, flipY = false;
        switch (id)
        {
            case MapSpriteDefinitions.WorldTitle:
                int letter = WorldTitleText.Length - 2 - index / 2;
                int textColumn = letter < 6 ? letter : letter + 1;
                bool top = letter == 0 ? (index & 1) == 0 : (index & 1) != 0;
                x = 8 * (textColumn - 6); y = top ? -8 : 0; priority = 3;
                tile = TitleGlyphTile(WorldTitleText[textColumn], top);
                break;
            case MapSpriteDefinitions.ElevatorCrateria:
            case MapSpriteDefinitions.ElevatorBrinstar:
            case MapSpriteDefinitions.ElevatorNorfair:
            case MapSpriteDefinitions.ElevatorMaridia:
                int column = 3 - index;
                x = -8 + 8 * column; y = -8;
                tile = id switch
                {
                    MapSpriteDefinitions.ElevatorCrateria => column,
                    MapSpriteDefinitions.ElevatorBrinstar => 4 + column,
                    MapSpriteDefinitions.ElevatorNorfair => 0x10 + column + (column >= 1 ? 1 : 0),
                    MapSpriteDefinitions.ElevatorMaridia => 0x15 + column + (column >= 2 ? 1 : 0),
                    _ => throw new ArgumentOutOfRangeException(nameof(id)),
                };
                break;
            case MapSpriteDefinitions.ElevatorWreckedShip:
                bool bottom = index < 2;
                int rowColumn = bottom ? 1 - index : 5 - index;
                x = (bottom ? -4 : -12) + 8 * rowColumn; y = bottom ? 0 : -8;
                tile = (bottom ? 0x44 : 0x53) + rowColumn;
                break;
            case MapSpriteDefinitions.ArrowRight:
            case MapSpriteDefinitions.ArrowLeft:
                x = -4; y = -7 * index; tile = 0x9e; priority = 3;
                flipX = id == MapSpriteDefinitions.ArrowLeft; flipY = index == 0;
                break;
            case MapSpriteDefinitions.ArrowUp:
            case MapSpriteDefinitions.ArrowDown:
                x = -1 - 7 * index; y = -4; tile = 0x9d; priority = 3;
                flipX = index == 0; flipY = id == MapSpriteDefinitions.ArrowUp;
                break;
            case MapSpriteDefinitions.MarkerGunship:
                x = 4 - 8 * index; y = -2; tile = 0x8f; flipX = index == 0;
                break;
            case MapSpriteDefinitions.MarkerDefeatedBoss:
                flipX = index < 2; flipY = (index & 1) == 0;
                x = flipX ? 3 : -4; y = flipY ? 4 : -4; tile = 0x9f;
                break;
            case MapSpriteDefinitions.IndicatorFrame0:
            case MapSpriteDefinitions.IndicatorFrame1:
            case MapSpriteDefinitions.IndicatorFrame2:
                int radius = 4 + id - MapSpriteDefinitions.IndicatorFrame0;
                flipX = (index & 1) == 0; flipY = index < 2;
                x = flipX ? radius : -radius; y = flipY ? radius : -radius; tile = 0xaf;
                break;
            default:
                x = 1; y = 0;
                tile = id switch
                {
                    MapSpriteDefinitions.MarkerBoss => 0x8a,
                    MapSpriteDefinitions.StationEnergy => 0x8c,
                    MapSpriteDefinitions.StationMissile => 0x8b,
                    MapSpriteDefinitions.StationMap => 0x8e,
                    MapSpriteDefinitions.IndicatorBacking => 0x89,
                    _ => throw new ArgumentOutOfRangeException(nameof(id)),
                };
                break;
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

    /// <summary>Checks whether editable sprite-part data reproduces a supported marker's compiled geometry and OBJ attributes.</summary>
    /// <param name="id">Compiled map-sprite identifier defining the expected composition.</param>
    /// <param name="parts">Visual parts to compare in native draw order.</param>
    /// <returns><see langword="true"/> when every part matches position, size, palette, priority, tile, and flips; otherwise, <see langword="false"/>.</returns>
    internal static bool Matches(ushort id, SpriteVisualPart[] parts)
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

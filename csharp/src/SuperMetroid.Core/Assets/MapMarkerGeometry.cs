using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Regular map marker compositions from bank82:C232..C262,C298,C38C..C3A8,CF7C..CFE0.
/// Arrows join mirrored halves seven pixels apart; pulse corners expand at radius4..6.
/// Boss corners use the native seven-pixel horizontal/eight-pixel vertical spacing;
/// gunship halves are eight pixels apart. Native draw order is preserved before clipping.
/// All parts are small, inherit the caller palette, and use priority2 (arrows priority3).
/// </summary>
internal static class MapMarkerGeometry
{
    internal static int PartCount(ushort id) => id switch
    {
        MapSpriteDefinitions.ArrowRight or MapSpriteDefinitions.ArrowLeft or
        MapSpriteDefinitions.ArrowUp or MapSpriteDefinitions.ArrowDown or MapSpriteDefinitions.MarkerGunship => 2,
        MapSpriteDefinitions.MarkerDefeatedBoss or MapSpriteDefinitions.IndicatorFrame0 or
        MapSpriteDefinitions.IndicatorFrame1 or MapSpriteDefinitions.IndicatorFrame2 => 4,
        MapSpriteDefinitions.MarkerBoss or MapSpriteDefinitions.StationEnergy or MapSpriteDefinitions.StationMissile or
        MapSpriteDefinitions.StationMap or MapSpriteDefinitions.IndicatorBacking => 1,
        _ => 0,
    };

    internal static CompiledSpritePart Part(ushort id, int index)
    {
        if ((uint)index >= (uint)PartCount(id)) throw new ArgumentOutOfRangeException(nameof(index));
        int x, y, tile, priority = 2;
        bool flipX = false, flipY = false;
        switch (id)
        {
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

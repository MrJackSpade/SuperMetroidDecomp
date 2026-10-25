using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// The cartridge's menu spritemap ordinals naming each <see cref="MapSpriteId"/> (#1416): indices into the $82:C569
/// pointer table, which the map sprite, arrow and landmark extractors translate once at import.
/// </summary>
internal static class MapSpriteOrdinals
{
    /// <summary>The spritemap ordinal of each map sprite.</summary>
    internal static readonly NativeWordIdentities<MapSpriteId> Spritemaps = new(new Dictionary<MapSpriteId, ushort>
    {
        [MapSpriteId.ArrowRight] = 4,
        [MapSpriteId.ArrowLeft] = 5,
        [MapSpriteId.ArrowUp] = 6,
        [MapSpriteId.ArrowDown] = 7,
        [MapSpriteId.MarkerBoss] = 9,
        [MapSpriteId.StationEnergy] = 10,
        [MapSpriteId.StationMissile] = 11,
        [MapSpriteId.StationMap] = 78,
        [MapSpriteId.MarkerDefeatedBoss] = 98,
        [MapSpriteId.MarkerGunship] = 99,
        [MapSpriteId.IndicatorBacking] = 18,
        [MapSpriteId.IndicatorFrame0] = 95,
        [MapSpriteId.IndicatorFrame1] = 96,
        [MapSpriteId.IndicatorFrame2] = 97,
        [MapSpriteId.ElevatorCrateria] = 89,
        [MapSpriteId.ElevatorBrinstar] = 90,
        [MapSpriteId.ElevatorNorfair] = 91,
        [MapSpriteId.ElevatorWreckedShip] = 92,
        [MapSpriteId.ElevatorMaridia] = 93,
        [MapSpriteId.WorldTitle] = 56,
        [MapSpriteId.WorldCrateria] = 57,
        [MapSpriteId.WorldBrinstar] = 58,
        [MapSpriteId.WorldNorfair] = 59,
        [MapSpriteId.WorldWreckedShip] = 60,
        [MapSpriteId.WorldMaridia] = 61,
        [MapSpriteId.WorldTourian] = 62,
    });
}

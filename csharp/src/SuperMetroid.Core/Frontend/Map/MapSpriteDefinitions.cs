using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Presentation keys for the <see cref="MapSpriteId"/> map compositions.</summary>
public static class MapSpriteDefinitions
{
    /// <summary>Required named compositions in the map-sprite presentation document, one per <see cref="MapSpriteId"/>.</summary>
    public const int Count = 26;

    /// <summary>The named roles in the established presentation-document order.</summary>
    internal static readonly MapSpriteId[] FrameOrder =
    [
        MapSpriteId.ArrowRight,
        MapSpriteId.ArrowLeft,
        MapSpriteId.ArrowUp,
        MapSpriteId.ArrowDown,
        MapSpriteId.MarkerBoss,
        MapSpriteId.StationEnergy,
        MapSpriteId.StationMissile,
        MapSpriteId.StationMap,
        MapSpriteId.MarkerDefeatedBoss,
        MapSpriteId.MarkerGunship,
        MapSpriteId.IndicatorBacking,
        MapSpriteId.IndicatorFrame0,
        MapSpriteId.IndicatorFrame1,
        MapSpriteId.IndicatorFrame2,
        MapSpriteId.ElevatorCrateria,
        MapSpriteId.ElevatorBrinstar,
        MapSpriteId.ElevatorNorfair,
        MapSpriteId.ElevatorWreckedShip,
        MapSpriteId.ElevatorMaridia,
        MapSpriteId.WorldTitle,
        MapSpriteId.WorldCrateria,
        MapSpriteId.WorldBrinstar,
        MapSpriteId.WorldNorfair,
        MapSpriteId.WorldWreckedShip,
        MapSpriteId.WorldMaridia,
        MapSpriteId.WorldTourian,
    ];

    /// <summary>Selects the world-map label naming each key.</summary>
    internal static readonly IReadOnlyDictionary<AreaId, MapSpriteId> WorldLabels =
        new Dictionary<AreaId, MapSpriteId>
        {
            [AreaId.Crateria] = MapSpriteId.WorldCrateria,
            [AreaId.Brinstar] = MapSpriteId.WorldBrinstar,
            [AreaId.Norfair] = MapSpriteId.WorldNorfair,
            [AreaId.WreckedShip] = MapSpriteId.WorldWreckedShip,
            [AreaId.Maridia] = MapSpriteId.WorldMaridia,
            [AreaId.Tourian] = MapSpriteId.WorldTourian,
        };

    /// <summary>Resolves the exact case-sensitive presentation key for a map sprite, preserving semantic roles such as <c>Indicator.Frame0</c> and <c>World.Crateria</c>.</summary>
    internal static readonly IReadOnlyDictionary<MapSpriteId, string> Names =
        new Dictionary<MapSpriteId, string>
        {
            [MapSpriteId.ArrowRight] = "Arrow.Right",
            [MapSpriteId.ArrowLeft] = "Arrow.Left",
            [MapSpriteId.ArrowUp] = "Arrow.Up",
            [MapSpriteId.ArrowDown] = "Arrow.Down",
            [MapSpriteId.MarkerBoss] = "Marker.Boss",
            [MapSpriteId.StationEnergy] = "Station.Energy",
            [MapSpriteId.StationMissile] = "Station.Missile",
            [MapSpriteId.StationMap] = "Station.Map",
            [MapSpriteId.MarkerDefeatedBoss] = "Marker.DefeatedBoss",
            [MapSpriteId.MarkerGunship] = "Marker.Gunship",
            [MapSpriteId.IndicatorBacking] = "Indicator.Backing",
            [MapSpriteId.IndicatorFrame0] = "Indicator.Frame0",
            [MapSpriteId.IndicatorFrame1] = "Indicator.Frame1",
            [MapSpriteId.IndicatorFrame2] = "Indicator.Frame2",
            [MapSpriteId.ElevatorCrateria] = "Elevator.Crateria",
            [MapSpriteId.ElevatorBrinstar] = "Elevator.Brinstar",
            [MapSpriteId.ElevatorNorfair] = "Elevator.Norfair",
            [MapSpriteId.ElevatorWreckedShip] = "Elevator.WreckedShip",
            [MapSpriteId.ElevatorMaridia] = "Elevator.Maridia",
            [MapSpriteId.WorldTitle] = "World.Title",
            [MapSpriteId.WorldCrateria] = "World.Crateria",
            [MapSpriteId.WorldBrinstar] = "World.Brinstar",
            [MapSpriteId.WorldNorfair] = "World.Norfair",
            [MapSpriteId.WorldWreckedShip] = "World.WreckedShip",
            [MapSpriteId.WorldMaridia] = "World.Maridia",
            [MapSpriteId.WorldTourian] = "World.Tourian",
        };
}

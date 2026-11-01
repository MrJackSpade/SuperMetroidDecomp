using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Presentation keys for the <see cref="MapSpriteId"/> map compositions.</summary>
public static class MapSpriteDefinitions
{
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

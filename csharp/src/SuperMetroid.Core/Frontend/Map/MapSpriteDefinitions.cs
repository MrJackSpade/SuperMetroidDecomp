using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Presentation keys for the <see cref="MapSpriteId"/> map compositions.</summary>
public static class MapSpriteDefinitions
{
    /// <summary>Required named compositions in the map-sprite presentation document, one per <see cref="MapSpriteId"/>.</summary>
    public const int Count = 26;

    /// <summary>The map sprite sheet's frames, one per named role.</summary>
    public static IEnumerable<string> SheetFrames => Frames.Select(frame => frame.Name);

    /// <summary>Enumerates named roles in the established presentation-document order.</summary>
    public static IEnumerable<MapSpriteFrameName> Frames =>
    [
        new(MapSpriteId.ArrowRight, MapSpriteNames.Name(MapSpriteId.ArrowRight)),
        new(MapSpriteId.ArrowLeft, MapSpriteNames.Name(MapSpriteId.ArrowLeft)),
        new(MapSpriteId.ArrowUp, MapSpriteNames.Name(MapSpriteId.ArrowUp)),
        new(MapSpriteId.ArrowDown, MapSpriteNames.Name(MapSpriteId.ArrowDown)),
        new(MapSpriteId.MarkerBoss, MapSpriteNames.Name(MapSpriteId.MarkerBoss)),
        new(MapSpriteId.StationEnergy, MapSpriteNames.Name(MapSpriteId.StationEnergy)),
        new(MapSpriteId.StationMissile, MapSpriteNames.Name(MapSpriteId.StationMissile)),
        new(MapSpriteId.StationMap, MapSpriteNames.Name(MapSpriteId.StationMap)),
        new(MapSpriteId.MarkerDefeatedBoss, MapSpriteNames.Name(MapSpriteId.MarkerDefeatedBoss)),
        new(MapSpriteId.MarkerGunship, MapSpriteNames.Name(MapSpriteId.MarkerGunship)),
        new(MapSpriteId.IndicatorBacking, MapSpriteNames.Name(MapSpriteId.IndicatorBacking)),
        new(MapSpriteId.IndicatorFrame0, MapSpriteNames.Name(MapSpriteId.IndicatorFrame0)),
        new(MapSpriteId.IndicatorFrame1, MapSpriteNames.Name(MapSpriteId.IndicatorFrame1)),
        new(MapSpriteId.IndicatorFrame2, MapSpriteNames.Name(MapSpriteId.IndicatorFrame2)),
        new(MapSpriteId.ElevatorCrateria, MapSpriteNames.Name(MapSpriteId.ElevatorCrateria)),
        new(MapSpriteId.ElevatorBrinstar, MapSpriteNames.Name(MapSpriteId.ElevatorBrinstar)),
        new(MapSpriteId.ElevatorNorfair, MapSpriteNames.Name(MapSpriteId.ElevatorNorfair)),
        new(MapSpriteId.ElevatorWreckedShip, MapSpriteNames.Name(MapSpriteId.ElevatorWreckedShip)),
        new(MapSpriteId.ElevatorMaridia, MapSpriteNames.Name(MapSpriteId.ElevatorMaridia)),
        new(MapSpriteId.WorldTitle, MapSpriteNames.Name(MapSpriteId.WorldTitle)),
        new(MapSpriteId.WorldCrateria, MapSpriteNames.Name(MapSpriteId.WorldCrateria)),
        new(MapSpriteId.WorldBrinstar, MapSpriteNames.Name(MapSpriteId.WorldBrinstar)),
        new(MapSpriteId.WorldNorfair, MapSpriteNames.Name(MapSpriteId.WorldNorfair)),
        new(MapSpriteId.WorldWreckedShip, MapSpriteNames.Name(MapSpriteId.WorldWreckedShip)),
        new(MapSpriteId.WorldMaridia, MapSpriteNames.Name(MapSpriteId.WorldMaridia)),
        new(MapSpriteId.WorldTourian, MapSpriteNames.Name(MapSpriteId.WorldTourian)),
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

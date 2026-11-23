using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Presentation keys for the <see cref="MapSpriteId"/> map compositions.</summary>
public static partial class MapSpriteDefinitions
{
    /// <summary>The world-map label of each Zebes area.</summary>
    internal static readonly IdentityTable<ZebesArea, MapSpriteId> WorldLabels =
        new(new Dictionary<ZebesArea, MapSpriteId>
        {
            [ZebesArea.Crateria] = MapSpriteId.WorldCrateria,
            [ZebesArea.Brinstar] = MapSpriteId.WorldBrinstar,
            [ZebesArea.Norfair] = MapSpriteId.WorldNorfair,
            [ZebesArea.WreckedShip] = MapSpriteId.WorldWreckedShip,
            [ZebesArea.Maridia] = MapSpriteId.WorldMaridia,
            [ZebesArea.Tourian] = MapSpriteId.WorldTourian,
        });

    /// <summary>Resolves the exact case-sensitive presentation key for a map sprite, preserving semantic roles such as <c>Indicator.Small</c> and <c>World.Crateria</c>.</summary>
    internal static readonly IdentityTable<MapSpriteId, string> Names =
        new(new Dictionary<MapSpriteId, string>
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
            [MapSpriteId.IndicatorSmall] = "Indicator.Small",
            [MapSpriteId.IndicatorMiddle] = "Indicator.Middle",
            [MapSpriteId.IndicatorLarge] = "Indicator.Large",
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
        });
}

using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Presentation keys for the <see cref="MapSpriteId"/> map compositions.</summary>
public static class MapSpriteDefinitions
{
    /// <summary>Required named compositions in the map-sprite presentation document: 26 sparse native identities, not the size of the complete $82:C569 pointer table or a bound on its IDs.</summary>
    public const int Count = 26;

    /// <summary>Enumerates named roles in the established presentation-document order.</summary>
    public static IEnumerable<(MapSpriteId NativeId, string Name)> Frames
    {
        get
        {
            yield return (MapSpriteId.ArrowRight, Name(MapSpriteId.ArrowRight));
            yield return (MapSpriteId.ArrowLeft, Name(MapSpriteId.ArrowLeft));
            yield return (MapSpriteId.ArrowUp, Name(MapSpriteId.ArrowUp));
            yield return (MapSpriteId.ArrowDown, Name(MapSpriteId.ArrowDown));
            yield return (MapSpriteId.MarkerBoss, Name(MapSpriteId.MarkerBoss));
            yield return (MapSpriteId.StationEnergy, Name(MapSpriteId.StationEnergy));
            yield return (MapSpriteId.StationMissile, Name(MapSpriteId.StationMissile));
            yield return (MapSpriteId.StationMap, Name(MapSpriteId.StationMap));
            yield return (MapSpriteId.MarkerDefeatedBoss, Name(MapSpriteId.MarkerDefeatedBoss));
            yield return (MapSpriteId.MarkerGunship, Name(MapSpriteId.MarkerGunship));
            yield return (MapSpriteId.IndicatorBacking, Name(MapSpriteId.IndicatorBacking));
            yield return (MapSpriteId.IndicatorFrame0, Name(MapSpriteId.IndicatorFrame0));
            yield return (MapSpriteId.IndicatorFrame1, Name(MapSpriteId.IndicatorFrame1));
            yield return (MapSpriteId.IndicatorFrame2, Name(MapSpriteId.IndicatorFrame2));
            yield return (MapSpriteId.ElevatorCrateria, Name(MapSpriteId.ElevatorCrateria));
            yield return (MapSpriteId.ElevatorBrinstar, Name(MapSpriteId.ElevatorBrinstar));
            yield return (MapSpriteId.ElevatorNorfair, Name(MapSpriteId.ElevatorNorfair));
            yield return (MapSpriteId.ElevatorWreckedShip, Name(MapSpriteId.ElevatorWreckedShip));
            yield return (MapSpriteId.ElevatorMaridia, Name(MapSpriteId.ElevatorMaridia));
            yield return (MapSpriteId.WorldTitle, Name(MapSpriteId.WorldTitle));
            yield return (MapSpriteId.WorldCrateria, Name(MapSpriteId.WorldCrateria));
            yield return (MapSpriteId.WorldBrinstar, Name(MapSpriteId.WorldBrinstar));
            yield return (MapSpriteId.WorldNorfair, Name(MapSpriteId.WorldNorfair));
            yield return (MapSpriteId.WorldWreckedShip, Name(MapSpriteId.WorldWreckedShip));
            yield return (MapSpriteId.WorldMaridia, Name(MapSpriteId.WorldMaridia));
            yield return (MapSpriteId.WorldTourian, Name(MapSpriteId.WorldTourian));
        }
    }
    /// <summary>Selects the world-map label naming <paramref name="area"/>; the labels follow
    /// PLANET ZEBES in area order at $82:C5DB..$C5E5.</summary>
    public static MapSpriteId WorldLabel(AreaId area) => area switch
    {
        AreaId.Crateria => MapSpriteId.WorldCrateria,
        AreaId.Brinstar => MapSpriteId.WorldBrinstar,
        AreaId.Norfair => MapSpriteId.WorldNorfair,
        AreaId.WreckedShip => MapSpriteId.WorldWreckedShip,
        AreaId.Maridia => MapSpriteId.WorldMaridia,
        AreaId.Tourian => MapSpriteId.WorldTourian,
        AreaId.Ceres => throw new ArgumentOutOfRangeException(nameof(area), $"Area {area} has no world-map label."),
        _ => throw new ArgumentOutOfRangeException(nameof(area), $"Area {area} has no world-map label."),
    };

    /// <summary>Resolves the exact case-sensitive presentation key for a named native menu spritemap, preserving semantic roles such as <c>Indicator.Frame0</c> and <c>World.Crateria</c>.</summary>
    /// <param name="id">Unscaled native $82:C569 table ordinal belonging to this map-only subset.</param>
    /// <returns>The stable key used to validate, serialize, and bind the installed map-sprite composition.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The identity has no named map-presentation role.</exception>
    public static string Name(MapSpriteId id) => id switch
    {
        MapSpriteId.ArrowRight => "Arrow.Right",
        MapSpriteId.ArrowLeft => "Arrow.Left",
        MapSpriteId.ArrowUp => "Arrow.Up",
        MapSpriteId.ArrowDown => "Arrow.Down",
        MapSpriteId.MarkerBoss => "Marker.Boss",
        MapSpriteId.StationEnergy => "Station.Energy",
        MapSpriteId.StationMissile => "Station.Missile",
        MapSpriteId.StationMap => "Station.Map",
        MapSpriteId.MarkerDefeatedBoss => "Marker.DefeatedBoss",
        MapSpriteId.MarkerGunship => "Marker.Gunship",
        MapSpriteId.IndicatorBacking => "Indicator.Backing",
        MapSpriteId.IndicatorFrame0 => "Indicator.Frame0",
        MapSpriteId.IndicatorFrame1 => "Indicator.Frame1",
        MapSpriteId.IndicatorFrame2 => "Indicator.Frame2",
        MapSpriteId.ElevatorCrateria => "Elevator.Crateria",
        MapSpriteId.ElevatorBrinstar => "Elevator.Brinstar",
        MapSpriteId.ElevatorNorfair => "Elevator.Norfair",
        MapSpriteId.ElevatorWreckedShip => "Elevator.WreckedShip",
        MapSpriteId.ElevatorMaridia => "Elevator.Maridia",
        MapSpriteId.WorldTitle => "World.Title",
        MapSpriteId.WorldCrateria => "World.Crateria",
        MapSpriteId.WorldBrinstar => "World.Brinstar",
        MapSpriteId.WorldNorfair => "World.Norfair",
        MapSpriteId.WorldWreckedShip => "World.WreckedShip",
        MapSpriteId.WorldMaridia => "World.Maridia",
        MapSpriteId.WorldTourian => "World.Tourian",
        _ => throw new ArgumentOutOfRangeException(nameof(id), $"Undefined MapSpriteId {(ushort)id:X4}."),
    };
}

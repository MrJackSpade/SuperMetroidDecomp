using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Presentation keys for the <see cref="MapSpriteId"/> map compositions.</summary>
public static class MapSpriteDefinitions
{
    /// <summary>Required named compositions in the map-sprite presentation document: 26 sparse native identities, not the size of the complete $82:C569 pointer table or a bound on its IDs.</summary>
    public const int Count = 26;

    /// <summary>The map sprite sheet's frames, one per named role.</summary>
    public static IEnumerable<string> SheetFrames => Frames.Select(frame => frame.Name);

    /// <summary>Enumerates named roles in the established presentation-document order.</summary>
    public static IEnumerable<MapSpriteFrameName> Frames
    {
        get
        {
            yield return new MapSpriteFrameName(MapSpriteId.ArrowRight, Name(MapSpriteId.ArrowRight));
            yield return new MapSpriteFrameName(MapSpriteId.ArrowLeft, Name(MapSpriteId.ArrowLeft));
            yield return new MapSpriteFrameName(MapSpriteId.ArrowUp, Name(MapSpriteId.ArrowUp));
            yield return new MapSpriteFrameName(MapSpriteId.ArrowDown, Name(MapSpriteId.ArrowDown));
            yield return new MapSpriteFrameName(MapSpriteId.MarkerBoss, Name(MapSpriteId.MarkerBoss));
            yield return new MapSpriteFrameName(MapSpriteId.StationEnergy, Name(MapSpriteId.StationEnergy));
            yield return new MapSpriteFrameName(MapSpriteId.StationMissile, Name(MapSpriteId.StationMissile));
            yield return new MapSpriteFrameName(MapSpriteId.StationMap, Name(MapSpriteId.StationMap));
            yield return new MapSpriteFrameName(MapSpriteId.MarkerDefeatedBoss, Name(MapSpriteId.MarkerDefeatedBoss));
            yield return new MapSpriteFrameName(MapSpriteId.MarkerGunship, Name(MapSpriteId.MarkerGunship));
            yield return new MapSpriteFrameName(MapSpriteId.IndicatorBacking, Name(MapSpriteId.IndicatorBacking));
            yield return new MapSpriteFrameName(MapSpriteId.IndicatorFrame0, Name(MapSpriteId.IndicatorFrame0));
            yield return new MapSpriteFrameName(MapSpriteId.IndicatorFrame1, Name(MapSpriteId.IndicatorFrame1));
            yield return new MapSpriteFrameName(MapSpriteId.IndicatorFrame2, Name(MapSpriteId.IndicatorFrame2));
            yield return new MapSpriteFrameName(MapSpriteId.ElevatorCrateria, Name(MapSpriteId.ElevatorCrateria));
            yield return new MapSpriteFrameName(MapSpriteId.ElevatorBrinstar, Name(MapSpriteId.ElevatorBrinstar));
            yield return new MapSpriteFrameName(MapSpriteId.ElevatorNorfair, Name(MapSpriteId.ElevatorNorfair));
            yield return new MapSpriteFrameName(MapSpriteId.ElevatorWreckedShip, Name(MapSpriteId.ElevatorWreckedShip));
            yield return new MapSpriteFrameName(MapSpriteId.ElevatorMaridia, Name(MapSpriteId.ElevatorMaridia));
            yield return new MapSpriteFrameName(MapSpriteId.WorldTitle, Name(MapSpriteId.WorldTitle));
            yield return new MapSpriteFrameName(MapSpriteId.WorldCrateria, Name(MapSpriteId.WorldCrateria));
            yield return new MapSpriteFrameName(MapSpriteId.WorldBrinstar, Name(MapSpriteId.WorldBrinstar));
            yield return new MapSpriteFrameName(MapSpriteId.WorldNorfair, Name(MapSpriteId.WorldNorfair));
            yield return new MapSpriteFrameName(MapSpriteId.WorldWreckedShip, Name(MapSpriteId.WorldWreckedShip));
            yield return new MapSpriteFrameName(MapSpriteId.WorldMaridia, Name(MapSpriteId.WorldMaridia));
            yield return new MapSpriteFrameName(MapSpriteId.WorldTourian, Name(MapSpriteId.WorldTourian));
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

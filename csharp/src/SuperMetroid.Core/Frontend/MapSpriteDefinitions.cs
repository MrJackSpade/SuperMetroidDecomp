using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>Native $82:C569 menu spritemap ordinals that map presentation draws.</summary>
public enum MapSpriteId : ushort
{
    /// <summary>$82:C571 pointer for Arrow.Right.</summary>
    ArrowRight = 0x4,

    /// <summary>$82:C573 pointer for Arrow.Left.</summary>
    ArrowLeft = 0x5,

    /// <summary>$82:C575 pointer for Arrow.Up.</summary>
    ArrowUp = 0x6,

    /// <summary>$82:C577 pointer for Arrow.Down.</summary>
    ArrowDown = 0x7,

    /// <summary>$82:C57B pointer for Marker.Boss.</summary>
    MarkerBoss = 0x9,

    /// <summary>$82:C57D pointer for Station.Energy.</summary>
    StationEnergy = 0xa,

    /// <summary>$82:C57F pointer for Station.Missile.</summary>
    StationMissile = 0xb,

    /// <summary>$82:C605 pointer for Station.Map.</summary>
    StationMap = 0x4e,

    /// <summary>$82:C62D pointer for Marker.DefeatedBoss.</summary>
    MarkerDefeatedBoss = 0x62,

    /// <summary>$82:C62F pointer for Marker.Gunship.</summary>
    MarkerGunship = 0x63,

    /// <summary>$82:C58D pointer for Indicator.Backing.</summary>
    IndicatorBacking = 0x12,

    /// <summary>$82:C627 pointer for Indicator.Frame0.</summary>
    IndicatorFrame0 = 0x5f,

    /// <summary>$82:C629 pointer for Indicator.Frame1.</summary>
    IndicatorFrame1 = 0x60,

    /// <summary>$82:C62B pointer for Indicator.Frame2.</summary>
    IndicatorFrame2 = 0x61,

    /// <summary>$82:C61B pointer for Elevator.Crateria.</summary>
    ElevatorCrateria = 0x59,

    /// <summary>$82:C61D pointer for Elevator.Brinstar.</summary>
    ElevatorBrinstar = 0x5a,

    /// <summary>$82:C61F pointer for Elevator.Norfair.</summary>
    ElevatorNorfair = 0x5b,

    /// <summary>$82:C621 pointer for Elevator.WreckedShip.</summary>
    ElevatorWreckedShip = 0x5c,

    /// <summary>$82:C623 pointer for Elevator.Maridia.</summary>
    ElevatorMaridia = 0x5d,

    /// <summary>$82:C5D9 pointer for World.Title.</summary>
    WorldTitle = 0x38,

    /// <summary>$82:C5DB pointer for World.Crateria.</summary>
    WorldCrateria = 0x39,

    /// <summary>$82:C5DD pointer for World.Brinstar.</summary>
    WorldBrinstar = 0x3a,

    /// <summary>$82:C5DF pointer for World.Norfair.</summary>
    WorldNorfair = 0x3b,

    /// <summary>$82:C5E1 pointer for World.WreckedShip.</summary>
    WorldWreckedShip = 0x3c,

    /// <summary>$82:C5E3 pointer for World.Maridia.</summary>
    WorldMaridia = 0x3d,

    /// <summary>$82:C5E5 pointer for World.Tourian.</summary>
    WorldTourian = 0x3e,
}

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

namespace SuperMetroid.Core.Frontend;

/// <summary>Semantic identities for $82:C569 menu spritemaps used by map presentation.</summary>
public static class MapSpriteDefinitions
{
    /// <summary>Required named compositions in the map-sprite presentation document: 26 sparse native identities, not the size of the complete $82:C569 pointer table or a bound on its IDs.</summary>
    public const int Count = 26;
    /// <summary>$82:C571 pointer for Arrow.Right.</summary>
    public const ushort ArrowRight = 0x4;
    /// <summary>$82:C573 pointer for Arrow.Left.</summary>
    public const ushort ArrowLeft = 0x5;
    /// <summary>$82:C575 pointer for Arrow.Up.</summary>
    public const ushort ArrowUp = 0x6;
    /// <summary>$82:C577 pointer for Arrow.Down.</summary>
    public const ushort ArrowDown = 0x7;
    /// <summary>$82:C57B pointer for Marker.Boss.</summary>
    public const ushort MarkerBoss = 0x9;
    /// <summary>$82:C57D pointer for Station.Energy.</summary>
    public const ushort StationEnergy = 0xa;
    /// <summary>$82:C57F pointer for Station.Missile.</summary>
    public const ushort StationMissile = 0xb;
    /// <summary>$82:C605 pointer for Station.Map.</summary>
    public const ushort StationMap = 0x4e;
    /// <summary>$82:C62D pointer for Marker.DefeatedBoss.</summary>
    public const ushort MarkerDefeatedBoss = 0x62;
    /// <summary>$82:C62F pointer for Marker.Gunship.</summary>
    public const ushort MarkerGunship = 0x63;
    /// <summary>$82:C58D pointer for Indicator.Backing.</summary>
    public const ushort IndicatorBacking = 0x12;
    /// <summary>$82:C627 pointer for Indicator.Frame0.</summary>
    public const ushort IndicatorFrame0 = 0x5f;
    /// <summary>$82:C629 pointer for Indicator.Frame1.</summary>
    public const ushort IndicatorFrame1 = 0x60;
    /// <summary>$82:C62B pointer for Indicator.Frame2.</summary>
    public const ushort IndicatorFrame2 = 0x61;
    /// <summary>$82:C61B pointer for Elevator.Crateria.</summary>
    public const ushort ElevatorCrateria = 0x59;
    /// <summary>$82:C61D pointer for Elevator.Brinstar.</summary>
    public const ushort ElevatorBrinstar = 0x5a;
    /// <summary>$82:C61F pointer for Elevator.Norfair.</summary>
    public const ushort ElevatorNorfair = 0x5b;
    /// <summary>$82:C621 pointer for Elevator.WreckedShip.</summary>
    public const ushort ElevatorWreckedShip = 0x5c;
    /// <summary>$82:C623 pointer for Elevator.Maridia.</summary>
    public const ushort ElevatorMaridia = 0x5d;
    /// <summary>$82:C5D9 pointer for World.Title.</summary>
    public const ushort WorldTitle = 0x38;
    /// <summary>$82:C5DB pointer for World.Crateria.</summary>
    public const ushort WorldCrateria = 0x39;
    /// <summary>$82:C5DD pointer for World.Brinstar.</summary>
    public const ushort WorldBrinstar = 0x3a;
    /// <summary>$82:C5DF pointer for World.Norfair.</summary>
    public const ushort WorldNorfair = 0x3b;
    /// <summary>$82:C5E1 pointer for World.WreckedShip.</summary>
    public const ushort WorldWreckedShip = 0x3c;
    /// <summary>$82:C5E3 pointer for World.Maridia.</summary>
    public const ushort WorldMaridia = 0x3d;
    /// <summary>$82:C5E5 pointer for World.Tourian.</summary>
    public const ushort WorldTourian = 0x3e;

    /// <summary>Enumerates named roles in the established presentation-document order.</summary>
    public static IEnumerable<(ushort NativeId, string Name)> Frames
    {
        get
        {
            yield return (ArrowRight, Name(ArrowRight));
            yield return (ArrowLeft, Name(ArrowLeft));
            yield return (ArrowUp, Name(ArrowUp));
            yield return (ArrowDown, Name(ArrowDown));
            yield return (MarkerBoss, Name(MarkerBoss));
            yield return (StationEnergy, Name(StationEnergy));
            yield return (StationMissile, Name(StationMissile));
            yield return (StationMap, Name(StationMap));
            yield return (MarkerDefeatedBoss, Name(MarkerDefeatedBoss));
            yield return (MarkerGunship, Name(MarkerGunship));
            yield return (IndicatorBacking, Name(IndicatorBacking));
            yield return (IndicatorFrame0, Name(IndicatorFrame0));
            yield return (IndicatorFrame1, Name(IndicatorFrame1));
            yield return (IndicatorFrame2, Name(IndicatorFrame2));
            yield return (ElevatorCrateria, Name(ElevatorCrateria));
            yield return (ElevatorBrinstar, Name(ElevatorBrinstar));
            yield return (ElevatorNorfair, Name(ElevatorNorfair));
            yield return (ElevatorWreckedShip, Name(ElevatorWreckedShip));
            yield return (ElevatorMaridia, Name(ElevatorMaridia));
            yield return (WorldTitle, Name(WorldTitle));
            yield return (WorldCrateria, Name(WorldCrateria));
            yield return (WorldBrinstar, Name(WorldBrinstar));
            yield return (WorldNorfair, Name(WorldNorfair));
            yield return (WorldWreckedShip, Name(WorldWreckedShip));
            yield return (WorldMaridia, Name(WorldMaridia));
            yield return (WorldTourian, Name(WorldTourian));
        }
    }
    /// <summary>Tests whether a native menu spritemap identity belongs to the map-presentation subset; other valid bank-$82 menu sprites are not accepted.</summary>
    /// <param name="id">Unscaled $82:C569 table ordinal, not its two-byte pointer offset or an ordinal among the 26 named map compositions.</param>
    /// <returns>True for a named map role, otherwise false without throwing.</returns>
    public static bool Contains(ushort id) => NameOrNull(id) is not null;
    /// <summary>Resolves the exact case-sensitive presentation key for a named native menu spritemap, preserving semantic roles such as <c>Indicator.Frame0</c> and <c>World.Crateria</c>.</summary>
    /// <param name="id">Unscaled native $82:C569 table ordinal belonging to this map-only subset.</param>
    /// <returns>The stable key used to validate, serialize, and bind the installed map-sprite composition.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The identity has no named map-presentation role.</exception>
    public static string Name(ushort id) => NameOrNull(id)
        ?? throw new ArgumentOutOfRangeException(nameof(id), $"Menu sprite {id:X4} is not a map frame.");

    private static string? NameOrNull(ushort id) => id switch
    {
        ArrowRight => "Arrow.Right",
        ArrowLeft => "Arrow.Left",
        ArrowUp => "Arrow.Up",
        ArrowDown => "Arrow.Down",
        MarkerBoss => "Marker.Boss",
        StationEnergy => "Station.Energy",
        StationMissile => "Station.Missile",
        StationMap => "Station.Map",
        MarkerDefeatedBoss => "Marker.DefeatedBoss",
        MarkerGunship => "Marker.Gunship",
        IndicatorBacking => "Indicator.Backing",
        IndicatorFrame0 => "Indicator.Frame0",
        IndicatorFrame1 => "Indicator.Frame1",
        IndicatorFrame2 => "Indicator.Frame2",
        ElevatorCrateria => "Elevator.Crateria",
        ElevatorBrinstar => "Elevator.Brinstar",
        ElevatorNorfair => "Elevator.Norfair",
        ElevatorWreckedShip => "Elevator.WreckedShip",
        ElevatorMaridia => "Elevator.Maridia",
        WorldTitle => "World.Title",
        WorldCrateria => "World.Crateria",
        WorldBrinstar => "World.Brinstar",
        WorldNorfair => "World.Norfair",
        WorldWreckedShip => "World.WreckedShip",
        WorldMaridia => "World.Maridia",
        WorldTourian => "World.Tourian",
        _ => null,
    };
}

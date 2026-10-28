namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The map screens' sprites: arrows, markers, station icons, the position indicator and the elevator and world labels.
/// Values are arbitrary (#1416); extraction's <c>MapSpriteOrdinals</c> names the cartridge spritemap each one is read from.
/// </summary>
public enum MapSpriteId
{
    /// <summary>The right scroll arrow.</summary>
    ArrowRight = 1,

    /// <summary>The left scroll arrow.</summary>
    ArrowLeft,

    /// <summary>The upward scroll arrow.</summary>
    ArrowUp,

    /// <summary>The downward scroll arrow.</summary>
    ArrowDown,

    /// <summary>A boss's marker.</summary>
    MarkerBoss,

    /// <summary>An energy-refill station's icon.</summary>
    StationEnergy,

    /// <summary>A missile-refill station's icon.</summary>
    StationMissile,

    /// <summary>A map station's icon.</summary>
    StationMap,

    /// <summary>The cross drawn over a defeated boss's marker.</summary>
    MarkerDefeatedBoss,

    /// <summary>The gunship's marker.</summary>
    MarkerGunship,

    /// <summary>The backing drawn behind the position indicator on alternate loops.</summary>
    IndicatorBacking,

    /// <summary>The position indicator's first frame.</summary>
    IndicatorFrame0,

    /// <summary>The position indicator's second frame.</summary>
    IndicatorFrame1,

    /// <summary>The position indicator's third frame.</summary>
    IndicatorFrame2,

    /// <summary>The elevator label naming Crateria.</summary>
    ElevatorCrateria,

    /// <summary>The elevator label naming Brinstar.</summary>
    ElevatorBrinstar,

    /// <summary>The elevator label naming Norfair.</summary>
    ElevatorNorfair,

    /// <summary>The elevator label naming the Wrecked Ship.</summary>
    ElevatorWreckedShip,

    /// <summary>The elevator label naming Maridia.</summary>
    ElevatorMaridia,

    /// <summary>The world map's PLANET ZEBES title.</summary>
    WorldTitle,

    /// <summary>The world map's Crateria label.</summary>
    WorldCrateria,

    /// <summary>The world map's Brinstar label.</summary>
    WorldBrinstar,

    /// <summary>The world map's Norfair label.</summary>
    WorldNorfair,

    /// <summary>The world map's Wrecked Ship label.</summary>
    WorldWreckedShip,

    /// <summary>The world map's Maridia label.</summary>
    WorldMaridia,

    /// <summary>The world map's Tourian label.</summary>
    WorldTourian,
}

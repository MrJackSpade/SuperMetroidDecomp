namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Exclusive meanings of behaviour $47-$4D after a special-solid block selects the station
/// access dispatcher. Left/right describe the side of the station containing the block.
/// </summary>
public enum StationAccessBehavior
{
    /// <summary>Behaviour $47: the map-station access block on the station's right side.</summary>
    MapRight = 0x47,
    /// <summary>Behaviour $48: the map-station access block on the station's left side.</summary>
    MapLeft = 0x48,
    /// <summary>Behaviour $49: the energy-recharge access block on the station's right side.</summary>
    EnergyRight = 0x49,
    /// <summary>Behaviour $4A: the energy-recharge access block on the station's left side.</summary>
    EnergyLeft = 0x4a,
    /// <summary>Behaviour $4B: the missile-recharge access block on the station's right side.</summary>
    MissileRight = 0x4b,
    /// <summary>Behaviour $4C: the missile-recharge access block on the station's left side.</summary>
    MissileLeft = 0x4c,
    /// <summary>Behaviour $4D: the floor access block that activates the save station.</summary>
    SaveFloor = 0x4d,
}

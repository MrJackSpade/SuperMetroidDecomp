namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Exclusive meanings of BTS $47-$4D after a special-solid block selects the station
/// access dispatcher. Left/right describe the side of the station containing the block.
/// </summary>
public enum StationAccessBehavior : byte
{
    /// <summary>BTS $47: the map-station access block on the station's right side.</summary>
    MapRight = 0x47,
    /// <summary>BTS $48: the map-station access block on the station's left side.</summary>
    MapLeft = 0x48,
    /// <summary>BTS $49: the energy-recharge access block on the station's right side.</summary>
    EnergyRight = 0x49,
    /// <summary>BTS $4A: the energy-recharge access block on the station's left side.</summary>
    EnergyLeft = 0x4a,
    /// <summary>BTS $4B: the missile-recharge access block on the station's right side.</summary>
    MissileRight = 0x4b,
    /// <summary>BTS $4C: the missile-recharge access block on the station's left side.</summary>
    MissileLeft = 0x4c,
    /// <summary>BTS $4D: the floor access block that activates the save station.</summary>
    SaveFloor = 0x4d,
}

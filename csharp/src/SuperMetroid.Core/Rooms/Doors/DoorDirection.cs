namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The direction Samus travels through a door. The values are arbitrary; the import pipeline decodes the cartridge's
/// orientation byte into them (#1414).
/// </summary>
public enum DoorDirection
{
    /// <summary>The destination lies to the right.</summary>
    Right = 1,
    /// <summary>The destination lies to the left.</summary>
    Left = 2,
    /// <summary>The destination lies below.</summary>
    Down = 3,
    /// <summary>The destination lies above.</summary>
    Up = 4,
}

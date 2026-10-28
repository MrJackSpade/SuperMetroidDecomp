namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What closes behind Samus on arrival through a door. The values are arbitrary; the import pipeline decodes the
/// cartridge's orientation byte into them (#1414).
/// </summary>
public enum DoorClosingBehavior
{
    /// <summary>Nothing closes.</summary>
    None = 1,
    /// <summary>The blue door-closing PLM facing the direction spawns.</summary>
    BlueDoorCloses = 2,
    /// <summary>The gate that closes in the first escape room spawns.</summary>
    EscapeGateCloses = 3,
}

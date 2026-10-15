namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What closes behind Samus on arrival: bits 2-3 of the orientation byte, which selects a
/// row of <c>Door_Closing_PLMs</c> (<c>$8F:E68A</c>, spawned by <c>$82:E8F9</c>).
/// </summary>
public enum DoorClosingBehavior : byte
{
    /// <summary>Entries 0-3 are <c>$0000</c>: nothing closes.</summary>
    None = 0,
    /// <summary>Entries 4-7 spawn the blue door-closing PLM facing the direction.</summary>
    BlueDoorCloses = 1,
    /// <summary>Entries 8-B spawn <c>PLMEntries_gateThatClosesInEscapeRoom1_PLM</c>.</summary>
    EscapeGateCloses = 2,
}

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The two zero-destination elevator pseudo-door entries of the room door lists.</summary>
public enum ElevatorPseudoDoor
{
    /// <summary>
    /// The elevator pseudo-door that the elevator rooms share. Its zero destination is what marks
    /// it as a pseudo-door; the remaining values overlap the first physical door record because collision never consumes them
    /// for this entry.
    /// </summary>
    Shared,

    /// <summary>
    /// The elevator pseudo-door of the Maridia elevator room's door 3 and the first Tourian room's door 2. This second
    /// zero-destination elevator sentinel overlaps the Bowling Alley room's door 0; it is a list entry, not a terminator.
    /// </summary>
    MaridiaTourian,
}

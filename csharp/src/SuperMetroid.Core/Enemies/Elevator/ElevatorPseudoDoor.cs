namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>The two zero-destination elevator pseudo-door entries of the room door lists.</summary>
public enum ElevatorPseudoDoor
{
    /// <summary>
    /// The shared elevator pseudo-door, <c>Door_VariousRooms_Elevator</c>. Its zero destination word is the native
    /// discriminator; the remaining bytes overlap the first physical door record because collision never consumes them
    /// for this entry.
    /// </summary>
    Shared,

    /// <summary>
    /// <c>Door_MaridiaElev_3_TourianFirst_2</c>. This second zero-destination elevator sentinel overlaps
    /// Door_BowlingAlley_0; it is a list entry, not a terminator.
    /// </summary>
    MaridiaTourian,
}

namespace SuperMetroid.Core.Rooms;

/// <summary>The two zero-destination elevator pseudo-door entries of the bank-$83 door lists, valued by pointer.</summary>
public enum ElevatorPseudoDoorPointer : ushort
{
    /// <summary>
    /// Shared elevator pseudo-door at <c>$83:88FC</c>. Its zero destination word is the
    /// native discriminator; the remaining bytes intentionally overlap the first physical
    /// door record because collision never consumes them for this special entry.
    /// </summary>
    Shared = 0x88fc,

    /// <summary>
    /// <c>Door_MaridiaElev_3_TourianFirst_2</c> at $83:A18A. This second zero-destination
    /// elevator sentinel overlaps Door_BowlingAlley_0; it is a list entry, not a terminator.
    /// </summary>
    MaridiaTourian = 0xa18a,
}

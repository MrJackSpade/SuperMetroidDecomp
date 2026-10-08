namespace SuperMetroid.Core.Rooms;

/// <summary>Native bank-$83 door-header ranges retained for cartridge parity diagnostics.</summary>
public static class DoorHeaderRomData
{
    /// <summary>SNES bank containing all physical retail door records.</summary>
    public const int BankAddress = 0x830000;

    /// <summary>
    /// Shared elevator pseudo-door at <c>$83:88FC</c>. Its zero destination word is the
    /// native discriminator; the remaining bytes intentionally overlap the first physical
    /// door record because collision never consumes them for this special entry.
    /// </summary>
    public const ushort ElevatorPseudoDoorPointer = 0x88fc;

    /// <summary>
    /// <c>Door_MaridiaElev_3_TourianFirst_2</c> at $83:A18A. This second zero-destination
    /// elevator sentinel overlaps Door_BowlingAlley_0; it is a list entry, not a terminator.
    /// </summary>
    public const ushort MaridiaTourianElevatorPseudoDoorPointer = 0xa18a;
}

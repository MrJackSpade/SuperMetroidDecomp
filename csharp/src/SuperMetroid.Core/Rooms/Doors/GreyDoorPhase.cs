namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a grey door cap is doing (#1554).</summary>
internal enum GreyDoorPhase
{
    /// <summary>The cap was just placed: it takes its first look at its door on its next update (#2560).</summary>
    Placed,
    /// <summary>The cap has looked at its door, is closed and waits for its condition.</summary>
    Locked,
    /// <summary>The condition holds: the cap flashes until a shot opens it.</summary>
    Flashing,
    /// <summary>A shot opens the cap, which then leaves the room.</summary>
    Opening,
    /// <summary>The cap closes behind Samus, then locks.</summary>
    Closing,
    /// <summary>
    /// Bomb Torizo's door, closing behind Samus, is held open, again and again, until she has Bombs, then closes
    /// (#2482).
    /// </summary>
    HeldOpen,
}

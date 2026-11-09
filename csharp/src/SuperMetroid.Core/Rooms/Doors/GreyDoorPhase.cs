namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a grey door cap is doing (#1554).</summary>
internal enum GreyDoorPhase
{
    /// <summary>The cap is closed and waits for its condition; one just placed takes its first look on its next update.</summary>
    Locked,
    /// <summary>The condition holds: the cap flashes until a shot opens it.</summary>
    Flashing,
    /// <summary>A shot opens the cap, which then leaves the room.</summary>
    Opening,
    /// <summary>The cap closes behind Samus, then locks.</summary>
    Closing,
}

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a blue door cap is doing (#1554).</summary>
internal enum BlueDoorCapPhase
{
    /// <summary>A shot opens the cap, which then leaves the room.</summary>
    Opening,
    /// <summary>The cap closes behind Samus, then closes for good.</summary>
    Closing,
    /// <summary>The cap is shootable again and shows its closed image, then leaves the room.</summary>
    Closed,
}

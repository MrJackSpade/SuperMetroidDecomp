namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a yellow, green or red door cap is doing (#1554).</summary>
internal enum ColoredDoorPhase
{
    /// <summary>The cap was just placed: it takes its first look at its door on its next update (#2560).</summary>
    Placed,
    /// <summary>The cap has looked at its door, is closed and waits for its colour's shot.</summary>
    Waiting,
    /// <summary>A hit short of the colour's count flashes the cap before it waits again.</summary>
    Flashing,
    /// <summary>The hit that reached the colour's count opens the cap, which then leaves the room.</summary>
    Opening,
    /// <summary>The cap closes behind Samus, then waits.</summary>
    Closing,
}

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Debugger-visible phase of one resident colored-door actor.</summary>
public enum ColoredDoorPhase : byte
{
    /// <summary>Resident cap has drawn its closed color and waits for projectile triggers while its instruction list sleeps.</summary>
    Waiting,
    /// <summary>A nonfatal accepted hit runs the shared blue/colored flash list before returning the resident actor to its sleeping wait.</summary>
    Flashing,
    /// <summary>The accepted-hit threshold has been reached; the shared interpreter runs the opening list after recording the door's persistent opened bit.</summary>
    Opening,
    /// <summary>Room setup found the persistent opened bit and will replace the colored cap with the closed blue-door draw and BTS, then release the slot.</summary>
    ConvertToBlue,
    /// <summary>Room-entry closing draws run through the shared interpreter before handing the same resident slot back to its initial waiting list.</summary>
    Closing,
}

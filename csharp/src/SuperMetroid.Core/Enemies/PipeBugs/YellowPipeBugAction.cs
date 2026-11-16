namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// The behaviours a yellow Pipe Bug runs (#2063).
/// Its values are arbitrary.
/// </summary>
public enum YellowPipeBugAction
{
    /// <summary>Waits for Samus to enter a yellow Pipe Bug's trigger region.</summary>
    WaitForSamus,
    /// <summary>Delays before a yellow Pipe Bug begins straight flight.</summary>
    EmergenceDelay,
    /// <summary>Flies a yellow Pipe Bug straight the way it emerged (<see cref="YellowPipeBugState.FlightDirection"/>).</summary>
    Fly,
    /// <summary>Performs the yellow Pipe Bug's arc, moving on the way it emerged.</summary>
    Arc,
}

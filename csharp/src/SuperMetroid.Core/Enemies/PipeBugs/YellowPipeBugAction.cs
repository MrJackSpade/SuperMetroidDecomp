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
    /// <summary>Flies a yellow Pipe Bug straight the way it emerged, the way its placement faces, until it passes Samus.</summary>
    Fly,
    /// <summary>Flies a yellow Pipe Bug straight on after its one arc, which it never takes again (#2560).</summary>
    FlyAfterArc,
    /// <summary>
    /// The first half of the yellow Pipe Bug's one arc, moving on the way it emerged (#2473): it dips downward ever more
    /// slowly as its place on the speed curve moves back from <see cref="PipeBugDefinitions.YellowArcStart"/>, until the
    /// place would pass the curve's start.
    /// </summary>
    ArcDipping,
    /// <summary>
    /// The second half of the yellow Pipe Bug's arc, moving on the way it emerged (#2473): it climbs back ever faster as its
    /// place on the speed curve moves on, until it is back above its spawn height.
    /// </summary>
    ArcClimbing,
}

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// The behaviours a Norfair Pipe Bug formation member runs (#2063).
/// Its values are arbitrary.
/// </summary>
public enum NorfairPipeBugAction
{
    /// <summary>Waits for the five-member Norfair formation to become ready.</summary>
    WaitForFormation,
    /// <summary>Waits for Samus to trigger the Norfair formation.</summary>
    WaitForSamus,
    /// <summary>Raises one Norfair formation member toward Samus.</summary>
    Rise,
    /// <summary>Advances the formation leader's stagger counter.</summary>
    LeaderStagger,
    /// <summary>Moves a formation member by its role's stagger offset from where its rise ended.</summary>
    Stagger,
    /// <summary>Flies a released Norfair formation member the way it departed (<see cref="NorfairPipeBugState.FlightDirection"/>).</summary>
    Fly,
    /// <summary>Waits for one Norfair member's stagger target.</summary>
    WaitForStagger,
}

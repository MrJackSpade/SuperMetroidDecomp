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
    /// <summary>Moves the near upper formation member to its stagger height.</summary>
    UpperNearStagger,
    /// <summary>Moves the far upper formation member to its stagger height.</summary>
    UpperFarStagger,
    /// <summary>Moves the near lower formation member to its stagger height.</summary>
    LowerNearStagger,
    /// <summary>Moves the far lower formation member to its stagger height.</summary>
    LowerFarStagger,
    /// <summary>Flies a released Norfair formation member left.</summary>
    FlyLeft,
    /// <summary>Flies a released Norfair formation member right.</summary>
    FlyRight,
    /// <summary>Waits for one Norfair member's stagger target.</summary>
    WaitForStagger,
}

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// The behaviours of all three Pipe Bug state machines; an unknown branch fails explicitly instead of
/// silently selecting a plausible-looking behaviour.
/// Its values are arbitrary.
/// </summary>
public enum PipeBugEnemyFunction
{
    /// <summary>Waits until a Brinstar Pipe Bug's origin is on screen.</summary>
    BrinstarWaitUntilOnScreen = 1,
    /// <summary>Waits for Samus to enter the Brinstar emergence region.</summary>
    BrinstarWaitForSamus,
    /// <summary>Raises a Brinstar Pipe Bug out of its pipe.</summary>
    BrinstarEmerge,
    /// <summary>Flies a Brinstar Pipe Bug horizontally until it leaves the screen.</summary>
    BrinstarFlyHorizontally,
    /// <summary>Holds a hidden Brinstar Pipe Bug before rearming it.</summary>
    BrinstarRespawnDelay,

    /// <summary>Waits for the five-member Norfair formation to become ready.</summary>
    NorfairWaitForFormation,
    /// <summary>Waits for Samus to trigger the Norfair formation.</summary>
    NorfairWaitForSamus,
    /// <summary>Raises one Norfair formation member toward Samus.</summary>
    NorfairRise,
    /// <summary>Advances the formation leader's stagger counter.</summary>
    NorfairLeaderStagger,
    /// <summary>Moves the near upper formation member to its stagger height.</summary>
    NorfairUpperNearStagger,
    /// <summary>Moves the far upper formation member to its stagger height.</summary>
    NorfairUpperFarStagger,
    /// <summary>Moves the near lower formation member to its stagger height.</summary>
    NorfairLowerNearStagger,
    /// <summary>Moves the far lower formation member to its stagger height.</summary>
    NorfairLowerFarStagger,
    /// <summary>Flies a released Norfair formation member left.</summary>
    NorfairFlyLeft,
    /// <summary>Flies a released Norfair formation member right.</summary>
    NorfairFlyRight,
    /// <summary>Waits for one Norfair member's stagger target.</summary>
    NorfairWaitForStagger,

    /// <summary>Waits for Samus to enter a yellow Pipe Bug's trigger region.</summary>
    YellowWaitForSamus,
    /// <summary>Delays before a yellow Pipe Bug begins straight flight.</summary>
    YellowEmergenceDelay,
    /// <summary>Flies a yellow Pipe Bug left.</summary>
    YellowFlyLeft,
    /// <summary>Flies a yellow Pipe Bug right.</summary>
    YellowFlyRight,
    /// <summary>Performs the leftward yellow Pipe Bug arc.</summary>
    YellowArcLeft,
    /// <summary>Performs the rightward yellow Pipe Bug arc.</summary>
    YellowArcRight,
}

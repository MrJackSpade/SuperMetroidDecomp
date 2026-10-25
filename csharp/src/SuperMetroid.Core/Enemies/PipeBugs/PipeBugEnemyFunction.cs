namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// The routines of all three Pipe Bug state machines; an unknown branch fails explicitly instead of
/// silently selecting a plausible-looking behaviour.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum PipeBugEnemyFunction : ushort
{
    /// <summary>$B3:8880, waits until a Brinstar Pipe Bug's origin is on screen.</summary>
    BrinstarWaitUntilOnScreen = 1,
    /// <summary>$B3:8890, waits for Samus to enter the Brinstar emergence region.</summary>
    BrinstarWaitForSamus,
    /// <summary>$B3:88E3, raises a Brinstar Pipe Bug out of its pipe.</summary>
    BrinstarEmerge,
    /// <summary>$B3:891C, flies a Brinstar Pipe Bug horizontally until it leaves the screen.</summary>
    BrinstarFlyHorizontally,
    /// <summary>$B3:897E, holds a hidden Brinstar Pipe Bug before rearming it.</summary>
    BrinstarRespawnDelay,

    /// <summary>$B3:8BCD, waits for the five-member Norfair formation to become ready.</summary>
    NorfairWaitForFormation,
    /// <summary>$B3:8BFF, waits for Samus to trigger the Norfair formation.</summary>
    NorfairWaitForSamus,
    /// <summary>$B3:8CA6, raises one Norfair formation member toward Samus.</summary>
    NorfairRise,
    /// <summary>$B3:8CFF, advances the formation leader's stagger counter.</summary>
    NorfairLeaderStagger,
    /// <summary>$B3:8D0C, moves the near upper formation member to its stagger height.</summary>
    NorfairUpperNearStagger,
    /// <summary>$B3:8D4E, moves the far upper formation member to its stagger height.</summary>
    NorfairUpperFarStagger,
    /// <summary>$B3:8D90, moves the near lower formation member to its stagger height.</summary>
    NorfairLowerNearStagger,
    /// <summary>$B3:8DD2, moves the far lower formation member to its stagger height.</summary>
    NorfairLowerFarStagger,
    /// <summary>$B3:8E14, flies a released Norfair formation member left.</summary>
    NorfairFlyLeft,
    /// <summary>$B3:8E35, flies a released Norfair formation member right.</summary>
    NorfairFlyRight,
    /// <summary>$B3:8E5A, waits for one Norfair member's stagger target.</summary>
    NorfairWaitForStagger,

    /// <summary>$B3:8FB5, waits for Samus to enter a yellow Pipe Bug's trigger region.</summary>
    YellowWaitForSamus,
    /// <summary>$B3:8FF5, delays before a yellow Pipe Bug begins straight flight.</summary>
    YellowEmergenceDelay,
    /// <summary>$B3:9028, flies a yellow Pipe Bug left.</summary>
    YellowFlyLeft,
    /// <summary>$B3:90BD, flies a yellow Pipe Bug right.</summary>
    YellowFlyRight,
    /// <summary>$B3:915A, performs the leftward yellow Pipe Bug arc.</summary>
    YellowArcLeft,
    /// <summary>$B3:91D8, performs the rightward yellow Pipe Bug arc.</summary>
    YellowArcRight,
}

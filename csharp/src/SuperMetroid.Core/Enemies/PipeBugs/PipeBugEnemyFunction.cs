namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// Literal bank-$B3 function words used by all three Pipe Bug state machines. These are
/// addresses, not a host-designed progression enum: retaining the ROM values makes the
/// debugger state directly comparable with enemy WRAM and makes an unknown branch fail
/// explicitly instead of silently selecting a plausible-looking behavior.
/// </summary>
public enum PipeBugEnemyFunction : ushort
{
    /// <summary>$B3:8880, waits until a Brinstar Pipe Bug's origin is on screen.</summary>
    BrinstarWaitUntilOnScreen = 0x8880,
    /// <summary>$B3:8890, waits for Samus to enter the Brinstar emergence region.</summary>
    BrinstarWaitForSamus = 0x8890,
    /// <summary>$B3:88E3, raises a Brinstar Pipe Bug out of its pipe.</summary>
    BrinstarEmerge = 0x88e3,
    /// <summary>$B3:891C, flies a Brinstar Pipe Bug horizontally until it leaves the screen.</summary>
    BrinstarFlyHorizontally = 0x891c,
    /// <summary>$B3:897E, holds a hidden Brinstar Pipe Bug before rearming it.</summary>
    BrinstarRespawnDelay = 0x897e,

    /// <summary>$B3:8BCD, waits for the five-member Norfair formation to become ready.</summary>
    NorfairWaitForFormation = 0x8bcd,
    /// <summary>$B3:8BFF, waits for Samus to trigger the Norfair formation.</summary>
    NorfairWaitForSamus = 0x8bff,
    /// <summary>$B3:8CA6, raises one Norfair formation member toward Samus.</summary>
    NorfairRise = 0x8ca6,
    /// <summary>$B3:8CFF, advances the formation leader's stagger counter.</summary>
    NorfairLeaderStagger = 0x8cff,
    /// <summary>$B3:8D0C, moves the near upper formation member to its stagger height.</summary>
    NorfairUpperNearStagger = 0x8d0c,
    /// <summary>$B3:8D4E, moves the far upper formation member to its stagger height.</summary>
    NorfairUpperFarStagger = 0x8d4e,
    /// <summary>$B3:8D90, moves the near lower formation member to its stagger height.</summary>
    NorfairLowerNearStagger = 0x8d90,
    /// <summary>$B3:8DD2, moves the far lower formation member to its stagger height.</summary>
    NorfairLowerFarStagger = 0x8dd2,
    /// <summary>$B3:8E14, flies a released Norfair formation member left.</summary>
    NorfairFlyLeft = 0x8e14,
    /// <summary>$B3:8E35, flies a released Norfair formation member right.</summary>
    NorfairFlyRight = 0x8e35,
    /// <summary>$B3:8E5A, waits for one Norfair member's stagger target.</summary>
    NorfairWaitForStagger = 0x8e5a,

    /// <summary>$B3:8FB5, waits for Samus to enter a yellow Pipe Bug's trigger region.</summary>
    YellowWaitForSamus = 0x8fb5,
    /// <summary>$B3:8FF5, delays before a yellow Pipe Bug begins straight flight.</summary>
    YellowEmergenceDelay = 0x8ff5,
    /// <summary>$B3:9028, flies a yellow Pipe Bug left.</summary>
    YellowFlyLeft = 0x9028,
    /// <summary>$B3:90BD, flies a yellow Pipe Bug right.</summary>
    YellowFlyRight = 0x90bd,
    /// <summary>$B3:915A, performs the leftward yellow Pipe Bug arc.</summary>
    YellowArcLeft = 0x915a,
    /// <summary>$B3:91D8, performs the rightward yellow Pipe Bug arc.</summary>
    YellowArcRight = 0x91d8,
}

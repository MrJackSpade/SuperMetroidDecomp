namespace SuperMetroid.Core.Game;

/// <summary>
/// Native function pointers stored in Polyp's <c>$0FA8,x</c> word. These are sequential AI
/// states, not combinable flags, so retaining their cartridge addresses is the most useful
/// debugger representation.
/// </summary>
public enum PolypEnemyFunction : ushort
{
    /// <summary>Function_Polyp_WaitForSamusToGetNear, $A2:B596: tests strict modular distances below 64 whole pixels on both axes, selecting the shoot function without spawning until the next AI update.</summary>
    WaitingForSamus = 0xb596,
    /// <summary>Function_Polyp_ShootRock, $A2:B5B2: consumes separate shared-RNG samples for horizontal velocity, initial quadratic Y-speed selector, and cooldown; requests one bank-$86 lava rock and enters cooldown even if the projectile pool is full.</summary>
    ShootingRock = 0xb5b2,
    /// <summary>Function_Polyp_Cooldown, $A2:B5EA: decrements the native word once per AI update and rearms proximity checking only when the signed result is negative, so an initial nonnegative count N waits N+1 updates.</summary>
    Cooldown = 0xb5ea,
}

/// <summary>Typed view of Polyp's function and cooldown words.</summary>
public sealed class PolypEnemyState
{
    /// <summary>Live enemy slot whose variable words hold the function pointer and cooldown.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Wraps a live slot so AI transitions and countdown edits update its native variable words.</summary>
    internal PolypEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Native <c>$0FA8,x</c> indirect main-AI pointer.</summary>
    public PolypEnemyFunction Function
    {
        get => (PolypEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Native <c>$0FAA,x</c>, decremented through $FFFF before rearming.</summary>
    public NativeCountdown CooldownTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableB);
        internal set => _slot.VariableB = value.RawValue;
    }
}

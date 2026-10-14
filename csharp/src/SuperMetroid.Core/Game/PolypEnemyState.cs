namespace SuperMetroid.Core.Game;

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

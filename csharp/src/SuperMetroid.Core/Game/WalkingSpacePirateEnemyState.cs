namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-facing projection of one walking Space Pirate's native state. The patrol limits
/// deliberately remain wrapped 16-bit world coordinates because population parameter two
/// is added/subtracted without clamping by <c>$B2:FD02</c>.
/// </summary>
public sealed class WalkingSpacePirateEnemyState
{
    /// <summary>Native enemy slot backing the walking Pirate's function and patrol posts.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Binds the typed walking-Pirate view to its existing enemy slot.</summary>
    internal WalkingSpacePirateEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Bank-$B2 function address stored in native enemy variable A.</summary>
    public WalkingSpacePirateFunction Function
    {
        get => (WalkingSpacePirateFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Left patrol post stored in native variable E.</summary>
    public ushort LeftPostXPosition
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Right patrol post stored in native variable F.</summary>
    public ushort RightPostXPosition
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Successful allocations from the actor's three-shot firing animations.</summary>
    public int SpawnedLaserCount { get; internal set; }
}

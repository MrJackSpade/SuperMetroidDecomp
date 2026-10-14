namespace SuperMetroid.Core.Game;

/// <summary>
/// Named projection of Rio's six native AI variables. The properties deliberately remain
/// backed by the physical enemy slot so a debugger watch agrees with WRAM $0FA8-$0FB2.
/// </summary>
public sealed class RioEnemyState
{
    /// <summary>Native enemy slot backing Rio's saved velocity, function, motion, and animation words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Binds typed Rio state to its existing enemy slot.</summary>
    internal RioEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Horizontal velocity retained while the hovering phase temporarily stops X.</summary>
    public ushort SavedHorizontalVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Gets the bank-$A2 function pointer corresponding to native $0FAA,x Rio.function, backed by the physical slot and dispatched after the main AI's RNG advance.</summary>
    public RioEnemyFunction Function
    {
        get => (RioEnemyFunction)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    /// <summary>Signed 8.8 velocity: the horizontal word is native common variable D and the vertical word variable C; the horizontal word feeds the 16.16 collision helper.</summary>
    public EightEightVector<RoomPixels> Velocity
    {
        get => new(new SnesSignedEightEight(_slot.VariableD), new SnesSignedEightEight(_slot.VariableC));
        internal set
        {
            _slot.VariableD = value.X.RawValue;
            _slot.VariableC = value.Y.RawValue;
        }
    }

    /// <summary>Handshake set by instruction $A2:BBC3 and consumed by main AI.</summary>
    public bool AnimationFinished
    {
        get => _slot.VariableE != 0;
        internal set => _slot.VariableE = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Last instruction-list address installed by Rio_6; zero at room load.</summary>
    public ushort InstalledInstructionList
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }
}

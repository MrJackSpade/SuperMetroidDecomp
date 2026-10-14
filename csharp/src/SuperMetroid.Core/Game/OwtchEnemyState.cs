namespace SuperMetroid.Core.Game;

/// <summary>
/// Typed view of Owtch's six ordinary enemy words and three extended WRAM words. Keeping
/// the native split velocities visible makes debugger watches directly comparable with
/// <c>$0FA8-$0FB2</c> and the extended <c>$7E:7800-$7804</c> record.
/// </summary>
public sealed class OwtchEnemyState
{
    /// <summary>Ordinary enemy record supplying Owtch's common velocity, state, and burial words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Attaches the typed Owtch view to its physical enemy slot.</summary>
    internal OwtchEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Native <c>$0FA8,x</c>, fractional half of positive 16.16 speed.</summary>
    public ushort RightSubvelocity { get; internal set; }

    /// <summary>Native <c>$0FAA,x</c>, whole half of positive 16.16 speed.</summary>
    public ushort RightVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Native <c>$0FAC,x</c>, fractional half of negative 16.16 speed.</summary>
    public ushort LeftSubvelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Native <c>$0FAE,x</c>, whole half of negative 16.16 speed.</summary>
    public ushort LeftVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Native <c>$0FB0,x</c> state/table index.</summary>
    public OwtchBehaviorState Behavior
    {
        get => (OwtchBehaviorState)_slot.VariableD;
        internal set => _slot.VariableD = (ushort)value;
    }

    /// <summary>Native <c>$0FB2,x</c>, current zero-to-sixteen burial depth.</summary>
    public ushort SinkYOffset
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Native extended word <c>$7E:7800,x</c>.</summary>
    public NativeCountdown UndergroundTimer { get; internal set; }

    /// <summary>Native extended word <c>$7E:7802,x</c>.</summary>
    public ushort MinimumXPosition { get; internal set; }

    /// <summary>Native extended word <c>$7E:7804,x</c>.</summary>
    public ushort MaximumXPosition { get; internal set; }
}

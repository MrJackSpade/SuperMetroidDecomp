using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>
/// Values stored in Owtch's native <c>$0FB0,x</c> direction/state word. Unlike a simple
/// facing enum, this word also dispatches the underground animation. The all-ones value is
/// a real one-frame table-underflow state produced by retail code at the left patrol bound.
/// </summary>
public enum OwtchBehaviorState : ushort
{
    /// <summary>Index 0, $A2:A4B0 Function_Owtch_0_MovingLeft: applies the negative 16.16 patrol velocity, then samples the burial gate; crossing the minimum X bound decrements the index to $FFFF.</summary>
    MovingLeft = 0,
    /// <summary>Index 1, $A2:A4D9 Function_Owtch_1_MovingRight: applies the positive 16.16 patrol velocity and samples the burial gate, returning to index 0 at the maximum X bound.</summary>
    MovingRight = 1,
    /// <summary>Index 2, $A2:A502 Function_Owtch_2_Underground: remains buried while the wrapping 16-bit timer decrements; reaching zero selects the rising state.</summary>
    Underground = 2,
    /// <summary>Index 3, $A2:A517 Function_Owtch_3_Sinking: moves down one world pixel per AI update to depth sixteen, then reloads the parameter-selected underground timer.</summary>
    Sinking = 3,
    /// <summary>Index 4, $A2:A53E Function_Owtch_4_Rising: moves up one world pixel per AI update until burial depth reaches zero, then chooses left or right from RNG bit $0100.</summary>
    Rising = 4,
    /// <summary>Retail index -1 produced at the left patrol bound; table underflow calls $A2:A49D SetOwtchInitialInstListPointer_MovingRight, whose animation instruction restores index 1.</summary>
    LeftBoundaryUnderflow = 0xffff,
}

/// <summary>
/// Typed view of Owtch's six ordinary enemy words and three extended WRAM words. Keeping
/// the native split velocities visible makes debugger watches directly comparable with
/// <c>$0FA8-$0FB2</c> and the extended <c>$7E:7800-$7804</c> record.
/// </summary>
public sealed class OwtchEnemyState
{
    private readonly RoomEnemySlot _slot;

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

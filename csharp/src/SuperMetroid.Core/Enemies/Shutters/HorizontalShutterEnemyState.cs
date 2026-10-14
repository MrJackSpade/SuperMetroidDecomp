namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>Typed state for the cartridge's complete, though retail-unused, horizontal shutter.</summary>
public sealed class HorizontalShutterEnemyState
{
    /// <summary>Backs native variable A-F access for one horizontal shutter slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates the typed view over a room-owned horizontal shutter slot.</summary>
    internal HorizontalShutterEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Enemy variable A's bank-$A2 dispatcher pointer for the retail-unused horizontal shutter, retained across wait, movement, and endpoint-rest updates.</summary>
    public HorizontalShutterFunction Function
    {
        get => (HorizontalShutterFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Variable B: wrapping AI-update countdown; initial wait tests for zero after decrement, whereas endpoint rests test for signed underflow.</summary>
    public NativeCountdown FunctionTimer { get => NativeCountdown.FromRaw(_slot.VariableB); internal set => _slot.VariableB = value.RawValue; }
    /// <summary>Variable C: low word of rightward 16.16 pixels-per-update velocity, also supplied to an overlapping Samus's extra-X displacement.</summary>
    public ushort RightSubvelocity { get => _slot.VariableC; internal set => _slot.VariableC = value; }
    /// <summary>Variable D: signed whole word of rightward 16.16 pixels-per-update velocity.</summary>
    public short RightVelocity { get => unchecked((short)_slot.VariableD); internal set => _slot.VariableD = unchecked((ushort)value); }
    /// <summary>Variable E: fractional low word of signed leftward 16.16 pixels-per-update velocity.</summary>
    public ushort LeftSubvelocity { get => _slot.VariableE; internal set => _slot.VariableE = value; }
    /// <summary>Variable F: signed whole word of leftward 16.16 pixels-per-update velocity.</summary>
    public short LeftVelocity { get => unchecked((short)_slot.VariableF); internal set => _slot.VariableF = unchecked((ushort)value); }

    /// <summary>Low spawn instruction byte selecting an eight-byte common speed record, used for the rightward and leftward velocity pairs.</summary>
    public ushort SpeedTableIndex { get; internal set; }
    /// <summary>High spawn instruction byte: zero starts leftward; nonzero starts rightward.</summary>
    public ushort PrimaryDirection { get; internal set; }
    /// <summary>Low extra-properties byte giving the left endpoint's rest count in sixteen-update units; $FF selects an inert endpoint.</summary>
    public ushort MovedLeftRestParameter { get; internal set; }
    /// <summary>High extra-properties byte giving the right endpoint's rest count in sixteen-update units; $FF selects an inert endpoint.</summary>
    public ushort MovedRightRestParameter { get; internal set; }
    /// <summary>Low parameter-1 byte: 0 timer, 1 horizontal proximity, 2 immediate, 3 one-shot reaction, or 4 repeatable reaction activation.</summary>
    public ushort TriggerMode { get; internal set; }
    /// <summary>High parameter-1 byte specifying whole-pixel X endpoint separation; the native storage calls this field YDistance.</summary>
    public ushort TravelDistance { get; internal set; }
    /// <summary>Parameter 2: initial AI-update countdown or strict absolute horizontal pixel-distance threshold, according to trigger mode.</summary>
    public ushort HorizontalProximityOrWaitTime { get; internal set; }
    /// <summary>Twice the trigger mode, giving a byte offset 0 through 8 into the native five-entry initial-function table.</summary>
    public ushort InitialFunctionTableOffset { get; internal set; }
    /// <summary>Left endpoint's wrapping rest countdown, the parameter shifted left four; $0FF0 installs the inert function.</summary>
    public ushort MovedLeftRestTime { get; internal set; }
    /// <summary>Right endpoint's wrapping rest countdown, the parameter shifted left four; $0FF0 installs the inert function.</summary>
    public ushort MovedRightRestTime { get; internal set; }
    /// <summary>Initially clear mode-3 activation latch; the first eligible reaction sets it permanently for this enemy lifetime.</summary>
    public bool ShotActivated { get; internal set; }
    /// <summary>The shutter's whole-pixel X snapshot immediately before its current fixed-point move.</summary>
    public ushort PreviousXPosition { get; internal set; }
    /// <summary>Left whole-pixel travel boundary derived from spawn X and travel distance; leftward motion stops upon reaching or passing it.</summary>
    public ushort MinimumXPosition { get; internal set; }
    /// <summary>Right whole-pixel travel boundary derived from spawn X and travel distance; rightward motion stops upon reaching or passing it.</summary>
    public ushort MaximumXPosition { get; internal set; }
    /// <summary>Whether Samus overlaps the shutter on its leading side before movement, enabling fixed-point extra-X displacement and opposing-input ejection.</summary>
    public bool MovingSamus { get; internal set; }
    /// <summary>Alternating reaction direction, initialized as primary direction XOR 1 and toggled before choosing left for zero or right for nonzero.</summary>
    public ushort ReactionDirection { get; internal set; }
    /// <summary>Samus's whole-pixel X captured after each main-AI dispatch, including inert updates, preserving the native extended-word snapshot.</summary>
    public ushort PreviousSamusXPosition { get; internal set; }
    /// <summary>Samus's fractional X low word captured after every main-AI dispatch alongside the whole-pixel snapshot.</summary>
    public ushort PreviousSamusXSubposition { get; internal set; }
}

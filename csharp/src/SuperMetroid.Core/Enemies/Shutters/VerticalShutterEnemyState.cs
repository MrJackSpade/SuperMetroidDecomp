using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// Typed host representation of the vertical shutter's ordinary and extended enemy words.
/// Values not backed by variables A-F correspond one-for-one with the native $7800/$8800
/// arrays; keeping them explicit avoids another opaque bag of numbered scratch fields.
/// </summary>
public sealed class VerticalShutterEnemyState
{
    /// <summary>Backs native variable A-F access for one vertical shutter slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates the typed view over a room-owned vertical shutter slot.</summary>
    internal VerticalShutterEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<ShutterProgram> Program { get; set; }

    /// <summary>Enemy variable A's bank-$A2 dispatcher pointer, shared by shootable/destroyable shutters and the Kamer vertical platform.</summary>
    public VerticalShutterFunction Function
    {
        get => (VerticalShutterFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Variable B: wrapping AI-update countdown; initial waiting activates at zero, while endpoint resting resumes on signed underflow.</summary>
    public NativeCountdown FunctionTimer { get => NativeCountdown.FromRaw(_slot.VariableB); internal set => _slot.VariableB = value.RawValue; }
    /// <summary>Variable C: low word of downward 16.16 pixels-per-update velocity, whose carry contributes to the whole-pixel movement.</summary>
    public ushort DownSubvelocity { get => _slot.VariableC; internal set => _slot.VariableC = value; }
    /// <summary>Variable D: signed high word of downward 16.16 pixels-per-update velocity from the common linear-speed table.</summary>
    public short DownVelocity { get => unchecked((short)_slot.VariableD); internal set => _slot.VariableD = unchecked((ushort)value); }
    /// <summary>Variable E: low word of the signed upward 16.16 velocity; it is added with the negative whole word, not independently negated.</summary>
    public ushort UpSubvelocity { get => _slot.VariableE; internal set => _slot.VariableE = value; }
    /// <summary>Variable F: signed high word of upward 16.16 pixels-per-update velocity.</summary>
    public short UpVelocity { get => unchecked((short)_slot.VariableF); internal set => _slot.VariableF = unchecked((ushort)value); }

    /// <summary>Low spawn instruction byte selecting an eight-byte common speed record containing downward and upward velocity pairs.</summary>
    public ushort SpeedTableIndex { get; internal set; }
    /// <summary>High spawn instruction byte: zero starts upward; nonzero starts downward.</summary>
    public ushort PrimaryDirection { get; internal set; }
    /// <summary>Low extra-properties byte specifying the upward endpoint's rest time in units of sixteen AI updates; $FF selects the inert endpoint.</summary>
    public ushort MovedUpRestParameter { get; internal set; }
    /// <summary>High extra-properties byte specifying the downward endpoint's rest time in units of sixteen AI updates; $FF selects the inert endpoint.</summary>
    public ushort MovedDownRestParameter { get; internal set; }
    /// <summary>Low parameter-1 byte: 0 timer, 1 proximity, 2 immediate, 3 one-shot reaction, or 4 repeatable reaction activation.</summary>
    public ushort TriggerMode { get; internal set; }
    /// <summary>High parameter-1 byte giving whole-pixel separation of the two Y endpoints.</summary>
    public ushort TravelDistance { get; internal set; }
    /// <summary>Parameter 2: initial AI-update wait in timer mode, or strict absolute-X pixel-distance threshold in proximity mode.</summary>
    public ushort HorizontalProximityOrWaitTime { get; internal set; }
    /// <summary>Twice the trigger mode: byte offset 0, 2, 4, 6, or 8 into the native initial-function pointer table.</summary>
    public ushort InitialFunctionTableOffset { get; internal set; }
    /// <summary>Upward endpoint countdown loaded from the rest parameter shifted left four; $0FF0 selects the inert function instead of counting down.</summary>
    public ushort MovedUpRestTime { get; internal set; }
    /// <summary>Downward endpoint countdown loaded from the rest parameter shifted left four; $0FF0 selects the inert function instead of counting down.</summary>
    public ushort MovedDownRestTime { get; internal set; }
    /// <summary>Whether the pre-movement riding test found Samus supported by this shutter; enables whole-pixel extra-Y carry for the current movement.</summary>
    public bool MovingSamus { get; internal set; }
    /// <summary>One-shot reaction latch, initially clear and set by the first mode-3 reaction so later reactions cannot activate movement again.</summary>
    public bool ShotActivated { get; internal set; }
    /// <summary>The shutter's whole-pixel Y saved before its current move to derive the rider's wrapped displacement.</summary>
    public ushort PreviousYPosition { get; internal set; }
    /// <summary>Upper whole-pixel travel boundary derived from spawn Y and travel distance; upward movement stops upon reaching or passing it.</summary>
    public ushort MinimumYPosition { get; internal set; }
    /// <summary>Lower whole-pixel travel boundary derived from spawn Y and travel distance; downward movement stops upon reaching or passing it.</summary>
    public ushort MaximumYPosition { get; internal set; }
    /// <summary>Next reaction's direction selector, initially the primary direction: zero moves up, nonzero moves down, then bit zero is toggled.</summary>
    public ushort ReactionDirection { get; internal set; }
}

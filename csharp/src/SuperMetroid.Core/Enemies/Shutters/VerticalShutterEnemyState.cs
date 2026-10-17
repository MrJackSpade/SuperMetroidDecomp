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

    /// <summary>Spawn speed selector, 0 through 255: the common linear-speed record containing the downward and upward velocities.</summary>
    public int SpeedTableIndex { get; internal set; }
    /// <summary>Spawn direction selector, 0 through 255: zero starts upward; any other value starts downward.</summary>
    public int PrimaryDirection { get; internal set; }
    /// <summary>Upward endpoint's rest length in sixteen-update units, 0 through 255; 255 selects the inert endpoint.</summary>
    public int MovedUpRestParameter { get; internal set; }
    /// <summary>Downward endpoint's rest length in sixteen-update units, 0 through 255; 255 selects the inert endpoint.</summary>
    public int MovedDownRestParameter { get; internal set; }
    /// <summary>Population trigger mode, validated to 0 through 4: timer, proximity, immediate, one-shot reaction, or repeatable reaction activation.</summary>
    public int TriggerMode { get; internal set; }
    /// <summary>Whole-pixel separation of the two Y endpoints, 0 through 255.</summary>
    public int TravelDistance { get; internal set; }
    /// <summary>Parameter 2, 0 through 65535: initial AI-update wait in timer mode, or strict absolute-X pixel-distance threshold in proximity mode.</summary>
    public int HorizontalProximityOrWaitTime { get; internal set; }
    /// <summary>Twice the trigger mode, 0 through 8: the even offset selecting one of the five initial functions.</summary>
    public int InitialFunctionTableOffset { get; internal set; }
    /// <summary>Upward endpoint's rest length in AI updates, sixteen per parameter unit (0 through 4080); 4080 selects the inert function instead of counting down.</summary>
    public int MovedUpRestTime { get; internal set; }
    /// <summary>Downward endpoint's rest length in AI updates, sixteen per parameter unit (0 through 4080); 4080 selects the inert function instead of counting down.</summary>
    public int MovedDownRestTime { get; internal set; }
    /// <summary>Whether the pre-movement riding test found Samus supported by this shutter; enables whole-pixel extra-Y carry for the current movement.</summary>
    public bool MovingSamus { get; internal set; }
    /// <summary>One-shot reaction latch, initially clear and set by the first mode-3 reaction so later reactions cannot activate movement again.</summary>
    public bool ShotActivated { get; internal set; }
    /// <summary>The shutter's whole-pixel position saved before its current move to derive the rider's wrapped vertical displacement.</summary>
    public Position16<RoomPixels> PreviousPosition { get; internal set; }
    /// <summary>Upper whole-pixel travel boundary derived from spawn Y and travel distance; upward movement stops upon reaching or passing it.</summary>
    public ushort MinimumYPosition { get; internal set; }
    /// <summary>Lower whole-pixel travel boundary derived from spawn Y and travel distance; downward movement stops upon reaching or passing it.</summary>
    public ushort MaximumYPosition { get; internal set; }
    /// <summary>Next reaction's direction selector, 0 through 255, initially the primary direction: zero moves up, any other value moves down, then bit zero is toggled.</summary>
    public int ReactionDirection { get; internal set; }
}

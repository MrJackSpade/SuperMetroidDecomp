using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A8 function words stored in a morph-ball eye's native variable F. These values are
/// intentionally the cartridge addresses: debugger watches can therefore be compared with
/// WRAM traces without translating a host-only state number back into a ROM routine.
/// </summary>
public enum MorphBallEyeAiFunction : ushort
{
    /// <summary>Native <c>Function_Eye_WaitForSamusToGetNear</c> at <c>$A8:90F1</c>; waits until Samus is strictly within 128 room pixels on both axes before starting the opening animation.</summary>
    WaitForSamus = 0x90f1,
    /// <summary>Native <c>Function_Eye_Activating</c> at <c>$A8:912E</c>; decrements the 32-update opening countdown, then requests the reveal beam and begins tracking Samus.</summary>
    Activating = 0x912e,
    /// <summary>Native <c>Function_Eye_Active</c> at <c>$A8:9160</c>; aims toward Samus and selects angle-dependent eye artwork while within 176 horizontal and 128 vertical pixels, otherwise begins closing.</summary>
    Active = 0x9160,
    /// <summary>Native <c>Function_Eye_Deactivating</c> at <c>$A8:91CE</c>; waits for the 32-update closing countdown to reach zero or wrap negative, then resumes proximity waiting.</summary>
    Deactivating = 0x91ce,
    /// <summary>Native <c>RTL_A891DC</c> at <c>$A8:91DC</c> for the decorative mount selected by population parameter 2's sign bit; it does not track Samus or own the beam.</summary>
    MountNoOp = 0x91dc,
}

/// <summary>
/// Typed aliases for the four common enemy words used by retail enemy <c>$E6BF</c>.
/// The mount and eye body share a definition, but parameter two's sign selects their role.
/// </summary>
public sealed class MorphBallEyeEnemyState
{
    /// <summary>Native enemy slot whose variables C through F hold this eye's state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects the eye's typed state over an existing enemy slot.</summary>
    internal MorphBallEyeEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Native <c>Eye.activatedFlag</c> in variable C. The bank-$88 beam object sets this
    /// when its initialization instruction runs and the body clears it on deactivation.
    /// </summary>
    public ushort ActivatedFlag
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>
    /// Zero-up, clockwise, 1/256-turn aim angle in native variable D. Only the low byte is
    /// meaningful, but the cartridge stores the complete accumulator result as a word.
    /// </summary>
    public SnesAngle Angle
    {
        get => SnesAngle.FromTableIndex(unchecked((byte)_slot.VariableD));
        internal set => _slot.VariableD = value.TableIndex;
    }

    /// <summary>Activation/deactivation countdown in native variable E.</summary>
    public NativeCountdown FunctionTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableE);
        internal set => _slot.VariableE = value.RawValue;
    }

    /// <summary>Indirect bank-$A8 function word in native variable F.</summary>
    public MorphBallEyeAiFunction Function
    {
        get => (MorphBallEyeAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}

/// <summary>The translated lifetime of the morph-ball eye's bank-$88 HDMA object.</summary>
public enum MorphBallEyeBeamPhase
{
    /// <summary>No reveal HDMA object is installed; beam accumulators and fixed-color components are at their reset values.</summary>
    Inactive,
    /// <summary>The eye AI requested a beam, but its initialization instruction will run in the next HDMA-object pass, before the next enemy AI pass.</summary>
    PendingInitialization,
    /// <summary>The initialized object accelerates its 16.16 angular half-width until it reaches four of the 256 angle units per full turn.</summary>
    Widening,
    /// <summary>The four-unit half-width beam tracks the body angle and cycles sixteen yellow brightness steps while the body's activation flag remains set.</summary>
    Full,
    /// <summary>The object's fixed-color RGB components fade toward zero; reaching the green selector-only value deletes the beam on the following pass.</summary>
    Deactivating,
}

/// <summary>
/// Debugger-readable form of the eye beam's HDMA-object variables and three shared WRAM
/// accumulators. It describes a color-math reveal, not an enemy projectile or damage source.
/// </summary>
public sealed class MorphBallEyeBeamState
{
    /// <summary>Current host projection of the room-global HDMA object's lifetime, advanced before enemy AI; separate from the body's bank-$A8 function word.</summary>
    public MorphBallEyeBeamPhase Phase { get; internal set; }

    /// <summary>Literal body slot read by <c>$88:E96D/$88:E98B</c>; retail uses slot one.</summary>
    public int BodySlotIndex { get; internal set; } = -1;

    /// <summary>Whole angular half-width, corresponding to HDMA object variable two.</summary>
    public ushort AngularWidth { get; internal set; }

    /// <summary>Fractional angular half-width, corresponding to HDMA object variable three.</summary>
    public ushort AngularSubwidth { get; internal set; }

    /// <summary>Whole widening velocity in the shared eye-beam delta word.</summary>
    public ushort AngularWidthDelta { get; internal set; }

    /// <summary>Fractional widening velocity in the shared eye-beam sub-delta word.</summary>
    public ushort AngularSubwidthDelta { get; internal set; }

    /// <summary>Index into the native sixteen-step yellow brightness cycle.</summary>
    public ushort ColorIndex { get; internal set; }

    /// <summary>Raw SNES COLDATA red selector/component byte.</summary>
    public byte Red { get; internal set; } = 0x20;

    /// <summary>Raw SNES COLDATA green selector/component byte.</summary>
    public byte Green { get; internal set; } = 0x40;

    /// <summary>Raw SNES COLDATA blue selector/component byte.</summary>
    public byte Blue { get; internal set; } = 0x80;

    /// <summary>Clears the HDMA beam lifetime and restores neutral COLDATA selector bytes.</summary>
    internal void Reset()
    {
        Phase = MorphBallEyeBeamPhase.Inactive;
        BodySlotIndex = -1;
        AngularWidth = 0;
        AngularSubwidth = 0;
        AngularWidthDelta = 0;
        AngularSubwidthDelta = 0;
        ColorIndex = 0;
        Red = 0x20;
        Green = 0x40;
        Blue = 0x80;
    }
}

/// <summary>
/// Immutable eye-beam inputs published with the OAM image by an accepted NMI.
/// </summary>
/// <remarks>
/// The eye body, its bank-$88 HDMA object, and layer-one scroll all advance during the
/// main loop. The PPU does not see any of those shadow values until NMI. Keeping this
/// compact presentation record prevents the software compositor from pairing yesterday's
/// OAM with today's cone angle or camera position.
/// </remarks>
/// <param name="Phase">Beam lifetime phase latched with the accepted NMI's actor image.</param>
/// <param name="WorldPosition">Eye body's origin in whole room pixels, before layer-one scroll is subtracted.</param>
/// <param name="Angle">Latched beam aim, measured clockwise from up in 256 table units per turn.</param>
/// <param name="AngularWidth">Whole angular half-width in table units, normally zero through four; fractional widening state is not included in this render snapshot.</param>
/// <param name="Red">Raw SNES COLDATA red-selector/component byte, with selector bit <c>$20</c> and intensity in the low five bits.</param>
/// <param name="Green">Raw SNES COLDATA green-selector/component byte, with selector bit <c>$40</c> and intensity in the low five bits.</param>
/// <param name="Blue">Raw SNES COLDATA blue-selector/component byte, with selector bit <c>$80</c> and intensity in the low five bits.</param>
public readonly record struct MorphBallEyeBeamRenderSnapshot(
    MorphBallEyeBeamPhase Phase,
    Position16<RoomPixels> WorldPosition,
    SnesAngle Angle,
    ushort AngularWidth,
    byte Red,
    byte Green,
    byte Blue);

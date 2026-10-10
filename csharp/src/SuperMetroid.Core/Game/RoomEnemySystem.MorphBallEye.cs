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
    private readonly RoomEnemySlot _slot;

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
    public ushort FunctionTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
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
/// <param name="WorldX">Eye body's horizontal origin in whole room pixels, before layer-one scroll is subtracted.</param>
/// <param name="WorldY">Eye body's vertical origin in whole room pixels, before layer-one scroll is subtracted.</param>
/// <param name="Angle">Latched beam aim, measured clockwise from up in 256 table units per turn.</param>
/// <param name="AngularWidth">Whole angular half-width in table units, normally zero through four; fractional widening state is not included in this render snapshot.</param>
/// <param name="Red">Raw SNES COLDATA red-selector/component byte, with selector bit <c>$20</c> and intensity in the low five bits.</param>
/// <param name="Green">Raw SNES COLDATA green-selector/component byte, with selector bit <c>$40</c> and intensity in the low five bits.</param>
/// <param name="Blue">Raw SNES COLDATA blue-selector/component byte, with selector bit <c>$80</c> and intensity in the low five bits.</param>
public readonly record struct MorphBallEyeBeamRenderSnapshot(
    MorphBallEyeBeamPhase Phase,
    ushort WorldX,
    ushort WorldY,
    SnesAngle Angle,
    ushort AngularWidth,
    byte Red,
    byte Green,
    byte Blue);

/// <summary>
/// Literal translation of morph-ball eye enemy <c>$E6BF</c> from $A8:8FAC-$91DC and its
/// dedicated bank-$88 beam object at $88:E8D9-$EB57. The two retail records are deliberately
/// kept separate: the first is a fixed decorative mount and the second is the tracking eye.
/// </summary>
public sealed partial class RoomEnemySystem
{

    private const ushort MorphBallItemMask = 0x0004;
    private const ushort EyeActivateXDistance = 0x0080;
    private const ushort EyeDeactivateXDistance = 0x00b0;
    private const ushort EyeActivateYDistance = 0x0080;
    private const ushort EyeDeactivateYDistance = 0x0080;
    private const ushort EyeTransitionDuration = 0x0020;
    private const ushort EyeActivationSound = 0x0017;
    private const ushort EyeDeactivationSound = 0x0071;

    private readonly MorphBallEyeEnemyState?[] _morphBallEyeStates =
        new MorphBallEyeEnemyState?[MaximumEnemyCount];

    /// <summary>Typed native state for every loaded mount/body record.</summary>
    public IReadOnlyList<MorphBallEyeEnemyState?> MorphBallEyeStates => _morphBallEyeStates;

    /// <summary>The room-global bank-$88 beam object spawned by an eye body.</summary>
    public MorphBallEyeBeamState MorphBallEyeBeam { get; } = new();

    /// <summary>Most recent library-two eye sound requested during this enemy frame.</summary>
    public ushort? LastMorphBallEyeSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Eye</c> at $A8:9058.</summary>
    private void InitializeMorphBallEye(RoomEnemySlot slot)
    {
        var state = new MorphBallEyeEnemyState(slot);
        _morphBallEyeStates[slot.SlotIndex] = state;

        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.SpritemapPointer = 0x804d;
        slot.InstructionTimer = 1;
        slot.Timer = 0;

        if (unchecked((short)slot.Parameter2) >= 0)
        {
            // Parameter one bit zero is a fixed mounting direction, not a live facing test.
            state.Function = MorphBallEyeAiFunction.WaitForSamus;
            slot.CurrentInstruction = (slot.Parameter1 & 1) == 0
                ? MorphBallEyeInstructionProgramDefinitions.FacingRightClosed
                : MorphBallEyeInstructionProgramDefinitions.FacingLeftClosed;
            return;
        }

        int mountDirection = slot.Parameter2 & 0x000f;
        var mount = MorphBallEyeGeometryDefinitions.Mount(mountDirection);
        slot.XPosition = unchecked((ushort)(slot.XPosition + mount.X));
        slot.YPosition = unchecked((ushort)(slot.YPosition + mount.Y));
        state.Function = MorphBallEyeAiFunction.MountNoOp;
        slot.CurrentInstruction = mount.Program;

        // Native clears all 256 window endpoint words from the mount initializer. Host
        // geometry is generated from BeamState, so resetting that state is the equivalent
        // room-load boundary and prevents a prior room's reveal from leaking forward.
        MorphBallEyeBeam.Reset();
    }

    /// <summary>Ports <c>MainAI_Eye</c> and its four body functions at $A8:90E2-$91DB.</summary>
    private void RunMorphBallEyeMain(
        RoomEnemySlot slot,
        MorphBallEyeEnemyState state,
        SamusState? samus)
    {
        // The cartridge tests collected_items ($09A4), not equipped_items ($09A2). This
        // matters after the player disables Morph Ball in the equipment menu.
        if (samus is null || (samus.CollectedItems & MorphBallItemMask) == 0)
            return;

        switch (state.Function)
        {
            case MorphBallEyeAiFunction.MountNoOp:
                return;
            case MorphBallEyeAiFunction.WaitForSamus:
                WaitForSamusNearMorphBallEye(slot, state, samus);
                return;
            case MorphBallEyeAiFunction.Activating:
                AdvanceMorphBallEyeActivation(slot, state, samus);
                return;
            case MorphBallEyeAiFunction.Active:
                TrackSamusWithMorphBallEye(slot, state, samus);
                return;
            case MorphBallEyeAiFunction.Deactivating:
                AdvanceMorphBallEyeDeactivation(state);
                return;
            default:
                throw new InvalidDataException(
                    $"Morph-ball eye function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private static void WaitForSamusNearMorphBallEye(
        RoomEnemySlot eye,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        if (!IsWithinStrictModularDistance(samus.YPosition, eye.YPosition, EyeActivateYDistance) ||
            !IsWithinStrictModularDistance(samus.XPosition, eye.XPosition, EyeActivateXDistance))
        {
            return;
        }

        state.FunctionTimer = EyeTransitionDuration;
        eye.InstructionTimer = 1;
        eye.CurrentInstruction = (eye.Parameter1 & 1) == 0
            ? MorphBallEyeInstructionProgramDefinitions.FacingRightActivating
            : MorphBallEyeInstructionProgramDefinitions.FacingLeftActivating;
        state.Function = MorphBallEyeAiFunction.Activating;
    }

    private void AdvanceMorphBallEyeActivation(
        RoomEnemySlot eye,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        // DEC followed by BEQ/BPL expires on zero and also tolerates an already-zero timer's
        // wrapped $FFFF value. This is deliberately not a host-side "if timer > 0" clamp.
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
        state.FunctionTimer = timer.Value;
        if (!timer.IsZero && timer.IsNonNegative)
            return;

        LastMorphBallEyeSoundEffect = EyeActivationSound;
        RequestMorphBallEyeBeam(eye);
        state.Function = MorphBallEyeAiFunction.Active;
        state.Angle = CalculateMorphBallEyeAngle(eye, samus);
    }

    private void TrackSamusWithMorphBallEye(
        RoomEnemySlot eye,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        bool inProximity =
            IsWithinStrictModularDistance(samus.YPosition, eye.YPosition, EyeDeactivateYDistance) &&
            IsWithinStrictModularDistance(samus.XPosition, eye.XPosition, EyeDeactivateXDistance);
        if (!inProximity)
        {
            LastMorphBallEyeSoundEffect = EyeDeactivationSound;
            state.ActivatedFlag = 0;
            state.FunctionTimer = EyeTransitionDuration;
            eye.CurrentInstruction = (eye.Parameter1 & 1) == 0
                ? MorphBallEyeInstructionProgramDefinitions.FacingRightDeactivating
                : MorphBallEyeInstructionProgramDefinitions.FacingLeftDeactivating;
            state.Function = MorphBallEyeAiFunction.Deactivating;
            eye.InstructionTimer = 1;
            return;
        }

        state.Angle = CalculateMorphBallEyeAngle(eye, samus);

        // Sixteen four-byte duration/map records begin at $8FAC. The high angle nibble is
        // shifted down by two, producing byte offsets $00,$04,...,$3C exactly as native.
        eye.CurrentInstruction = unchecked((ushort)(
            MorphBallEyeInstructionProgramDefinitions.Active +
            ((state.Angle.TableIndex & 0x00f0) >> 2)));
        eye.InstructionTimer = 1;
    }

    private static void AdvanceMorphBallEyeDeactivation(MorphBallEyeEnemyState state)
    {
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
        state.FunctionTimer = timer.Value;
        if (timer.IsZeroOrNegative)
            state.Function = MorphBallEyeAiFunction.WaitForSamus;
    }

    private static SnesAngle CalculateMorphBallEyeAngle(RoomEnemySlot eye, SamusState samus) =>
        SnesAngle.FromTableIndex(CalculateCartridgeAngle(
            unchecked((short)(samus.XPosition - eye.XPosition)),
            unchecked((short)(samus.YPosition - eye.YPosition))));

    private void RequestMorphBallEyeBeam(RoomEnemySlot eye)
    {
        // The native bank-$88 code addresses Enemy[1] and Eye.*+$40 literally. Refusing a
        // different layout catches a malformed/custom population instead of quietly aiming
        // the room-global beam from whichever actor happened to request it.
        if (eye.SlotIndex != 1 || eye.EnemyDefinitionPointer != EnemyDefinitionId.Eye)
        {
            throw new InvalidDataException(
                $"Morph-ball eye beam requires the body in retail enemy slot 1, got {eye.SlotIndex}.");
        }

        MorphBallEyeBeam.Reset();
        MorphBallEyeBeam.BodySlotIndex = eye.SlotIndex;
        MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.PendingInitialization;
    }

    /// <summary>
    /// Advances $88:E917-$EB57 before enemy AI, matching the native HDMA-object pass. A beam
    /// spawned by this frame's eye AI consequently initializes on the following frame.
    /// </summary>
    private void StepMorphBallEyeBeam()
    {
        switch (MorphBallEyeBeam.Phase)
        {
            case MorphBallEyeBeamPhase.Inactive:
                return;
            case MorphBallEyeBeamPhase.PendingInitialization:
                InitializeMorphBallEyeBeamObject();
                return;
            case MorphBallEyeBeamPhase.Widening:
                WidenMorphBallEyeBeam();
                return;
            case MorphBallEyeBeamPhase.Full:
                StepFullMorphBallEyeBeam();
                return;
            case MorphBallEyeBeamPhase.Deactivating:
                FadeMorphBallEyeBeam();
                return;
            default:
                throw new InvalidDataException(
                    $"Unknown morph-ball eye beam phase {MorphBallEyeBeam.Phase}.");
        }
    }

    private void InitializeMorphBallEyeBeamObject()
    {
        RoomEnemySlot body = RequireMorphBallEyeBeamBody();
        MorphBallEyeEnemyState state = RequireMorphBallEyeState(body);

        MorphBallEyeBeam.Red = 0x30;
        MorphBallEyeBeam.Green = 0x50;
        MorphBallEyeBeam.Blue = 0x80;
        MorphBallEyeBeam.AngularWidth = 0;
        MorphBallEyeBeam.AngularSubwidth = 0;
        MorphBallEyeBeam.AngularWidthDelta = 0;
        MorphBallEyeBeam.AngularSubwidthDelta = 0;
        MorphBallEyeBeam.ColorIndex = 0;
        state.ActivatedFlag = 1;
        MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Widening;
    }

    private void WidenMorphBallEyeBeam()
    {
        uint deltaFraction = (uint)MorphBallEyeBeam.AngularSubwidthDelta + 0x4000u;
        MorphBallEyeBeam.AngularSubwidthDelta = unchecked((ushort)deltaFraction);
        MorphBallEyeBeam.AngularWidthDelta = unchecked((ushort)(
            MorphBallEyeBeam.AngularWidthDelta + (deltaFraction >> 16)));

        uint widthFraction = (uint)MorphBallEyeBeam.AngularSubwidth +
            MorphBallEyeBeam.AngularSubwidthDelta;
        MorphBallEyeBeam.AngularSubwidth = unchecked((ushort)widthFraction);
        MorphBallEyeBeam.AngularWidth = unchecked((ushort)(
            MorphBallEyeBeam.AngularWidth +
            MorphBallEyeBeam.AngularWidthDelta +
            (widthFraction >> 16)));

        if (unchecked((short)(MorphBallEyeBeam.AngularWidth - 4)) >= 0)
        {
            MorphBallEyeBeam.AngularWidth = 4;
            MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Full;
        }
    }

    private void StepFullMorphBallEyeBeam()
    {
        MorphBallEyeEnemyState bodyState = RequireMorphBallEyeState(RequireMorphBallEyeBeamBody());
        if (bodyState.ActivatedFlag == 0)
        {
            MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Deactivating;
            return;
        }

        int colorIndex = MorphBallEyeBeam.ColorIndex & 0x000f;
        (MorphBallEyeBeam.Red, MorphBallEyeBeam.Green) = MorphBallEyeGeometryDefinitions.BeamColor(colorIndex);
        MorphBallEyeBeam.Blue = 0x80;
        MorphBallEyeBeam.ColorIndex = unchecked((ushort)((colorIndex + 1) & 0x000f));
    }

    private void FadeMorphBallEyeBeam()
    {
        // $88:EAD6 tests green before updating the table or decrementing any component.
        // Once green reaches selector/component $40, it clears the table and deletes the
        // object on this pass. Reset publishes the same fixed-color base values.
        if (MorphBallEyeBeam.Green == 0x40)
        {
            MorphBallEyeBeam.Reset();
            return;
        }

        if (MorphBallEyeBeam.Red != 0x20)
            MorphBallEyeBeam.Red--;
        if (MorphBallEyeBeam.Green != 0x40)
            MorphBallEyeBeam.Green--;
        if (MorphBallEyeBeam.Blue != 0x80)
            MorphBallEyeBeam.Blue--;
    }

    private RoomEnemySlot RequireMorphBallEyeBeamBody()
    {
        int index = MorphBallEyeBeam.BodySlotIndex;
        if ((uint)index >= _slots.Length ||
            _slots[index].EnemyDefinitionPointer != EnemyDefinitionId.Eye ||
            unchecked((short)_slots[index].Parameter2) < 0)
        {
            throw new InvalidDataException(
                $"Morph-ball eye HDMA object lost its body in enemy slot {index}.");
        }
        return _slots[index];
    }

    private MorphBallEyeEnemyState RequireMorphBallEyeState(RoomEnemySlot slot) =>
        _morphBallEyeStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Morph-ball eye slot {slot.SlotIndex} has no initialized native state.");
}

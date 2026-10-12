using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>The Morph Ball eye species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class MorphBallEyeBehavior : IEnemySpecies
{
    /// <summary>Stores slot table.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Initializes a new morph ball eye behavior instance.</summary>
    internal MorphBallEyeBehavior(EnemySlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>Defines morph ball item mask.</summary>
    private const ushort MorphBallItemMask = 0x0004;
    /// <summary>Defines eye activate xdistance.</summary>
    private const ushort EyeActivateXDistance = 0x0080;
    /// <summary>Defines eye deactivate xdistance.</summary>
    private const ushort EyeDeactivateXDistance = 0x00b0;
    /// <summary>Defines eye activate ydistance.</summary>
    private const ushort EyeActivateYDistance = 0x0080;
    /// <summary>Defines eye deactivate ydistance.</summary>
    private const ushort EyeDeactivateYDistance = 0x0080;
    /// <summary>Defines eye transition duration.</summary>
    private const ushort EyeTransitionDuration = 0x0020;
    /// <summary>Defines eye activation sound.</summary>
    private const ushort EyeActivationSound = 0x0017;
    /// <summary>Defines eye deactivation sound.</summary>
    private const ushort EyeDeactivationSound = 0x0071;

    /// <summary>Stores morph ball eye states.</summary>
    internal readonly MorphBallEyeEnemyState?[] _morphBallEyeStates =
        new MorphBallEyeEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Typed native state for every loaded mount/body record.</summary>
    public IReadOnlyList<MorphBallEyeEnemyState?> MorphBallEyeStates => _morphBallEyeStates;

    /// <summary>The room-global bank-$88 beam object spawned by an eye body.</summary>
    public MorphBallEyeBeamState MorphBallEyeBeam { get; } = new();

    /// <summary>Most recent library-two eye sound requested during this enemy frame.</summary>
    public ushort? LastMorphBallEyeSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Eye</c> at $A8:9058.</summary>
    internal void InitializeMorphBallEye(RoomEnemySlot slot)
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
        slot.Position += new Offset16<RoomPixels>(unchecked((short)(mount.X)), unchecked((short)(mount.Y)));
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

    /// <summary>wait for samus near morph ball eye.</summary>
    private static void WaitForSamusNearMorphBallEye(
        RoomEnemySlot eye,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        if (!EnemyMath.IsWithinStrictModularDistance(samus.Position.Y.Whole, eye.Position.Y.Whole, EyeActivateYDistance) ||
            !EnemyMath.IsWithinStrictModularDistance(samus.Position.X.Whole, eye.Position.X.Whole, EyeActivateXDistance))
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

    /// <summary>advance morph ball eye activation.</summary>
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

    /// <summary>track samus with morph ball eye.</summary>
    private void TrackSamusWithMorphBallEye(
        RoomEnemySlot eye,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        bool inProximity =
            EnemyMath.IsWithinStrictModularDistance(samus.Position.Y.Whole, eye.Position.Y.Whole, EyeDeactivateYDistance) &&
            EnemyMath.IsWithinStrictModularDistance(samus.Position.X.Whole, eye.Position.X.Whole, EyeDeactivateXDistance);
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

    /// <summary>advance morph ball eye deactivation.</summary>
    private static void AdvanceMorphBallEyeDeactivation(MorphBallEyeEnemyState state)
    {
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
        state.FunctionTimer = timer.Value;
        if (timer.IsZeroOrNegative)
            state.Function = MorphBallEyeAiFunction.WaitForSamus;
    }

    /// <summary>calculate morph ball eye angle.</summary>
    private static SnesAngle CalculateMorphBallEyeAngle(RoomEnemySlot eye, SamusState samus) =>
        SnesAngle.FromTableIndex(EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)(samus.Position.X.Whole - eye.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - eye.Position.Y.Whole)))));

    /// <summary>request morph ball eye beam.</summary>
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
    internal void StepMorphBallEyeBeam()
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

    /// <summary>initialize morph ball eye beam object.</summary>
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

    /// <summary>widen morph ball eye beam.</summary>
    private void WidenMorphBallEyeBeam()
    {
        uint deltaFraction = (uint)MorphBallEyeBeam.AngularSubwidthDelta + 0x4000u;
        MorphBallEyeBeam.AngularSubwidthDelta = unchecked((ushort)deltaFraction);
        MorphBallEyeBeam.AngularWidthDelta = unchecked((ushort)(
            MorphBallEyeBeam.AngularWidthDelta + SnesCarry.OutOfWord(deltaFraction)));

        uint widthFraction = (uint)MorphBallEyeBeam.AngularSubwidth +
            MorphBallEyeBeam.AngularSubwidthDelta;
        MorphBallEyeBeam.AngularSubwidth = unchecked((ushort)widthFraction);
        MorphBallEyeBeam.AngularWidth = unchecked((ushort)(
            MorphBallEyeBeam.AngularWidth +
            MorphBallEyeBeam.AngularWidthDelta +
            SnesCarry.OutOfWord(widthFraction)));

        if (unchecked((short)(MorphBallEyeBeam.AngularWidth - 4)) >= 0)
        {
            MorphBallEyeBeam.AngularWidth = 4;
            MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Full;
        }
    }

    /// <summary>step full morph ball eye beam.</summary>
    internal void StepFullMorphBallEyeBeam()
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

    /// <summary>fade morph ball eye beam.</summary>
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

    /// <summary>require morph ball eye beam body.</summary>
    private RoomEnemySlot RequireMorphBallEyeBeamBody()
    {
        int index = MorphBallEyeBeam.BodySlotIndex;
        if ((uint)index >= _slotTable._slots.Length ||
            _slotTable._slots[index].EnemyDefinitionPointer != EnemyDefinitionId.Eye ||
            unchecked((short)_slotTable._slots[index].Parameter2) < 0)
        {
            throw new InvalidDataException(
                $"Morph-ball eye HDMA object lost its body in enemy slot {index}.");
        }
        return _slotTable._slots[index];
    }

    /// <summary>require morph ball eye state.</summary>
    private MorphBallEyeEnemyState RequireMorphBallEyeState(RoomEnemySlot slot) =>
        _morphBallEyeStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Morph-ball eye slot {slot.SlotIndex} has no initialized native state.");

    /// <summary>register morph ball eye initialization ai.</summary>
    internal void RegisterMorphBallEyeInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Eye, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Eye, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeMorphBallEye(slot);
        });
    }

    /// <summary>register morph ball eye main ai.</summary>
    internal void RegisterMorphBallEyeMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Eye, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Eye, (slot, frame, nmiFrameCounter8) =>
        {
            RunMorphBallEyeMain(slot, RequireMorphBallEyeState(slot), frame.Samus);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastMorphBallEyeSoundEffect = null;
        Array.Clear(_morphBallEyeStates);
        MorphBallEyeBeam.Reset();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastMorphBallEyeSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastMorphBallEyeSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}

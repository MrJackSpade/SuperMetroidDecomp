using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.MorphBallEye.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>The Morph Ball eye species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class MorphBallEyeBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Eye];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new MorphBallEyeEntity(this, slot, common);

    /// <summary>Validates that the room-global beam still belongs to the retail body slot.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Uses native enemy-slot layout to bind the beam object to its eye body.</summary>
    internal MorphBallEyeBehavior(EnemySlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>The eye responds to collected Morph Ball even when it is unequipped.</summary>
    /// <summary>Strict horizontal range for starting the opening animation.</summary>
    private const ushort EyeActivateXDistance = 128;
    /// <summary>Wider strict horizontal range that keeps an active beam open.</summary>
    private const ushort EyeDeactivateXDistance = 176;
    /// <summary>Strict vertical range for initial activation.</summary>
    private const ushort EyeActivateYDistance = 128;
    /// <summary>Strict vertical range retained while tracking Samus.</summary>
    private const ushort EyeDeactivateYDistance = 128;
    /// <summary>Native word countdown for both opening and closing eye transitions.</summary>
    private const ushort EyeTransitionDuration = 32;


    /// <summary>Typed native state for every loaded mount/body record.</summary>
    public IReadOnlyList<MorphBallEyeEnemyState?> MorphBallEyeStates =>
        [.. _slotTable._slots.Select(slot => slot.SpeciesState<MorphBallEyeEnemyState>())];

    /// <summary>The room-global bank-$88 beam object spawned by an eye body.</summary>
    public MorphBallEyeBeamState MorphBallEyeBeam { get; } = new();

    /// <summary>Most recent library-two eye sound requested during this enemy frame.</summary>
    public SoundEffectId? LastMorphBallEyeSoundEffect { get; private set; }

    /// <summary>Initializes a closed body from parameter-one facing, or offsets a mount and resets the room-global beam for negative parameter two.</summary>
    internal void InitializeMorphBallEye(RoomEnemySlot slot)
    {
        var state = new MorphBallEyeEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.Frame = EnemyFrameId.Empty;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);

        if (unchecked((short)slot.Parameter2) >= 0)
        {
            // Parameter one bit zero is a fixed mounting direction, not a live facing test.
            state.Function = MorphBallEyeAiFunction.WaitForSamus;
            state.Program = AnimationProgramCursor<MorphBallEyeProgram>.Start((slot.Parameter1 & 1) == 0
                ? MorphBallEyeProgram.FacingRightClosed
                : MorphBallEyeProgram.FacingLeftClosed);
            return;
        }

        int mountDirection = slot.Parameter2 & MorphBallEyeBeamDefinitions.MountDirectionBits;
        var mount = MorphBallEyeGeometryDefinitions.Mount(mountDirection);
        slot.Position += new Offset16<RoomPixels>(unchecked((short)(mount.X)), unchecked((short)(mount.Y)));
        state.Function = MorphBallEyeAiFunction.MountNoOp;
        state.Program = AnimationProgramCursor<MorphBallEyeProgram>.Start(mount.Program);

        // Native clears all 256 window endpoint words from the mount initializer. Host
        // geometry is generated from BeamState, so resetting that state is the equivalent
        // room-load boundary and prevents a prior room's reveal from leaking forward.
        MorphBallEyeBeam.Reset();
    }

    /// <summary>Runs the body state machine only after Morph Ball is collected, preserving activation and tracking independently of equipment selection.</summary>
    private void RunMorphBallEyeMain(
        RoomEnemySlot slot,
        MorphBallEyeEnemyState state,
        SamusState? samus)
    {
        // The cartridge tests collected_items ($09A4), not equipped_items ($09A2). This
        // matters after the player disables Morph Ball in the equipment menu.
        if (samus is null || (samus.CollectedItems & SamusEquipmentFlags.MorphBall) == 0)
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
                    $"Morph-ball eye function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Starts the 32-frame opening list only inside strict modular X and Y activation ranges.</summary>
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

        state.FunctionTimer = NativeCountdown.Start(EyeTransitionDuration);
        eye.InstructionTimer = NativeCountdown.Start(1);
        state.Program = AnimationProgramCursor<MorphBallEyeProgram>.Start((eye.Parameter1 & 1) == 0
            ? MorphBallEyeProgram.FacingRightActivating
            : MorphBallEyeProgram.FacingLeftActivating);
        state.Function = MorphBallEyeAiFunction.Activating;
    }

    /// <summary>Uses native decrement/wrap semantics, then sounds activation, requests the beam, and captures Samus's angle.</summary>
    private void AdvanceMorphBallEyeActivation(
        RoomEnemySlot eye,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        // DEC followed by BEQ/BPL expires on zero and also tolerates an already-zero timer's
        // wrapped $FFFF value. This is deliberately not a host-side "if timer > 0" clamp.
        NativeWordCounterStep timer = state.FunctionTimer.Decrement();
        state.FunctionTimer = timer.Remaining;
        if (!timer.IsZero && timer.IsNonNegative)
            return;

        LastMorphBallEyeSoundEffect = SoundEffectId.FromCartridge(
            SoundEffectLibrary.Library2, EnemySoundEffectDefinitions.EyeActivationSound);
        RequestMorphBallEyeBeam(eye);
        state.Function = MorphBallEyeAiFunction.Active;
        state.Angle = CalculateMorphBallEyeAngle(eye, samus);
    }

    /// <summary>Aims one of sixteen active frames at Samus or starts closing after the wider horizontal/vertical range fails.</summary>
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
            LastMorphBallEyeSoundEffect = SoundEffectId.FromCartridge(
                SoundEffectLibrary.Library2, EnemySoundEffectDefinitions.EyeDeactivationSound);
            state.IsActivated = false;
            state.FunctionTimer = NativeCountdown.Start(EyeTransitionDuration);
            state.Program = AnimationProgramCursor<MorphBallEyeProgram>.Start((eye.Parameter1 & 1) == 0
                ? MorphBallEyeProgram.FacingRightDeactivating
                : MorphBallEyeProgram.FacingLeftDeactivating);
            state.Function = MorphBallEyeAiFunction.Deactivating;
            eye.InstructionTimer = NativeCountdown.Start(1);
            return;
        }

        state.Angle = CalculateMorphBallEyeAngle(eye, samus);

        // The native pointer is $8FAC plus the high angle nibble shifted down by two, one four-byte record per
        // pose: the tracking loop resumes at the pose that nibble selects.
        state.Program = new AnimationProgramCursor<MorphBallEyeProgram>(MorphBallEyeProgram.Active,
            (state.Angle.TableIndex & MorphBallEyeBeamDefinitions.PoseAngleBits) >> 4);
        eye.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Returns to waiting when the native closing word reaches zero or a negative wrapped value.</summary>
    private static void AdvanceMorphBallEyeDeactivation(MorphBallEyeEnemyState state)
    {
        NativeWordCounterStep timer = state.FunctionTimer.Decrement();
        state.FunctionTimer = timer.Remaining;
        if (timer.IsZeroOrNegative)
            state.Function = MorphBallEyeAiFunction.WaitForSamus;
    }

    /// <summary>Calculates the cartridge angle from signed wrapped eye-to-Samus displacement.</summary>
    private static SnesAngle CalculateMorphBallEyeAngle(RoomEnemySlot eye, SamusState samus) =>
        SnesAngle.FromTableIndex(EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)(samus.Position.X.Whole - eye.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - eye.Position.Y.Whole)))));

    /// <summary>Requires the body in retail slot one before scheduling the room-global bank-$88 beam for next frame.</summary>
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

    /// <summary>Seeds the beam's fixed colors and zero angular width, then marks its body activated.</summary>
    private void InitializeMorphBallEyeBeamObject()
    {
        RoomEnemySlot body = RequireMorphBallEyeBeamBody();
        MorphBallEyeEnemyState state = RequireMorphBallEyeState(body);

        MorphBallEyeBeam.Red = MorphBallEyeBeamDefinitions.InitialRed;
        MorphBallEyeBeam.Green = MorphBallEyeBeamDefinitions.InitialGreen;
        MorphBallEyeBeam.Blue = MorphBallEyeBeamDefinitions.Blue;
        MorphBallEyeBeam.AngularHalfWidth = SnesSignedSixteenSixteen.Zero;
        MorphBallEyeBeam.AngularHalfWidthGrowth = SnesSignedSixteenSixteen.Zero;
        MorphBallEyeBeam.ColorIndex = 0;
        state.IsActivated = true;
        MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Widening;
    }

    /// <summary>
    /// Accelerates the beam's angular half-width by a quarter table unit per update until its whole part reaches four;
    /// that update sets the whole part to four, keeping the fraction, and the beam is full.
    /// </summary>
    private void WidenMorphBallEyeBeam()
    {
        MorphBallEyeBeam.AngularHalfWidthGrowth += MorphBallEyeBeamDefinitions.WidthGrowthStep;
        MorphBallEyeBeam.AngularHalfWidth += MorphBallEyeBeam.AngularHalfWidthGrowth;
        if (MorphBallEyeBeam.AngularWidth >= MorphBallEyeBeamDefinitions.FullAngularWidth)
        {
            MorphBallEyeBeam.AngularHalfWidth = MorphBallEyeBeam.AngularHalfWidth.WithWholePixels(MorphBallEyeBeamDefinitions.FullAngularWidth);
            MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Full;
        }
    }

    /// <summary>Cycles the sixteen native beam colors while the body stays activated, otherwise begins fading.</summary>
    internal void StepFullMorphBallEyeBeam()
    {
        MorphBallEyeEnemyState bodyState = RequireMorphBallEyeState(RequireMorphBallEyeBeamBody());
        if (!bodyState.IsActivated)
        {
            MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Deactivating;
            return;
        }

        int colorIndex = MorphBallEyeBeam.ColorIndex & MorphBallEyeBeamDefinitions.ColorIndexBits;
        (byte red, byte green) = MorphBallEyeGeometryDefinitions.BeamColor(colorIndex);
        MorphBallEyeBeam.Red = red;
        MorphBallEyeBeam.Green = green;
        MorphBallEyeBeam.Blue = MorphBallEyeBeamDefinitions.Blue;
        MorphBallEyeBeam.ColorIndex = (colorIndex + 1) & MorphBallEyeBeamDefinitions.ColorIndexBits;
    }

    /// <summary>Steps the beam's colour levels toward their faded levels and removes the beam once green has faded.</summary>
    private void FadeMorphBallEyeBeam()
    {
        // $88:EAD6 tests green before updating the table or decrementing any component.
        // Once green reaches selector/component $40, it clears the table and deletes the
        // object on this pass. Reset publishes the same fixed-color base values.
        if (MorphBallEyeBeam.Green == MorphBallEyeBeamDefinitions.FadedGreen)
        {
            MorphBallEyeBeam.Reset();
            return;
        }

        MorphBallEyeBeam.FadeOneStep();
    }

    /// <summary>Rejects a beam whose saved body slot is missing, wrong species, or a mount rather than the eye body.</summary>
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

    /// <summary>Rejects AI or beam processing before the eye slot's native state exists.</summary>
    private static MorphBallEyeEnemyState RequireMorphBallEyeState(RoomEnemySlot slot) =>
        slot.SpeciesState<MorphBallEyeEnemyState>() ?? throw new InvalidOperationException(
            $"Morph-ball eye slot {slot.SlotIndex} has no initialized native state.");

    /// <summary>Advances MorphBallEye's typed animation.</summary>
    internal static void AdvanceMorphBallEyeTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireMorphBallEyeState(slot);
        AnimationProgramCursor<MorphBallEyeProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, MorphBallEyeAnimations.Programs, ref cursor, MorphBallEyeFrameDefinitions.Frame);
        state.Program = cursor;
    }

    /// <summary>Ports MorphBallEye's initialization AI.</summary>
    internal void RunMorphBallEyeInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeMorphBallEye(slot);
    }

    /// <summary>Ports MorphBallEye's main AI.</summary>
    internal void RunMorphBallEyeMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMorphBallEyeMain(slot, RequireMorphBallEyeState(slot), frame.Samus);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastMorphBallEyeSoundEffect = null;
        foreach (RoomEnemySlot cleared in _slotTable._slots)
            cleared.SetSpeciesState<MorphBallEyeEnemyState>(null);
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
        if (LastMorphBallEyeSoundEffect is { } sound)
            audio.QueueEnemySound(sound, maximumQueued: 6);
    }
}

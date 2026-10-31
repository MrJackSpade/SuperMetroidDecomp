using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>The Elevator species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class ElevatorBehavior : IEnemySpecies, IEnemyEntityFactory, IElevatorJourney
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Elevator];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ElevatorEntity(this, slot, common);

    /// <summary>Provides the active room bus used when departure sets up Samus's elevator pose.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues the separate library-one and library-three departure sounds.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>The enemy pass's results, which the elevator writes.</summary>
    private readonly EnemyFrameResultBuilder _frameResults;

    /// <summary>Binds elevator movement to the room, audio queue, and encounter state.</summary>
    internal ElevatorBehavior(EnemyRoomBinding room, EnemyAudioQueue audio, EnemyFrameResultBuilder frameResults)
    {
        _frameResults = frameResults;
        _room = room;
        _audio = audio;
    }

    /// <summary>Native elevator travel advances one and a half pixels per enemy update.</summary>
    private const int ElevatorSpeedFixed = 98304;

    /// <summary>What has armed the elevator's pseudo-door: nothing, a collision with it, or an elevator door load.</summary>
    public ElevatorJourneyFlag ElevatorFlags
    {
        get => _room.Session.Elevator.Flags;
        internal set => _room.Session.Elevator.Flags = value;
    }

    /// <summary>WRAM $0E18, shared across the departure and destination room actors.</summary>
    public ElevatorActorStatus ElevatorStatus
    {
        get => _room.Session.Elevator.Status;
        internal set => _room.Session.Elevator.Status = value;
    }

    /// <summary>Whether the active elevator is travelling upward; otherwise it travels downward.</summary>
    public bool ElevatorTravelsUpward
    {
        get => _room.Session.Elevator.TravelsUpward;
        private set => _room.Session.Elevator.TravelsUpward = value;
    }

    /// <summary>
    /// Command seven installs the moving Samus beta handler on departure. At $82:E6A2,
    /// downward arrivals reinstall it, but upward arrivals retain command zero's no-op.
    /// The test reads the upward travel direction.
    /// </summary>
    public bool ElevatorRunsSamusMovement =>
        ElevatorStatus == ElevatorActorStatus.Departing ||
        (ElevatorStatus != ElevatorActorStatus.Inactive && !ElevatorTravelsUpward);

    /// <summary>
    /// WRAM $0795. Door-transition code owns this gate; nonzero suppresses actor movement
    /// while room loading/fading is already manipulating the same global coordinates.
    /// </summary>
    public bool ElevatorDoorTransitionActive { get; set; }

    /// <summary>Arrival slot whose initializer placed Samus; consumed by the door loader this update.</summary>
    [NonSerialized]
    private int? _arrivalPlacementSlot;

    /// <summary>
    /// Returns and clears the slot of the arriving elevator whose <c>Elevator_Init</c>
    /// ($A3:9526) placed Samus during the room load just performed.
    /// </summary>
    public int? ConsumeElevatorArrivalPlacementSlot()
    {
        int? slot = _arrivalPlacementSlot;
        _arrivalPlacementSlot = null;
        return slot;
    }

    /// <summary>
    /// Publishes bank-$94's type-$9 pseudo-door collision before the next EnemyMain pass.
    /// Repeated collision scans may set the same word again; the waiting AI clears it when
    /// the direction button is not newly pressed, exactly like <c>Elevator_Func_1</c>.
    /// </summary>
    public void PublishElevatorDoorContact() => ElevatorFlags = ElevatorJourneyFlag.DoorContact;

    /// <summary>
    /// <c>ResumeGameplay</c> ($80:A149) reloads the tiles through <c>$82:E78C</c>, whose first
    /// write clears $0E16. Unpausing mid-ride therefore leaves the elevator's pseudo-door
    /// unarmed: reaching it starts the door at once, without the 48-frame down-elevator wait.
    /// </summary>
    public void ClearElevatorFlagsOnGameplayResume() => ElevatorFlags = ElevatorJourneyFlag.None;

    /// <summary>
    /// Establishes the globals carried through an elevator door load. The destination
    /// actor's initializer recognizes status two and starts at its parameter-2 Y position.
    /// </summary>
    public void PrepareElevatorArrival(ElevatorJourneyFlag doorFlags = ElevatorJourneyFlag.DoorLoad)
    {
        ElevatorFlags = doorFlags;
        ElevatorStatus = ElevatorActorStatus.BeginArrivalReturn;
    }

    /// <inheritdoc/>
    public void ResetStatus()
    {
        ElevatorStatus = ElevatorActorStatus.Inactive;
        ElevatorFlags = ElevatorJourneyFlag.None;
    }

    /// <summary>Ports <c>Elevator_Init</c> at $A3:94E6.</summary>
    internal void InitializeElevator(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new ElevatorEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        slot.Frame = EnemyFrameId.Empty;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        state.Program = AnimationProgramCursor<ElevatorProgram>.Start(ElevatorProgram.Loop);
        state.DirectionTableByteOffset = unchecked((ushort)(slot.SpawnSettings<ElevatorSpawnSettings>().DirectionIndex * 2));
        state.RestingHeight = new Position16<RoomPixels>(0, slot.Position.Y.Whole);

        if (ElevatorStatus != ElevatorActorStatus.BeginArrivalReturn)
        {
            ElevatorFlags = ElevatorJourneyFlag.None;
            ElevatorStatus = ElevatorActorStatus.Inactive;
        }

        // The native 32-bit OR test is true for status two even if a malformed caller lost
        // the door flag. Preserve that behavior and reject only the impossible missing Samus.
        if (ElevatorStatus != ElevatorActorStatus.Inactive || ElevatorFlags != ElevatorJourneyFlag.None)
        {
            if (samus is null)
                throw new InvalidOperationException("Arriving elevator initialization requires Samus state.");
            slot.Position = slot.Position.WithWholeY(slot.SpawnSettings<ElevatorSpawnSettings>().ArrivalY);
            PinSamusToElevator(slot, samus);
            _arrivalPlacementSlot = slot.SlotIndex;
        }
    }

    /// <summary>Ports <c>Elevator_Frozen</c> and its four-entry dispatcher at $A3:952A.</summary>
    private void RunElevatorMain(
        RoomEnemySlot slot,
        ElevatorEnemyState state,
        SamusState? samus,
        ControllerButton newlyPressedControllerInput,
        SamusProjectileSystem? samusProjectiles)
    {
        if (ElevatorDoorTransitionActive ||
            (ElevatorStatus == ElevatorActorStatus.Inactive && ElevatorFlags == ElevatorJourneyFlag.None))
        {
            return;
        }
        if (samus is null)
            throw new InvalidOperationException("Active elevator AI requires Samus state.");

        switch (ElevatorStatus)
        {
            case ElevatorActorStatus.Inactive:
                WaitForElevatorDirectionInput(
                    slot,
                    state,
                    samus,
                    newlyPressedControllerInput,
                    samusProjectiles);
                return;

            case ElevatorActorStatus.Departing:
                MoveDepartingElevator(slot, state, samus);
                return;

            case ElevatorActorStatus.BeginArrivalReturn:
                ElevatorStatus = ElevatorActorStatus.ReturningToRest;
                _frameResults.Elevator = ElevatorFrameEvent.ArrivalReturnStarted;
                MoveElevatorBackToRest(slot, state, samus);
                return;

            case ElevatorActorStatus.ReturningToRest:
                MoveElevatorBackToRest(slot, state, samus);
                return;

            default:
                throw new InvalidDataException(
                    $"Elevator status {ElevatorStatus} is outside the elevator's dispatcher.");
        }
    }

    /// <summary>Ports <c>Elevator_Func_1</c> at $A3:9548.</summary>
    internal void WaitForElevatorDirectionInput(
        RoomEnemySlot slot,
        ElevatorEnemyState state,
        SamusState samus,
        ControllerButton newlyPressedControllerInput,
        SamusProjectileSystem? samusProjectiles)
    {
        ControllerButton requiredInput =
            ElevatorTravel.RequiredDirectionInput(state.DirectionTableByteOffset);
        if (!newlyPressedControllerInput.HasAny(requiredInput))
        {
            ElevatorFlags = ElevatorJourneyFlag.None;
            return;
        }

        // $A3:9548 issues both calls in this order before changing Samus's pose. Keeping
        // them as two requests matters because each library owns an independent SPC port.
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.ElevatorDepartureSoundLibrary3, maximumQueued: SoundQueueLimit.Max6);
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.ElevatorDepartureSoundLibrary1, maximumQueued: SoundQueueLimit.Max6);
        SamusPoseMetadata.SetupForElevator(samus);
        samusProjectiles?.Reset();
        _frameResults.ElevatorClearedProjectileData = true;
        PinSamusToElevator(slot, samus);
        ElevatorStatus = ElevatorActorStatus.Departing;
        _frameResults.Elevator = ElevatorFrameEvent.DepartureStarted;
    }

    /// <summary>Ports <c>Elevator_Func_2</c> at $A3:9579.</summary>
    private void MoveDepartingElevator(
        RoomEnemySlot slot,
        ElevatorEnemyState state,
        SamusState samus)
    {
        if (state.DirectionTableByteOffset != 0)
        {
            ElevatorTravelsUpward = true;
            AddElevatorYVelocity(slot, -ElevatorSpeedFixed);
        }
        else
        {
            ElevatorTravelsUpward = false;
            AddElevatorYVelocity(slot, ElevatorSpeedFixed);
        }
        PinSamusToElevator(slot, samus);
    }

    /// <summary>Ports <c>Elevator_Func_3</c>/<c>Func3b</c> at $A3:95B9/$95BC.</summary>
    private void MoveElevatorBackToRest(
        RoomEnemySlot slot,
        ElevatorEnemyState state,
        SamusState samus)
    {
        bool stillReturning;
        if (state.DirectionTableByteOffset != 0)
        {
            AddElevatorYVelocity(slot, ElevatorSpeedFixed);
            stillReturning = slot.Position.Y.Whole < state.RestingHeight.Y;
        }
        else
        {
            AddElevatorYVelocity(slot, -ElevatorSpeedFixed);
            stillReturning = slot.Position.Y.Whole >= state.RestingHeight.Y;
        }

        if (stillReturning)
        {
            PinSamusToElevator(slot, samus);
            return;
        }

        ElevatorStatus = ElevatorActorStatus.Inactive;
        ElevatorFlags = ElevatorJourneyFlag.None;
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.ElevatorArrivalSoundLibrary3, maximumQueued: SoundQueueLimit.Max6);
        slot.Position = slot.Position.WithWholeY(state.RestingHeight.Y);
        samus.Control.Unlock();
        PinSamusToElevator(slot, samus);
        _frameResults.Elevator = ElevatorFrameEvent.ArrivalCompleted;
    }

    /// <summary>Ports the direct 16.16 <c>AddToHiLo</c> used by both travel directions.</summary>
    private static void AddElevatorYVelocity(RoomEnemySlot slot, int signedVelocity)
    {
        uint fixedPosition = slot.Position.Y.RawValue;
        fixedPosition = unchecked(fixedPosition + (uint)signedVelocity);
        slot.Position = slot.Position.WithY(FixedCoordinate.FromRaw(unchecked(fixedPosition)));
    }

    /// <summary>Ports <c>Elevator_Func_4</c> at $A3:9612.</summary>
    private static void PinSamusToElevator(RoomEnemySlot slot, SamusState samus)
    {
        samus.Position = samus.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - ElevatorActorDefinitions.SamusYOffset)));
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0);
        samus.Position = samus.Position.WithWholeX(slot.Position.X.Whole);
        samus.Kinematics.YSpeed = default;
    }

    /// <summary>Advances Elevator's typed animation.</summary>
    internal static void AdvanceElevatorTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, ElevatorAnimations.Programs, ElevatorFrameDefinitions.ShownFrames.Of,
            static (ElevatorEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor);

    /// <summary>Ports Elevator's initialization AI.</summary>
    internal void RunElevatorInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeElevator(slot, surroundings.Samus);
    }

    /// <summary>Ports Elevator's main AI.</summary>
    internal void RunElevatorMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunElevatorMain(
            slot,
            slot.RequireSpeciesState<ElevatorEnemyState>(),
            frame.Samus,
            frame.NewlyPressedControllerInput,
            frame.SamusProjectiles);
    }
}

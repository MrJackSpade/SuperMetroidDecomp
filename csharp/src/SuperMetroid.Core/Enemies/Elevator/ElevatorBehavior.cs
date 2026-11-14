using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>The Elevator species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed class ElevatorBehavior : IEnemySpecies, IEnemyEntityFactory, IElevatorJourney
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Elevator];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ElevatorEntity(this, slot, common);

    /// <summary>Provides the active room bus used when departure sets up Samus's elevator pose.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues the separate Samus-library and room-library departure sounds.</summary>
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

    /// <summary>Elevator travel advances one and a half pixels per enemy update.</summary>
    private static readonly SignedSixteenSixteen ElevatorSpeed = SignedSixteenSixteen.FromParts(1, 32768);

    /// <summary>What has armed the elevator's pseudo-door: nothing, a collision with it, or an elevator door load.</summary>
    public ElevatorJourneyFlag ElevatorFlags
    {
        get => _room.Session.Elevator.Flags;
        internal set => _room.Session.Elevator.Flags = value;
    }

    /// <summary>The elevator status, shared across the departure and destination room actors.</summary>
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
    /// The elevator setup installs the moving Samus handler on departure. On arrival,
    /// downward arrivals reinstall it, but upward arrivals retain the lock's no-op.
    /// The test reads the upward travel direction.
    /// </summary>
    public bool ElevatorRunsSamusMovement =>
        ElevatorStatus == ElevatorActorStatus.Departing ||
        (ElevatorStatus != ElevatorActorStatus.Inactive && !ElevatorTravelsUpward);

    /// <summary>
    /// The door-transition gate. Door-transition code owns this gate; nonzero suppresses actor movement
    /// while room loading/fading is already manipulating the same global coordinates.
    /// </summary>
    public bool ElevatorDoorTransitionActive { get; set; }

    /// <summary>The arriving elevator whose setup placed Samus; consumed by the door loader this update.</summary>
    private RoomEnemySlot? _arrivalPlacement;

    /// <summary>
    /// Returns and clears the arriving elevator whose setup
    /// placed Samus during the room load just performed.
    /// </summary>
    public RoomEnemySlot? ConsumeElevatorArrivalPlacement()
    {
        RoomEnemySlot? elevator = _arrivalPlacement;
        _arrivalPlacement = null;
        return elevator;
    }

    /// <summary>
    /// Publishes the elevator's pseudo-door collision before the next enemy pass.
    /// Repeated collision scans may set the same status again; the waiting AI clears it when
    /// the direction button is not newly pressed, exactly like the elevator's waiting step.
    /// </summary>
    public void PublishElevatorDoorContact() => ElevatorFlags = ElevatorJourneyFlag.DoorContact;

    /// <summary>
    /// Resuming gameplay reloads the room's pictures through the elevator's picture reload, whose first
    /// write clears the elevator's journey status. Unpausing mid-ride therefore leaves the elevator's pseudo-door
    /// unarmed: reaching it starts the door at once, without the 48-frame down-elevator wait.
    /// </summary>
    public void ClearElevatorFlagsOnGameplayResume() => ElevatorFlags = ElevatorJourneyFlag.None;

    /// <summary>
    /// Establishes the globals carried through an elevator door load. The destination
    /// actor's initializer recognizes status two and starts at its arrival height.
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

    /// <summary>Initializes the Elevator.</summary>
    internal void InitializeElevator(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new ElevatorEnemyState();
        slot.SetSpeciesState(state);

        slot.Frame = EnemyFrameId.Empty;
        state.Playback = EnemyAnimationPlayback.Start(slot, ElevatorAnimations.StartingWith(ElevatorSegment.Loop));
        state.Direction = slot.SpawnSettings<ElevatorSpawnSettings>().Direction;
        state.RestingHeight = new Position16<RoomPixels>(0, slot.Position.Y.Whole);

        if (ElevatorStatus != ElevatorActorStatus.BeginArrivalReturn)
        {
            ElevatorFlags = ElevatorJourneyFlag.None;
            ElevatorStatus = ElevatorActorStatus.Inactive;
        }

        // The 32-bit OR test is true for status two even if a malformed caller lost
        // the door flag. Preserve that behavior and reject only the impossible missing Samus.
        if (ElevatorStatus != ElevatorActorStatus.Inactive || ElevatorFlags != ElevatorJourneyFlag.None)
        {
            if (samus is null)
                throw new InvalidOperationException("Arriving elevator initialization requires Samus state.");
            slot.Position = slot.Position.WithWholeY(slot.SpawnSettings<ElevatorSpawnSettings>().ArrivalY);
            PinSamusToElevator(slot, samus);
            _arrivalPlacement = slot;
        }
    }

    /// <summary>Runs the elevator's main behaviour and its four phases.</summary>
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
                    $"Elevator status {ElevatorStatus} is outside the elevator's phases.");
        }
    }

    /// <summary>Waits for the travel direction input.</summary>
    internal void WaitForElevatorDirectionInput(
        RoomEnemySlot slot,
        ElevatorEnemyState state,
        SamusState samus,
        ControllerButton newlyPressedControllerInput,
        SamusProjectileSystem? samusProjectiles)
    {
        ControllerButton requiredInput = ElevatorActorDefinitions.RequiredDirectionInputs[state.Direction];
        if (!newlyPressedControllerInput.HasAny(requiredInput))
        {
            ElevatorFlags = ElevatorJourneyFlag.None;
            return;
        }

        // Leaving the room issues both calls in this order before changing Samus's pose. Keeping
        // them as two requests matters because each sound library plays independently.
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.ElevatorDepartureSound, priority: SoundRequestPriority.Normal);
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.ElevatorDepartureSpinEndSound, priority: SoundRequestPriority.Normal);
        SamusPoseMetadata.SetupForElevator(samus);
        samusProjectiles?.Reset();
        _frameResults.ElevatorClearedProjectileData = true;
        PinSamusToElevator(slot, samus);
        ElevatorStatus = ElevatorActorStatus.Departing;
        _frameResults.Elevator = ElevatorFrameEvent.DepartureStarted;
    }

    /// <summary>Moves the departing elevator.</summary>
    private void MoveDepartingElevator(
        RoomEnemySlot slot,
        ElevatorEnemyState state,
        SamusState samus)
    {
        if (state.Direction is ElevatorTravelDirection.Up)
        {
            ElevatorTravelsUpward = true;
            AddElevatorYVelocity(slot, -ElevatorSpeed);
        }
        else
        {
            ElevatorTravelsUpward = false;
            AddElevatorYVelocity(slot, ElevatorSpeed);
        }
        PinSamusToElevator(slot, samus);
    }

    /// <summary>Moves the arriving elevator back to rest.</summary>
    private void MoveElevatorBackToRest(
        RoomEnemySlot slot,
        ElevatorEnemyState state,
        SamusState samus)
    {
        bool stillReturning;
        if (state.Direction is ElevatorTravelDirection.Up)
        {
            AddElevatorYVelocity(slot, ElevatorSpeed);
            stillReturning = slot.Position.Y.Whole < state.RestingHeight.Y;
        }
        else
        {
            AddElevatorYVelocity(slot, -ElevatorSpeed);
            stillReturning = slot.Position.Y.Whole >= state.RestingHeight.Y;
        }

        if (stillReturning)
        {
            PinSamusToElevator(slot, samus);
            return;
        }

        ElevatorStatus = ElevatorActorStatus.Inactive;
        ElevatorFlags = ElevatorJourneyFlag.None;
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.ElevatorArrivalSound, priority: SoundRequestPriority.Normal);
        slot.Position = slot.Position.WithWholeY(state.RestingHeight.Y);
        samus.Control.Unlock();
        PinSamusToElevator(slot, samus);
        _frameResults.Elevator = ElevatorFrameEvent.ArrivalCompleted;
    }

    /// <summary>The direct 1/65536-pixel addition used by both travel directions.</summary>
    private static void AddElevatorYVelocity(RoomEnemySlot slot, SignedSixteenSixteen velocity)
    {
        slot.Position = slot.Position.WithY(slot.Position.Y.MovedBy(velocity));
    }

    /// <summary>Pins Samus to the elevator.</summary>
    private static void PinSamusToElevator(RoomEnemySlot slot, SamusState samus)
    {
        samus.Position = samus.Position.WithWrappedWholeY(slot.Position.Y.Whole - ElevatorActorDefinitions.SamusYOffset);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0);
        samus.Position = samus.Position.WithWholeX(slot.Position.X.Whole);
        samus.Kinematics.YSpeed = default;
    }

    /// <summary>Advances Elevator's typed animation.</summary>
    internal static void AdvanceElevatorTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, ElevatorAnimations.Segments,
            static (ElevatorEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Elevator animation raised {cue}."));

    /// <summary>Runs Elevator's initialization AI.</summary>
    internal void RunElevatorInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeElevator(slot, surroundings.Samus);
    }

    /// <summary>Runs Elevator's main AI.</summary>
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

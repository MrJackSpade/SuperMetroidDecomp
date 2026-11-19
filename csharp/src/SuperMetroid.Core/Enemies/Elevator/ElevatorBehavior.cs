using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>The Elevator species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ElevatorBehavior : IEnemySpecies, IEnemyEntityFactory, IElevatorJourney
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Elevator];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new ElevatorEntity(this, place, common);

    /// <summary>The loaded room's bindings, whose game-state services set up Samus's elevator pose when the elevator departs.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues the separate Samus-library and room-library departure sounds.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>The enemy pass's results, which the elevator writes.</summary>
    private readonly EnemyPassEvents _passEvents;

    /// <summary>Samus's passage through a door, during which the elevator holds still.</summary>
    private readonly DoorPassage _doorPassage;

    /// <summary>Binds elevator movement to the room, audio queue, encounter state and Samus's passage through a door.</summary>
    internal ElevatorBehavior(EnemyRoomBinding room, EnemyAudioQueue audio, EnemyPassEvents passEvents, DoorPassage doorPassage)
    {
        _passEvents = passEvents;
        _doorPassage = doorPassage;
        _room = room;
        _audio = audio;
    }

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

    /// <summary>The way the active elevator travels (#2277).</summary>
    public ElevatorTravelDirection ElevatorTravel
    {
        get => _room.Session.Elevator.Travel;
        private set => _room.Session.Elevator.Travel = value;
    }

    /// <summary>
    /// The elevator setup hands Samus's movement to the elevator on departure. On arrival,
    /// downward arrivals hand it back, but on upward arrivals her movement stays locked, doing nothing.
    /// The test reads the travel direction.
    /// </summary>
    public bool ElevatorRunsSamusMovement =>
        ElevatorStatus == ElevatorActorStatus.Departing ||
        (ElevatorStatus != ElevatorActorStatus.Inactive && ElevatorTravel == ElevatorTravelDirection.Down);

    /// <summary>The room's elevator that a ride arriving in the room started on, with Samus on it, once its setup has run.</summary>
    private RoomEnemyPlace? _arrivedOn;

    /// <summary>
    /// The elevator whose setup placed Samus on it as the room loaded for an arriving ride, while that arrival is still
    /// beginning, before the elevator's first update starts its return (#2562); null in any other room or once it has
    /// started. The door loader asks for it on the room's loading update.
    /// </summary>
    public RoomEnemyPlace? ArrivalElevator => ElevatorStatus == ElevatorActorStatus.BeginArrivalReturn ? _arrivedOn : null;

    /// <summary>
    /// Publishes the elevator's pseudo-door collision before the next enemy pass.
    /// Repeated collision scans may set the same status again; the waiting AI clears it when
    /// the direction is not newly pressed, exactly like the elevator's waiting step.
    /// </summary>
    public void PublishElevatorDoorContact() => ElevatorFlags = ElevatorJourneyFlag.DoorContact;

    /// <summary>
    /// Gameplay resumes from the pause, which disarms the pseudo-door (<see cref="UnpauseDisarmsElevator"/>): reaching it
    /// mid-ride starts the door at once, without the down-elevator wait.
    /// </summary>
    public void ResumeFromPause() => ElevatorFlags = UnpauseDisarmsElevator.ArmingAfterResume(ElevatorJourneyFlag.None);

    /// <summary>
    /// Marks the ride as passing through the elevator's door, so the destination elevator's setup sees an arrival
    /// beginning and starts at its arrival height.
    /// </summary>
    public void PrepareElevatorArrival()
    {
        ElevatorFlags = ElevatorJourneyFlag.DoorLoad;
        ElevatorStatus = ElevatorActorStatus.BeginArrivalReturn;
    }

    /// <inheritdoc/>
    public void ResetStatus()
    {
        ElevatorStatus = ElevatorActorStatus.Inactive;
        ElevatorFlags = ElevatorJourneyFlag.None;
    }

    /// <summary>Initializes the Elevator.</summary>
    internal void InitializeElevator(RoomEnemyPlace place, SamusState samus)
    {
        var state = new ElevatorEnemyState();
        place.SetSpeciesState(state);

        place.Frame = EnemyFrameId.Empty;
        state.Playback = EnemyAnimationPlayback.Start(place, ElevatorAnimations.StartingWith(ElevatorSegment.Idle));
        state.RestingY = place.Position.Y.Whole;

        // An elevator in a room the ride is not arriving in rests idle with its journey cleared; one in the room a ride
        // is arriving in starts at its arrival height with Samus on it.
        if (ElevatorStatus != ElevatorActorStatus.BeginArrivalReturn)
        {
            ElevatorFlags = ElevatorJourneyFlag.None;
            ElevatorStatus = ElevatorActorStatus.Inactive;
            return;
        }

        place.Position = place.Position.WithWholeY(place.SpawnSettings<ElevatorSpawnSettings>().ArrivalY);
        PinSamusToElevator(place, samus);
        _arrivedOn = place;
    }

    /// <summary>Runs the elevator's main behaviour and its four phases.</summary>
    private void RunElevatorMain(
        RoomEnemyPlace place,
        ElevatorEnemyState state,
        SamusState samus,
        GameplayAction newlyPressedControllerInput,
        SamusProjectileSystem samusProjectiles)
    {
        if (_doorPassage.IsUnderWay ||
            (ElevatorStatus == ElevatorActorStatus.Inactive && ElevatorFlags == ElevatorJourneyFlag.None))
        {
            return;
        }

        switch (ElevatorStatus)
        {
            case ElevatorActorStatus.Inactive:
                WaitForElevatorDirectionInput(
                    place,
                    samus,
                    newlyPressedControllerInput,
                    samusProjectiles);
                return;

            case ElevatorActorStatus.Departing:
                MoveDepartingElevator(place, samus);
                return;

            case ElevatorActorStatus.BeginArrivalReturn:
                ElevatorStatus = ElevatorActorStatus.ReturningToRest;
                _passEvents.Publish(new ElevatorEvent(ElevatorFrameEvent.ArrivalReturnStarted));
                MoveElevatorBackToRest(place, state, samus);
                return;

            case ElevatorActorStatus.ReturningToRest:
                MoveElevatorBackToRest(place, state, samus);
                return;

            default:
                throw new InvalidDataException(
                    $"Elevator status {ElevatorStatus} is outside the elevator's phases.");
        }
    }

    /// <summary>Waits for the travel direction input.</summary>
    internal void WaitForElevatorDirectionInput(
        RoomEnemyPlace place,
        SamusState samus,
        GameplayAction newlyPressedControllerInput,
        SamusProjectileSystem samusProjectiles)
    {
        ElevatorTravelDirection direction = place.SpawnSettings<ElevatorSpawnSettings>().Direction;
        GameplayAction requiredInput = ElevatorActorDefinitions.RequiredDirectionInputs[direction];
        if (!newlyPressedControllerInput.HasAny(requiredInput))
        {
            ElevatorFlags = ElevatorJourneyFlag.None;
            return;
        }

        // Leaving the room queues the departure sound and then the spin-end sound before Samus's pose changes; they
        // stay two requests because each sound library plays independently.
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.ElevatorDepartureSound, priority: SoundRequestPriority.Normal);
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.ElevatorDepartureSpinEndSound, priority: SoundRequestPriority.Normal);
        SamusPoseMetadata.SetupForElevator(samus);
        samusProjectiles.Reset();
        PinSamusToElevator(place, samus);
        // The ride travels its elevator's way from its departure to its arrival in the destination room (#2531).
        ElevatorTravel = direction;
        ElevatorStatus = ElevatorActorStatus.Departing;
        _passEvents.Publish(new ElevatorEvent(ElevatorFrameEvent.DepartureStarted));
    }

    /// <summary>Moves the departing elevator the way its ride travels.</summary>
    private void MoveDepartingElevator(
        RoomEnemyPlace place,
        SamusState samus)
    {
        AddElevatorYVelocity(place, ElevatorTravel is ElevatorTravelDirection.Up
            ? -ElevatorActorDefinitions.TravelSpeed
            : ElevatorActorDefinitions.TravelSpeed);
        PinSamusToElevator(place, samus);
    }

    /// <summary>Moves the arriving elevator back to rest.</summary>
    private void MoveElevatorBackToRest(
        RoomEnemyPlace place,
        ElevatorEnemyState state,
        SamusState samus)
    {
        bool stillReturning;
        if (place.SpawnSettings<ElevatorSpawnSettings>().Direction is ElevatorTravelDirection.Up)
        {
            AddElevatorYVelocity(place, ElevatorActorDefinitions.TravelSpeed);
            stillReturning = place.Position.Y.Whole < state.RestingY;
        }
        else
        {
            AddElevatorYVelocity(place, -ElevatorActorDefinitions.TravelSpeed);
            stillReturning = place.Position.Y.Whole >= state.RestingY;
        }

        if (stillReturning)
        {
            PinSamusToElevator(place, samus);
            return;
        }

        ElevatorStatus = ElevatorActorStatus.Inactive;
        ElevatorFlags = ElevatorJourneyFlag.None;
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.ElevatorSoundStop, priority: SoundRequestPriority.Normal);
        place.Position = place.Position.WithWholeY(state.RestingY);
        samus.Control.Unlock();
        PinSamusToElevator(place, samus);
        _passEvents.Publish(new ElevatorEvent(ElevatorFrameEvent.ArrivalCompleted));
    }

    /// <summary>Moves the elevator vertically by <paramref name="velocity"/>, as it travels either way.</summary>
    private static void AddElevatorYVelocity(RoomEnemyPlace place, SignedSixteenSixteen velocity)
    {
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(velocity));
    }

    /// <summary>Pins Samus to the elevator.</summary>
    private static void PinSamusToElevator(RoomEnemyPlace place, SamusState samus)
    {
        // A departing up-elevator leaves through the door blocks in the room's top row, which only the ride's one-pixel
        // downward probe meets: Samus's feet reach the top row once her centre has risen above the room's top, so her
        // height wraps for the last updates before the door.
        samus.Position = OffRoomPlaces.WithWholeY(samus.Position, place.Position.Y.Whole - ElevatorActorDefinitions.SamusYOffset);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(PixelFraction.None);
        samus.Position = samus.Position.WithWholeX(place.Position.X.Whole);
        samus.Kinematics.YSpeed = default;
    }

    /// <summary>Advances Elevator's typed animation.</summary>
    internal static void AdvanceElevatorTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, ElevatorAnimations.Segments,
            static (ElevatorEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Elevator animation raised {cue}."));

    /// <summary>Runs Elevator's initialization AI.</summary>
    internal void RunElevatorInitializationAi(RoomEnemyPlace place, EnemyPopulationSurroundings surroundings)
    {
        InitializeElevator(place, surroundings.Samus);
    }

    /// <summary>Runs Elevator's main AI.</summary>
    internal void RunElevatorMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunElevatorMain(
            place,
            place.RequireSpeciesState<ElevatorEnemyState>(),
            frame.Samus,
            frame.NewlyPressedControllerInput,
            frame.SamusProjectiles);
    }
}

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>The Elevator species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class ElevatorBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Elevator];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ElevatorEntity(this, slot, common);

    /// <summary>Provides the active room bus used when departure sets up Samus's elevator pose.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues the separate library-one and library-three departure sounds.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Shares the room's Mother Brain HDMA cleanup state with elevator frame reset.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;

    /// <summary>Binds elevator movement to the room, audio queue, and encounter state.</summary>
    internal ElevatorBehavior(EnemyRoomBinding room, EnemyAudioQueue audio, MotherBrainEncounter motherBrainEncounter)
    {
        _motherBrainEncounter = motherBrainEncounter;
        _room = room;
        _audio = audio;
    }

    /// <summary>Native elevator travel advances one and a half pixels per enemy update.</summary>
    private const int ElevatorSpeedFixed = 98304;

    /// <summary>WRAM $0E16. Collision with an elevator pseudo-door publishes value one.</summary>
    public ushort ElevatorFlags { get; private set; }

    /// <summary>WRAM $0E18, shared across the departure and destination room actors.</summary>
    public ElevatorActorStatus ElevatorStatus { get; private set; }

    /// <summary>WRAM $0799. Bit 15 means the active elevator is travelling upward.</summary>
    public ushort ElevatorDirection { get; private set; }

    /// <summary>
    /// Command seven installs the moving Samus beta handler on departure. At $82:E6A2,
    /// downward arrivals reinstall it, but upward arrivals retain command zero's no-op.
    /// The signed test reads the native upward-direction bit in $0799.
    /// </summary>
    public bool ElevatorRunsSamusMovement =>
        ElevatorStatus == ElevatorActorStatus.Departing ||
        (ElevatorStatus != ElevatorActorStatus.Inactive && (short)ElevatorDirection >= 0);

    /// <summary>
    /// WRAM $0795. Door-transition code owns this gate; nonzero suppresses actor movement
    /// while room loading/fading is already manipulating the same global coordinates.
    /// </summary>
    public bool ElevatorDoorTransitionActive { get; set; }

    /// <summary>Most recent lifecycle event published by elevator AI during this enemy update; reset to <see cref="ElevatorFrameEvent.None"/> before actors run, not retained across updates.</summary>
    public ElevatorFrameEvent LastElevatorEvent { get; private set; }

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
    /// <summary>Library-one sound selector queued by elevator AI this update: <c>$32</c> on departure, otherwise null; a debugger witness, not an additional playback request.</summary>
    public ushort? LastElevatorSoundEffectLibrary1 { get; private set; }
    /// <summary>Latest library-three sound selector queued by elevator AI this update: <c>$0B</c> on departure or <c>$25</c> when arrival completes, otherwise null; reset before each enemy update.</summary>
    public ushort? LastElevatorSoundEffectLibrary3 { get; private set; }

    /// <summary>
    /// True when <c>ResetProjectileData</c> was called by $A3:9548 this frame. The enemy
    /// system clears the ordinary projectile owner directly; the runtime consumes this
    /// event to clear the separate bomb/power-bomb owner at the same boundary.
    /// </summary>
    public bool ElevatorClearedProjectileData { get; private set; }

    /// <summary>
    /// Publishes bank-$94's type-$9 pseudo-door collision before the next EnemyMain pass.
    /// Repeated collision scans may set the same word again; the waiting AI clears it when
    /// the direction button is not newly pressed, exactly like <c>Elevator_Func_1</c>.
    /// </summary>
    public void PublishElevatorDoorContact() => ElevatorFlags = 1;

    /// <summary>
    /// <c>ResumeGameplay</c> ($80:A149) reloads the tiles through <c>$82:E78C</c>, whose first
    /// write clears $0E16. Unpausing mid-ride therefore leaves the elevator's pseudo-door
    /// unarmed: reaching it starts the door at once, without the 48-frame down-elevator wait.
    /// </summary>
    public void ClearElevatorFlagsOnGameplayResume() => ElevatorFlags = 0;

    /// <summary>
    /// Establishes the globals carried through an elevator door load. The destination
    /// actor's initializer recognizes status two and starts at its parameter-2 Y position.
    /// </summary>
    public void PrepareElevatorArrival(ushort doorFlags = ElevatorFlagDefinitions.DoorLoad)
    {
        ElevatorFlags = doorFlags;
        ElevatorStatus = ElevatorActorStatus.BeginArrivalReturn;
    }

    /// <summary>
    /// Debugger/test seam for the already-public runtime WRAM word. Production changes are
    /// still made by collision, actor AI, and room loading rather than arbitrary callers.
    /// </summary>
    internal void SetElevatorStatusForDebugging(ushort status)
    {
        if (status > (ushort)ElevatorActorStatus.ReturningToRest)
            throw new ArgumentOutOfRangeException(nameof(status));
        ElevatorStatus = (ElevatorActorStatus)status;
        if (ElevatorStatus == ElevatorActorStatus.Inactive)
            ElevatorFlags = 0;
    }

    /// <summary>Clears room-local actors while preserving travel globals across the source and destination rooms.</summary>
    internal void ResetElevatorRoomActors()
    {
        // Do not clear flags/status/direction here. Unlike actor-private state, these words
        // intentionally survive destruction of the source room and initialization of the
        // destination room. Elevator_Init itself clears stale values unless status is two.
        _arrivalPlacementSlot = null;
        ElevatorDoorTransitionActive = false;
        BeginElevatorFrame();
    }

    /// <summary>Clears per-update events and sound witnesses before enemy actors run.</summary>
    internal void BeginElevatorFrame()
    {
        LastElevatorEvent = ElevatorFrameEvent.None;
        LastElevatorSoundEffectLibrary1 = null;
        LastElevatorSoundEffectLibrary3 = null;
        ElevatorClearedProjectileData = false;
        _motherBrainEncounter.MotherBrainDeletedHdmaObjects = false;
    }

    /// <summary>Requires the elevator slot to have been initialized before main AI dispatch.</summary>
    internal static ElevatorEnemyState RequireElevatorState(RoomEnemySlot slot) =>
        (slot.Entity as ElevatorEntity)?.State ?? throw new InvalidOperationException(
            $"Elevator slot {slot.SlotIndex} was not initialized.");

    /// <summary>Ports <c>Elevator_Init</c> at $A3:94E6.</summary>
    internal void InitializeElevator(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new ElevatorEnemyState(slot);
        (slot.Entity as ElevatorEntity ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} is not bound to a Elevator.")).State = state;

        slot.SpritemapPointer = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.Program = AnimationProgramCursor<ElevatorProgram>.Start(ElevatorProgram.Loop);
        slot.Parameter1 = unchecked((ushort)(slot.Parameter1 * 2));
        state.RestingYPosition = slot.Position.Y.Whole;

        if (ElevatorStatus != ElevatorActorStatus.BeginArrivalReturn)
        {
            ElevatorFlags = 0;
            ElevatorStatus = ElevatorActorStatus.Inactive;
        }

        // The native 32-bit OR test is true for status two even if a malformed caller lost
        // the door flag. Preserve that behavior and reject only the impossible missing Samus.
        if (ElevatorStatus != ElevatorActorStatus.Inactive || ElevatorFlags != 0)
        {
            if (samus is null)
                throw new InvalidOperationException("Arriving elevator initialization requires Samus state.");
            slot.Position = slot.Position.WithWholeY(slot.Parameter2);
            PinSamusToElevator(slot, samus);
            _arrivalPlacementSlot = slot.SlotIndex;
        }
    }

    /// <summary>Ports <c>Elevator_Frozen</c> and its four-entry dispatcher at $A3:952A.</summary>
    private void RunElevatorMain(
        RoomEnemySlot slot,
        ElevatorEnemyState state,
        SamusState? samus,
        SnesButton newlyPressedControllerInput,
        SamusProjectileSystem? samusProjectiles)
    {
        if (ElevatorDoorTransitionActive ||
            (ElevatorStatus == ElevatorActorStatus.Inactive && ElevatorFlags == 0))
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
                LastElevatorEvent = ElevatorFrameEvent.ArrivalReturnStarted;
                MoveElevatorBackToRest(slot, state, samus);
                return;

            case ElevatorActorStatus.ReturningToRest:
                MoveElevatorBackToRest(slot, state, samus);
                return;

            default:
                throw new InvalidDataException(
                    $"Elevator status {(ushort)ElevatorStatus} is outside $A3:9540's dispatcher.");
        }
    }

    /// <summary>Ports <c>Elevator_Func_1</c> at $A3:9548.</summary>
    internal void WaitForElevatorDirectionInput(
        RoomEnemySlot slot,
        ElevatorEnemyState state,
        SamusState samus,
        SnesButton newlyPressedControllerInput,
        SamusProjectileSystem? samusProjectiles)
    {
        SnesButton requiredInput =
            ElevatorActorDefinitions.RequiredDirectionInput(state.DirectionTableByteOffset);
        if ((newlyPressedControllerInput & requiredInput) == 0)
        {
            ElevatorFlags = 0;
            return;
        }

        LastElevatorSoundEffectLibrary3 = EnemySoundEffectDefinitions.ElevatorDepartureSoundLibrary3;
        LastElevatorSoundEffectLibrary1 = EnemySoundEffectDefinitions.ElevatorDepartureSoundLibrary1;
        // $A3:9548 issues both calls in this order before changing Samus's pose. Keeping
        // them as two requests matters because each library owns an independent SPC port.
        _audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, EnemySoundEffectDefinitions.ElevatorDepartureSoundLibrary3), maximumQueued: 6);
        _audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, EnemySoundEffectDefinitions.ElevatorDepartureSoundLibrary1), maximumQueued: 6);
        samus.PoseMetadata.SetupForElevator(_room.LoadedBus);
        samusProjectiles?.Reset();
        ElevatorClearedProjectileData = true;
        PinSamusToElevator(slot, samus);
        ElevatorStatus = ElevatorActorStatus.Departing;
        LastElevatorEvent = ElevatorFrameEvent.DepartureStarted;
    }

    /// <summary>Ports <c>Elevator_Func_2</c> at $A3:9579.</summary>
    private void MoveDepartingElevator(
        RoomEnemySlot slot,
        ElevatorEnemyState state,
        SamusState samus)
    {
        if (state.DirectionTableByteOffset != 0)
        {
            ElevatorDirection = 32768;
            AddElevatorYVelocity(slot, -ElevatorSpeedFixed);
        }
        else
        {
            ElevatorDirection = 0;
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
            stillReturning = slot.Position.Y.Whole < state.RestingYPosition;
        }
        else
        {
            AddElevatorYVelocity(slot, -ElevatorSpeedFixed);
            stillReturning = slot.Position.Y.Whole >= state.RestingYPosition;
        }

        if (stillReturning)
        {
            PinSamusToElevator(slot, samus);
            return;
        }

        ElevatorStatus = ElevatorActorStatus.Inactive;
        ElevatorFlags = 0;
        LastElevatorSoundEffectLibrary3 = EnemySoundEffectDefinitions.ElevatorArrivalSoundLibrary3;
        _audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, EnemySoundEffectDefinitions.ElevatorArrivalSoundLibrary3), maximumQueued: 6);
        slot.Position = slot.Position.WithWholeY(state.RestingYPosition);
        samus.InputLocked = false;
        PinSamusToElevator(slot, samus);
        LastElevatorEvent = ElevatorFrameEvent.ArrivalCompleted;
    }

    /// <summary>Ports the direct 16.16 <c>AddToHiLo</c> used by both travel directions.</summary>
    private static void AddElevatorYVelocity(RoomEnemySlot slot, int signedVelocity)
    {
        uint fixedPosition = slot.Position.Y.RawValue;
        fixedPosition = unchecked(fixedPosition + (uint)signedVelocity);
        slot.Position = slot.Position.WithY(SnesFixedPosition.FromRaw(unchecked((uint)fixedPosition)));
    }

    /// <summary>Ports <c>Elevator_Func_4</c> at $A3:9612.</summary>
    private static void PinSamusToElevator(RoomEnemySlot slot, SamusState samus)
    {
        samus.Position = samus.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - ElevatorActorDefinitions.SamusYOffset)));
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0);
        samus.Position = samus.Position.WithWholeX(slot.Position.X.Whole);
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
    }

    /// <summary>Advances Elevator's typed animation.</summary>
    internal static void AdvanceElevatorTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireElevatorState(slot);
        AnimationProgramCursor<ElevatorProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, ElevatorAnimations.Programs, ref cursor, ElevatorFrameDefinitions.Spritemap);
        state.Program = cursor;
    }

    /// <summary>Ports Elevator's initialization AI.</summary>
    internal void RunElevatorInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? samus, SnesButton _1, ushort _2)
    {
        InitializeElevator(slot, samus);
    }

    /// <summary>Ports Elevator's main AI.</summary>
    internal void RunElevatorMainAi(RoomEnemySlot slot, IEnemyAiFrame frame, byte _)
    {
        RunElevatorMain(
            slot,
            RequireElevatorState(slot),
            frame.Samus,
            frame.NewlyPressedControllerInput,
            frame.SamusProjectiles);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetElevatorRoomActors();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        BeginElevatorFrame();
    }
}

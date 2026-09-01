namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal values of WRAM <c>elevator_status</c> consumed by the four-entry dispatcher at
/// <c>$A3:9540</c>. The value is global because it survives the room transition between the
/// departing and arriving elevator actors.
/// </summary>
public enum ElevatorActorStatus : ushort
{
    Inactive = 0,
    Departing = 1,
    BeginArrivalReturn = 2,
    ReturningToRest = 3,
}

/// <summary>Cross-system effects published by elevator AI during the current enemy frame.</summary>
public enum ElevatorFrameEvent
{
    None,
    DepartureStarted,
    ArrivalReturnStarted,
    ArrivalCompleted,
}

/// <summary>
/// Typed projection of the one private word used by elevator enemy <c>$D73F</c>. Native
/// variable A retains the room-side resting Y coordinate while the global status survives
/// travel to the destination room.
/// </summary>
public sealed class ElevatorEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal ElevatorEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Enemy variable A, captured before an arriving actor is moved to parameter 2.</summary>
    public ushort RestingYPosition
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>
    /// Parameter 1 after the initializer doubles it. This is a byte offset into the two-word
    /// input-mask table at <c>$A3:94E2</c>, not an invented direction boolean.
    /// </summary>
    public ushort DirectionTableByteOffset => _slot.Parameter1;
}

/// <summary>Literal translation of the ordinary elevator actor at $A3:94E6-$A3:962E.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort ElevatorDefinition = 0xd73f;

    private const ushort ElevatorNothingSpritemap = 0x804d;
    private const ushort ElevatorInstructionList = 0x94d6;
    private const int ElevatorInputMaskTable = 0xa394e2;
    private const int ElevatorSpeedFixed = 0x00018000;
    private const ushort ElevatorSamusYOffset = 26;
    private const ushort ElevatorDepartureSoundLibrary1 = 0x0032;
    private const ushort ElevatorDepartureSoundLibrary3 = 0x000b;
    private const ushort ElevatorArrivalSoundLibrary3 = 0x0025;

    private readonly ElevatorEnemyState?[] _elevatorStates =
        new ElevatorEnemyState?[MaximumEnemyCount];

    /// <summary>Typed actor extensions in fixed native slot order.</summary>
    public IReadOnlyList<ElevatorEnemyState?> ElevatorStates => _elevatorStates;

    /// <summary>WRAM $0E16. Collision with an elevator pseudo-door publishes value one.</summary>
    public ushort ElevatorFlags { get; private set; }

    /// <summary>WRAM $0E18, shared across the departure and destination room actors.</summary>
    public ElevatorActorStatus ElevatorStatus { get; private set; }

    /// <summary>WRAM $0799. Bit 15 means the active elevator is travelling upward.</summary>
    public ushort ElevatorDirection { get; private set; }

    /// <summary>
    /// WRAM $0795. Door-transition code owns this gate; nonzero suppresses actor movement
    /// while room loading/fading is already manipulating the same global coordinates.
    /// </summary>
    public bool ElevatorDoorTransitionActive { get; set; }

    public ElevatorFrameEvent LastElevatorEvent { get; private set; }
    public ushort? LastElevatorSoundEffectLibrary1 { get; private set; }
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
    /// Establishes the globals carried through an elevator door load. The destination
    /// actor's initializer recognizes status two and starts at its parameter-2 Y position.
    /// </summary>
    public void PrepareElevatorArrival(ushort doorFlags = 0x0080)
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

    private void ResetElevatorRoomActors()
    {
        // Do not clear flags/status/direction here. Unlike actor-private state, these words
        // intentionally survive destruction of the source room and initialization of the
        // destination room. Elevator_Init itself clears stale values unless status is two.
        Array.Clear(_elevatorStates);
        ElevatorDoorTransitionActive = false;
        BeginElevatorFrame();
    }

    private void BeginElevatorFrame()
    {
        LastElevatorEvent = ElevatorFrameEvent.None;
        LastElevatorSoundEffectLibrary1 = null;
        LastElevatorSoundEffectLibrary3 = null;
        ElevatorClearedProjectileData = false;
    }

    private ElevatorEnemyState RequireElevatorState(RoomEnemySlot slot) =>
        _elevatorStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Elevator slot {slot.SlotIndex} was not initialized.");

    /// <summary>Ports <c>Elevator_Init</c> at $A3:94E6.</summary>
    private void InitializeElevator(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new ElevatorEnemyState(slot);
        _elevatorStates[slot.SlotIndex] = state;

        slot.SpritemapPointer = ElevatorNothingSpritemap;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = ElevatorInstructionList;
        slot.Parameter1 = unchecked((ushort)(slot.Parameter1 * 2));
        state.RestingYPosition = slot.YPosition;

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
            slot.YPosition = slot.Parameter2;
            PinSamusToElevator(slot, samus);
        }
    }

    /// <summary>Ports <c>Elevator_Frozen</c> and its four-entry dispatcher at $A3:952A.</summary>
    private void RunElevatorMain(
        RoomEnemySlot slot,
        ElevatorEnemyState state,
        SamusState? samus,
        ushort newlyPressedControllerInput,
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
    private void WaitForElevatorDirectionInput(
        RoomEnemySlot slot,
        ElevatorEnemyState state,
        SamusState samus,
        ushort newlyPressedControllerInput,
        SamusProjectileSystem? samusProjectiles)
    {
        if (state.DirectionTableByteOffset > 2 ||
            (state.DirectionTableByteOffset & 1) != 0)
        {
            throw new InvalidDataException(
                $"Elevator direction offset ${state.DirectionTableByteOffset:X4} exceeds " +
                "$A3:94E2's two-word input table.");
        }

        ushort requiredInput = ReadWord(
            _bus!,
            ElevatorInputMaskTable + state.DirectionTableByteOffset);
        if ((newlyPressedControllerInput & requiredInput) == 0)
        {
            ElevatorFlags = 0;
            return;
        }

        LastElevatorSoundEffectLibrary3 = ElevatorDepartureSoundLibrary3;
        LastElevatorSoundEffectLibrary1 = ElevatorDepartureSoundLibrary1;
        // $A3:9548 issues both calls in this order before changing Samus's pose. Keeping
        // them as two requests matters because each library owns an independent SPC port.
        QueueEnemySound(library: 3, soundId: ElevatorDepartureSoundLibrary3, maximumQueued: 6);
        QueueEnemySound(library: 1, soundId: ElevatorDepartureSoundLibrary1, maximumQueued: 6);
        samus.ApplyForwardFacingPoseSetup(_bus!);
        samus.InputLocked = true;
        samus.PrimeGraphics(_bus!);
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
            ElevatorDirection = 0x8000;
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
            stillReturning = slot.YPosition < state.RestingYPosition;
        }
        else
        {
            AddElevatorYVelocity(slot, -ElevatorSpeedFixed);
            stillReturning = slot.YPosition >= state.RestingYPosition;
        }

        if (stillReturning)
        {
            PinSamusToElevator(slot, samus);
            return;
        }

        ElevatorStatus = ElevatorActorStatus.Inactive;
        ElevatorFlags = 0;
        LastElevatorSoundEffectLibrary3 = ElevatorArrivalSoundLibrary3;
        QueueEnemySound(library: 3, soundId: ElevatorArrivalSoundLibrary3, maximumQueued: 6);
        slot.YPosition = state.RestingYPosition;
        samus.InputLocked = false;
        PinSamusToElevator(slot, samus);
        LastElevatorEvent = ElevatorFrameEvent.ArrivalCompleted;
    }

    /// <summary>Ports the direct 16.16 <c>AddToHiLo</c> used by both travel directions.</summary>
    private static void AddElevatorYVelocity(RoomEnemySlot slot, int signedVelocity)
    {
        uint fixedPosition = ((uint)slot.YPosition << 16) | slot.YSubposition;
        fixedPosition = unchecked(fixedPosition + (uint)signedVelocity);
        slot.YPosition = unchecked((ushort)(fixedPosition >> 16));
        slot.YSubposition = unchecked((ushort)fixedPosition);
    }

    /// <summary>Ports <c>Elevator_Func_4</c> at $A3:9612.</summary>
    private static void PinSamusToElevator(RoomEnemySlot slot, SamusState samus)
    {
        samus.YPosition = unchecked((ushort)(slot.YPosition - ElevatorSamusYOffset));
        samus.Kinematics.YSubposition = 0;
        samus.XPosition = slot.XPosition;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
    }
}

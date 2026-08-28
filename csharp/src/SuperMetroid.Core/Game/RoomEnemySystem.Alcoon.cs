using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A8 function pointer stored in Alcoon's common variable zero.</summary>
public enum AlcoonEnemyFunction : ushort
{
    WaitingForSamus = 0xdd71,
    EmergingRising = 0xddc6,
    EmergingFalling = 0xde05,
    WalkingAndFiring = 0xde4b,
    WaitingForFireAnimation = 0xdecc,
    HidingRising = 0xdecd,
    HidingFalling = 0xdeec,
}

/// <summary>
/// Typed debugger view of Alcoon's common enemy-slot aliases and its five words in extra
/// enemy WRAM. The properties deliberately retain native 16-bit fixed-point halves: this
/// makes carry, signed wrap, and the initializer's surviving landing subposition visible.
/// </summary>
public sealed class AlcoonEnemyState
{
    private readonly RoomEnemySlot _slot;
    private readonly ushort[] _yAccelerations;
    private readonly ushort[] _ySubaccelerations;
    private readonly ushort[] _spawnXPositions;
    private readonly ushort[] _landingYPositions;
    private readonly ushort[] _stepCounters;

    internal AlcoonEnemyState(
        RoomEnemySlot slot,
        ushort[] yAccelerations,
        ushort[] ySubaccelerations,
        ushort[] spawnXPositions,
        ushort[] landingYPositions,
        ushort[] stepCounters)
    {
        _slot = slot;
        _yAccelerations = yAccelerations;
        _ySubaccelerations = ySubaccelerations;
        _spawnXPositions = spawnXPositions;
        _landingYPositions = landingYPositions;
        _stepCounters = stepCounters;
    }

    public AlcoonEnemyFunction Function
    {
        get => (AlcoonEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    public ushort YVelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    public ushort YSubvelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Signed whole-pixel horizontal velocity; retail Alcoon uses -2, 0, or +2.</summary>
    public ushort XVelocity
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>
    /// Common variable four is cleared by <c>SetupAlcoonJumpMovement</c> even though this
    /// actor never reads it. Retaining the alias prevents the translated setup from quietly
    /// omitting an observable WRAM side effect.
    /// </summary>
    public ushort ClearedVariable
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    public ushort SpawnYPosition
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    public ushort YAcceleration
    {
        get => _yAccelerations[_slot.SlotIndex];
        internal set => _yAccelerations[_slot.SlotIndex] = value;
    }

    public ushort YSubacceleration
    {
        get => _ySubaccelerations[_slot.SlotIndex];
        internal set => _ySubaccelerations[_slot.SlotIndex] = value;
    }

    public ushort SpawnXPosition
    {
        get => _spawnXPositions[_slot.SlotIndex];
        internal set => _spawnXPositions[_slot.SlotIndex] = value;
    }

    /// <summary>Floor-aligned center Y discovered by the complete initialization jump.</summary>
    public ushort LandingYPosition
    {
        get => _landingYPositions[_slot.SlotIndex];
        internal set => _landingYPositions[_slot.SlotIndex] = value;
    }

    /// <summary>Number of four-step walking cycles remaining before another fire volley.</summary>
    public ushort StepCounter
    {
        get => _stepCounters[_slot.SlotIndex];
        internal set => _stepCounters[_slot.SlotIndex] = value;
    }
}

/// <summary>Literal translation of Alcoon enemy <c>$E9BF</c> at <c>$A8:DBE7-$DF9C</c>.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort AlcoonDefinition = 0xe9bf;

    private const ushort AlcoonWalkingLeftEntry = 0xdbe7;
    private const ushort AlcoonWalkingLeftFirstFrame = 0xdbe9;
    private const ushort AlcoonFireLeft = 0xdc03;
    private const ushort AlcoonAirborneLeftLookingUp = 0xdc4b;
    private const ushort AlcoonAirborneLeftLookingForward = 0xdc51;
    private const ushort AlcoonWalkingRightEntry = 0xdc57;
    private const ushort AlcoonWalkingRightFirstFrame = 0xdc59;
    private const ushort AlcoonFireRight = 0xdc73;
    private const ushort AlcoonAirborneRightLookingUp = 0xdcbb;
    private const ushort AlcoonAirborneRightLookingForward = 0xdcc1;
    private const ushort AlcoonEmergeXDistance = 0x0050;
    private const ushort AlcoonHideXDistance = 0x0070;
    private const ushort AlcoonEmergeSound = 0x005e;
    private const ushort AlcoonFireSound = 0x003f;
    private const int AlcoonJumpSimulationLimit = 4096;

    private readonly ushort[] _alcoonYAccelerations = new ushort[MaximumEnemyCount];
    private readonly ushort[] _alcoonYSubaccelerations = new ushort[MaximumEnemyCount];
    private readonly ushort[] _alcoonSpawnXPositions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _alcoonLandingYPositions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _alcoonStepCounters = new ushort[MaximumEnemyCount];
    private readonly AlcoonEnemyState?[] _alcoonStates =
        new AlcoonEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Alcoon state for all 32 physical enemy slots.</summary>
    public IReadOnlyList<AlcoonEnemyState?> AlcoonStates => _alcoonStates;

    /// <summary>Ports <c>InitAI_Alcoon</c> at <c>$A8:DCCD</c>, including its floor search.</summary>
    private void InitializeAlcoon(RoomEnemySlot slot, RoomLevelData? level)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                "Alcoon initialization requires the room's collision layer to simulate its landing.");
        }

        var state = new AlcoonEnemyState(
            slot,
            _alcoonYAccelerations,
            _alcoonYSubaccelerations,
            _alcoonSpawnXPositions,
            _alcoonLandingYPositions,
            _alcoonStepCounters);
        _alcoonStates[slot.SlotIndex] = state;

        state.StepCounter = 0;
        state.SpawnYPosition = slot.YPosition;
        state.SpawnXPosition = slot.XPosition;
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.CurrentInstruction = AlcoonWalkingLeftEntry;
        state.Function = AlcoonEnemyFunction.WaitingForSamus;
        SetupAlcoonJumpMovement(state);

        // The original initializer really executes every half-pixel-accelerated ascent
        // step, then every collision-aware descent step. It ultimately restores only the
        // whole Y coordinate; the collision-produced subposition deliberately survives.
        int iterations = 0;
        do
        {
            AddAlcoonYVelocity(slot, state);
            AccelerateAlcoonY(state);
            GuardAlcoonJumpSimulation(++iterations, slot);
        }
        while (unchecked((short)state.YVelocity) < 0);

        while (true)
        {
            int displacement = ComposeSignedFixed(state.YVelocity, state.YSubvelocity);
            if (MoveEnemyVertically(level, slot, displacement))
                break;
            AccelerateAlcoonY(state);
            GuardAlcoonJumpSimulation(++iterations, slot);
        }

        state.LandingYPosition = slot.YPosition;
        slot.YPosition = state.SpawnYPosition;
    }

    /// <summary>Ports <c>MainAI_Alcoon</c> and all seven indirect targets.</summary>
    private void RunAlcoonMain(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        switch (state.Function)
        {
            case AlcoonEnemyFunction.WaitingForSamus:
                WaitForAlcoonActivation(slot, state, samus);
                return;
            case AlcoonEnemyFunction.EmergingRising:
                RunAlcoonEmergingRise(slot, state);
                return;
            case AlcoonEnemyFunction.EmergingFalling:
                RunAlcoonEmergingFall(slot, state, samus, level);
                return;
            case AlcoonEnemyFunction.WalkingAndFiring:
                RunEmergedAlcoon(slot, state, samus, level);
                return;
            case AlcoonEnemyFunction.WaitingForFireAnimation:
                return;
            case AlcoonEnemyFunction.HidingRising:
                RunAlcoonHidingRise(slot, state);
                return;
            case AlcoonEnemyFunction.HidingFalling:
                RunAlcoonHidingFall(slot, state);
                return;
            default:
                throw new NotSupportedException(
                    $"Alcoon function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void WaitForAlcoonActivation(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Alcoon proximity AI requires the active Samus actor.");

        if (WrappedMagnitude(unchecked((ushort)(state.LandingYPosition - samus.YPosition))) >= 0x20)
            return;

        ushort signedXDistance = unchecked((ushort)(samus.XPosition - slot.XPosition));
        if (WrappedMagnitude(signedXDistance) >= AlcoonEmergeXDistance)
            return;

        SetupAlcoonJumpMovement(state);
        if (unchecked((short)signedXDistance) < 0)
        {
            state.XVelocity = unchecked((ushort)-2);
            InstallAlcoonInstruction(slot, AlcoonAirborneLeftLookingUp);
        }
        else
        {
            state.XVelocity = 2;
            InstallAlcoonInstruction(slot, AlcoonAirborneRightLookingUp);
        }
        state.Function = AlcoonEnemyFunction.EmergingRising;
        LastAlcoonSoundEffect = AlcoonEmergeSound;
    }

    private static void RunAlcoonEmergingRise(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        AccelerateAlcoonY(state);
        if (unchecked((short)state.YVelocity) < 0)
            return;

        state.Function = AlcoonEnemyFunction.EmergingFalling;
        InstallAlcoonInstruction(
            slot,
            unchecked((short)state.XVelocity) < 0
                ? AlcoonAirborneLeftLookingForward
                : AlcoonAirborneRightLookingForward);
    }

    private void RunAlcoonEmergingFall(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Alcoon landing direction requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Alcoon falling AI requires room collision data.");

        int displacement = ComposeSignedFixed(state.YVelocity, state.YSubvelocity);
        if (!MoveEnemyVertically(level, slot, displacement))
        {
            AccelerateAlcoonY(state);
            return;
        }

        ushort signedXDistance = unchecked((ushort)(samus.XPosition - slot.XPosition));
        if (unchecked((short)signedXDistance) < 0)
        {
            state.XVelocity = unchecked((ushort)-2);
            InstallAlcoonInstruction(slot, AlcoonWalkingLeftEntry);
        }
        else
        {
            state.XVelocity = 2;
            InstallAlcoonInstruction(slot, AlcoonWalkingRightEntry);
        }
        state.Function = AlcoonEnemyFunction.WalkingAndFiring;
        state.StepCounter = 1;
    }

    private void RunEmergedAlcoon(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Alcoon firing AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Alcoon walking AI requires room collision data.");

        // This is a real two-pixel downward move, not merely a floor probe. On ordinary
        // floors it collides and keeps Alcoon aligned; if the floor vanishes it lets the
        // walking actor fall by the same amount before the remainder of main AI runs.
        MoveEnemyVertically(level, slot, 2 << 16);

        ushort spawnDistance = unchecked((ushort)(state.SpawnXPosition - slot.XPosition));
        if (WrappedMagnitude(spawnDistance) >= AlcoonHideXDistance)
        {
            state.Function = AlcoonEnemyFunction.HidingRising;
            state.YVelocity = unchecked((ushort)-4);
            state.YSubvelocity = 0;
            InstallAlcoonInstruction(
                slot,
                unchecked((short)state.XVelocity) < 0
                    ? AlcoonAirborneLeftLookingForward
                    : AlcoonAirborneRightLookingForward);
            return;
        }

        if (state.StepCounter != 0)
            return;

        bool samusIsLeft = unchecked((short)(samus.XPosition - slot.XPosition)) < 0;
        bool movingLeft = unchecked((short)state.XVelocity) < 0;
        if (samusIsLeft != movingLeft)
            return;

        InstallAlcoonInstruction(slot, movingLeft ? AlcoonFireLeft : AlcoonFireRight);
        state.Function = AlcoonEnemyFunction.WaitingForFireAnimation;
    }

    private static void RunAlcoonHidingRise(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        AccelerateAlcoonY(state);
        if (unchecked((short)state.YVelocity) >= 0)
            state.Function = AlcoonEnemyFunction.HidingFalling;
    }

    private static void RunAlcoonHidingFall(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        if (unchecked((short)(slot.YPosition - state.SpawnYPosition)) < 0)
        {
            AccelerateAlcoonY(state);
            return;
        }

        // Like the original, the reset snaps only whole coordinates. Both subpositions and
        // the last animation map remain live while the actor waits underground.
        slot.YPosition = state.SpawnYPosition;
        slot.XPosition = state.SpawnXPosition;
        state.Function = AlcoonEnemyFunction.WaitingForSamus;
    }

    private static void SetupAlcoonJumpMovement(AlcoonEnemyState state)
    {
        state.YVelocity = unchecked((ushort)-12);
        state.YSubvelocity = 0;
        state.XVelocity = 0;
        state.ClearedVariable = 0;
        state.YAcceleration = 0;
        state.YSubacceleration = 0x8000;
    }

    private static void AccelerateAlcoonY(AlcoonEnemyState state)
    {
        uint fractionalSum = (uint)state.YSubvelocity + state.YSubacceleration;
        state.YSubvelocity = unchecked((ushort)fractionalSum);
        state.YVelocity = unchecked((ushort)(
            state.YVelocity + state.YAcceleration + (fractionalSum >> 16)));
    }

    private static void AddAlcoonYVelocity(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        uint fixedY = ((uint)slot.YPosition << 16) | slot.YSubposition;
        fixedY = unchecked(fixedY + (uint)ComposeSignedFixed(
            state.YVelocity,
            state.YSubvelocity));
        slot.YPosition = unchecked((ushort)(fixedY >> 16));
        slot.YSubposition = unchecked((ushort)fixedY);
    }

    private static ushort WrappedMagnitude(ushort value) =>
        unchecked((short)value) < 0 ? unchecked((ushort)-value) : value;

    private static void GuardAlcoonJumpSimulation(int iterations, RoomEnemySlot slot)
    {
        if (iterations > AlcoonJumpSimulationLimit)
        {
            throw new InvalidDataException(
                $"Alcoon slot {slot.SlotIndex} did not find a landing floor within " +
                $"{AlcoonJumpSimulationLimit} initialization steps.");
        }
    }

    private static void InstallAlcoonInstruction(RoomEnemySlot slot, ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
    }

    /// <summary>Implements Alcoon instruction <c>$A8:DF3F</c> and returns native Y.</summary>
    private ushort StartAlcoonWalking(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        state.Function = AlcoonEnemyFunction.WalkingAndFiring;
        Func<ushort> readRandomNumber = _readRandomNumber ??
            throw new InvalidOperationException(
                "Alcoon instruction $A8:DF3F requires a non-advancing random-seed reader.");
        ushort count = unchecked((ushort)(readRandomNumber() & 3));
        state.StepCounter = count == 0 ? (ushort)2 : count;
        return unchecked((short)state.XVelocity) < 0
            ? AlcoonWalkingLeftEntry
            : AlcoonWalkingRightEntry;
    }

    /// <summary>
    /// Implements both movement bytecodes at <c>$A8:DF63/$DF71</c>. The caller passes the
    /// interpreter cursor already advanced beyond the opcode, matching the native Y register.
    /// </summary>
    private ushort MoveAlcoonHorizontally(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        RoomLevelData? level,
        ushort nextCursor,
        bool decrementStepCounter)
    {
        if (level is null)
            throw new InvalidOperationException("Alcoon movement instruction requires room collision data.");

        if (decrementStepCounter && state.StepCounter != 0)
            state.StepCounter = unchecked((ushort)(state.StepCounter - 1));

        int displacement = unchecked((short)state.XVelocity) << 16;
        if (!MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
        {
            AlignEnemyYWithNonSquareSlope(level, slot);
            return nextCursor;
        }

        ushort oldVelocity = state.XVelocity;
        state.XVelocity = unchecked((ushort)-oldVelocity);
        return unchecked((short)oldVelocity) >= 0
            ? AlcoonWalkingLeftFirstFrame
            : AlcoonWalkingRightFirstFrame;
    }

    private AlcoonEnemyState RequireAlcoonState(RoomEnemySlot slot) =>
        _alcoonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Alcoon state.");
}

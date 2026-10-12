using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A8 function pointer stored in Alcoon's common variable zero.</summary>
public enum AlcoonEnemyFunction : ushort
{
    /// <summary>$A8:DD71, <c>Function_Alcoon_WaitForSamusToGetNear</c>: starts the emergence jump when Samus is strictly within 80 horizontal pixels and 32 pixels of the previously discovered landing Y.</summary>
    WaitingForSamus = 0xdd71,
    /// <summary>$A8:DDC6, <c>Function_Alcoon_Emerging_Rising</c>: integrates upward vertical motion with half-pixel acceleration until the whole velocity becomes nonnegative, then selects the falling pose.</summary>
    EmergingRising = 0xddc6,
    /// <summary>$A8:DE05, <c>Function_Alcoon_Emerging_Falling</c>: descends through the terrain collision mover, then faces Samus and begins walking with one walking cycle remaining.</summary>
    EmergingFalling = 0xde05,
    /// <summary>$A8:DE4B, <c>Function_Alcoon_MoveHorizontally_SpitFireballsAtSamus</c>: follows the floor, hides after travelling 112 pixels from spawn, or requests a volley when the step counter is zero and Samus is ahead; animation commands perform horizontal movement.</summary>
    WalkingAndFiring = 0xde4b,
    /// <summary>$A8:DECC, <c>RTL_A8DECC</c>: performs no main-AI movement while the fire-volley animation runs, until its command restores walking.</summary>
    WaitingForFireAnimation = 0xdecc,
    /// <summary>$A8:DECD, <c>Function_Alcoon_Hiding_Rising</c>: integrates the retreat jump's upward motion until acceleration changes its whole velocity to nonnegative.</summary>
    HidingRising = 0xdecd,
    /// <summary>$A8:DEEC, <c>Function_Alcoon_Hiding_Falling</c>: falls without terrain collision until reaching spawn Y, then restores the whole spawn coordinates and proximity-wait state while preserving subpositions.</summary>
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

    /// <summary>Bank-$A8 indirect AI target, represented by common variable A here and corresponding to native <c>Alcoon.function</c> at $0FA8,x.</summary>
    public AlcoonEnemyFunction Function
    {
        get => (AlcoonEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Motion per step. Y is the signed 16.16 vertical velocity in native $0FAA,x (whole) and
    /// $0FAC,x (fraction): emergence starts at -12 and retreat at -4, and acceleration carries
    /// from the fraction into the whole word. X is the signed whole-pixel horizontal velocity in
    /// common variable D (retail uses -2, 0 or +2); Alcoon has no horizontal fraction word, so
    /// a velocity with a nonzero X fraction is rejected.
    /// </summary>
    public FixedVector<RoomPixels> Velocity
    {
        get => FixedVector<RoomPixels>.FromWords(_slot.VariableD, 0, _slot.VariableB, _slot.VariableC);
        internal set
        {
            if (value.X.Fraction != 0)
                throw new InvalidOperationException(
                    $"Alcoon has no horizontal subvelocity word; X fraction ${value.X.Fraction:X4} cannot be stored.");
            _slot.VariableB = value.Y.WholeWord;
            _slot.VariableC = value.Y.Fraction;
            _slot.VariableD = value.X.WholeWord;
        }
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

    /// <summary>
    /// Original population whole-pixel position. Y (native $0FB2,x, common variable F) is
    /// restored after the initialization floor search and after retreat, leaving the live Y
    /// subposition unchanged; X (native $7E:7804,x) enforces the 112-pixel walking range and is
    /// restored when hiding completes.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition
    {
        get => new(_spawnXPositions[_slot.SlotIndex], _slot.VariableF);
        internal set
        {
            _spawnXPositions[_slot.SlotIndex] = value.X;
            _slot.VariableF = value.Y;
        }
    }

    /// <summary>Whole-pixel component of the vertical velocity increment per acceleration step, paired with <see cref="YSubacceleration"/>; jump setup writes zero. Native $7E:7800,x.</summary>
    public ushort YAcceleration
    {
        get => _yAccelerations[_slot.SlotIndex];
        internal set => _yAccelerations[_slot.SlotIndex] = value;
    }

    /// <summary>Fractional vertical velocity increment in 1/65536 pixel per motion-step squared; jump setup writes $8000, adding half a pixel per step to downward velocity. Native $7E:7802,x.</summary>
    public ushort YSubacceleration
    {
        get => _ySubaccelerations[_slot.SlotIndex];
        internal set => _ySubaccelerations[_slot.SlotIndex] = value;
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
    /// <summary>Executes Alcoon's private animation instructions.</summary>
    private bool TryProcessAlcoonInstruction(RoomEnemySlot slot, RoomLevelData? level, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Alcoon ||
            !Enum.IsDefined((AlcoonInstruction)word))
            return false;

        switch ((AlcoonInstruction)word)
        {
            case AlcoonInstruction.SpawnFireballUpward:
                SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 0);
                LastAlcoonSoundEffect = AlcoonFireSound;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case AlcoonInstruction.SpawnFireballHorizontally:
                SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 2);
                LastAlcoonSoundEffect = AlcoonFireSound;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case AlcoonInstruction.SpawnFireballDownward:
                SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 4);
                LastAlcoonSoundEffect = AlcoonFireSound;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case AlcoonInstruction.StartWalking:
                cursor = StartAlcoonWalking(RequireAlcoonState(slot));
                return true;
            case AlcoonInstruction.DecrementStepCounterMoveHorizontally:
                cursor = MoveAlcoonHorizontally(
                    slot,
                    RequireAlcoonState(slot),
                    level,
                    unchecked((ushort)(cursor + 2)),
                    decrementStepCounter: true);
                return true;
            case AlcoonInstruction.MoveHorizontallyTurnIfWallCollision:
                cursor = MoveAlcoonHorizontally(
                    slot,
                    RequireAlcoonState(slot),
                    level,
                    unchecked((ushort)(cursor + 2)),
                    decrementStepCounter: false);
                return true;
            default:
                throw new InvalidOperationException(
                    $"Alcoon does not own instruction ${word:X4}.");
        }
    }


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
        state.SpawnPosition = slot.Position.Whole;
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.CurrentInstruction = AlcoonInstructionProgramDefinitions.WalkingLeft;
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
        while (state.Velocity.Y.WholePixels < 0);

        while (true)
        {
            int displacement = state.Velocity.Y.RawValue;
            if (MoveEnemyVertically(level, slot, displacement))
                break;
            AccelerateAlcoonY(state);
            GuardAlcoonJumpSimulation(++iterations, slot);
        }

        state.LandingYPosition = slot.Position.Y.Whole;
        slot.Position = slot.Position.WithWholeY(state.SpawnPosition.Y);
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
                throw new InvalidDataException(
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

        if (WrappedMagnitude(unchecked((ushort)(state.LandingYPosition - samus.Position.Y.Whole))) >= 0x20)
            return;

        ushort signedXDistance = unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole));
        if (WrappedMagnitude(signedXDistance) >= AlcoonEmergeXDistance)
            return;

        SetupAlcoonJumpMovement(state);
        if (unchecked((short)signedXDistance) < 0)
        {
            state.Velocity = state.Velocity.WithWholeX(-2);
            InstallAlcoonInstruction(slot, AlcoonInstructionProgramDefinitions.AirborneLeftLookingUp);
        }
        else
        {
            state.Velocity = state.Velocity.WithWholeX(2);
            InstallAlcoonInstruction(slot, AlcoonInstructionProgramDefinitions.AirborneRightLookingUp);
        }
        state.Function = AlcoonEnemyFunction.EmergingRising;
        LastAlcoonSoundEffect = AlcoonEmergeSound;
    }

    private static void RunAlcoonEmergingRise(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        AccelerateAlcoonY(state);
        if (state.Velocity.Y.WholePixels < 0)
            return;

        state.Function = AlcoonEnemyFunction.EmergingFalling;
        InstallAlcoonInstruction(
            slot,
            state.Velocity.X.WholePixels < 0
                ? AlcoonInstructionProgramDefinitions.AirborneLeftLookingForward
                : AlcoonInstructionProgramDefinitions.AirborneRightLookingForward);
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

        int displacement = state.Velocity.Y.RawValue;
        if (!MoveEnemyVertically(level, slot, displacement))
        {
            AccelerateAlcoonY(state);
            return;
        }

        ushort signedXDistance = unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole));
        if (unchecked((short)signedXDistance) < 0)
        {
            state.Velocity = state.Velocity.WithWholeX(-2);
            InstallAlcoonInstruction(slot, AlcoonInstructionProgramDefinitions.WalkingLeft);
        }
        else
        {
            state.Velocity = state.Velocity.WithWholeX(2);
            InstallAlcoonInstruction(slot, AlcoonInstructionProgramDefinitions.WalkingRight);
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

        ushort spawnDistance = unchecked((ushort)(state.SpawnPosition.X - slot.Position.X.Whole));
        if (WrappedMagnitude(spawnDistance) >= AlcoonHideXDistance)
        {
            state.Function = AlcoonEnemyFunction.HidingRising;
            state.Velocity = state.Velocity.WithY(SnesSignedSixteenSixteen.FromParts(-4, 0));
            InstallAlcoonInstruction(
                slot,
                state.Velocity.X.WholePixels < 0
                    ? AlcoonInstructionProgramDefinitions.AirborneLeftLookingForward
                    : AlcoonInstructionProgramDefinitions.AirborneRightLookingForward);
            return;
        }

        if (state.StepCounter != 0)
            return;

        bool samusIsLeft = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0;
        bool movingLeft = state.Velocity.X.WholePixels < 0;
        if (samusIsLeft != movingLeft)
            return;

        InstallAlcoonInstruction(
            slot,
            movingLeft
                ? AlcoonInstructionProgramDefinitions.FireLeft
                : AlcoonInstructionProgramDefinitions.FireRight);
        state.Function = AlcoonEnemyFunction.WaitingForFireAnimation;
    }

    private static void RunAlcoonHidingRise(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        AccelerateAlcoonY(state);
        if (state.Velocity.Y.WholePixels >= 0)
            state.Function = AlcoonEnemyFunction.HidingFalling;
    }

    private static void RunAlcoonHidingFall(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        if (unchecked((short)(slot.Position.Y.Whole - state.SpawnPosition.Y)) < 0)
        {
            AccelerateAlcoonY(state);
            return;
        }

        // Like the original, the reset snaps only whole coordinates. Both subpositions and
        // the last animation map remain live while the actor waits underground.
        slot.Position = slot.Position.WithWhole(state.SpawnPosition);
        state.Function = AlcoonEnemyFunction.WaitingForSamus;
    }

    private static void SetupAlcoonJumpMovement(AlcoonEnemyState state)
    {
        state.Velocity = new FixedVector<RoomPixels>(
            SnesSignedSixteenSixteen.Zero, SnesSignedSixteenSixteen.FromParts(-12, 0));
        state.ClearedVariable = 0;
        state.YAcceleration = 0;
        state.YSubacceleration = 0x8000;
    }

    private static void AccelerateAlcoonY(AlcoonEnemyState state)
    {
        SnesSignedSixteenSixteen acceleration = SnesSignedSixteenSixteen.FromParts(
            unchecked((short)state.YAcceleration), state.YSubacceleration);
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + acceleration);
    }

    private static void AddAlcoonYVelocity(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        slot.Position = slot.Position.AdvanceY(state.Velocity.Y);
    }

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
    private ushort StartAlcoonWalking(AlcoonEnemyState state)
    {
        state.Function = AlcoonEnemyFunction.WalkingAndFiring;
        Func<ushort> readRandomNumber = EnemyRandom.Read;
        ushort count = unchecked((ushort)(readRandomNumber() & 3));
        state.StepCounter = count == 0 ? (ushort)2 : count;
        return state.Velocity.X.WholePixels < 0
            ? AlcoonInstructionProgramDefinitions.WalkingLeft
            : AlcoonInstructionProgramDefinitions.WalkingRight;
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

        int displacement = state.Velocity.X.RawValue;
        if (!MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
        {
            AlignEnemyYWithNonSquareSlope(level, slot);
            return nextCursor;
        }

        short oldVelocity = state.Velocity.X.WholePixels;
        state.Velocity = state.Velocity.WithWholeX(unchecked((short)-oldVelocity));
        return oldVelocity >= 0
            ? AlcoonInstructionProgramDefinitions.WalkingLeftFirstFrame
            : AlcoonInstructionProgramDefinitions.WalkingRightFirstFrame;
    }

    private AlcoonEnemyState RequireAlcoonState(RoomEnemySlot slot) =>
        _alcoonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Alcoon state.");
}

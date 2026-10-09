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
    /// <summary>Common enemy-slot words and coordinates shared with the room's enemy system.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Per-slot whole-word vertical acceleration used by Alcoon's fixed-point jumps.</summary>
    private readonly ushort[] _yAccelerations;
    /// <summary>Per-slot fractional vertical acceleration paired with <see cref="_yAccelerations"/>.</summary>
    private readonly ushort[] _ySubaccelerations;
    /// <summary>Per-slot original X coordinates used for the actor's walking range and retreat reset.</summary>
    private readonly ushort[] _spawnXPositions;
    /// <summary>Per-slot floor-aligned Y coordinates used by the proximity activation check.</summary>
    private readonly ushort[] _landingYPositions;
    /// <summary>Per-slot walking-cycle count controlling when Alcoon may request another volley.</summary>
    private readonly ushort[] _stepCounters;

    /// <summary>Creates a typed view over one enemy slot and the parallel arrays holding Alcoon's extra state.</summary>
    /// <param name="slot">The common room slot whose native variable aliases this view exposes.</param>
    /// <param name="yAccelerations">Whole-word vertical acceleration storage indexed by enemy slot.</param>
    /// <param name="ySubaccelerations">Fractional vertical acceleration storage indexed by enemy slot.</param>
    /// <param name="spawnXPositions">Original X-coordinate storage used to bound walking and restore position.</param>
    /// <param name="landingYPositions">Landing-height storage used to decide whether Samus is near enough to activate Alcoon.</param>
    /// <param name="stepCounters">Walking-cycle storage used to pace fire volleys.</param>
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

    /// <summary>Signed two's-complement whole-pixel vertical velocity per motion step, paired with <see cref="YSubvelocity"/> as signed 16.16; emergence starts at -12 and retreat at -4. Native $0FAA,x.</summary>
    public ushort YVelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Unsigned fractional vertical velocity in 1/65536 pixel per motion step; acceleration carries from this word into <see cref="YVelocity"/>. Native $0FAC,x.</summary>
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

    /// <summary>Original population Y coordinate in room pixels, restored after the initialization floor search and after retreat; restoring this whole word intentionally leaves the live Y subposition unchanged. Native $0FB2,x.</summary>
    public ushort SpawnYPosition
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
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

    /// <summary>Original population X coordinate in room pixels, used to enforce the 112-pixel walking range and restore the whole X coordinate when hiding completes. Native $7E:7804,x.</summary>
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
    /// <summary>Native enemy-definition pointer used to identify Alcoon in the room's enemy data.</summary>
    internal const ushort AlcoonDefinition = 0xe9bf;

    /// <summary>Maximum horizontal separation at which a waiting Alcoon begins its emergence jump, in pixels.</summary>
    private const ushort AlcoonEmergeXDistance = 0x0050;
    /// <summary>Walking distance from the original X coordinate that triggers Alcoon's retreat, in pixels.</summary>
    private const ushort AlcoonHideXDistance = 0x0070;
    /// <summary>Native sound-effect identifier emitted when Alcoon starts emerging.</summary>
    private const ushort AlcoonEmergeSound = 0x005e;
    /// <summary>Native sound-effect identifier associated with Alcoon's fire volley.</summary>
    private const ushort AlcoonFireSound = 0x003f;
    /// <summary>Safety bound for the initializer's simulated ascent and collision-aware landing search.</summary>
    private const int AlcoonJumpSimulationLimit = 4096;

    /// <summary>Whole-word vertical acceleration values indexed by enemy slot.</summary>
    private readonly ushort[] _alcoonYAccelerations = new ushort[MaximumEnemyCount];
    /// <summary>Fractional vertical acceleration values paired with <see cref="_alcoonYAccelerations"/>.</summary>
    private readonly ushort[] _alcoonYSubaccelerations = new ushort[MaximumEnemyCount];
    /// <summary>Original X positions retained so each Alcoon can enforce its walk range and return underground.</summary>
    private readonly ushort[] _alcoonSpawnXPositions = new ushort[MaximumEnemyCount];
    /// <summary>Floor-aligned Y positions retained for each Alcoon's activation-distance check.</summary>
    private readonly ushort[] _alcoonLandingYPositions = new ushort[MaximumEnemyCount];
    /// <summary>Walking-cycle counts that pace each Alcoon's next fire volley.</summary>
    private readonly ushort[] _alcoonStepCounters = new ushort[MaximumEnemyCount];
    /// <summary>Typed views created during initialization; a null entry means the slot is not an initialized Alcoon.</summary>
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
        state.SpawnYPosition = slot.YPosition;
        state.SpawnXPosition = slot.XPosition;
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
                throw new InvalidDataException(
                    $"Alcoon function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Starts Alcoon's jump when Samus is within the stored landing-height and horizontal activation limits.</summary>
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
            InstallAlcoonInstruction(slot, AlcoonInstructionProgramDefinitions.AirborneLeftLookingUp);
        }
        else
        {
            state.XVelocity = 2;
            InstallAlcoonInstruction(slot, AlcoonInstructionProgramDefinitions.AirborneRightLookingUp);
        }
        state.Function = AlcoonEnemyFunction.EmergingRising;
        LastAlcoonSoundEffect = AlcoonEmergeSound;
    }

    /// <summary>Applies one upward-motion step and switches to falling once vertical velocity turns nonnegative.</summary>
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
                ? AlcoonInstructionProgramDefinitions.AirborneLeftLookingForward
                : AlcoonInstructionProgramDefinitions.AirborneRightLookingForward);
    }

    /// <summary>Moves Alcoon down with room collision, then chooses its walking direction relative to Samus.</summary>
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
            InstallAlcoonInstruction(slot, AlcoonInstructionProgramDefinitions.WalkingLeft);
        }
        else
        {
            state.XVelocity = 2;
            InstallAlcoonInstruction(slot, AlcoonInstructionProgramDefinitions.WalkingRight);
        }
        state.Function = AlcoonEnemyFunction.WalkingAndFiring;
        state.StepCounter = 1;
    }

    /// <summary>Keeps the emerged actor aligned to the floor, retreats at its walk limit, and starts eligible volleys.</summary>
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
                    ? AlcoonInstructionProgramDefinitions.AirborneLeftLookingForward
                    : AlcoonInstructionProgramDefinitions.AirborneRightLookingForward);
            return;
        }

        if (state.StepCounter != 0)
            return;

        bool samusIsLeft = unchecked((short)(samus.XPosition - slot.XPosition)) < 0;
        bool movingLeft = unchecked((short)state.XVelocity) < 0;
        if (samusIsLeft != movingLeft)
            return;

        InstallAlcoonInstruction(
            slot,
            movingLeft
                ? AlcoonInstructionProgramDefinitions.FireLeft
                : AlcoonInstructionProgramDefinitions.FireRight);
        state.Function = AlcoonEnemyFunction.WaitingForFireAnimation;
    }

    /// <summary>Applies one retreat ascent step and changes to the falling phase when the jump reaches its apex.</summary>
    private static void RunAlcoonHidingRise(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        AccelerateAlcoonY(state);
        if (unchecked((short)state.YVelocity) >= 0)
            state.Function = AlcoonEnemyFunction.HidingFalling;
    }

    /// <summary>Continues the retreat descent and restores whole spawn coordinates once the actor reaches its origin.</summary>
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

    /// <summary>Initializes velocity and half-pixel acceleration words for Alcoon's native jump trajectory.</summary>
    private static void SetupAlcoonJumpMovement(AlcoonEnemyState state)
    {
        state.YVelocity = unchecked((ushort)-12);
        state.YSubvelocity = 0;
        state.XVelocity = 0;
        state.ClearedVariable = 0;
        state.YAcceleration = 0;
        state.YSubacceleration = 0x8000;
    }

    /// <summary>Adds fractional acceleration and carries overflow into the whole vertical-velocity word.</summary>
    private static void AccelerateAlcoonY(AlcoonEnemyState state)
    {
        uint fractionalSum = (uint)state.YSubvelocity + state.YSubacceleration;
        state.YSubvelocity = unchecked((ushort)fractionalSum);
        state.YVelocity = unchecked((ushort)(
            state.YVelocity + state.YAcceleration + (fractionalSum >> 16)));
    }

    /// <summary>Adds Alcoon's signed 16.16 vertical velocity to the slot's 16.16 Y coordinate with native wraparound.</summary>
    private static void AddAlcoonYVelocity(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        uint fixedY = ((uint)slot.YPosition << 16) | slot.YSubposition;
        fixedY = unchecked(fixedY + (uint)ComposeSignedFixed(
            state.YVelocity,
            state.YSubvelocity));
        slot.YPosition = unchecked((ushort)(fixedY >> 16));
        slot.YSubposition = unchecked((ushort)fixedY);
    }

    /// <summary>Rejects initialization if the simulated jump exceeds its bound without finding a landing floor.</summary>
    private static void GuardAlcoonJumpSimulation(int iterations, RoomEnemySlot slot)
    {
        if (iterations > AlcoonJumpSimulationLimit)
        {
            throw new InvalidDataException(
                $"Alcoon slot {slot.SlotIndex} did not find a landing floor within " +
                $"{AlcoonJumpSimulationLimit} initialization steps.");
        }
    }

    /// <summary>Selects an animation instruction list and makes its first command eligible on the next update.</summary>
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

        int displacement = unchecked((short)state.XVelocity) << 16;
        if (!MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
        {
            AlignEnemyYWithNonSquareSlope(level, slot);
            return nextCursor;
        }

        ushort oldVelocity = state.XVelocity;
        state.XVelocity = unchecked((ushort)-oldVelocity);
        return unchecked((short)oldVelocity) >= 0
            ? AlcoonInstructionProgramDefinitions.WalkingLeftFirstFrame
            : AlcoonInstructionProgramDefinitions.WalkingRightFirstFrame;
    }

    /// <summary>Returns the state view created for this slot, failing when Alcoon initialization has not run.</summary>
    private AlcoonEnemyState RequireAlcoonState(RoomEnemySlot slot) =>
        _alcoonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Alcoon state.");
}

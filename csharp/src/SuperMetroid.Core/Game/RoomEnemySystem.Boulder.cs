using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The six literal bank-$A6 function words stored in Boulder's variable A.</summary>
public enum BoulderAiFunction : ushort
{
    /// <summary>$A6:879A, <c>Function_Boulder_WaitForSamusToGetNear</c>: waits for Samus to enter the direction-dependent X window and vertical trigger range before selecting an initial fall or immediate roll.</summary>
    WaitingForSamus = 0x879a,
    /// <summary>$A6:87ED, <c>Function_Boulder_Falling</c>: accelerates downward without terrain collision checks until the original spawn Y is reached, then starts the first rebound.</summary>
    InitialFall = 0x87ed,
    /// <summary>$A6:8832, <c>Function_Boulder_Bounce_Rising</c>: decelerates the upward quadratic-table motion while accelerating horizontally, then enters the falling half of the bounce.</summary>
    Rebound = 0x8832,
    /// <summary>$A6:888B, <c>Function_Boulder_Bounce_Falling</c>: falls with terrain collision, selects progressively smaller rebounds, and enters rolling when the bounce counter underflows; direction two breaks on impact.</summary>
    Falling = 0x888b,
    /// <summary>$A6:8942, <c>Function_Boulder_Rolling</c>: follows terrain with vertical compensation and accelerates horizontally until wall collision hides and deletes the actor with a dust effect.</summary>
    Rolling = 0x8942,
    /// <summary>$A6:89FC, <c>Function_Boulder_LoadEnemyIndex</c>: native load-index-and-return target written after terminal rolling impact; no further movement occurs before slot cleanup.</summary>
    Inert = 0x89fc,
}

/// <summary>
/// Debugger-facing view of the otherwise anonymous Boulder words at enemy offsets
/// <c>$7800-$780E</c>. The function and three motion counters remain backed by the common
/// enemy slot so watches retain the same values that the instruction/AI dispatcher sees.
/// </summary>
public sealed class BoulderEnemyState
{
    /// <summary>Physical enemy slot backing the Boulder function and motion-counter words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a typed view over the slot containing this Boulder actor's native state.</summary>
    /// <param name="slot">Enemy record whose ordinary variables are exposed through this state.</param>
    internal BoulderEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Current bank-$A6 indirect AI target, backed by common variable A here and corresponding to native <c>Boulder.function</c> at $0FA8,x.</summary>
    public BoulderAiFunction Function
    {
        get => (BoulderAiFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Variable B, advanced in $20/$40 steps and indexed by its high byte.</summary>
    public ushort HorizontalSpeedAccumulator
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Variable C, the corresponding vertical quadratic-table accumulator.</summary>
    public ushort VerticalSpeedAccumulator
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Variable D. Its deliberate zero-to-$FFFF underflow ends the bounce phase.</summary>
    public ushort BounceCounter
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Variable E, copied from parameter one's high byte and used as direction.</summary>
    public ushort DirectionSelector
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Signed pixel threshold for Samus's X displacement, from parameter two's low byte: positive for rightward activation and negative for leftward activation. Native <c>XProximity</c> at $7E:7800,x.</summary>
    public short HorizontalTriggerLimit { get; internal set; }
    /// <summary>Whole-pixel extra downward collision reach during rolling, subtracted from Y afterward; initially two or five from parameter one's low byte, then zero once the full Y position stops changing. Native <c>minimumDistanceFromGround</c> at $7E:7802,x.</summary>
    public ushort VerticalCompensation { get; internal set; }
    /// <summary>Saved fractional Y position in 1/65536 pixel units, compared with the saved whole position to detect stationary rolling and refreshed after each rolling update. Native <c>previousEnemyYSubPosition</c> at $7E:7804,x.</summary>
    public ushort PreviousYSubposition { get; internal set; }
    /// <summary>Saved room Y coordinate in pixels for the rolling stability check; captured at initialization and on the final bounce, then refreshed during rolling. Native <c>previousEnemyYPosition</c> at $7E:7806,x.</summary>
    public ushort PreviousYPosition { get; internal set; }
    /// <summary>Original population X coordinate in room pixels, retained as native <c>spawnXPosition</c> at $7E:7808,x; the translated movement does not subsequently consume it.</summary>
    public ushort InitialXPosition { get; internal set; }
    /// <summary>Original population Y coordinate in room pixels, before the initial upward offset; the first fall snaps to this target before rebounding. Native <c>fallingTargetYPosition</c> at $7E:780A,x.</summary>
    public ushort InitialYPosition { get; internal set; }
    /// <summary>Exclusive upper pixel threshold for Samus's nonnegative Y displacement, taken from the population initialization instruction word's low byte. Native <c>YProximity</c> at $7E:780C,x.</summary>
    public byte VerticalTriggerLimit { get; internal set; }
    /// <summary>True when parameter two's high byte is zero: initialization substitutes a one-pixel upward offset and activation bypasses the falling and bounce sequence. Native <c>type</c> at $7E:780E,x.</summary>
    public bool StartsRollingWithoutBounce { get; internal set; }
}

/// <summary>
/// Literal translation of retail Boulder enemy <c>$DFBF</c> from $A6:86F5-$8A56. Its
/// apparent rolling arc is assembled from the shared ROM quadratic table, exact 16.16
/// collision movers, two independent acceleration clocks, and the population record's
/// packed trigger/direction parameters.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Enemy definition pointer for the bank-$A6 Boulder actor.</summary>
    internal const ushort BoulderDefinition = 0xdfbf;

    /// <summary>Library-two sound identifier requested when the Boulder lands or hits a wall.</summary>
    private const ushort BoulderImpactSound = 0x0042;
    /// <summary>Library-two sound identifier requested for a breaking impact.</summary>
    private const ushort BoulderBreakSound = 0x0043;
    /// <summary>Room-graphics dust animation used when the Boulder breaks on impact.</summary>
    private const ushort BoulderDustAnimationIndex = 0x0011;

    /// <summary>Per-slot typed views installed during Boulder initialization.</summary>
    private readonly BoulderEnemyState?[] _boulderStates =
        new BoulderEnemyState?[MaximumEnemyCount];

    /// <summary>Most recent library-two sound emitted by a landing or terminal impact.</summary>
    public ushort? LastBoulderSoundEffect { get; private set; }

    /// <summary>Ports <c>Boulder_Init</c> at $A6:86F5.</summary>
    private void InitializeBoulder(RoomEnemySlot slot)
    {
        var state = new BoulderEnemyState(slot);
        _boulderStates[slot.SlotIndex] = state;

        state.VerticalSpeedAccumulator = 0;
        state.HorizontalSpeedAccumulator = 0;
        state.BounceCounter = 2;
        state.Function = BoulderAiFunction.WaitingForSamus;
        state.InitialXPosition = slot.XPosition;
        state.InitialYPosition = slot.YPosition;
        state.PreviousYPosition = slot.YPosition;
        state.PreviousYSubposition = slot.YSubposition;

        byte initialYOffset = unchecked((byte)(slot.Parameter2 >> 8));
        if (initialYOffset == 0)
        {
            // A zero high byte is not “no offset.” Native substitutes one pixel and sets
            // var07 so the trigger jumps directly to rolling instead of the bounce chain.
            initialYOffset = 1;
            state.StartsRollingWithoutBounce = true;
        }
        slot.YPosition = unchecked((ushort)(state.InitialYPosition - initialYOffset));

        state.HorizontalTriggerLimit = unchecked((short)-(byte)slot.Parameter2);

        // The population record's initialization parameter was copied to current_instruction
        // before init AI ran. Boulder consumes only its low byte as the strict Samus-Y
        // trigger threshold, then replaces that word with the real animation list.
        state.VerticalTriggerLimit = unchecked((byte)slot.CurrentInstruction);
        state.DirectionSelector = unchecked((byte)(slot.Parameter1 >> 8));
        if (state.DirectionSelector == 0)
        {
            state.HorizontalTriggerLimit = unchecked((short)-state.HorizontalTriggerLimit);
            slot.CurrentInstruction = BoulderInstructionProgramDefinitions.Right;
        }
        else
        {
            slot.CurrentInstruction = BoulderInstructionProgramDefinitions.Left;
        }

        state.VerticalCompensation = (byte)slot.Parameter1 == 0 ? (ushort)2 : (ushort)5;
    }

    /// <summary>Ports <c>Boulder_Main</c> and its six indirect targets.</summary>
    private void RunBoulderMain(RoomEnemySlot slot, BoulderEnemyState state, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Boulder AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Boulder AI requires active room collision data.");

        switch (state.Function)
        {
            case BoulderAiFunction.WaitingForSamus:
                RunBoulderWaiting(slot, state, samus);
                break;
            case BoulderAiFunction.InitialFall:
                RunBoulderInitialFall(slot, state);
                break;
            case BoulderAiFunction.Rebound:
                RunBoulderRebound(slot, state);
                break;
            case BoulderAiFunction.Falling:
                RunBoulderFalling(slot, state, level);
                break;
            case BoulderAiFunction.Rolling:
                RunBoulderRolling(slot, state, level);
                break;
            case BoulderAiFunction.Inert:
                break;
            default:
                throw new InvalidDataException(
                    $"Boulder function $A6:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Waits for Samus to enter the configured vertical and direction-dependent horizontal trigger window.</summary>
    private static void RunBoulderWaiting(RoomEnemySlot slot, BoulderEnemyState state, SamusState samus)
    {
        short deltaY = unchecked((short)(samus.YPosition - slot.YPosition));
        if (deltaY < 0 || unchecked((short)(deltaY - state.VerticalTriggerLimit)) >= 0)
            return;

        short deltaX = unchecked((short)(samus.XPosition - slot.XPosition));
        bool enteredHorizontalWindow = state.DirectionSelector != 0
            ? deltaX < 0 && unchecked((short)(deltaX - state.HorizontalTriggerLimit)) >= 0
            : deltaX >= 0 && unchecked((short)(deltaX - state.HorizontalTriggerLimit)) < 0;
        if (!enteredHorizontalWindow)
            return;

        state.Function = state.StartsRollingWithoutBounce
            ? BoulderAiFunction.Rolling
            : BoulderAiFunction.InitialFall;
    }

    /// <summary>Falls without terrain collision until the saved spawn height, then starts the first rebound.</summary>
    private static void RunBoulderInitialFall(RoomEnemySlot slot, BoulderEnemyState state)
    {
        AddBoulderVerticalVelocity(slot, state.VerticalSpeedAccumulator, negative: false);
        if (unchecked((short)(slot.YPosition - state.InitialYPosition)) < 0)
        {
            state.VerticalSpeedAccumulator = AddAndCapUnsigned(
                state.VerticalSpeedAccumulator,
                0x0100,
                0x5000);
            return;
        }

        slot.YPosition = state.InitialYPosition;
        state.Function = BoulderAiFunction.Rebound;
        state.VerticalSpeedAccumulator = 0x2000;
    }

    /// <summary>Applies the rising quadratic-table arc and horizontal acceleration until the vertical phase turns downward.</summary>
    private static void RunBoulderRebound(RoomEnemySlot slot, BoulderEnemyState state)
    {
        AddBoulderVerticalVelocity(slot, state.VerticalSpeedAccumulator, negative: true);
        state.VerticalSpeedAccumulator = unchecked((ushort)(state.VerticalSpeedAccumulator - 0x0100));
        if (unchecked((short)state.VerticalSpeedAccumulator) < 0)
        {
            state.VerticalSpeedAccumulator = 0;
            state.Function = BoulderAiFunction.Falling;
            return;
        }

        AddBoulderHorizontalVelocity(slot, state);
        state.HorizontalSpeedAccumulator = AddAndCapUnsigned(
            state.HorizontalSpeedAccumulator,
            0x0020,
            0x5000);
    }

    /// <summary>Falls with terrain collision, rebounding on impacts or breaking immediately for the authored break direction.</summary>
    private void RunBoulderFalling(RoomEnemySlot slot, BoulderEnemyState state, RoomLevelData level)
    {
        int verticalDisplacement = ReadQuadraticEnemySpeed(
            unchecked((byte)(state.VerticalSpeedAccumulator >> 8)),
            negative: false);
        if (MoveEnemyVertically(level, slot, verticalDisplacement))
        {
            LastBoulderSoundEffect = BoulderImpactSound;
            if (state.DirectionSelector == 2)
            {
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
                SpawnRoomGraphicsDustExplosion(
                    slot.XPosition,
                    slot.YPosition,
                    BoulderDustAnimationIndex);
                LastBoulderSoundEffect = BoulderBreakSound;
                return;
            }

            state.Function = BoulderAiFunction.Rebound;

            // The third impact still installs the leading zero sample before the counter
            // underflows. Preserve that write even though this impact enters rolling.
            state.VerticalSpeedAccumulator = BoulderBounceDefinitions.SpeedIndex(state.BounceCounter);
            state.BounceCounter = unchecked((ushort)(state.BounceCounter - 1));
            if ((state.BounceCounter & 0x8000) != 0)
            {
                state.PreviousYPosition = slot.YPosition;
                state.PreviousYSubposition = slot.YSubposition;
                state.Function = BoulderAiFunction.Rolling;
            }
            return;
        }

        state.VerticalSpeedAccumulator = unchecked((ushort)(
            state.VerticalSpeedAccumulator + 0x0100));
        AddBoulderHorizontalVelocity(slot, state);
        state.HorizontalSpeedAccumulator = AddAndCapUnsigned(
            state.HorizontalSpeedAccumulator,
            0x0020,
            0x5000);
    }

    /// <summary>Rolls along terrain with vertical compensation, then hides, deletes, and breaks the actor at a wall.</summary>
    private void RunBoulderRolling(RoomEnemySlot slot, BoulderEnemyState state, RoomLevelData level)
    {
        int verticalDisplacement = unchecked(
            ReadQuadraticEnemySpeed(
                unchecked((byte)(state.HorizontalSpeedAccumulator >> 8)),
                negative: false) +
            (state.VerticalCompensation << 16));
        MoveEnemyVertically(level, slot, verticalDisplacement);
        slot.YPosition = unchecked((ushort)(slot.YPosition - state.VerticalCompensation));

        int horizontalDisplacement = ReadBoulderHorizontalDisplacement(state);
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalDisplacement))
        {
            // Native ORA #$0300 both hides and deletes the record. The inert function is
            // still written before DetermineWhichEnemiesToProcess clears it next frame.
            slot.Properties = slot.Properties.With(
                EnemyProperties.Invisible | EnemyProperties.Deleted);
            state.Function = BoulderAiFunction.Inert;
            LastBoulderSoundEffect = BoulderImpactSound;
            SpawnRoomGraphicsDustExplosion(
                slot.XPosition,
                slot.YPosition,
                BoulderDustAnimationIndex);
            LastBoulderSoundEffect = BoulderBreakSound;
        }
        else
        {
            state.HorizontalSpeedAccumulator = AddAndCapUnsigned(
                state.HorizontalSpeedAccumulator,
                0x0040,
                0x4000);
            if (slot.YPosition == state.PreviousYPosition &&
                slot.YSubposition == state.PreviousYSubposition)
            {
                state.VerticalCompensation = 0;
            }
        }

        state.PreviousYPosition = slot.YPosition;
        state.PreviousYSubposition = slot.YSubposition;
    }

    /// <summary>Applies the direction-signed quadratic horizontal displacement to the slot's 16.16 X position.</summary>
    private static void AddBoulderHorizontalVelocity(RoomEnemySlot slot, BoulderEnemyState state)
    {
        int displacement = ReadBoulderHorizontalDisplacement(state);
        uint position = ((uint)slot.XPosition << 16) | slot.XSubposition;
        position = unchecked(position + (uint)displacement);
        slot.XPosition = unchecked((ushort)(position >> 16));
        slot.XSubposition = unchecked((ushort)position);
    }

    /// <summary>Reads the current quadratic horizontal speed and applies the selected travel direction.</summary>
    private static int ReadBoulderHorizontalDisplacement(BoulderEnemyState state) =>
        ReadQuadraticEnemySpeed(
            unchecked((byte)(state.HorizontalSpeedAccumulator >> 8)),
            negative: state.DirectionSelector != 0);

    /// <summary>Applies one quadratic vertical displacement to the slot's 16.16 Y position.</summary>
    private static void AddBoulderVerticalVelocity(RoomEnemySlot slot, ushort accumulator, bool negative)
    {
        int displacement = ReadQuadraticEnemySpeed(
            unchecked((byte)(accumulator >> 8)),
            negative);
        uint position = ((uint)slot.YPosition << 16) | slot.YSubposition;
        position = unchecked(position + (uint)displacement);
        slot.YPosition = unchecked((ushort)(position >> 16));
        slot.YSubposition = unchecked((ushort)position);
    }

    /// <summary>Adds an unsigned acceleration step and saturates at the configured maximum.</summary>
    private static ushort AddAndCapUnsigned(ushort value, ushort increment, ushort maximum)
    {
        ushort result = unchecked((ushort)(value + increment));
        return unchecked((short)(result - maximum)) < 0 ? result : maximum;
    }

    /// <summary>Returns the initialized state view associated with a Boulder enemy slot.</summary>
    private BoulderEnemyState RequireBoulderState(RoomEnemySlot slot) =>
        _boulderStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Boulder state.");
}

using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The six literal bank-$A6 function words stored in Boulder's variable A.</summary>
public enum BoulderAiFunction : ushort
{
    WaitingForSamus = 0x879a,
    InitialFall = 0x87ed,
    Rebound = 0x8832,
    Falling = 0x888b,
    Rolling = 0x8942,
    Inert = 0x89fc,
}

/// <summary>
/// Debugger-facing view of the otherwise anonymous Boulder words at enemy offsets
/// <c>$7800-$780E</c>. The function and three motion counters remain backed by the common
/// enemy slot so watches retain the same values that the instruction/AI dispatcher sees.
/// </summary>
public sealed class BoulderEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal BoulderEnemyState(RoomEnemySlot slot) => _slot = slot;

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

    public short HorizontalTriggerLimit { get; internal set; }
    public ushort VerticalCompensation { get; internal set; }
    public ushort PreviousYSubposition { get; internal set; }
    public ushort PreviousYPosition { get; internal set; }
    public ushort InitialXPosition { get; internal set; }
    public ushort InitialYPosition { get; internal set; }
    public byte VerticalTriggerLimit { get; internal set; }
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
    internal const ushort BoulderDefinition = 0xdfbf;

    private const ushort BoulderLeftInstructionList = 0x86a7;
    private const ushort BoulderRightInstructionList = 0x86cb;
    private const int BoulderBounceSpeedTable = 0xa686f1;
    private const ushort BoulderImpactSound = 0x0042;
    private const ushort BoulderBreakSound = 0x0043;
    private const ushort BoulderDustAnimationIndex = 0x0011;

    private readonly BoulderEnemyState?[] _boulderStates =
        new BoulderEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Boulder state for every physical enemy slot.</summary>
    public IReadOnlyList<BoulderEnemyState?> BoulderStates => _boulderStates;

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
            slot.CurrentInstruction = BoulderRightInstructionList;
        }
        else
        {
            slot.CurrentInstruction = BoulderLeftInstructionList;
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
                throw new NotSupportedException(
                    $"Boulder function $A6:{(ushort)state.Function:X4} is not translated.");
        }
    }

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

    private void RunBoulderInitialFall(RoomEnemySlot slot, BoulderEnemyState state)
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

    private void RunBoulderRebound(RoomEnemySlot slot, BoulderEnemyState state)
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

            // The third collision deliberately indexes one word before the two-word table
            // when D is zero, then D underflows. Read through ROM so that shipped overread
            // remains observable instead of “fixing” it with a host array bounds check.
            short bounceIndex = unchecked((short)(state.BounceCounter - 1));
            state.VerticalSpeedAccumulator = ReadWord(
                _bus!,
                BoulderBounceSpeedTable + bounceIndex * 2);
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

    private void AddBoulderHorizontalVelocity(RoomEnemySlot slot, BoulderEnemyState state)
    {
        int displacement = ReadBoulderHorizontalDisplacement(state);
        uint position = ((uint)slot.XPosition << 16) | slot.XSubposition;
        position = unchecked(position + (uint)displacement);
        slot.XPosition = unchecked((ushort)(position >> 16));
        slot.XSubposition = unchecked((ushort)position);
    }

    private int ReadBoulderHorizontalDisplacement(BoulderEnemyState state) =>
        ReadQuadraticEnemySpeed(
            unchecked((byte)(state.HorizontalSpeedAccumulator >> 8)),
            negative: state.DirectionSelector != 0);

    private void AddBoulderVerticalVelocity(RoomEnemySlot slot, ushort accumulator, bool negative)
    {
        int displacement = ReadQuadraticEnemySpeed(
            unchecked((byte)(accumulator >> 8)),
            negative);
        uint position = ((uint)slot.YPosition << 16) | slot.YSubposition;
        position = unchecked(position + (uint)displacement);
        slot.YPosition = unchecked((ushort)(position >> 16));
        slot.YSubposition = unchecked((ushort)position);
    }

    private static ushort AddAndCapUnsigned(ushort value, ushort increment, ushort maximum)
    {
        ushort result = unchecked((ushort)(value + increment));
        return unchecked((short)(result - maximum)) < 0 ? result : maximum;
    }

    private BoulderEnemyState RequireBoulderState(RoomEnemySlot slot) =>
        _boulderStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Boulder state.");
}

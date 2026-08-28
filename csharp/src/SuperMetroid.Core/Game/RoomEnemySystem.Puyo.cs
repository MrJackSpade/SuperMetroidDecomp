using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A2 function pointers stored in Puyo's ordinary enemy variable C. Keeping
/// the ROM addresses as enum values makes a debugger display directly comparable with
/// <c>$0FAE,x</c> rather than hiding cartridge state behind invented host ordinals.
/// </summary>
public enum PuyoEnemyFunction : ushort
{
    Grounded = 0x9b65,
    Airborne = 0x9b81,
}

/// <summary>
/// Literal indirect targets stored in Puyo's ordinary enemy variable D. Entries zero
/// through six in the ROM hop table select these functions.
/// </summary>
public enum PuyoAirborneFunction : ushort
{
    NormalShortHop = 0x9d0b,
    NormalBigHop = 0x9d2b,
    NormalLongHop = 0x9d4b,
    GiantHop = 0x9d6b,
    Dropping = 0x9d98,
    Dropped = 0x9dcd,
}

/// <summary>
/// The seven concrete indexes into <c>PuyoHopTable</c> at $A2:9A07. This is a discriminant,
/// not a flags word: every value selects one eight-byte height/speed/function record.
/// </summary>
public enum PuyoHopType : ushort
{
    NormalSmall = 0,
    NormalBig = 1,
    NormalLong = 2,
    Giant = 3,
    Dropping = 4,
    DroppedSmall = 5,
    DroppedBig = 6,
}

/// <summary>The two literal values written to Puyo's direction word.</summary>
public enum PuyoDirection : ushort
{
    Right = 0,
    Left = 1,
}

/// <summary>
/// Typed view of Puyo's five ordinary enemy variables and eight parallel extended-WRAM
/// words. Ordinary members remain backed by <see cref="RoomEnemySlot"/> so inspecting the
/// actor shows the same layout as <c>$0FAA-$0FB2</c> in the cartridge.
/// </summary>
public sealed class PuyoEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal PuyoEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Unsigned 8.8 index accumulator used to choose a row of the shared quadratic speed
    /// table. It is variable A at <c>$0FAA,x</c>, despite its misleading native name.
    /// </summary>
    public ushort YSpeedTableIndex
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Grounded-frame countdown copied from population parameter one.</summary>
    public ushort HopCooldownTimer
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Current top-level bank-$A2 function pointer.</summary>
    public PuyoEnemyFunction Function
    {
        get => (PuyoEnemyFunction)_slot.VariableC;
        internal set => _slot.VariableC = (ushort)value;
    }

    /// <summary>Current airborne indirect function pointer.</summary>
    public PuyoAirborneFunction AirborneFunction
    {
        get => (PuyoAirborneFunction)_slot.VariableD;
        internal set => _slot.VariableD = (ushort)value;
    }

    /// <summary>Byte offset into the seven-record ROM hop table.</summary>
    public ushort HopTableIndex
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Native <c>$7E:7800,x</c>; the record selected for the next hop.</summary>
    public PuyoHopType HopType { get; internal set; }

    /// <summary>
    /// Native <c>$7E:7802,x</c>. The name is inherited from the disassembly: one means the
    /// hopping pose is still active, while zero permits the grounded-list handoff.
    /// </summary>
    public bool HoppingAnimationActive { get; internal set; }

    /// <summary>Native <c>$7E:7804,x</c>; zero is right and one is left.</summary>
    public PuyoDirection Direction { get; internal set; }

    /// <summary>Native <c>$7E:7806,x</c>; selects positive gravity-table rows.</summary>
    public bool Falling { get; internal set; }

    /// <summary>
    /// Native <c>$7E:7808,x</c>. A terrain collision requests that the next hop use the
    /// precomputed opposite direction instead of re-aiming at Samus.
    /// </summary>
    public bool InvertDirection { get; internal set; }

    /// <summary>Opposite direction captured when Puyo strikes a wall or ceiling.</summary>
    public PuyoDirection InvertedDirection { get; internal set; }

    /// <summary>Three-quarter point of the initial vertical speed-table index.</summary>
    public ushort InitialYSpeedTableIndexThreeQuarters { get; internal set; }

    /// <summary>One-half point of the initial vertical speed-table index.</summary>
    public ushort InitialYSpeedTableIndexHalf { get; internal set; }
}

/// <summary>
/// Literal translation of retail Puyo enemy $CFBF, covering $A2:998D-$9DF3. Puyo attacks
/// through the ordinary enemy-contact path; this file owns its load state, all seven hop
/// variants, collision-aware movement, and the instruction-list changes that drive its
/// eight ROM spritemaps.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort PuyoDefinition = 0xcfbf;

    private const ushort PuyoGroundedFastInstructionList = 0x99ad;
    private const ushort PuyoGroundedMediumInstructionList = 0x99c1;
    private const ushort PuyoGroundedSlowInstructionList = 0x99d5;
    private const ushort PuyoRightFrame0LeftFrame4InstructionList = 0x99e9;
    private const ushort PuyoRightFrame1LeftFrame3InstructionList = 0x99ef;
    private const ushort PuyoFrame2InstructionList = 0x99f5;
    private const ushort PuyoRightFrame3LeftFrame1InstructionList = 0x99fb;
    private const ushort PuyoRightFrame4LeftFrame0InstructionList = 0x9a01;
    private const int PuyoHopTableAddress = 0xa29a07;
    private const int PuyoHopRecordSize = 8;
    private const int QuadraticSpeedTableAddress = 0xa0838f;
    private const int QuadraticSpeedRecordSize = 8;
    private const ushort MaximumPuyoYSpeedTableIndex = 0x4000;

    private readonly PuyoEnemyState?[] _puyoStates =
        new PuyoEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for all 32 physical Puyo-capable enemy slots.</summary>
    public IReadOnlyList<PuyoEnemyState?> PuyoStates => _puyoStates;

    /// <summary>Ports <c>InitAI_Puyo</c> at $A2:9A3F.</summary>
    private void InitializePuyo(RoomEnemySlot slot)
    {
        // The native initializer writes the common empty map and clears Enemy.var0 before
        // installing the fast grounded list. Enemy.var0 is not one of the six exposed
        // per-family variable words; no later Puyo routine reads it.
        slot.SpritemapPointer = 0x804d;
        SetPuyoInstructionList(slot, PuyoGroundedFastInstructionList);

        var state = new PuyoEnemyState(slot)
        {
            YSpeedTableIndex = 0,
            HopCooldownTimer = slot.Parameter1,
            Function = PuyoEnemyFunction.Grounded,
            // InitAI never writes $0FB0 or the inactive extended words. The room loader's
            // cleared enemy allocation therefore leaves them zero until InitiateHop owns
            // them; do not pre-populate plausible values the cartridge has not written.
            AirborneFunction = (PuyoAirborneFunction)0,
            HopTableIndex = 0,
            HopType = PuyoHopType.NormalSmall,
            HoppingAnimationActive = false,
            Direction = PuyoDirection.Right,
            Falling = false,
            InvertDirection = false,
            InvertedDirection = PuyoDirection.Right,
            InitialYSpeedTableIndexThreeQuarters = 0,
            InitialYSpeedTableIndexHalf = 0,
        };
        _puyoStates[slot.SlotIndex] = state;
    }

    /// <summary>Ports <c>MainAI_Puyo</c> and its indirect top-level dispatch.</summary>
    private void RunPuyoMain(
        RoomEnemySlot slot,
        PuyoEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Puyo AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Puyo movement requires decoded room collision data.");

        switch (state.Function)
        {
            case PuyoEnemyFunction.Grounded:
                RunGroundedPuyo(slot, state, samus);
                return;
            case PuyoEnemyFunction.Airborne:
                RunAirbornePuyo(slot, state, level);
                return;
            default:
                throw new NotSupportedException(
                    $"Puyo function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports <c>Function_Puyo_Grounded</c> at $A2:9B65.</summary>
    private void RunGroundedPuyo(RoomEnemySlot slot, PuyoEnemyState state, SamusState samus)
    {
        // DEC followed by BPL means a zero cooldown waits one frame and then wraps to FFFF
        // before hopping. This is not equivalent to a conventional "if timer != 0" test.
        state.HopCooldownTimer = unchecked((ushort)(state.HopCooldownTimer - 1));
        if (!IsNegative16(state.HopCooldownTimer))
            return;

        state.Function = PuyoEnemyFunction.Airborne;
        state.HopCooldownTimer = slot.Parameter1;
        state.HoppingAnimationActive = true;
        InitiatePuyoHop(slot, state, samus);
    }

    /// <summary>Ports <c>InitiateHop</c>, <c>ChooseHopType</c>, and their random selection.</summary>
    private void InitiatePuyoHop(RoomEnemySlot slot, PuyoEnemyState state, SamusState samus)
    {
        // Normal follow-up hops are promoted from type zero to type one when Samus is
        // strictly inside parameter-two's horizontal radius. Giant/dropping/dropped types
        // deliberately bypass this proximity rewrite.
        if ((ushort)state.HopType < (ushort)PuyoHopType.Giant)
        {
            int distance = Math.Abs(unchecked((short)(samus.XPosition - slot.XPosition)));
            state.HopType = distance < slot.Parameter2
                ? PuyoHopType.NormalBig
                : PuyoHopType.NormalSmall;
        }

        // Aim toward Samus unless the preceding terrain collision latched the opposite
        // direction. Equality follows the cartridge's BPL path and therefore aims right.
        PuyoDirection direction = unchecked((short)(samus.XPosition - slot.XPosition)) < 0
            ? PuyoDirection.Left
            : PuyoDirection.Right;
        if (state.InvertDirection)
            direction = state.InvertedDirection;
        state.Direction = direction;
        state.InvertDirection = false;

        // GenerateRandomNumber updates the global seed before the frame counter is added.
        // Types three and above ignore this value but still advance the seed, an observable
        // side effect shared with every other random enemy in the room.
        ushort randomChoice = unchecked((ushort)((_nextRandom!() + slot.FrameCounter) & 7));
        int hopType = (ushort)state.HopType;
        if (hopType < 3)
        {
            if (hopType == 0)
                randomChoice &= 1;
            if (randomChoice >= 2)
                randomChoice = 2;
            hopType = randomChoice;
            state.HopType = (PuyoHopType)hopType;
        }

        state.HopTableIndex = checked((ushort)(hopType * PuyoHopRecordSize));
        state.AirborneFunction = ReadPuyoHopFunction(state.HopTableIndex);
        CalculateInitialPuyoHopSpeed(state);
    }

    /// <summary>Ports the integration loop at $A2:9B1A without simplifying its odd units.</summary>
    private void CalculateInitialPuyoHopSpeed(PuyoEnemyState state)
    {
        ushort timeAccumulator = 0;
        ushort distanceAccumulator = 0;
        ushort xSpeed = ReadPuyoHopWord(state.HopTableIndex, 2);
        ushort swappedJumpHeight = SwapBytes(ReadPuyoHopWord(state.HopTableIndex, 0));

        for (int guard = 0; guard <= ushort.MaxValue; guard++)
        {
            timeAccumulator = unchecked((ushort)(timeAccumulator + xSpeed));
            int speedRow = (timeAccumulator >> 8) * QuadraticSpeedRecordSize;

            // The cartridge intentionally reads a word at table+1: the high byte of the
            // fractional speed followed by the low byte of the whole speed. Accumulating
            // that misaligned value produces an 8.8 distance estimate used only to find
            // the initial curve index.
            distanceAccumulator = unchecked((ushort)(distanceAccumulator +
                ReadWord(_bus!, QuadraticSpeedTableAddress + speedRow + 1)));
            if (!IsNegative16(unchecked((ushort)(swappedJumpHeight - distanceAccumulator))))
                continue;

            state.YSpeedTableIndex = timeAccumulator;
            state.Falling = false;
            state.InitialYSpeedTableIndexHalf = (ushort)(timeAccumulator >> 1);
            state.InitialYSpeedTableIndexThreeQuarters = unchecked((ushort)(
                (timeAccumulator >> 2) + state.InitialYSpeedTableIndexHalf));
            return;
        }

        throw new InvalidDataException(
            $"Puyo hop record {state.HopTableIndex / PuyoHopRecordSize} never reached " +
            "its ROM jump-height threshold.");
    }

    /// <summary>Ports the indirect airborne dispatcher at $A2:9B81.</summary>
    private void RunAirbornePuyo(
        RoomEnemySlot slot,
        PuyoEnemyState state,
        RoomLevelData level)
    {
        switch (state.AirborneFunction)
        {
            case PuyoAirborneFunction.NormalShortHop:
                RunNormalPuyoHop(slot, state, level, PuyoGroundedSlowInstructionList);
                return;
            case PuyoAirborneFunction.NormalBigHop:
                RunNormalPuyoHop(slot, state, level, PuyoGroundedMediumInstructionList);
                return;
            case PuyoAirborneFunction.NormalLongHop:
                RunNormalPuyoHop(slot, state, level, PuyoGroundedFastInstructionList);
                return;
            case PuyoAirborneFunction.GiantHop:
                RunGiantPuyoHop(slot, state, level);
                return;
            case PuyoAirborneFunction.Dropping:
                RunDroppingPuyo(slot, state, level);
                return;
            case PuyoAirborneFunction.Dropped:
                RunDroppedPuyo(slot, state, level);
                return;
            default:
                throw new NotSupportedException(
                    $"Puyo airborne function $A2:{(ushort)state.AirborneFunction:X4} " +
                    "is not translated.");
        }
    }

    /// <summary>Ports the common body of short, big, and nominally-unused long hops.</summary>
    private void RunNormalPuyoHop(
        RoomEnemySlot slot,
        PuyoEnemyState state,
        RoomLevelData level,
        ushort landingInstructionList)
    {
        MovePuyo(slot, state, level);
        if (!state.InvertDirection && state.HoppingAnimationActive)
            return;

        state.HoppingAnimationActive = false;
        SetPuyoInstructionList(slot, landingInstructionList);
    }

    /// <summary>Ports <c>Function_Puyo_Airborne_GiantHop</c> at $A2:9D6B.</summary>
    private void RunGiantPuyoHop(RoomEnemySlot slot, PuyoEnemyState state, RoomLevelData level)
    {
        MovePuyo(slot, state, level);
        if (!state.InvertDirection && state.HoppingAnimationActive)
            return;

        // A normal landing returns giant hops to type zero. A wall/ceiling collision takes
        // the native branch around these two writes, leaving type four's dropping recovery.
        if (!state.InvertDirection)
        {
            state.HopType = PuyoHopType.NormalSmall;
            state.Function = PuyoEnemyFunction.Grounded;
        }

        state.HoppingAnimationActive = false;
        SetPuyoInstructionList(slot, PuyoGroundedSlowInstructionList);
    }

    /// <summary>Ports the constant-speed collision fall at $A2:9D98.</summary>
    private void RunDroppingPuyo(RoomEnemySlot slot, PuyoEnemyState state, RoomLevelData level)
    {
        int displacement = ReadPuyoFixedPointDisplacement(state.HopTableIndex, 4);
        if (!MoveEnemyVertically(level, slot, displacement))
            return;

        // The collision frame consumes another RNG value and chooses one of two dropped
        // bounce heights. Its random result does not include Enemy.frameCounter here.
        state.HopType = (_nextRandom!() & 1) == 0
            ? PuyoHopType.DroppedSmall
            : PuyoHopType.DroppedBig;
        state.Function = PuyoEnemyFunction.Grounded;
    }

    /// <summary>Ports the post-drop bounce wrapper at $A2:9DCD.</summary>
    private void RunDroppedPuyo(RoomEnemySlot slot, PuyoEnemyState state, RoomLevelData level)
    {
        MovePuyo(slot, state, level);
        if (state.HoppingAnimationActive)
            return;

        state.HopType = PuyoHopType.Giant;
        state.Function = PuyoEnemyFunction.Grounded;
        SetPuyoInstructionList(slot, PuyoGroundedSlowInstructionList);
    }

    /// <summary>Ports <c>PuyoMovement</c> at $A2:9B88.</summary>
    private void MovePuyo(RoomEnemySlot slot, PuyoEnemyState state, RoomLevelData level)
    {
        ushort cappedIndex = state.YSpeedTableIndex;
        if (!IsNegative16(unchecked((ushort)(cappedIndex - MaximumPuyoYSpeedTableIndex))))
            cappedIndex = MaximumPuyoYSpeedTableIndex;

        int speedRow = (cappedIndex >> 8) * QuadraticSpeedRecordSize;
        int verticalOffset = state.Falling ? 0 : 4;
        int verticalDisplacement = ReadFixedPointDisplacement(
            QuadraticSpeedTableAddress + speedRow + verticalOffset);
        bool verticalCollision = MoveEnemyVertically(level, slot, verticalDisplacement);
        if (verticalCollision)
        {
            if (!state.Falling)
            {
                // $A2:9BBF reads stale direct-page byte $01. EnemyMain normally leaves it
                // zero, but the read is retained as a named quirk instead of pretending the
                // collision helper returns a direction flag. Zero matches retail room play.
                state.InvertDirection = false;
                state.InvertedDirection = Opposite(state.Direction);
                state.HopType = PuyoHopType.Dropping;
                state.AirborneFunction = PuyoAirborneFunction.Dropping;
                state.HoppingAnimationActive = false;
            }
            else
            {
                state.Function = PuyoEnemyFunction.Grounded;
                state.HoppingAnimationActive = false;
            }
            return;
        }

        // Pose selection happens before the index changes, matching the five discrete
        // squash/stretch thresholds in the ROM rather than interpolating sprite frames.
        SetPuyoAirborneInstructionList(slot, state);

        ushort delta = ReadPuyoHopWord(state.HopTableIndex, 4);
        state.YSpeedTableIndex = state.Falling
            ? unchecked((ushort)(state.YSpeedTableIndex + delta))
            : unchecked((ushort)(state.YSpeedTableIndex - delta));
        if (IsNegative16(state.YSpeedTableIndex))
        {
            state.Falling = true;
            state.YSpeedTableIndex = 0;
        }

        // X speed is stored as 8.8. The NTSC routine negates only the whole word when
        // moving left and leaves the fractional word positive. That makes $0140 mean
        // +1.25 px right but -0.75 px left; preserve the shipped asymmetry literally.
        ushort xSpeed = ReadPuyoHopWord(state.HopTableIndex, 2);
        ushort fraction = unchecked((ushort)((xSpeed & 0x00ff) << 8));
        short whole = unchecked((short)(xSpeed >> 8));
        if (state.Direction == PuyoDirection.Left)
            whole = unchecked((short)-whole);
        int horizontalDisplacement = (whole << 16) | fraction;

        if (!MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                horizontalDisplacement))
        {
            return;
        }

        state.InvertDirection = true;
        state.InvertedDirection = Opposite(state.Direction);
        state.HoppingAnimationActive = false;
        state.HopType = PuyoHopType.Dropping;
        state.AirborneFunction = PuyoAirborneFunction.Dropping;
    }

    /// <summary>Selects the rising/falling one-frame sleep list for the current curve index.</summary>
    private static void SetPuyoAirborneInstructionList(RoomEnemySlot slot, PuyoEnemyState state)
    {
        ushort instructionList;
        if (!state.Falling)
        {
            if (state.Direction == PuyoDirection.Right)
            {
                instructionList = state.YSpeedTableIndex >=
                        state.InitialYSpeedTableIndexThreeQuarters
                    ? PuyoRightFrame0LeftFrame4InstructionList
                    : state.YSpeedTableIndex >= state.InitialYSpeedTableIndexHalf
                        ? PuyoRightFrame1LeftFrame3InstructionList
                        : PuyoFrame2InstructionList;
            }
            else
            {
                instructionList = state.YSpeedTableIndex >=
                        state.InitialYSpeedTableIndexThreeQuarters
                    ? PuyoRightFrame4LeftFrame0InstructionList
                    : state.YSpeedTableIndex >= state.InitialYSpeedTableIndexHalf
                        ? PuyoRightFrame3LeftFrame1InstructionList
                        : PuyoFrame2InstructionList;
            }
        }
        else if (state.Direction == PuyoDirection.Right)
        {
            instructionList = state.YSpeedTableIndex < state.InitialYSpeedTableIndexHalf
                ? PuyoFrame2InstructionList
                : state.YSpeedTableIndex < state.InitialYSpeedTableIndexThreeQuarters
                    ? PuyoRightFrame3LeftFrame1InstructionList
                    : PuyoRightFrame4LeftFrame0InstructionList;
        }
        else
        {
            instructionList = state.YSpeedTableIndex < state.InitialYSpeedTableIndexHalf
                ? PuyoFrame2InstructionList
                : state.YSpeedTableIndex < state.InitialYSpeedTableIndexThreeQuarters
                    ? PuyoRightFrame1LeftFrame3InstructionList
                    : PuyoRightFrame0LeftFrame4InstructionList;
        }

        SetPuyoInstructionList(slot, instructionList);
    }

    private ushort ReadPuyoHopWord(ushort tableIndex, int fieldOffset)
    {
        if (tableIndex % PuyoHopRecordSize != 0 ||
            tableIndex / PuyoHopRecordSize >= 7)
        {
            throw new InvalidDataException(
                $"Puyo hop-table byte index ${tableIndex:X4} is outside seven ROM records.");
        }
        return ReadWord(_bus!, PuyoHopTableAddress + tableIndex + fieldOffset);
    }

    private PuyoAirborneFunction ReadPuyoHopFunction(ushort tableIndex) =>
        (PuyoAirborneFunction)ReadPuyoHopWord(tableIndex, 6);

    private int ReadPuyoFixedPointDisplacement(ushort tableIndex, int fieldOffset)
    {
        ushort packed = ReadPuyoHopWord(tableIndex, fieldOffset);
        ushort fraction = unchecked((ushort)((packed & 0x00ff) << 8));
        short whole = unchecked((short)(packed >> 8));
        return (whole << 16) | fraction;
    }

    private int ReadFixedPointDisplacement(int fractionAddress)
    {
        ushort fraction = ReadWord(_bus!, fractionAddress);
        short whole = unchecked((short)ReadWord(_bus!, fractionAddress + 2));
        return (whole << 16) | fraction;
    }

    private static ushort SwapBytes(ushort value) =>
        unchecked((ushort)((value << 8) | (value >> 8)));

    private static PuyoDirection Opposite(PuyoDirection direction) => direction switch
    {
        PuyoDirection.Right => PuyoDirection.Left,
        PuyoDirection.Left => PuyoDirection.Right,
        _ => throw new InvalidDataException(
            $"Puyo direction {(ushort)direction} is outside the two-entry ROM domain."),
    };

    private static void SetPuyoInstructionList(RoomEnemySlot slot, ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private PuyoEnemyState RequirePuyoState(RoomEnemySlot slot) =>
        _puyoStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Puyo state.");
}

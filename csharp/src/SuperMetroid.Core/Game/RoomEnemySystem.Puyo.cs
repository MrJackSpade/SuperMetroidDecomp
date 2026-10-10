using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A2 function pointers stored in Puyo's ordinary enemy variable C. Keeping
/// the ROM addresses as enum values makes a debugger display directly comparable with
/// <c>$0FAE,x</c> rather than hiding cartridge state behind invented host ordinals.
/// </summary>
public enum PuyoEnemyFunction : ushort
{
    /// <summary>$A2:9B65, <c>Function_Puyo_Grounded</c>: decrements the hop cooldown and selects an airborne hop when the countdown becomes negative.</summary>
    Grounded = 0x9b65,
    /// <summary>$A2:9B81, <c>Function_Puyo_Airborne</c>: dispatches the current hop or collision-recovery routine through the airborne function word.</summary>
    Airborne = 0x9b81,
}

/// <summary>
/// Literal indirect targets stored in Puyo's ordinary enemy variable D. Entries zero
/// through six in the ROM hop table select these functions.
/// </summary>
public enum PuyoAirborneFunction : ushort
{
    /// <summary>$A2:9D0B, <c>Function_Puyo_Airborne_Normal_ShortHop</c>: moves the small normal hop and selects the slow grounded animation on landing or terrain collision.</summary>
    NormalShortHop = 0x9d0b,
    /// <summary>$A2:9D2B, <c>Function_Puyo_Airborne_Normal_BigHop</c>: moves the taller normal hop and selects the medium grounded animation on landing or terrain collision.</summary>
    NormalBigHop = 0x9d2b,
    /// <summary>$A2:9D4B, <c>Function_Puyo_Airborne_Normal_LongHop</c>: moves hop-table record two with increased horizontal speed and selects the fast grounded animation on landing or terrain collision.</summary>
    NormalLongHop = 0x9d4b,
    /// <summary>$A2:9D6B, <c>Function_Puyo_Airborne_GiantHop</c>: moves the 128-pixel-height hop; normal landing restores small-hop selection, while wall or ceiling collision retains dropping recovery.</summary>
    GiantHop = 0x9d6b,
    /// <summary>$A2:9D98, <c>Function_Puyo_Airborne_Dropping</c>: falls vertically at the hop record's packed 8.8 speed until terrain contact selects a random small or big post-drop bounce.</summary>
    Dropping = 0x9d98,
    /// <summary>$A2:9DCD, <c>Function_Puyo_Airborne_Dropped</c>: moves either post-drop bounce and selects a giant hop for the next grounded launch when the hopping animation is cleared by terrain contact.</summary>
    Dropped = 0x9dcd,
}

/// <summary>
/// The seven concrete indexes into <c>PuyoHopTable</c> at $A2:9A07. This is a discriminant,
/// not a flags word: every value selects one eight-byte height/speed/function record.
/// </summary>
public enum PuyoHopType : ushort
{
    /// <summary>Record zero at $A2:9A07: 16-pixel height, packed 8.8 X speed $0100, and the short-hop function; ordinary distant launches may randomize to record one.</summary>
    NormalSmall = 0,
    /// <summary>Record one at $A2:9A0F: 32-pixel height, packed 8.8 X speed $0100, and the big-hop function; proximity to Samus permits random selection among normal records zero through two.</summary>
    NormalBig = 1,
    /// <summary>Record three at $A2:9A1F: 128-pixel height and packed 8.8 X speed $0140, whose fractional byte is discarded by NTSC movement; selected after a post-drop bounce.</summary>
    Giant = 3,
    /// <summary>Record four at $A2:9A27: no horizontal motion and packed 8.8 downward speed $0100; wall or ceiling collision selects this fall before a recovery bounce.</summary>
    Dropping = 4,
    /// <summary>Record five at $A2:9A2F: 16-pixel post-drop bounce with packed 8.8 X speed $0100 and curve-index delta $01C0; one of two randomized recovery heights.</summary>
    DroppedSmall = 5,
    /// <summary>Record six at $A2:9A37: 21-pixel post-drop bounce with packed 8.8 X speed $0100 and curve-index delta $01C0; the taller randomized recovery height.</summary>
    DroppedBig = 6,
}

/// <summary>The two literal values written to Puyo's direction word.</summary>
public enum PuyoDirection : ushort
{
    /// <summary>Native direction word zero: positive X motion and right-facing hop poses; also selected when Samus and Puyo have equal X coordinates.</summary>
    Right = 0,
    /// <summary>Native direction word one: negative X motion and left-facing hop poses; terrain collision can latch its opposite for the next launch.</summary>
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

    private const int QuadraticSpeedRecordSize = 8;
    private const ushort MaximumPuyoYSpeedTableIndex = 0x4000;

    private readonly PuyoEnemyState?[] _puyoStates =
        new PuyoEnemyState?[MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Puyo</c> at $A2:9A3F.</summary>
    private void InitializePuyo(RoomEnemySlot slot)
    {
        // The native initializer writes the common empty map and clears Enemy.var0 before
        // installing the fast grounded list. Enemy.var0 is not one of the six exposed
        // per-family variable words; no later Puyo routine reads it.
        slot.SpritemapPointer = 0x804d;
        SetPuyoInstructionList(slot, PuyoInstructionProgramDefinitions.GroundedFast);

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
                throw new InvalidDataException(
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

        // Types three and above ignore this value but still advance the seed, an observable
        // side effect shared with every other random enemy in the room.
        ushort randomChoice = NextPuyoRandom0To7(slot);
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

        state.HopTableIndex = checked((ushort)(hopType * PuyoHopDefinitions.RecordSize));
        state.AirborneFunction = PuyoHopDefinitions.FromByteIndex(state.HopTableIndex).Function;
        CalculateInitialPuyoHopSpeed(state);
    }

    /// <summary>Ports the integration loop at $A2:9B1A without simplifying its odd units.</summary>
    private static void CalculateInitialPuyoHopSpeed(PuyoEnemyState state)
    {
        ushort timeAccumulator = 0;
        ushort distanceAccumulator = 0;
        PuyoHopDefinition hop = PuyoHopDefinitions.FromByteIndex(state.HopTableIndex);
        ushort xSpeed = hop.XSpeed;
        ushort swappedJumpHeight = SwapBytes(hop.Height);

        for (int guard = 0; guard <= ushort.MaxValue; guard++)
        {
            timeAccumulator = unchecked((ushort)(timeAccumulator + xSpeed));
            int speedRow = (timeAccumulator >> 8) * QuadraticSpeedRecordSize;

            // The cartridge intentionally reads a word at table+1: the high byte of the
            // fractional speed followed by the low byte of the whole speed. Accumulating
            // that misaligned value produces an 8.8 distance estimate used only to find
            // the initial curve index.
            distanceAccumulator = unchecked((ushort)(distanceAccumulator +
                EnemyQuadraticSpeedDefinitions.ReadWord(speedRow + 1)));
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
            $"Puyo hop record {state.HopTableIndex / PuyoHopDefinitions.RecordSize} never reached " +
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
                RunNormalPuyoHop(
                    slot,
                    state,
                    level,
                    PuyoInstructionProgramDefinitions.GroundedSlow);
                return;
            case PuyoAirborneFunction.NormalBigHop:
                RunNormalPuyoHop(
                    slot,
                    state,
                    level,
                    PuyoInstructionProgramDefinitions.GroundedMedium);
                return;
            case PuyoAirborneFunction.NormalLongHop:
                RunNormalPuyoHop(
                    slot,
                    state,
                    level,
                    PuyoInstructionProgramDefinitions.GroundedFast);
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
                throw new InvalidDataException(
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
        SetPuyoInstructionList(slot, PuyoInstructionProgramDefinitions.GroundedSlow);
    }

    /// <summary>Ports the constant-speed collision fall at $A2:9D98.</summary>
    private void RunDroppingPuyo(RoomEnemySlot slot, PuyoEnemyState state, RoomLevelData level)
    {
        ushort packed = PuyoHopDefinitions.FromByteIndex(state.HopTableIndex).YIndexDelta;
        int displacement = packed << 8;
        if (!MoveEnemyVertically(level, slot, displacement))
            return;

        // $A2:9DB6 picks one of the two dropped bounce heights from the same 0..7 value.
        state.HopType = (NextPuyoRandom0To7(slot) & 1) == 0
            ? PuyoHopType.DroppedSmall
            : PuyoHopType.DroppedBig;
        state.Function = PuyoEnemyFunction.Grounded;
    }

    /// <summary>
    /// <c>GetRandomNumber0_7</c> ($A2:9B06): generates a random number, then adds the
    /// enemy's frame counter before keeping the low three bits.
    /// </summary>
    private ushort NextPuyoRandom0To7(RoomEnemySlot slot) =>
        unchecked((ushort)((_nextRandom!() + slot.FrameCounter) & 7));

    /// <summary>Ports the post-drop bounce wrapper at $A2:9DCD.</summary>
    private void RunDroppedPuyo(RoomEnemySlot slot, PuyoEnemyState state, RoomLevelData level)
    {
        MovePuyo(slot, state, level);
        if (state.HoppingAnimationActive)
            return;

        state.HopType = PuyoHopType.Giant;
        state.Function = PuyoEnemyFunction.Grounded;
        SetPuyoInstructionList(slot, PuyoInstructionProgramDefinitions.GroundedSlow);
    }

    /// <summary>Ports <c>PuyoMovement</c> at $A2:9B88.</summary>
    private void MovePuyo(RoomEnemySlot slot, PuyoEnemyState state, RoomLevelData level)
    {
        ushort cappedIndex = state.YSpeedTableIndex;
        if (!IsNegative16(unchecked((ushort)(cappedIndex - MaximumPuyoYSpeedTableIndex))))
            cappedIndex = MaximumPuyoYSpeedTableIndex;

        int speedRow = (cappedIndex >> 8) * QuadraticSpeedRecordSize;
        int verticalOffset = state.Falling ? 0 : 4;
        int verticalDisplacement = EnemyQuadraticSpeedDefinitions.ReadDisplacement(speedRow + verticalOffset);
        bool verticalCollision = MoveEnemyVertically(level, slot, verticalDisplacement);
        if (verticalCollision)
        {
            if (!state.Falling)
            {
                // $A2:9BBF copies the stale direct-page word at $01 into the invert flag.
                // During gameplay HandleHUDTilemap_PausedAndRunning ($80:9BFE) leaves
                // $00 = $9DD3 every frame, so the flag is set. Door transitions can leave
                // other values before the first HUD pass of a room; those are treated as
                // nonzero too, by decision, rather than modelling every scratch writer.
                state.InvertDirection = true;
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

        ushort delta = PuyoHopDefinitions.FromByteIndex(state.HopTableIndex).YIndexDelta;
        state.YSpeedTableIndex = state.Falling
            ? unchecked((ushort)(state.YSpeedTableIndex + delta))
            : unchecked((ushort)(state.YSpeedTableIndex - delta));
        if (IsNegative16(state.YSpeedTableIndex))
        {
            state.Falling = true;
            state.YSpeedTableIndex = 0;
        }

        // X speed is stored as 8.8, but NTSC $A2:9C29-9C32 moves by the whole byte only
        // and zeroes the subspeed word; only the PAL build uses the fraction. The giant
        // hop's $0140 therefore moves exactly one pixel per frame either way.
        ushort xSpeed = PuyoHopDefinitions.FromByteIndex(state.HopTableIndex).XSpeed;
        short whole = unchecked((short)(xSpeed >> 8));
        if (state.Direction == PuyoDirection.Left)
            whole = unchecked((short)-whole);
        int horizontalDisplacement = whole << 16;

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
                    ? PuyoInstructionProgramDefinitions.RightFrame0LeftFrame4
                    : state.YSpeedTableIndex >= state.InitialYSpeedTableIndexHalf
                        ? PuyoInstructionProgramDefinitions.RightFrame1LeftFrame3
                        : PuyoInstructionProgramDefinitions.Frame2;
            }
            else
            {
                instructionList = state.YSpeedTableIndex >=
                        state.InitialYSpeedTableIndexThreeQuarters
                    ? PuyoInstructionProgramDefinitions.RightFrame4LeftFrame0
                    : state.YSpeedTableIndex >= state.InitialYSpeedTableIndexHalf
                        ? PuyoInstructionProgramDefinitions.RightFrame3LeftFrame1
                        : PuyoInstructionProgramDefinitions.Frame2;
            }
        }
        else if (state.Direction == PuyoDirection.Right)
        {
            instructionList = state.YSpeedTableIndex < state.InitialYSpeedTableIndexHalf
                ? PuyoInstructionProgramDefinitions.Frame2
                : state.YSpeedTableIndex < state.InitialYSpeedTableIndexThreeQuarters
                    ? PuyoInstructionProgramDefinitions.RightFrame3LeftFrame1
                    : PuyoInstructionProgramDefinitions.RightFrame4LeftFrame0;
        }
        else
        {
            instructionList = state.YSpeedTableIndex < state.InitialYSpeedTableIndexHalf
                ? PuyoInstructionProgramDefinitions.Frame2
                : state.YSpeedTableIndex < state.InitialYSpeedTableIndexThreeQuarters
                    ? PuyoInstructionProgramDefinitions.RightFrame1LeftFrame3
                    : PuyoInstructionProgramDefinitions.RightFrame0LeftFrame4;
        }

        SetPuyoInstructionList(slot, instructionList);
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

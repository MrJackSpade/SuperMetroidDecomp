using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>The Puyo species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class PuyoBehavior : IEnemySpecies
{
    /// <summary>Supplies the shared room RNG used when choosing Puyo hop type and bounce height.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves the room collisions that reverse, land or drop an airborne Puyo.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds Puyo hop decisions to room randomness and terrain collision.</summary>
    internal PuyoBehavior(EnemyRoomBinding room, EnemyTerrain terrain)
    {
        _room = room;
        _terrain = terrain;
    }

    /// <summary>Bytes per native whole/fraction quadratic-speed record.</summary>
    private const int QuadraticSpeedRecordSize = 8;
    /// <summary>Upper bound applied to the airborne speed-table index.</summary>
    private const ushort MaximumPuyoYSpeedTableIndex = 0x4000;

    /// <summary>Typed grounded, airborne and dropping state indexed by native enemy slot.</summary>
    internal readonly PuyoEnemyState?[] _puyoStates =
        new PuyoEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Puyo</c> at $A2:9A3F.</summary>
    internal void InitializePuyo(RoomEnemySlot slot)
    {
        // The native initializer writes the common empty map and clears Enemy.var0 before
        // installing the fast grounded list. Enemy.var0 is not one of the six exposed
        // per-family variable words; no later Puyo routine reads it.
        slot.SpritemapPointer = 0x804d;
        SetPuyoInstructionList(slot, PuyoInstructionProgramDefinitions.GroundedFast);

        var state = new PuyoEnemyState(slot)
        {
            YSpeedTableIndex = 0,
            HopCooldownTimer = NativeCountdown.FromRaw(slot.Parameter1),
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
        state.HopCooldownTimer = state.HopCooldownTimer.Decrement().Remaining;
        if (!EnemyMath.IsNegative16(state.HopCooldownTimer.RawValue))
            return;

        state.Function = PuyoEnemyFunction.Airborne;
        state.HopCooldownTimer = NativeCountdown.FromRaw(slot.Parameter1);
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
            int distance = Math.Abs(unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)));
            state.HopType = distance < slot.Parameter2
                ? PuyoHopType.NormalBig
                : PuyoHopType.NormalSmall;
        }

        // Aim toward Samus unless the preceding terrain collision latched the opposite
        // direction. Equality follows the cartridge's BPL path and therefore aims right.
        PuyoDirection direction = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0
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
    internal static void CalculateInitialPuyoHopSpeed(PuyoEnemyState state)
    {
        ushort timeAccumulator = 0;
        ushort distanceAccumulator = 0;
        PuyoHopDefinition hop = PuyoHopDefinitions.FromByteIndex(state.HopTableIndex);
        ushort xSpeed = hop.XSpeed;
        ushort swappedJumpHeight = SnesLittleEndian.SwapBytes(hop.Height);

        for (int guard = 0; guard <= ushort.MaxValue; guard++)
        {
            timeAccumulator = unchecked((ushort)(timeAccumulator + xSpeed));
            int speedRow = (SnesLittleEndian.HighByte(timeAccumulator)) * QuadraticSpeedRecordSize;

            // The cartridge intentionally reads a word at table+1: the high byte of the
            // fractional speed followed by the low byte of the whole speed. Accumulating
            // that misaligned value produces an 8.8 distance estimate used only to find
            // the initial curve index.
            distanceAccumulator = unchecked((ushort)(distanceAccumulator +
                EnemyQuadraticSpeedDefinitions.ReadWord(speedRow + 1)));
            if (!EnemyMath.IsNegative16(unchecked((ushort)(swappedJumpHeight - distanceAccumulator))))
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
        int displacement = SnesSixteenSixteen.FromEightEight(packed);
        if (!_terrain.MoveEnemyVertically(level, slot, displacement))
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
        unchecked((ushort)((_room.Random.Generate() + slot.FrameCounter) & 7));

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
        if (!EnemyMath.IsNegative16(unchecked((ushort)(cappedIndex - MaximumPuyoYSpeedTableIndex))))
            cappedIndex = MaximumPuyoYSpeedTableIndex;

        int speedRow = (SnesLittleEndian.HighByte(cappedIndex)) * QuadraticSpeedRecordSize;
        int verticalOffset = state.Falling ? 0 : 4;
        int verticalDisplacement = EnemyQuadraticSpeedDefinitions.ReadDisplacement(speedRow + verticalOffset);
        bool verticalCollision = _terrain.MoveEnemyVertically(level, slot, verticalDisplacement);
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
        if (EnemyMath.IsNegative16(state.YSpeedTableIndex))
        {
            state.Falling = true;
            state.YSpeedTableIndex = 0;
        }

        // X speed is stored as 8.8, but NTSC $A2:9C29-9C32 moves by the whole byte only
        // and zeroes the subspeed word; only the PAL build uses the fraction. The giant
        // hop's $0140 therefore moves exactly one pixel per frame either way.
        ushort xSpeed = PuyoHopDefinitions.FromByteIndex(state.HopTableIndex).XSpeed;
        short whole = unchecked((short)(SnesEightEight.WholeOf(xSpeed)));
        if (state.Direction == PuyoDirection.Left)
            whole = unchecked((short)-whole);
        int horizontalDisplacement = SnesSixteenSixteen.FromWhole(whole);

        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
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

    /// <summary>Flips the two-entry native direction domain after a terrain impact.</summary>
    private static PuyoDirection Opposite(PuyoDirection direction) => direction switch
    {
        PuyoDirection.Right => PuyoDirection.Left,
        PuyoDirection.Left => PuyoDirection.Right,
        _ => throw new InvalidDataException(
            $"Puyo direction {(ushort)direction} is outside the two-entry ROM domain."),
    };

    /// <summary>Starts a selected pose or hop list with a one-tick instruction countdown.</summary>
    private static void SetPuyoInstructionList(RoomEnemySlot slot, ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Requires initialized per-slot Puyo state before its AI dispatcher runs.</summary>
    private PuyoEnemyState RequirePuyoState(RoomEnemySlot slot) =>
        _puyoStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Puyo state.");

    /// <summary>Routes bank-$A2 Puyo initialization to matching enemy definitions.</summary>
    internal void RegisterPuyoInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Puyo, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Puyo, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializePuyo(slot);
        });
    }

    /// <summary>Registers Puyo's grounded, airborne, giant-hop and dropping dispatch.</summary>
    internal void RegisterPuyoMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Puyo, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Puyo, (slot, frame, nmiFrameCounter8) =>
        {
            RunPuyoMain(slot, RequirePuyoState(slot), frame.Samus, frame.Level);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_puyoStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}

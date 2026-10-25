using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Puyo.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>The Puyo species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PuyoBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The hop types in the order of the hop records they select.</summary>
    private static readonly IdentitySequence<PuyoHopType> HopRecords = new(
        PuyoHopType.NormalSmall, PuyoHopType.NormalBig, PuyoHopType.NormalHigh, PuyoHopType.Giant,
        PuyoHopType.Dropping, PuyoHopType.DroppedSmall, PuyoHopType.DroppedBig);

    /// <summary>Whether a hop is one of the three ordinary hops that proximity and chance choose between.</summary>
    private static bool IsNormalHop(PuyoHopType type) =>
        type is PuyoHopType.NormalSmall or PuyoHopType.NormalBig or PuyoHopType.NormalHigh;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Puyo];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new PuyoEntity(this, slot, common);

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
    private const ushort MaximumPuyoYSpeedTableIndex = 16384;

    /// <summary>Ports <c>InitAI_Puyo</c> at $A2:9A3F.</summary>
    internal static void InitializePuyo(RoomEnemySlot slot)
    {
        // The native initializer writes the common empty map and clears Enemy.var0 before
        // installing the fast grounded list. Enemy.var0 is not one of the six exposed
        // per-family variable words; no later Puyo routine reads it.
        slot.Frame = EnemyFrameId.Empty;

        var state = new PuyoEnemyState
        {
            YSpeedTableIndex = 0,
            HopCooldownTimer = WrappingCountdown.FromRaw(checked((ushort)slot.Spawn.Population.Parameter1)),
            Function = PuyoEnemyFunction.Grounded,
            // InitAI never writes the airborne function or the inactive extended words. The room loader's
            // cleared enemy allocation therefore leaves them empty until InitiateHop owns
            // them; do not pre-populate plausible values the cartridge has not written.
            AirborneFunction = null,
            HopRecord = 0,
            HopType = PuyoHopType.NormalSmall,
            HoppingAnimationActive = false,
            Direction = PuyoDirection.Right,
            Falling = false,
            InvertDirection = false,
            InvertedDirection = PuyoDirection.Right,
            InitialYSpeedTableIndexThreeQuarters = 0,
            InitialYSpeedTableIndexHalf = 0,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);
        SetPuyoProgram(slot, state, PuyoProgram.GroundedFast);
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
                    $"Puyo function {state.Function?.ToString() ?? "none"} is not translated.");
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
        state.HopCooldownTimer = WrappingCountdown.FromRaw(checked((ushort)slot.Spawn.Population.Parameter1));
        state.HoppingAnimationActive = true;
        InitiatePuyoHop(slot, state, samus);
    }

    /// <summary>Ports <c>InitiateHop</c>, <c>ChooseHopType</c>, and their random selection.</summary>
    private void InitiatePuyoHop(RoomEnemySlot slot, PuyoEnemyState state, SamusState samus)
    {
        // Normal follow-up hops are promoted from type zero to type one when Samus is
        // strictly inside parameter-two's horizontal radius. Giant/dropping/dropped types
        // deliberately bypass this proximity rewrite.
        if (IsNormalHop(state.HopType))
        {
            int distance = Math.Abs(unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)));
            state.HopType = distance < slot.Spawn.Population.Parameter2
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
        if (IsNormalHop(state.HopType))
        {
            if (state.HopType == PuyoHopType.NormalSmall)
                randomChoice &= 1;
            if (randomChoice >= 2)
                randomChoice = 2;
            state.HopType = HopRecords.At(randomChoice);
        }

        state.HopRecord = HopRecords.PositionOf(state.HopType);
        state.AirborneFunction = Hop(state).Function;
        CalculateInitialPuyoHopSpeed(state);
    }

    /// <summary>Ports the integration loop at $A2:9B1A without simplifying its odd units.</summary>
    internal static void CalculateInitialPuyoHopSpeed(PuyoEnemyState state)
    {
        ushort timeAccumulator = 0;
        ushort distanceAccumulator = 0;
        PuyoHopDefinition hop = Hop(state);
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
            state.InitialYSpeedTableIndexHalf = timeAccumulator >> 1;
            state.InitialYSpeedTableIndexThreeQuarters = (timeAccumulator >> 2) + state.InitialYSpeedTableIndexHalf;
            return;
        }

        throw new InvalidDataException(
            $"Puyo hop record {state.HopRecord} never reached " +
            "its ROM jump-height threshold.");
    }

    /// <summary>The hop-table record <paramref name="state"/> has selected.</summary>
    private static PuyoHopDefinition Hop(PuyoEnemyState state) =>
        PuyoHopDefinitions.FromByteIndex(checked((ushort)(state.HopRecord * PuyoHopDefinitions.RecordSize)));

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
                    PuyoProgram.GroundedSlow);
                return;
            case PuyoAirborneFunction.NormalBigHop:
                RunNormalPuyoHop(
                    slot,
                    state,
                    level,
                    PuyoProgram.GroundedMedium);
                return;
            case PuyoAirborneFunction.NormalLongHop:
                RunNormalPuyoHop(
                    slot,
                    state,
                    level,
                    PuyoProgram.GroundedFast);
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
                    $"Puyo airborne function {state.AirborneFunction?.ToString() ?? "none"} " +
                    "is not translated.");
        }
    }

    /// <summary>Ports the common body of short, big, and nominally-unused long hops.</summary>
    private void RunNormalPuyoHop(
        RoomEnemySlot slot,
        PuyoEnemyState state,
        RoomLevelData level,
        PuyoProgram landingProgram)
    {
        MovePuyo(slot, state, level);
        if (!state.InvertDirection && state.HoppingAnimationActive)
            return;

        state.HoppingAnimationActive = false;
        SetPuyoProgram(slot, state, landingProgram);
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
        SetPuyoProgram(slot, state, PuyoProgram.GroundedSlow);
    }

    /// <summary>Ports the constant-speed collision fall at $A2:9D98.</summary>
    private void RunDroppingPuyo(RoomEnemySlot slot, PuyoEnemyState state, RoomLevelData level)
    {
        ushort packed = Hop(state).YIndexDelta;
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
        SetPuyoProgram(slot, state, PuyoProgram.GroundedSlow);
    }

    /// <summary>Ports <c>PuyoMovement</c> at $A2:9B88.</summary>
    private void MovePuyo(RoomEnemySlot slot, PuyoEnemyState state, RoomLevelData level)
    {
        int cappedIndex = Math.Min(state.YSpeedTableIndex, MaximumPuyoYSpeedTableIndex);
        int speedRow = SnesEightEight.WholeOf(cappedIndex) * QuadraticSpeedRecordSize;
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
        SetPuyoAirborneProgram(slot, state);

        state.StepAirborneIndex(Hop(state).YIndexDelta);

        // X speed is stored as 8.8, but NTSC $A2:9C29-9C32 moves by the whole byte only
        // and zeroes the subspeed word; only the PAL build uses the fraction. The giant
        // hop's $0140 therefore moves exactly one pixel per frame either way.
        ushort xSpeed = Hop(state).XSpeed;
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

    /// <summary>Selects the rising/falling one-frame pose for the current curve index.</summary>
    private static void SetPuyoAirborneProgram(RoomEnemySlot slot, PuyoEnemyState state)
    {
        PuyoProgram program;
        if (!state.Falling)
        {
            if (state.Direction == PuyoDirection.Right)
            {
                program = state.YSpeedTableIndex >=
                        state.InitialYSpeedTableIndexThreeQuarters
                    ? PuyoProgram.RightFrame0LeftFrame4
                    : state.YSpeedTableIndex >= state.InitialYSpeedTableIndexHalf
                        ? PuyoProgram.RightFrame1LeftFrame3
                        : PuyoProgram.Frame2;
            }
            else
            {
                program = state.YSpeedTableIndex >=
                        state.InitialYSpeedTableIndexThreeQuarters
                    ? PuyoProgram.RightFrame4LeftFrame0
                    : state.YSpeedTableIndex >= state.InitialYSpeedTableIndexHalf
                        ? PuyoProgram.RightFrame3LeftFrame1
                        : PuyoProgram.Frame2;
            }
        }
        else if (state.Direction == PuyoDirection.Right)
        {
            program = state.YSpeedTableIndex < state.InitialYSpeedTableIndexHalf
                ? PuyoProgram.Frame2
                : state.YSpeedTableIndex < state.InitialYSpeedTableIndexThreeQuarters
                    ? PuyoProgram.RightFrame3LeftFrame1
                    : PuyoProgram.RightFrame4LeftFrame0;
        }
        else
        {
            program = state.YSpeedTableIndex < state.InitialYSpeedTableIndexHalf
                ? PuyoProgram.Frame2
                : state.YSpeedTableIndex < state.InitialYSpeedTableIndexThreeQuarters
                    ? PuyoProgram.RightFrame1LeftFrame3
                    : PuyoProgram.RightFrame0LeftFrame4;
        }

        SetPuyoProgram(slot, state, program);
    }

    /// <summary>Flips the two-entry native direction domain after a terrain impact.</summary>
    private static PuyoDirection Opposite(PuyoDirection direction) => direction switch
    {
        PuyoDirection.Right => PuyoDirection.Left,
        PuyoDirection.Left => PuyoDirection.Right,
        _ => throw new InvalidDataException(
            $"Puyo direction {direction} is outside the two-entry ROM domain."),
    };

    /// <summary>Starts a selected pose or hop animation with a one-tick instruction countdown.</summary>
    private static void SetPuyoProgram(RoomEnemySlot slot, PuyoEnemyState state, PuyoProgram program)
    {
        state.Program = AnimationProgramCursor<PuyoProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Advances Puyo's typed animation.</summary>
    internal static void AdvancePuyoTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, PuyoAnimations.Programs, PuyoFrameDefinitions.ShownFrames.Of,
            static (PuyoEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor);

    /// <summary>Ports Puyo's main AI.</summary>
    internal void RunPuyoMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunPuyoMain(slot, slot.RequireSpeciesState<PuyoEnemyState>(), frame.Samus, frame.Level);
    }
}

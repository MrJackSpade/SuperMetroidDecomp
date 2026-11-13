using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>The Puyo species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PuyoBehavior : IEnemySpecies, IEnemyEntityFactory
{
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
    /// <summary>Whether a rising Puyo that strikes a ceiling turns back on its next hop.</summary>
    private readonly PuyoCeilingTurnFromLastDraw _ceilingTurn;

    /// <summary>Binds Puyo hop decisions to room randomness, terrain collision and the ceiling strike's turn.</summary>
    internal PuyoBehavior(EnemyRoomBinding room, EnemyTerrain terrain, PuyoCeilingTurnFromLastDraw ceilingTurn)
    {
        _room = room;
        _terrain = terrain;
        _ceilingTurn = ceilingTurn;
    }

    /// <summary>Upper bound applied to the airborne speed-curve place when it selects a speed.</summary>
    private static readonly EnemyQuadraticSpeedIndex MaximumPuyoYSpeedCurvePlace = EnemyQuadraticSpeedIndex.FromRows(64);

    /// <summary>Initializes the Puyo.</summary>
    internal static void InitializePuyo(RoomEnemySlot slot)
    {
        // The initializer shows the common empty frame before starting the fast grounded animation.
        slot.Frame = EnemyFrameId.Empty;

        var state = new PuyoEnemyState
        {
            YSpeedCurvePlace = EnemyQuadraticSpeedIndex.Start,
            HopCooldownTimer = slot.SpawnSettings<PuyoSpawnSettings>().HopCooldown,
            Action = PuyoAction.Grounded,
            // The initialization AI never writes the airborne movement or the inactive extra values. The room loader's
            // cleared enemy allocation therefore leaves them empty until the first hop owns
            // them; do not pre-populate plausible values the game has not written.
            AirborneMovement = null,
            CurrentHop = PuyoHopType.NormalSmall,
            HopType = PuyoHopType.NormalSmall,
            HoppingAnimationActive = false,
            Direction = PuyoDirection.Right,
            Falling = false,
            InvertDirection = false,
            InvertedDirection = PuyoDirection.Right,
            InitialYSpeedCurvePlaceThreeQuarters = EnemyQuadraticSpeedIndex.Start,
            InitialYSpeedCurvePlaceHalf = EnemyQuadraticSpeedIndex.Start,
        };
        slot.SetSpeciesState(state);
        SetPuyoAnimation(slot, state, PuyoSegment.GroundedFast);
    }

    /// <summary>Runs a Puyo's per-frame behaviour and its top-level actions.</summary>
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

        switch (state.Action)
        {
            case PuyoAction.Grounded:
                RunGroundedPuyo(slot, state, samus);
                return;
            case PuyoAction.Airborne:
                RunAirbornePuyo(slot, state, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Puyo has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Runs the grounded Puyo.</summary>
    private void RunGroundedPuyo(RoomEnemySlot slot, PuyoEnemyState state, SamusState samus)
    {
        // Decrementing and then testing for a non-negative count means a zero cooldown waits one frame and then wraps to 65535
        // before hopping. This is not equivalent to a conventional "if timer != 0" test.
        if (--state.HopCooldownTimer >= 0)
            return;

        state.Action = PuyoAction.Airborne;
        state.HopCooldownTimer = slot.SpawnSettings<PuyoSpawnSettings>().HopCooldown;
        state.HoppingAnimationActive = true;
        InitiatePuyoHop(slot, state, samus);
    }

    /// <summary>Starts a hop: chooses the hop type and its random selection.</summary>
    private void InitiatePuyoHop(RoomEnemySlot slot, PuyoEnemyState state, SamusState samus)
    {
        // Normal follow-up hops are promoted from type zero to type one when Samus is
        // strictly inside its horizontal hop distance. Giant/dropping/dropped types
        // deliberately bypass this proximity rewrite.
        if (IsNormalHop(state.HopType))
        {
            int distance = CoordinateAxis.Distance(samus.Position.X.Whole, slot.Position.X.Whole);
            state.HopType = distance < slot.SpawnSettings<PuyoSpawnSettings>().HopDistance
                ? PuyoHopType.NormalBig
                : PuyoHopType.NormalSmall;
        }

        // Aim toward Samus unless the preceding terrain collision latched the opposite
        // direction. Equality follows the game's non-negative path and therefore aims right.
        PuyoDirection direction = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0
            ? PuyoDirection.Left
            : PuyoDirection.Right;
        if (state.InvertDirection)
            direction = state.InvertedDirection;
        state.Direction = direction;
        state.InvertDirection = false;

        // Types three and above ignore this sample but still advance the seed, an observable
        // side effect shared with every other random enemy in the room.
        int sample = _room.Random.Generate();
        if (IsNormalHop(state.HopType))
        {
            WeightedRandomChoice<PuyoHopType> nextHop = state.HopType == PuyoHopType.NormalSmall
                ? PuyoHopDefinitions.HopAfterSmallHop
                : PuyoHopDefinitions.HopAfterLargerHop;
            state.HopType = nextHop.Choose(sample, advancedBy: slot.FrameCounter);
        }

        state.CurrentHop = state.HopType;
        state.AirborneMovement = Hop(state).Movement;
        CalculateInitialPuyoHopSpeed(state);
    }

    /// <summary>Runs the integration loop without simplifying its odd units.</summary>
    internal static void CalculateInitialPuyoHopSpeed(PuyoEnemyState state)
    {
        int timeAccumulator = 0;
        int distanceAccumulator = 0;
        PuyoHopDefinition hop = Hop(state);
        int xSpeed = hop.XSpeed;
        // The game exchanges the height's two halves; every authored height is below 256 pixels, so that
        // makes the height the whole part of an 8.8 distance.
        int swappedJumpHeight = UnsignedEightEight.FromParts(whole: hop.Height, fraction: 0);

        for (int guard = 0; guard <= WrappingCounter.LargestCount; guard++)
        {
            timeAccumulator = WrappingCounter.AddCount(timeAccumulator, xSpeed);
            EnemyQuadraticSpeedRow speedRow = EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(timeAccumulator).Row;

            // Summing each row's coarse speed, in 256ths of a pixel, gives the 8.8 distance estimate used only to
            // find the initial curve row.
            distanceAccumulator = WrappingCounter.AddCount(distanceAccumulator,
                speedRow.CoarseToward(EnemySpeedSign.Positive));
            if (!WrappingCounter.IsBehind(swappedJumpHeight, distanceAccumulator))
                continue;

            state.YSpeedCurvePlace = EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(timeAccumulator);
            state.Falling = false;
            state.InitialYSpeedCurvePlaceHalf = EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(timeAccumulator >> 1);
            state.InitialYSpeedCurvePlaceThreeQuarters =
                EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(timeAccumulator >> 2) + state.InitialYSpeedCurvePlaceHalf;
            return;
        }

        throw new InvalidDataException(
            $"Puyo hop {state.CurrentHop} never reached " +
            "its authored jump-height threshold.");
    }

    /// <summary>The definition of the hop <paramref name="state"/> is making.</summary>
    private static PuyoHopDefinition Hop(PuyoEnemyState state) => PuyoHopDefinitions.Hops[state.CurrentHop];

    /// <summary>Runs the indirect airborne dispatcher.</summary>
    private void RunAirbornePuyo(
        RoomEnemySlot slot,
        PuyoEnemyState state,
        RoomLevelData level)
    {
        switch (state.AirborneMovement)
        {
            case PuyoAirborneMovement.NormalShortHop:
                RunNormalPuyoHop(
                    slot,
                    state,
                    level,
                    PuyoSegment.GroundedSlow);
                return;
            case PuyoAirborneMovement.NormalBigHop:
                RunNormalPuyoHop(
                    slot,
                    state,
                    level,
                    PuyoSegment.GroundedMedium);
                return;
            case PuyoAirborneMovement.NormalLongHop:
                RunNormalPuyoHop(
                    slot,
                    state,
                    level,
                    PuyoSegment.GroundedFast);
                return;
            case PuyoAirborneMovement.GiantHop:
                RunGiantPuyoHop(slot, state, level);
                return;
            case PuyoAirborneMovement.Dropping:
                RunDroppingPuyo(slot, state, level);
                return;
            case PuyoAirborneMovement.Dropped:
                RunDroppedPuyo(slot, state, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Puyo has no behaviour for airborne movement " +
                    $"{state.AirborneMovement?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Runs the common body of short, big, and nominally-unused long hops.</summary>
    private void RunNormalPuyoHop(
        RoomEnemySlot slot,
        PuyoEnemyState state,
        RoomLevelData level,
        PuyoSegment landingSegment)
    {
        MovePuyo(slot, state, level);
        if (!state.InvertDirection && state.HoppingAnimationActive)
            return;

        state.HoppingAnimationActive = false;
        SetPuyoAnimation(slot, state, landingSegment);
    }

    /// <summary>Runs the Puyo's giant airborne hop.</summary>
    private void RunGiantPuyoHop(RoomEnemySlot slot, PuyoEnemyState state, RoomLevelData level)
    {
        MovePuyo(slot, state, level);
        if (!state.InvertDirection && state.HoppingAnimationActive)
            return;

        // A normal landing returns giant hops to type zero. A wall/ceiling collision takes
        // the branch around these two writes, leaving type four's dropping recovery.
        if (!state.InvertDirection)
        {
            state.HopType = PuyoHopType.NormalSmall;
            state.Action = PuyoAction.Grounded;
        }

        state.HoppingAnimationActive = false;
        SetPuyoAnimation(slot, state, PuyoSegment.GroundedSlow);
    }

    /// <summary>Runs the constant-speed collision fall.</summary>
    private void RunDroppingPuyo(RoomEnemySlot slot, PuyoEnemyState state, RoomLevelData level)
    {
        int packed = Hop(state).YIndexDelta;
        SignedSixteenSixteen displacement = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(packed);
        if (!_terrain.MoveEnemyVertically(level, slot, displacement))
            return;

        // The bounce picks one of the two dropped bounce heights from the same advanced hop roll.
        state.HopType = PuyoHopDefinitions.BigDroppedBounce.IsTakenBy(_room.Random.Generate(), advancedBy: slot.FrameCounter)
            ? PuyoHopType.DroppedBig
            : PuyoHopType.DroppedSmall;
        state.Action = PuyoAction.Grounded;
    }

    /// <summary>Runs the post-drop bounce wrapper.</summary>
    private void RunDroppedPuyo(RoomEnemySlot slot, PuyoEnemyState state, RoomLevelData level)
    {
        MovePuyo(slot, state, level);
        if (state.HoppingAnimationActive)
            return;

        state.HopType = PuyoHopType.Giant;
        state.Action = PuyoAction.Grounded;
        SetPuyoAnimation(slot, state, PuyoSegment.GroundedSlow);
    }

    /// <summary>Moves the Puyo.</summary>
    private void MovePuyo(RoomEnemySlot slot, PuyoEnemyState state, RoomLevelData level)
    {
        EnemyQuadraticSpeedRow speedRow = EnemyQuadraticSpeedIndex.Min(state.YSpeedCurvePlace, MaximumPuyoYSpeedCurvePlace).Row;
        SignedSixteenSixteen verticalDisplacement = speedRow.Toward(state.Falling ? EnemySpeedSign.Positive : EnemySpeedSign.Negative);
        bool verticalCollision = _terrain.MoveEnemyVertically(level, slot, verticalDisplacement);
        if (verticalCollision)
        {
            if (!state.Falling)
            {
                // Striking a ceiling while rising starts the drop; whether the next hop turns back is what the room's
                // last draw and HUD redraw left (PuyoCeilingTurnFromLastDraw): always in an ordinary gameplay update.
                state.InvertDirection = _ceilingTurn.TurnsBack;
                state.InvertedDirection = Opposite(state.Direction);
                state.HopType = PuyoHopType.Dropping;
                state.AirborneMovement = PuyoAirborneMovement.Dropping;
                state.HoppingAnimationActive = false;
            }
            else
            {
                state.Action = PuyoAction.Grounded;
                state.HoppingAnimationActive = false;
            }
            return;
        }

        // Pose selection happens before the index changes, matching the five discrete
        // squash/stretch thresholds in the authored rather than interpolating sprite frames.
        SetPuyoAirborneAnimation(slot, state);

        state.StepAirborneIndex(EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(Hop(state).YIndexDelta));

        // X speed is authored as 8.8, but NTSC movement moves by the whole pixels only
        // and zeroes the subspeed; only the PAL build uses the fraction. The giant
        // hop's 1.25 therefore moves exactly one pixel per frame either way.
        int xSpeed = Hop(state).XSpeed;
        int whole = EightEight.WholeOf(xSpeed);
        if (state.Direction == PuyoDirection.Left)
            whole = -whole;
        SignedSixteenSixteen horizontalDisplacement = SignedSixteenSixteen.FromWholePixels(whole);

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
        state.AirborneMovement = PuyoAirborneMovement.Dropping;
    }

    /// <summary>Selects the rising/falling one-frame pose for the current curve index.</summary>
    private static void SetPuyoAirborneAnimation(RoomEnemySlot slot, PuyoEnemyState state)
    {
        PuyoSegment segment;
        if (!state.Falling)
        {
            if (state.Direction == PuyoDirection.Right)
            {
                segment = state.YSpeedCurvePlace >=
                        state.InitialYSpeedCurvePlaceThreeQuarters
                    ? PuyoSegment.RightFrame0LeftFrame4
                    : state.YSpeedCurvePlace >= state.InitialYSpeedCurvePlaceHalf
                        ? PuyoSegment.RightFrame1LeftFrame3
                        : PuyoSegment.Frame2;
            }
            else
            {
                segment = state.YSpeedCurvePlace >=
                        state.InitialYSpeedCurvePlaceThreeQuarters
                    ? PuyoSegment.RightFrame4LeftFrame0
                    : state.YSpeedCurvePlace >= state.InitialYSpeedCurvePlaceHalf
                        ? PuyoSegment.RightFrame3LeftFrame1
                        : PuyoSegment.Frame2;
            }
        }
        else if (state.Direction == PuyoDirection.Right)
        {
            segment = state.YSpeedCurvePlace < state.InitialYSpeedCurvePlaceHalf
                ? PuyoSegment.Frame2
                : state.YSpeedCurvePlace < state.InitialYSpeedCurvePlaceThreeQuarters
                    ? PuyoSegment.RightFrame3LeftFrame1
                    : PuyoSegment.RightFrame4LeftFrame0;
        }
        else
        {
            segment = state.YSpeedCurvePlace < state.InitialYSpeedCurvePlaceHalf
                ? PuyoSegment.Frame2
                : state.YSpeedCurvePlace < state.InitialYSpeedCurvePlaceThreeQuarters
                    ? PuyoSegment.RightFrame1LeftFrame3
                    : PuyoSegment.RightFrame0LeftFrame4;
        }

        SetPuyoAnimation(slot, state, segment);
    }

    /// <summary>Flips the two-entry direction domain after a terrain impact.</summary>
    private static PuyoDirection Opposite(PuyoDirection direction) => direction switch
    {
        PuyoDirection.Right => PuyoDirection.Left,
        PuyoDirection.Left => PuyoDirection.Right,
        _ => throw new InvalidDataException(
            $"Puyo direction {direction} is neither of the two directions."),
    };

    /// <summary>Starts a selected pose or hop animation with a one-tick frame countdown.</summary>
    private static void SetPuyoAnimation(RoomEnemySlot slot, PuyoEnemyState state, PuyoSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(slot, PuyoAnimations.StartingWith(segment));
    }

    /// <summary>Advances Puyo's typed animation.</summary>
    internal static void AdvancePuyoTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, PuyoAnimations.Segments,
            static (PuyoEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"Puyo's cue-free animation raised {cue}."));

    /// <summary>Runs Puyo's main AI.</summary>
    internal void RunPuyoMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunPuyoMain(slot, slot.RequireSpeciesState<PuyoEnemyState>(), frame.Samus, frame.Level);
    }
}

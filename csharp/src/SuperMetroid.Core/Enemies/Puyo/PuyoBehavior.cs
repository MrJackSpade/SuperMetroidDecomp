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
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new PuyoEntity(this, place, common);

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

    /// <summary>Initializes the Puyo.</summary>
    internal static void InitializePuyo(RoomEnemyPlace place)
    {
        // The initializer shows the common empty frame before starting the fast grounded animation.
        place.Frame = EnemyFrameId.Empty;

        var state = new PuyoEnemyState(PuyoAction.Grounded)
        {
            YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start,
            HopCooldownTimer = place.SpawnSettings<PuyoSpawnSettings>().HopCooldown,
            CurrentHop = PuyoHopType.NormalSmall,
            HopType = PuyoHopType.NormalSmall,
            HoppingAnimationActive = false,
            Direction = PuyoDirection.Right,
            Falling = false,
            InvertDirection = false,
            InvertedDirection = PuyoDirection.Right,
            InitialYSpeedCurvePlaceThreeQuarters = QuadraticSpeedCurvePlace.Start,
            InitialYSpeedCurvePlaceHalf = QuadraticSpeedCurvePlace.Start,
        };
        place.SetSpeciesState(state);
        SetPuyoAnimation(place, state, PuyoSegment.GroundedFast);
    }

    /// <summary>Runs a Puyo's per-frame behaviour and its top-level actions.</summary>
    private void RunPuyoMain(
        RoomEnemyPlace place,
        PuyoEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        switch (state.Action)
        {
            case PuyoAction.Grounded:
                RunGroundedPuyo(place, state, samus);
                return;
            case PuyoAction.Airborne:
                RunAirbornePuyo(place, state, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Puyo has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs the grounded Puyo.</summary>
    private void RunGroundedPuyo(RoomEnemyPlace place, PuyoEnemyState state, SamusState samus)
    {
        // The cooldown counts down before it is tested: a cooldown of N keeps the Puyo grounded for N updates and it hops
        // on the next, so a zero cooldown hops at once.
        if (--state.HopCooldownTimer >= 0)
            return;

        state.Action = PuyoAction.Airborne;
        state.HopCooldownTimer = place.SpawnSettings<PuyoSpawnSettings>().HopCooldown;
        state.HoppingAnimationActive = true;
        InitiatePuyoHop(place, state, samus);
    }

    /// <summary>Starts a hop: chooses the hop type and its random selection.</summary>
    private void InitiatePuyoHop(RoomEnemyPlace place, PuyoEnemyState state, SamusState samus)
    {
        // Normal follow-up hops are promoted from the small hop to the big one when Samus is
        // strictly inside its horizontal hop distance. Giant/dropping/dropped types
        // deliberately bypass this proximity rewrite.
        if (IsNormalHop(state.HopType))
        {
            int distance = CoordinateAxis.Distance(samus.Position.X.Whole, place.Position.X.Whole);
            state.HopType = distance < place.SpawnSettings<PuyoSpawnSettings>().HopDistance
                ? PuyoHopType.NormalBig
                : PuyoHopType.NormalSmall;
        }

        // Aim toward Samus unless the preceding terrain collision latched the opposite
        // direction. Equality follows the game's non-negative path and therefore aims right.
        PuyoDirection direction = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) < 0
            ? PuyoDirection.Left
            : PuyoDirection.Right;
        if (state.InvertDirection)
            direction = state.InvertedDirection;
        state.Direction = direction;
        state.InvertDirection = false;

        // The giant hop, the drop and the dropped bounces ignore this sample but still advance the seed, an observable
        // side effect shared with every other random enemy in the room.
        int sample = _room.Random.Generate();
        if (IsNormalHop(state.HopType))
        {
            WeightedRandomChoice<PuyoHopType> nextHop = state.HopType == PuyoHopType.NormalSmall
                ? PuyoHopDefinitions.HopAfterSmallHop
                : PuyoHopDefinitions.HopAfterLargerHop;
            state.HopType = nextHop.Choose(sample, advancedBy: place.FrameCounter);
        }

        state.CurrentHop = state.HopType;
        state.AirborneMovement = Hop(state).Movement;
        CalculateInitialPuyoHopSpeed(state);
    }

    /// <summary>
    /// Finds where the hop's rise starts on the speed curve: stepping from the curve's start by the hop's launch search step,
    /// the first place at which the coarse speeds (<see cref="QuadraticCurveSpeed"/>) of the places passed sum past the hop's
    /// height. The rise's half and three-quarter places, which pace its poses, are shares of that place. A search that steps
    /// nowhere is refused, and one that runs off the curve fails when its speed is read.
    /// </summary>
    /// <exception cref="InvalidDataException">The hop's search steps nowhere or runs off the curve.</exception>
    internal static void CalculateInitialPuyoHopSpeed(PuyoEnemyState state)
    {
        PuyoHopDefinition hop = Hop(state);
        if (hop.LaunchSearchStep <= QuadraticSpeedCurvePlace.Start)
            throw new InvalidDataException($"Puyo hop {state.CurrentHop} has no launch search step.");
        SignedSixteenSixteen height = SignedSixteenSixteen.FromWholePixels(hop.JumpHeight);
        QuadraticSpeedCurvePlace place = QuadraticSpeedCurvePlace.Start;
        SignedSixteenSixteen risen = SignedSixteenSixteen.Zero;
        do
        {
            place += hop.LaunchSearchStep;
            risen += SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(place.Speed.CoarseToward(EnemySpeedSign.Positive));
        }
        while (risen <= height);

        state.YSpeedCurvePlace = place;
        state.Falling = false;
        state.InitialYSpeedCurvePlaceHalf = place.DividedBy(2);
        state.InitialYSpeedCurvePlaceThreeQuarters = place.DividedBy(4) + state.InitialYSpeedCurvePlaceHalf;
    }

    /// <summary>The definition of the hop <paramref name="state"/> is making.</summary>
    private static PuyoHopDefinition Hop(PuyoEnemyState state) => PuyoHopDefinitions.Hops[state.CurrentHop];

    /// <summary>Runs the airborne step.</summary>
    private void RunAirbornePuyo(
        RoomEnemyPlace place,
        PuyoEnemyState state,
        RoomLevelData level)
    {
        switch (state.AirborneMovement)
        {
            case PuyoAirborneMovement.NormalShortHop:
                RunNormalPuyoHop(
                    place,
                    state,
                    level,
                    PuyoSegment.GroundedSlow);
                return;
            case PuyoAirborneMovement.NormalBigHop:
                RunNormalPuyoHop(
                    place,
                    state,
                    level,
                    PuyoSegment.GroundedMedium);
                return;
            case PuyoAirborneMovement.NormalLongHop:
                RunNormalPuyoHop(
                    place,
                    state,
                    level,
                    PuyoSegment.GroundedFast);
                return;
            case PuyoAirborneMovement.GiantHop:
                RunGiantPuyoHop(place, state, level);
                return;
            case PuyoAirborneMovement.Dropping:
                RunDroppingPuyo(place, state, level);
                return;
            case PuyoAirborneMovement.Dropped:
                RunDroppedPuyo(place, state, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Puyo has no behaviour for airborne movement {state.AirborneMovement}.");
        }
    }

    /// <summary>Runs the common body of short, big, and nominally-unused long hops.</summary>
    private void RunNormalPuyoHop(
        RoomEnemyPlace place,
        PuyoEnemyState state,
        RoomLevelData level,
        PuyoSegment landingSegment)
    {
        MovePuyo(place, state, level);
        if (!state.InvertDirection && state.HoppingAnimationActive)
            return;

        state.HoppingAnimationActive = false;
        SetPuyoAnimation(place, state, landingSegment);
    }

    /// <summary>Runs the Puyo's giant airborne hop.</summary>
    private void RunGiantPuyoHop(RoomEnemyPlace place, PuyoEnemyState state, RoomLevelData level)
    {
        MovePuyo(place, state, level);
        if (!state.InvertDirection && state.HoppingAnimationActive)
            return;

        // A normal landing returns a giant hop to the small hop. A wall or ceiling collision takes
        // the branch around these two writes, leaving the drop's recovery.
        if (!state.InvertDirection)
        {
            state.HopType = PuyoHopType.NormalSmall;
            state.Action = PuyoAction.Grounded;
        }

        state.HoppingAnimationActive = false;
        SetPuyoAnimation(place, state, PuyoSegment.GroundedSlow);
    }

    /// <summary>
    /// Runs the constant-speed fall: at the drop speed of the hop the Puyo last launched, which a wall or ceiling cut short
    /// (#1820).
    /// </summary>
    private void RunDroppingPuyo(RoomEnemyPlace place, PuyoEnemyState state, RoomLevelData level)
    {
        if (!_terrain.MoveEnemyVertically(level, place, Hop(state).DropSpeed))
            return;

        // The bounce picks one of the two dropped bounce heights from the same advanced hop roll.
        state.HopType = PuyoHopDefinitions.BigDroppedBounce.IsTakenBy(_room.Random.Generate(), advancedBy: place.FrameCounter)
            ? PuyoHopType.DroppedBig
            : PuyoHopType.DroppedSmall;
        state.Action = PuyoAction.Grounded;
    }

    /// <summary>Runs the post-drop bounce wrapper.</summary>
    private void RunDroppedPuyo(RoomEnemyPlace place, PuyoEnemyState state, RoomLevelData level)
    {
        MovePuyo(place, state, level);
        if (state.HoppingAnimationActive)
            return;

        state.HopType = PuyoHopType.Giant;
        state.Action = PuyoAction.Grounded;
        SetPuyoAnimation(place, state, PuyoSegment.GroundedSlow);
    }

    /// <summary>Moves the Puyo.</summary>
    private void MovePuyo(RoomEnemyPlace place, PuyoEnemyState state, RoomLevelData level)
    {
        QuadraticCurveSpeed curveSpeed = QuadraticSpeedCurvePlace.Min(state.YSpeedCurvePlace, PuyoHopDefinitions.FastestSpeedPlace).Speed;
        SignedSixteenSixteen verticalDisplacement = curveSpeed.Toward(state.Falling ? EnemySpeedSign.Positive : EnemySpeedSign.Negative);
        bool verticalCollision = _terrain.MoveEnemyVertically(level, place, verticalDisplacement);
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

        // Pose selection happens before the place changes, matching the five discrete
        // squash/stretch thresholds in the authored rather than interpolating sprite frames.
        SetPuyoAirborneAnimation(place, state);

        state.StepAirbornePlace(Hop(state).CurveStep);

        // X speed is authored with a fraction, but the Japanese and North American release moves it by the whole pixels only
        // and zeroes the subspeed; only the European release uses the fraction. The giant
        // hop's 1.25 therefore moves exactly one pixel per frame either way.
        int whole = Hop(state).XSpeed.WholePixels;
        if (state.Direction == PuyoDirection.Left)
            whole = -whole;
        SignedSixteenSixteen horizontalDisplacement = SignedSixteenSixteen.FromWholePixels(whole);

        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
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

    /// <summary>Selects the rising/falling one-frame pose for the current curve place.</summary>
    private static void SetPuyoAirborneAnimation(RoomEnemyPlace place, PuyoEnemyState state)
    {
        PuyoSegment segment;
        if (!state.Falling)
        {
            if (state.Direction == PuyoDirection.Right)
            {
                segment = state.YSpeedCurvePlace >=
                        state.InitialYSpeedCurvePlaceThreeQuarters
                    ? PuyoSegment.TiltedFarRight
                    : state.YSpeedCurvePlace >= state.InitialYSpeedCurvePlaceHalf
                        ? PuyoSegment.TiltedRight
                        : PuyoSegment.Upright;
            }
            else
            {
                segment = state.YSpeedCurvePlace >=
                        state.InitialYSpeedCurvePlaceThreeQuarters
                    ? PuyoSegment.TiltedFarLeft
                    : state.YSpeedCurvePlace >= state.InitialYSpeedCurvePlaceHalf
                        ? PuyoSegment.TiltedLeft
                        : PuyoSegment.Upright;
            }
        }
        else if (state.Direction == PuyoDirection.Right)
        {
            segment = state.YSpeedCurvePlace < state.InitialYSpeedCurvePlaceHalf
                ? PuyoSegment.Upright
                : state.YSpeedCurvePlace < state.InitialYSpeedCurvePlaceThreeQuarters
                    ? PuyoSegment.TiltedLeft
                    : PuyoSegment.TiltedFarLeft;
        }
        else
        {
            segment = state.YSpeedCurvePlace < state.InitialYSpeedCurvePlaceHalf
                ? PuyoSegment.Upright
                : state.YSpeedCurvePlace < state.InitialYSpeedCurvePlaceThreeQuarters
                    ? PuyoSegment.TiltedRight
                    : PuyoSegment.TiltedFarRight;
        }

        SetPuyoAnimation(place, state, segment);
    }

    /// <summary>Turns a Puyo to its other direction after a terrain impact.</summary>
    private static PuyoDirection Opposite(PuyoDirection direction) => direction switch
    {
        PuyoDirection.Right => PuyoDirection.Left,
        PuyoDirection.Left => PuyoDirection.Right,
        _ => throw new InvalidDataException(
            $"Puyo direction {direction} is neither of the two directions."),
    };

    /// <summary>Starts a selected pose or hop animation with a one-tick frame countdown.</summary>
    private static void SetPuyoAnimation(RoomEnemyPlace place, PuyoEnemyState state, PuyoSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, PuyoAnimations.StartingWith(segment));
    }

    /// <summary>Advances Puyo's typed animation.</summary>
    internal static void AdvancePuyoTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, PuyoAnimations.Segments,
            static (PuyoEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"Puyo's cue-free animation raised {cue}."));

    /// <summary>Runs Puyo's main AI.</summary>
    internal void RunPuyoMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunPuyoMain(place, place.RequireSpeciesState<PuyoEnemyState>(), frame.Samus, frame.Level);
    }
}

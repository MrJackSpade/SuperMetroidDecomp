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
    private static bool IsNormalHop(PuyoHopType type) => PuyoHopDefinitions.NormalHops.Contains(type);

    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.Puyo], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new PuyoEntity(this, occupant, place, common));

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
    internal static void InitializePuyo(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        // The setup shows the common empty frame before starting the fast grounded animation.
        place.Frame = EnemyFrameId.PointAtOrigin;

        var state = new PuyoEnemyState(PuyoAction.Grounded)
        {
            YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start,
            HopCooldownTimer = UpdateCountdown.For(occupant.SpawnSettings<PuyoSpawnSettings>().HopCooldown + EnemyWaitDefinitions.RunOutUpdate),
            CurrentHop = PuyoHopType.NormalSmall,
            HopType = PuyoHopType.NormalSmall,
            Direction = HorizontalDirection.Right,
            InitialYSpeedCurvePlaceThreeQuarters = QuadraticSpeedCurvePlace.Start,
            InitialYSpeedCurvePlaceHalf = QuadraticSpeedCurvePlace.Start,
        };
        place.SetSpeciesState(state);
        SetPuyoAnimation(place, state, PuyoSegment.GroundedFast);
    }

    /// <summary>What <c>RunPuyoMain</c> does for each <see cref="PuyoAction"/> member, each member's own arm.</summary>
    private IdentityTable<PuyoAction, Action<RoomEnemyOccupant, RoomEnemyPlace, PuyoEnemyState, SamusState, RoomLevelData>> RunPuyoMainForPuyoAction => field ??= new(new Dictionary<PuyoAction, Action<RoomEnemyOccupant, RoomEnemyPlace, PuyoEnemyState, SamusState, RoomLevelData>>
    {
        [PuyoAction.Grounded] = (occupant, place, state, samus, level) => RunGroundedPuyo(occupant, place, state, samus),
        [PuyoAction.Rising] = (_, place, state, samus, level) => RunAirbornePuyo(place, state, level, VerticalDirection.Up),
        [PuyoAction.Falling] = (_, place, state, samus, level) => RunAirbornePuyo(place, state, level, VerticalDirection.Down),
    });

    /// <summary>Runs a Puyo for one update and its top-level actions.</summary>
    private void RunPuyoMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        PuyoEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        RunPuyoMainForPuyoAction[state.Action](occupant, place, state, samus, level);
    }

    /// <summary>Runs the grounded Puyo.</summary>
    private void RunGroundedPuyo(RoomEnemyOccupant occupant, RoomEnemyPlace place, PuyoEnemyState state, SamusState samus)
    {
        // A placement's cooldown of N keeps the Puyo grounded for N updates and it hops on the next, so a zero cooldown hops
        // at once.
        if (!state.HopCooldownTimer.Step())
            return;

        // Every hop starts rising.
        state.Action = PuyoAction.Rising;
        state.HopCooldownTimer = UpdateCountdown.For(occupant.SpawnSettings<PuyoSpawnSettings>().HopCooldown + EnemyWaitDefinitions.RunOutUpdate);
        InitiatePuyoHop(occupant, place, state, samus);
    }

    /// <summary>Starts a hop: chooses the hop type and its random selection.</summary>
    private void InitiatePuyoHop(RoomEnemyOccupant occupant, RoomEnemyPlace place, PuyoEnemyState state, SamusState samus)
    {
        // Normal follow-up hops are promoted from the small hop to the big one when Samus is
        // strictly inside its horizontal hop distance. Giant/dropping/dropped types
        // deliberately bypass this proximity rewrite.
        if (IsNormalHop(state.HopType))
        {
            int distance = Math.Abs(samus.Position.X.Whole - place.Position.X.Whole);
            state.HopType = distance < occupant.SpawnSettings<PuyoSpawnSettings>().HopDistance
                ? PuyoHopType.NormalBig
                : PuyoHopType.NormalSmall;
        }

        // Aim toward Samus unless the terrain collision before turned the Puyo back; a Puyo level with Samus aims
        // right.
        HorizontalDirection towardSamus = samus.Position.X.Whole < place.Position.X.Whole
            ? HorizontalDirection.Left
            : HorizontalDirection.Right;
        state.Direction = state.TurnBack ?? towardSamus;
        state.TurnBack = null;

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
    /// height. The rise's half and three-quarter places, which pace its poses, are shares of that place. Every hop's search
    /// steps past the curve's start, as its definition requires; one that runs off the curve fails when its speed is read.
    /// </summary>
    /// <exception cref="InvalidDataException">The hop's search runs off the curve.</exception>
    private static void CalculateInitialPuyoHopSpeed(PuyoEnemyState state)
    {
        PuyoHopDefinition hop = Hop(state);
        SignedSixteenSixteen height = SignedSixteenSixteen.FromWholePixels(hop.JumpHeight);
        QuadraticSpeedCurvePlace place = QuadraticSpeedCurvePlace.Start;
        SignedSixteenSixteen risen = SignedSixteenSixteen.Zero;
        do
        {
            place += hop.LaunchSearchStep;
            risen += place.Speed.CoarseRise;
        }
        while (risen <= height);

        state.YSpeedCurvePlace = place;
        state.InitialYSpeedCurvePlaceHalf = place.DividedBy(2);
        state.InitialYSpeedCurvePlaceThreeQuarters = place.DividedBy(4) + state.InitialYSpeedCurvePlaceHalf;
    }

    /// <summary>The definition of the hop <paramref name="state"/> is making.</summary>
    private static PuyoHopDefinition Hop(PuyoEnemyState state) => PuyoHopDefinitions.Hops[state.CurrentHop];

    /// <summary>What <c>RunAirbornePuyo</c> does for each <see cref="PuyoAirborneMovement"/> member, each member's own arm.</summary>
    private IdentityTable<PuyoAirborneMovement, Action<RoomEnemyPlace, PuyoEnemyState, RoomLevelData, VerticalDirection>> RunAirbornePuyoForPuyoAirborneMovement => field ??= new(new Dictionary<PuyoAirborneMovement, Action<RoomEnemyPlace, PuyoEnemyState, RoomLevelData, VerticalDirection>>
    {
        [PuyoAirborneMovement.NormalShortHop] = (place, state, level, heading) =>
        {
            RunNormalPuyoHop(
                place,
                state,
                level,
                heading,
                PuyoSegment.GroundedSlow);
        },
        [PuyoAirborneMovement.NormalBigHop] = (place, state, level, heading) =>
        {
            RunNormalPuyoHop(
                place,
                state,
                level,
                heading,
                PuyoSegment.GroundedMedium);
        },
        [PuyoAirborneMovement.NormalLongHop] = (place, state, level, heading) =>
        {
            RunNormalPuyoHop(
                place,
                state,
                level,
                heading,
                PuyoSegment.GroundedFast);
        },
        [PuyoAirborneMovement.GiantHop] = (place, state, level, heading) => RunGiantPuyoHop(place, state, level, heading),
        [PuyoAirborneMovement.Dropping] = (place, state, level, heading) => RunDroppingPuyo(place, state, level),
        [PuyoAirborneMovement.Dropped] = (place, state, level, heading) => RunDroppedPuyo(place, state, level, heading),
    });

    /// <summary>Runs the airborne step of a Puyo rising or falling, the way <paramref name="heading"/> names.</summary>
    private void RunAirbornePuyo(
        RoomEnemyPlace place,
        PuyoEnemyState state,
        RoomLevelData level,
        VerticalDirection heading)
    {
        RunAirbornePuyoForPuyoAirborneMovement[state.AirborneMovement](place, state, level, heading);
    }

    /// <summary>Runs a short, big or long hop, which lands into the grounded animation the hop names.</summary>
    private void RunNormalPuyoHop(
        RoomEnemyPlace place,
        PuyoEnemyState state,
        RoomLevelData level,
        VerticalDirection heading,
        PuyoSegment landingSegment)
    {
        if (!MovePuyo(place, state, level, heading))
            return;

        SetPuyoAnimation(place, state, landingSegment);
    }

    /// <summary>Runs the Puyo's giant airborne hop.</summary>
    private void RunGiantPuyoHop(RoomEnemyPlace place, PuyoEnemyState state, RoomLevelData level, VerticalDirection heading)
    {
        if (!MovePuyo(place, state, level, heading))
            return;

        // A normal landing returns a giant hop to the small hop. A wall or ceiling collision that turns the Puyo back
        // leaves it dropping instead, its hop and its airborne behaviour the drop's.
        if (state.TurnBack is null)
        {
            state.HopType = PuyoHopType.NormalSmall;
            state.Action = PuyoAction.Grounded;
        }

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

    /// <summary>Runs the bounce after the drop.</summary>
    private void RunDroppedPuyo(RoomEnemyPlace place, PuyoEnemyState state, RoomLevelData level, VerticalDirection heading)
    {
        if (!MovePuyo(place, state, level, heading))
            return;

        state.HopType = PuyoHopType.Giant;
        state.Action = PuyoAction.Grounded;
        SetPuyoAnimation(place, state, PuyoSegment.GroundedSlow);
    }

    /// <summary>
    /// Moves the airborne Puyo one update along its hop, and whether the hop met terrain (#2450): a landing grounds it, and
    /// a ceiling or a wall starts its drop. Until it meets terrain it keeps its hopping poses.
    /// </summary>
    private bool MovePuyo(RoomEnemyPlace place, PuyoEnemyState state, RoomLevelData level, VerticalDirection heading)
    {
        QuadraticCurveSpeed curveSpeed = QuadraticSpeedCurvePlace.Min(state.YSpeedCurvePlace, PuyoHopDefinitions.FastestSpeedPlace).Speed;
        SignedSixteenSixteen verticalDisplacement = curveSpeed.Toward(heading);
        bool verticalCollision = _terrain.MoveEnemyVertically(level, place, verticalDisplacement);
        if (verticalCollision)
        {
            if (heading == VerticalDirection.Up)
            {
                // Striking a ceiling while rising starts the drop; whether the next hop turns back is what the room's
                // last draw and HUD redraw left (PuyoCeilingTurnFromLastDraw): always in an ordinary gameplay update.
                state.TurnBack = _ceilingTurn.TurnsBack ? state.Direction.Reversed() : null;
                state.HopType = PuyoHopType.Dropping;
                state.AirborneMovement = PuyoAirborneMovement.Dropping;
            }
            else
            {
                state.Action = PuyoAction.Grounded;
            }
            return true;
        }

        // The pose follows the curve place before this update's step moves it on: upright, tilted, or tilted far as the
        // place passes half and three quarters of the place the rise started from.
        SetPuyoAirborneAnimation(place, state, heading);

        state.StepAirbornePlace(Hop(state).CurveStep, heading);

        // The hop moves sideways by the whole pixels of its horizontal speed only, so the giant hop's one and a quarter
        // moves exactly one pixel each update.
        SignedSixteenSixteen horizontalDisplacement = SignedSixteenSixteen.FromWholePixels(Hop(state).XPixels.Toward(state.Direction));

        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                horizontalDisplacement))
        {
            return false;
        }

        state.TurnBack = state.Direction.Reversed();
        state.HopType = PuyoHopType.Dropping;
        state.AirborneMovement = PuyoAirborneMovement.Dropping;
        return true;
    }

    /// <summary>Selects the rising/falling one-frame pose for the current curve place.</summary>
    private static void SetPuyoAirborneAnimation(RoomEnemyPlace place, PuyoEnemyState state, VerticalDirection heading)
    {
        // A rising Puyo leans the way it hops and a falling one the other way, further the further its fall curve has run.
        bool leansRight = (heading == VerticalDirection.Up) == (state.Direction == HorizontalDirection.Right);
        PuyoSegment segment = state.YSpeedCurvePlace >= state.InitialYSpeedCurvePlaceThreeQuarters
            ? leansRight ? PuyoSegment.TiltedFarRight : PuyoSegment.TiltedFarLeft
            : state.YSpeedCurvePlace >= state.InitialYSpeedCurvePlaceHalf
                ? leansRight ? PuyoSegment.TiltedRight : PuyoSegment.TiltedLeft
                : PuyoSegment.Upright;

        SetPuyoAnimation(place, state, segment);
    }

    /// <summary>Starts a selected pose or hop animation on this update.</summary>
    private static void SetPuyoAnimation(RoomEnemyPlace place, PuyoEnemyState state, PuyoSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, PuyoAnimations.StartingWith(segment));
    }

    /// <summary>Advances Puyo's typed animation.</summary>
    internal static void AdvancePuyoTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.AdvanceSilently(place, PuyoAnimations.Segments,
            static (PuyoEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback);

    /// <summary>Runs Puyo's main AI.</summary>
    internal void RunPuyoMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunPuyoMain(occupant, place, place.RequireSpeciesState<PuyoEnemyState>(), frame.Samus, frame.Level);
    }
}

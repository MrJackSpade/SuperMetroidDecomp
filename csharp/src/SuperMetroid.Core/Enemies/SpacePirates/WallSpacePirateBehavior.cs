using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The wall space pirate species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class WallSpacePirateBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.PirateGoldWall, EnemyDefinitionId.PirateGreyWall, EnemyDefinitionId.PirateMagentaWall, EnemyDefinitionId.PirateSilverWall], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new WallSpacePirateEntity(this, occupant, place, common));

    /// <summary>Advances the shared random source when a climbing cue chooses a new direction.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Clamps vertical climbing to room blocks before a collision reverses direction.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>The owner of the shared Pirate contact reactions, which every Pirate family takes.</summary>
    internal WalkingSpacePirateBehavior Walking { get; }

    /// <summary>Binds wall climbing, jump arcs, and family combat reactions to room services.</summary>
    internal WallSpacePirateBehavior(EnemyRoomBinding room, EnemyTerrain terrain, WalkingSpacePirateBehavior walkingSpacePirateBehavior)
    {
        Walking = walkingSpacePirateBehavior;
        _room = room;
        _terrain = terrain;
    }

    /// <summary>Initializes the wall Space Pirate.</summary>
    internal static void InitializeWallSpacePirate(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        WallSpacePirateSpawnSettings settings = occupant.SpawnSettings<WallSpacePirateSpawnSettings>();
        // A slow jump and a fast jump each turn by their definition's steps, the fast one twice as quickly to endpoints
        // a little further round.
        WallSpacePirateJump jump = settings.JumpSpeed == WallSpacePirateJumpSpeed.Slow
            ? SpacePirateMotionDefinitions.SlowWallJump : SpacePirateMotionDefinitions.FastWallJump;
        var state = new WallSpacePirateEnemyState(WallSpacePirateAction.Climbing, settings.Wall, jump);
        place.SetSpeciesState(state);

        state.Playback = new(WallSpacePirateAnimations.StartingWith(settings.Wall == HorizontalDirection.Right
            ? WallSpacePirateSegment.MovingDownRightWall
            : WallSpacePirateSegment.MovingDownLeftWall), AnimationPlace.StartOfAnimation);
        state.ClimbDirection = WallSpacePirateClimbDirection.Down;

        SnapWallSpacePirateXToWallGrid(place);
    }

    /// <summary>What <c>RunWallSpacePirateMain</c> does for each <see cref="WallSpacePirateAction"/> member, each member's own arm.</summary>
    private static IdentityTable<WallSpacePirateAction, Action<RoomEnemyOccupant, RoomEnemyPlace, WallSpacePirateEnemyState, SamusState>> RunWallSpacePirateMainForWallSpacePirateAction => field ??= new(new Dictionary<WallSpacePirateAction, Action<RoomEnemyOccupant, RoomEnemyPlace, WallSpacePirateEnemyState, SamusState>>
    {
        [WallSpacePirateAction.Climbing] = static (_, place, state, samus) =>
        {
            // A Pirate on the left wall fires and jumps right, and one on the right wall left.
            if (WallSpacePirateCanAttack(place, samus))
            {
                StartWallSpacePirateAnimation(
                    place,
                    state,
                    state.Wall == HorizontalDirection.Left
                        ? WallSpacePirateSegment.FireAndJumpRight
                        : WallSpacePirateSegment.FireAndJumpLeft);
            }
        },
        [WallSpacePirateAction.Attacking] = static (_, place, state, samus) =>
        {
            // The attack animation moves and times the pirate; its cues ready the jump.
        },
        [WallSpacePirateAction.WallJumping] = static (occupant, place, state, samus) => StepWallSpacePirateJump(occupant, place, state, direction: state.Wall),
    });

    /// <summary>Runs a wall Space Pirate's update: climbing its wall, attacking from it or jumping to the other.</summary>
    private static void RunWallSpacePirateMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        WallSpacePirateEnemyState state,
        SamusState samus)
    {
        RunWallSpacePirateMainForWallSpacePirateAction[state.Action](occupant, place, state, samus);
    }

    /// <summary>
    /// Moves along the jump arc (the angle falls jumping right and rises jumping left) and starts the landing animation for
    /// the wall reached at that side's target angle.
    /// </summary>
    private static void StepWallSpacePirateJump(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        WallSpacePirateEnemyState state,
        HorizontalDirection direction)
    {
        PositionWallSpacePirateOnArc(occupant, place, state);
        state.WallJumpArcAngle = state.WallJumpArcAngle.TurnedBySteps(state.Jump.StepsPerUpdate.Toward(direction.Reversed()));
        if (state.WallJumpArcAngle != (direction == HorizontalDirection.Right ? state.Jump.RightTarget : state.Jump.LeftTarget))
            return;

        StartWallSpacePirateAnimation(
            place,
            state,
            direction == HorizontalDirection.Right ? WallSpacePirateSegment.LandedOnRightWall : WallSpacePirateSegment.LandedOnLeftWall);
        SnapWallSpacePirateXToWallGrid(place);
    }

    /// <summary>Uses sine and cosine products to place the Pirate on a jump ellipse sized by its jump span.</summary>
    private static void PositionWallSpacePirateOnArc(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        WallSpacePirateEnemyState state)
    {
        // The arc places the Pirate's whole pixels each update; its fractions carry over. The jump ellipse is the jump span wide and
        // a quarter of it tall.
        int span = occupant.SpawnSettings<WallSpacePirateSpawnSettings>().JumpSpan;
        place.Position = place.Position.WithWholeX(
            state.WallJumpArcCenter.X +
            EnemyMath.CircleNegativeSinePixels(state.WallJumpArcAngle.ToTrigonometryAngle(), span / 2));
        place.Position = place.Position.WithWholeY(
            state.WallJumpArcCenter.Y -
            EnemyMath.CircleCosinePixels(state.WallJumpArcAngle.ToTrigonometryAngle(), span / 4));
    }

    /// <summary>Allows the fire-and-jump sequence when Samus is within the strict vertical detection band.</summary>
    private static bool WallSpacePirateCanAttack(RoomEnemyPlace place, SamusState samus) =>
        Math.Abs(samus.Position.Y.Whole - place.Position.Y.Whole) <
            SpacePirateMotionDefinitions.WallDetectionBand;

    /// <summary>Applies the split alignment rule when a Pirate starts or lands on a wall.</summary>
    private static void SnapWallSpacePirateXToWallGrid(RoomEnemyPlace place)
    {
        // The wall alignment deliberately has two alignment grids. Offsets within a block below the forward snap round
        // down to a half-block boundary; the rest round up to the next block's start.
        int x = place.Position.X.Whole;
        place.Position = place.Position.WithWholeX(RoomGridConversions.OffsetWithinBlock(x) < SpacePirateMotionDefinitions.WallSnapForwardFrom
            ? RoomGridConversions.HalfBlockStart(x)
            : RoomGridConversions.NextBlockStart(x));
    }

    /// <summary>Starts a selected wall-Pirate animation on its first frame.</summary>
    private static void StartWallSpacePirateAnimation(
        RoomEnemyPlace place,
        WallSpacePirateEnemyState state,
        WallSpacePirateSegment segment)
    {
        state.Playback = new(WallSpacePirateAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Climbs, or after terrain contact returns the opposite climb to continue at; null runs on.</summary>
    private WallSpacePirateSegment? MoveWallSpacePirateAndReverseOnCollision(
        RoomEnemyPlace place,
        WallSpacePirateEnemyState state,
        RoomLevelData level,
        int downwardPixels,
        HorizontalDirection wall)
    {
        SignedSixteenSixteen displacement = SignedSixteenSixteen.FromWholePixels(downwardPixels);
        if (!_terrain.MoveEnemyVertically(level, place, displacement))
            return null;

        // A collision keeps the Pirate where the wall stopped it, flips only its climb direction,
        // and continues at a fresh climb. It does not resume after the cue.
        state.ClimbDirection = state.ClimbDirection == WallSpacePirateClimbDirection.Down
            ? WallSpacePirateClimbDirection.Up
            : WallSpacePirateClimbDirection.Down;
        return ClimbAnimation(new WallSpacePirateClimb(wall, state.ClimbDirection));
    }

    /// <summary>Consumes one random value and returns the chosen up or down climb to continue at.</summary>
    private WallSpacePirateSegment RandomizeWallSpacePirateClimbDirection(
        WallSpacePirateEnemyState state,
        HorizontalDirection wall)
    {
        state.ClimbDirection = SpacePirateDecisions.ClimbsUp.IsTakenBy(_room.Random.Generate())
            ? WallSpacePirateClimbDirection.Up
            : WallSpacePirateClimbDirection.Down;
        return ClimbAnimation(new WallSpacePirateClimb(wall, state.ClimbDirection));
    }

    /// <summary>The animation of <paramref name="climb"/>: the way it climbs, on the wall it clings to.</summary>
    private static WallSpacePirateSegment ClimbAnimation(WallSpacePirateClimb climb) => climb.Wall == HorizontalDirection.Left
        ? (climb.Direction == WallSpacePirateClimbDirection.Down ? WallSpacePirateSegment.MovingDownLeftWall : WallSpacePirateSegment.MovingUpLeftWall)
        : (climb.Direction == WallSpacePirateClimbDirection.Down ? WallSpacePirateSegment.MovingDownRightWall : WallSpacePirateSegment.MovingUpRightWall);

    /// <summary>Captures the wall-to-wall destination, arc center, and starting angle from the jump span.</summary>
    private static void PrepareWallSpacePirateJump(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        WallSpacePirateEnemyState state,
        HorizontalDirection direction)
    {
        int span = occupant.SpawnSettings<WallSpacePirateSpawnSettings>().JumpSpan;
        int signedHalfSpan = (span / 2).Toward(direction);
        state.WallJumpArcCenter = new Position16<RoomPixels>(
            place.Position.X.Whole + signedHalfSpan,
            place.Position.Y.Whole);
        state.WallJumpArcAngle = direction == HorizontalDirection.Right ? RotationAngle.QuarterTurn : RotationAngle.ThreeQuarterTurn;
    }

    /// <summary>Emits a wall laser through the shared Pirate owner and queues sound only if allocation succeeds.</summary>
    private void FireWallSpacePirateLaser(
        RoomEnemyOccupant occupant,
        HorizontalDirection direction)
    {
        // The laser setup queues the laser sound for every successful allocation. The later wall-jump cue
        // independently queues the wall-jump sound; both are observable, distinct requests.
        if (Walking.SpawnSpacePirateLaser(occupant, direction, SpacePirateMotionDefinitions.WallLaserMuzzleAbove,
            occupant.SpawnSettings<WallSpacePirateSpawnSettings>().LaserSpeed))
            Walking.RequestSpacePirateSound(EnemySoundEffectDefinitions.WallPirateLaserSound);
    }

    /// <summary>
    /// Carries out one cue of the wall Space Pirates' animations. A climb continues at the opposite climb after terrain
    /// contact and a random direction choice always continues at the chosen climb, as the cues return those
    /// animations; every other cue runs on.
    /// </summary>
    private WallSpacePirateSegment? ApplyWallSpacePirateCue(RoomEnemyOccupant occupant, RoomEnemyPlace place, WallSpacePirateCue cue, RoomLevelData level)
    {
        WallSpacePirateEnemyState state = place.RequireSpeciesState<WallSpacePirateEnemyState>();
        return cue.Match(ApplyWallSpacePirateCueCases, new ApplyWallSpacePirateCueContext(occupant, place, state, level, this));
    }

    /// <summary>What <c>ApplyWallSpacePirateCue</c> does on each <see cref="WallSpacePirateCue"/>, built once (#2553).</summary>
    private static WallSpacePirateCueCases<ApplyWallSpacePirateCueContext, WallSpacePirateSegment?> ApplyWallSpacePirateCueCases { get; } = new(
        Climb: static (climb, context) => context.Owner.MoveWallSpacePirateAndReverseOnCollision(context.Place, context.State, context.Level, climb.ClimbPixels, wall: climb.Wall),
        ChooseClimb: static (choose, context) => context.Owner.RandomizeWallSpacePirateClimbDirection(context.State, wall: choose.Wall),
        FireLaser: static (laser, context) =>
        {
            context.Owner.FireWallSpacePirateLaser(context.Occupant, direction: laser.Direction);
            return null;
        },
        PrepareJump: static (jump, context) =>
        {
            PrepareWallSpacePirateJump(context.Occupant, context.Place, context.State, direction: jump.Direction);
            return null;
        },
        Begin: static (begin, context) =>
        {
            context.State.Action = begin.Action;
            return null;
        },
        BeginOnWall: static (begin, context) =>
        {
            context.State.Action = begin.Action;
            context.State.Wall = begin.Wall;
            return null;
        },
        // The wall pirates' one signal plays their jump's sound.
        Signal: static (_, context) =>
        {
            context.Owner.Walking.RequestSpacePirateSound(EnemySoundEffectDefinitions.WallPirateJumpSound);
            return null;
        });

    /// <summary>What <c>ApplyWallSpacePirateCue</c>'s cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Occupant">The occupant whose cue is carried out.</param>
    /// <param name="Place">The <c>place</c> <c>ApplyWallSpacePirateCue</c> was handed.</param>
    /// <param name="State">The <c>state</c> <c>ApplyWallSpacePirateCue</c> works with.</param>
    /// <param name="Level">The <c>level</c> <c>ApplyWallSpacePirateCue</c> was handed.</param>
    /// <param name="Owner">The <see cref="WallSpacePirateBehavior"/> reacting.</param>
    private readonly record struct ApplyWallSpacePirateCueContext(RoomEnemyOccupant Occupant, RoomEnemyPlace Place, WallSpacePirateEnemyState State, RoomLevelData Level, WallSpacePirateBehavior Owner);

    /// <summary>Runs the wall Space Pirate's main AI.</summary>
    internal static void RunWallSpacePirateMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunWallSpacePirateMain(
            occupant, place,
            place.RequireSpeciesState<WallSpacePirateEnemyState>(),
            frame.Samus);
    }

    /// <summary>Advances the wall Space Pirate's typed animation.</summary>
    internal void AdvanceWallSpacePirateTypedAnimation(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(place, WallSpacePirateAnimations.Segments,
            static (WallSpacePirateEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyWallSpacePirateCue(occupant, place, cue, context.Level) is { } continuesAt
                ? new AnimationContinuation<WallSpacePirateSegment>(WallSpacePirateAnimations.StartingWith(continuesAt), AnimationPlace.StartOfAnimation)
                : null);
}

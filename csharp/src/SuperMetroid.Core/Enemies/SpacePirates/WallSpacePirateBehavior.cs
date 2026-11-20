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
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.PirateGoldWall, EnemyDefinitionId.PirateGreyWall, EnemyDefinitionId.PirateMagentaWall, EnemyDefinitionId.PirateSilverWall];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new WallSpacePirateEntity(this, place, common));

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
    internal static void InitializeWallSpacePirate(RoomEnemyPlace place)
    {
        WallSpacePirateSpawnSettings settings = place.SpawnSettings<WallSpacePirateSpawnSettings>();
        // A slow jump and a fast jump each turn by their definition's steps, the fast one twice as quickly to endpoints
        // a little further round.
        WallSpacePirateJump jump = settings.JumpSpeed switch
        {
            WallSpacePirateJumpSpeed.Slow => SpacePirateMotionDefinitions.SlowWallJump,
            WallSpacePirateJumpSpeed.Fast => SpacePirateMotionDefinitions.FastWallJump,
            _ => throw new InvalidDataException($"Undefined {nameof(WallSpacePirateJumpSpeed)} {settings.JumpSpeed}."),
        };
        var state = new WallSpacePirateEnemyState(WallSpacePirateAction.Climbing, settings.Wall, jump);
        place.SetSpeciesState(state);

        state.Playback = new(WallSpacePirateAnimations.StartingWith(settings.Wall == HorizontalDirection.Right
            ? WallSpacePirateSegment.MovingDownRightWall
            : WallSpacePirateSegment.MovingDownLeftWall), AnimationPlace.StartOfAnimation);
        state.ClimbDirection = WallSpacePirateClimbDirection.Down;

        SnapWallSpacePirateXToWallGrid(place);
    }

    /// <summary>Runs a wall Space Pirate's update: climbing its wall, attacking from it or jumping to the other.</summary>
    private static void RunWallSpacePirateMain(
        RoomEnemyPlace place,
        WallSpacePirateEnemyState state,
        SamusState samus)
    {
        switch (state.Action)
        {
            case WallSpacePirateAction.Climbing:
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
                return;

            case WallSpacePirateAction.Attacking:
                // The attack animation moves and times the pirate; its cues ready the jump.
                return;

            case WallSpacePirateAction.WallJumping:
                StepWallSpacePirateJump(place, state, direction: state.Wall);
                return;

            default:
                throw new InvalidDataException(
                    $"Wall Space Pirate has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>
    /// Moves along the jump arc (the angle falls jumping right and rises jumping left) and starts the landing animation for
    /// the wall reached at that side's target angle.
    /// </summary>
    private static void StepWallSpacePirateJump(
        RoomEnemyPlace place,
        WallSpacePirateEnemyState state,
        HorizontalDirection direction)
    {
        PositionWallSpacePirateOnArc(place, state);
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
        RoomEnemyPlace place,
        WallSpacePirateEnemyState state)
    {
        // Math rewrites both whole positions every frame and leaves
        // the fractions untouched. The jump ellipse is the jump span wide and
        // a quarter of it tall.
        int span = place.SpawnSettings<WallSpacePirateSpawnSettings>().JumpSpan;
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
        // The wall alignment deliberately has two alignment grids. Offsets 0..10 within a block round down to an
        // eight-pixel boundary; 11..15 round up to the next block's start.
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
    private static WallSpacePirateSegment ClimbAnimation(WallSpacePirateClimb climb) => climb switch
    {
        (HorizontalDirection.Left, WallSpacePirateClimbDirection.Down) =>
            WallSpacePirateSegment.MovingDownLeftWall,
        (HorizontalDirection.Left, WallSpacePirateClimbDirection.Up) =>
            WallSpacePirateSegment.MovingUpLeftWall,
        (HorizontalDirection.Right, WallSpacePirateClimbDirection.Down) =>
            WallSpacePirateSegment.MovingDownRightWall,
        (HorizontalDirection.Right, WallSpacePirateClimbDirection.Up) =>
            WallSpacePirateSegment.MovingUpRightWall,
        _ => throw new InvalidOperationException(
            $"Wall Pirate climb {climb} names no wall or climb direction."),
    };

    /// <summary>Captures the wall-to-wall destination, arc center, and starting angle from the jump span.</summary>
    private static void PrepareWallSpacePirateJump(
        RoomEnemyPlace place,
        WallSpacePirateEnemyState state,
        HorizontalDirection direction)
    {
        int span = place.SpawnSettings<WallSpacePirateSpawnSettings>().JumpSpan;
        int signedHalfSpan = (span / 2).Toward(direction);
        state.WallJumpArcCenter = new Position16<RoomPixels>(
            place.Position.X.Whole + signedHalfSpan,
            place.Position.Y.Whole);
        state.WallJumpArcAngle = direction == HorizontalDirection.Right ? RotationAngle.QuarterTurn : RotationAngle.ThreeQuarterTurn;
    }

    /// <summary>Emits a wall laser through the shared Pirate owner and queues sound only if allocation succeeds.</summary>
    private void FireWallSpacePirateLaser(
        RoomEnemyPlace place,
        HorizontalDirection direction)
    {
        // The laser setup queues the laser sound for every successful allocation. The later wall-jump cue
        // independently queues the wall-jump sound; both are observable, distinct requests.
        if (Walking.SpawnSpacePirateLaser(place, direction, SpacePirateMotionDefinitions.WallLaserMuzzleAbove,
            place.SpawnSettings<WallSpacePirateSpawnSettings>().LaserSpeed))
            Walking.RequestSpacePirateSound(EnemySoundEffectDefinitions.WallPirateLaserSound);
    }

    /// <summary>
    /// Carries out one cue of the wall Space Pirates' animations. A climb continues at the opposite climb after terrain
    /// contact and a random direction choice always continues at the chosen climb, as the cues return those
    /// animations; every other cue runs on.
    /// </summary>
    private WallSpacePirateSegment? ApplyWallSpacePirateCue(RoomEnemyPlace place, WallSpacePirateCue cue, RoomLevelData level)
    {
        WallSpacePirateEnemyState state = place.RequireSpeciesState<WallSpacePirateEnemyState>();
        switch (cue)
        {
            case WallSpacePirateCue.Climb climb:
                return MoveWallSpacePirateAndReverseOnCollision(place, state, level, climb.ClimbPixels, wall: climb.Wall);
            case WallSpacePirateCue.ChooseClimb choose:
                return RandomizeWallSpacePirateClimbDirection(state, wall: choose.Wall);
            case WallSpacePirateCue.PrepareJump jump:
                PrepareWallSpacePirateJump(place, state, direction: jump.Direction);
                return null;
            case WallSpacePirateCue.FireLaser laser:
                FireWallSpacePirateLaser(place, direction: laser.Direction);
                return null;
            case WallSpacePirateCue.Begin begin:
                state.Action = begin.Action;
                return null;
            case WallSpacePirateCue.BeginOnWall begin:
                state.Action = begin.Action;
                state.Wall = begin.Wall;
                return null;
            case WallSpacePirateCue.Signal { Kind: WallSpacePirateCueKind.PlayJumpSound }:
                Walking.RequestSpacePirateSound(EnemySoundEffectDefinitions.WallPirateJumpSound);
                return null;
            default:
                throw new InvalidOperationException($"The wall Space Pirates' animations have no {cue} cue.");
        }
    }

    /// <summary>Runs the wall Space Pirate's main AI.</summary>
    internal static void RunWallSpacePirateMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunWallSpacePirateMain(
            place,
            place.RequireSpeciesState<WallSpacePirateEnemyState>(),
            frame.Samus);
    }

    /// <summary>Advances the wall Space Pirate's typed animation.</summary>
    internal void AdvanceWallSpacePirateTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(place, WallSpacePirateAnimations.Segments,
            static (WallSpacePirateEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyWallSpacePirateCue(place, cue, context.Level) is { } continuesAt
                ? new AnimationContinuation<WallSpacePirateSegment>(WallSpacePirateAnimations.StartingWith(continuesAt), AnimationPlace.StartOfAnimation)
                : null);
}

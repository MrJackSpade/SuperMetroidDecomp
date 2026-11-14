using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new WallSpacePirateEntity(this, slot, common);

    /// <summary>Advances the shared random source when a climbing cue chooses a new direction.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Clamps vertical climbing to room blocks before a collision reverses direction.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Shares laser spawning and hitbox-shot behavior with walking Pirates.</summary>
    private readonly WalkingSpacePirateBehavior _walkingSpacePirateBehavior;

    /// <summary>The owner of the shared Pirate contact reactions, which every Pirate family takes.</summary>
    internal WalkingSpacePirateBehavior Walking => _walkingSpacePirateBehavior;

    /// <summary>Binds wall climbing, jump arcs, and family combat reactions to room services.</summary>
    internal WallSpacePirateBehavior(EnemyRoomBinding room, EnemyTerrain terrain, WalkingSpacePirateBehavior walkingSpacePirateBehavior)
    {
        _walkingSpacePirateBehavior = walkingSpacePirateBehavior;
        _room = room;
        _terrain = terrain;
    }

    /// <summary>Vertical distance below which a climbing Pirate starts its fire-and-jump animation.</summary>
    private const int WallPirateSamusDetectionBand = 32;
    /// <summary>Pixels above the actor origin where the shared Pirate laser is emitted.</summary>
    private const int WallPirateLaserMuzzleYOffset = 16;

    /// <summary>Recognizes the four color variants that use wall-climbing Pirate AI.</summary>
    internal static bool IsWallSpacePirateDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.PirateGreyWall or
        EnemyDefinitionId.PirateGoldWall or
        EnemyDefinitionId.PirateMagentaWall or
        EnemyDefinitionId.PirateSilverWall;

    /// <summary>Groups wall, walking, and Ninja Pirates that share ordinary Pirate combat reactions.</summary>
    internal static bool IsOrdinarySpacePirateDefinition(EnemyDefinitionId definition) =>
        IsWallSpacePirateDefinition(definition) ||
        WalkingSpacePirateBehavior.IsWalkingSpacePirateDefinition(definition) ||
        NinjaSpacePirateBehavior.IsNinjaSpacePirateDefinition(definition);

    /// <summary>Initializes the wall Space Pirate.</summary>
    internal static void InitializeWallSpacePirate(RoomEnemySlot slot)
    {
        var state = new WallSpacePirateEnemyState();
        slot.SetSpeciesState(state);

        WallSpacePirateOptions flags = slot.SpawnSettings<WallSpacePirateSpawnSettings>().Options;
        bool startsOnRight = (flags & WallSpacePirateOptions.StartsOnRightWall) != WallSpacePirateOptions.None;
        bool slowJump = (flags & WallSpacePirateOptions.SlowJumpAndLaser) != WallSpacePirateOptions.None;

        state.Playback = new(WallSpacePirateAnimations.StartingWith(startsOnRight
            ? WallSpacePirateSegment.MovingDownRightWall
            : WallSpacePirateSegment.MovingDownLeftWall), AnimationPlace.StartOfAnimation);
        state.Action = startsOnRight
            ? WallSpacePirateAction.ClimbingRightWall
            : WallSpacePirateAction.ClimbingLeftWall;
        state.ClimbDirection = WallSpacePirateClimbDirection.Down;

        // The USA/Japan build measures angles in 256ths of a turn. A slow jump takes 63 two-step frames; a fast
        // one takes 32 four-step frames with adjusted endpoints. These values are not approximations: they are the
        // literal 190/66/2 and 192/64/4 branches.
        state.RightJumpTargetAngle = RotationAngle.FromSteps(slowJump ? 190 : 192);
        state.LeftJumpTargetAngle = RotationAngle.FromSteps(slowJump ? 66 : 64);
        state.JumpAngleDelta = slowJump ? 2 : 4;

        SnapWallSpacePirateXToWallGrid(slot);
    }

    /// <summary>Runs a wall Space Pirate's per-frame behaviour and its six actions.</summary>
    private static void RunWallSpacePirateMain(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        SamusState? samus)
    {
        switch (state.Action)
        {
            case WallSpacePirateAction.ClimbingLeftWall:
                if (WallSpacePirateCanAttack(slot, RequireSamus()))
                {
                    StartWallSpacePirateAnimation(
                        slot,
                        state,
                        WallSpacePirateSegment.FireAndJumpRight);
                }
                return;

            case WallSpacePirateAction.ClimbingRightWall:
                if (WallSpacePirateCanAttack(slot, RequireSamus()))
                {
                    StartWallSpacePirateAnimation(
                        slot,
                        state,
                        WallSpacePirateSegment.FireAndJumpLeft);
                }
                return;

            case WallSpacePirateAction.Attacking:
                // The attack animation moves and times the pirate; its cues ready the jump.
                return;

            case WallSpacePirateAction.WallJumpingRight:
                StepWallSpacePirateJump(slot, state, direction: HorizontalDirection.Right);
                return;

            case WallSpacePirateAction.WallJumpingLeft:
                StepWallSpacePirateJump(slot, state, direction: HorizontalDirection.Left);
                return;

            default:
                throw new InvalidDataException(
                    $"Wall Space Pirate has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }

        SamusState RequireSamus() => samus ?? throw new InvalidOperationException(
            "Wall Space Pirate detection requires the active Samus position.");
    }

    /// <summary>
    /// Moves along the jump arc (the angle falls jumping right and rises jumping left) and starts the landing animation for
    /// the wall reached at that side's target angle.
    /// </summary>
    private static void StepWallSpacePirateJump(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        HorizontalDirection direction)
    {
        PositionWallSpacePirateOnArc(slot, state);
        state.WallJumpArcAngle = state.WallJumpArcAngle.TurnedBySteps(direction == HorizontalDirection.Right ? -state.JumpAngleDelta : state.JumpAngleDelta);
        if (state.WallJumpArcAngle != (direction == HorizontalDirection.Right ? state.RightJumpTargetAngle : state.LeftJumpTargetAngle))
            return;

        StartWallSpacePirateAnimation(
            slot,
            state,
            direction == HorizontalDirection.Right ? WallSpacePirateSegment.LandedOnRightWall : WallSpacePirateSegment.LandedOnLeftWall);
        SnapWallSpacePirateXToWallGrid(slot);
    }

    /// <summary>Uses sine and cosine products to place the Pirate on a jump ellipse sized by its jump span.</summary>
    private static void PositionWallSpacePirateOnArc(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state)
    {
        // Math rewrites both whole positions every frame and leaves
        // the fractions untouched. The jump ellipse is the jump span wide and
        // a quarter of it tall.
        slot.Position = slot.Position.WithWrappedWholeX(
            state.WallJumpArcCenter.X +
            EnemyMath.ReadEightBitNegativeSineProduct(
                state.WallJumpArcAngle.ToTrigonometryAngle(),
                CoordinateAxis.Wrap(slot.SpawnSettings<WallSpacePirateSpawnSettings>().JumpSpan >> 1)));
        slot.Position = slot.Position.WithWrappedWholeY(
            state.WallJumpArcCenter.Y -
            EnemyMath.ReadEightBitCosineProduct(state.WallJumpArcAngle.ToTrigonometryAngle(), CoordinateAxis.Wrap(slot.SpawnSettings<WallSpacePirateSpawnSettings>().JumpSpan >> 2)));
    }

    /// <summary>Allows the fire-and-jump sequence when Samus is within the strict vertical detection band.</summary>
    private static bool WallSpacePirateCanAttack(RoomEnemySlot slot, SamusState samus) =>
        CoordinateAxis.Distance(samus.Position.Y.Whole, slot.Position.Y.Whole) <
            WallPirateSamusDetectionBand;

    /// <summary>Applies the split alignment rule when a Pirate starts or lands on a wall.</summary>
    private static void SnapWallSpacePirateXToWallGrid(RoomEnemySlot slot)
    {
        // The wall alignment deliberately has two alignment grids. Offsets 0..10 within a block round down to an
        // eight-pixel boundary; 11..15 round up to the next block's start.
        int x = slot.Position.X.Whole;
        slot.Position = slot.Position.WithWholeX(RoomGridConversions.OffsetWithinBlock(x) < 11
            ? RoomGridConversions.HalfBlockStart(x)
            : CoordinateAxis.Wrap(RoomGridConversions.BlockStart(x) + 16));
    }

    /// <summary>Starts a selected wall-Pirate animation on its first frame.</summary>
    private static void StartWallSpacePirateAnimation(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        WallSpacePirateSegment segment)
    {
        state.Playback = new(WallSpacePirateAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Climbs, or after terrain contact returns the opposite climb to continue at; null runs on.</summary>
    private WallSpacePirateSegment? MoveWallSpacePirateAndReverseOnCollision(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        RoomLevelData? level,
        int downwardPixels,
        HorizontalDirection wall)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                "Wall Space Pirate climbing requires room collision data.");
        }

        SignedSixteenSixteen displacement = SignedSixteenSixteen.FromWholePixels(downwardPixels);
        if (!_terrain.MoveEnemyVertically(level, slot, displacement))
            return null;

        // A collision retains the collision helper's clamped position, flips only its climb direction,
        // and continues at a fresh climb. It does not resume after the cue.
        state.ClimbDirection = state.ClimbDirection == WallSpacePirateClimbDirection.Down
            ? WallSpacePirateClimbDirection.Up
            : WallSpacePirateClimbDirection.Down;
        return SelectWallSpacePirateClimbList(wall == HorizontalDirection.Right, state.ClimbDirection);
    }

    /// <summary>Consumes one random value and returns the chosen up or down climb to continue at.</summary>
    private WallSpacePirateSegment RandomizeWallSpacePirateClimbDirection(
        WallSpacePirateEnemyState state,
        HorizontalDirection wall)
    {
        state.ClimbDirection = SpacePirateDecisions.ClimbUp.IsSetIn(_room.Random.Generate())
            ? WallSpacePirateClimbDirection.Up
            : WallSpacePirateClimbDirection.Down;
        return SelectWallSpacePirateClimbList(wall == HorizontalDirection.Right, state.ClimbDirection);
    }

    /// <summary>Selects the climb animation for a wall side and vertical climb direction.</summary>
    private static WallSpacePirateSegment SelectWallSpacePirateClimbList(
        bool onRightWall,
        WallSpacePirateClimbDirection direction) => new WallSpacePirateClimb(onRightWall, direction) switch
    {
        (false, WallSpacePirateClimbDirection.Down) =>
            WallSpacePirateSegment.MovingDownLeftWall,
        (false, WallSpacePirateClimbDirection.Up) =>
            WallSpacePirateSegment.MovingUpLeftWall,
        (true, WallSpacePirateClimbDirection.Down) =>
            WallSpacePirateSegment.MovingDownRightWall,
        (true, WallSpacePirateClimbDirection.Up) =>
            WallSpacePirateSegment.MovingUpRightWall,
        _ => throw new InvalidOperationException(
            $"Wall Pirate climb direction {direction} is not binary."),
    };

    /// <summary>Captures the wall-to-wall destination, arc center, and starting angle from the jump span.</summary>
    private static void PrepareWallSpacePirateJump(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        HorizontalDirection direction)
    {
        int span = slot.SpawnSettings<WallSpacePirateSpawnSettings>().JumpSpan;
        int signedSpan = direction == HorizontalDirection.Right ? span : -span;
        int signedHalfSpan = direction == HorizontalDirection.Right
            ? span >> 1
            : -(span >> 1);
        state.WallJumpArcCenter = new Position16<RoomPixels>(
            CoordinateAxis.Wrap(slot.Position.X.Whole + signedHalfSpan),
            slot.Position.Y.Whole);
        state.WallJumpArcAngle = direction == HorizontalDirection.Right ? RotationAngle.QuarterTurn : RotationAngle.ThreeQuarterTurn;
    }

    /// <summary>Emits a wall laser through the shared Pirate owner and queues sound only if allocation succeeds.</summary>
    private void FireWallSpacePirateLaser(
        RoomEnemySlot slot,
        HorizontalDirection direction)
    {
        // The laser initializer queues the laser sound for every successful allocation. The later wall-jump cue
        // independently queues the wall-jump sound; both are observable, distinct requests.
        if (_walkingSpacePirateBehavior.SpawnSpacePirateLaser(slot, direction == HorizontalDirection.Right, WallPirateLaserMuzzleYOffset))
            _walkingSpacePirateBehavior.RequestSpacePirateSound(EnemySoundEffectDefinitions.WallPirateLaserSound);
    }

    /// <summary>
    /// Carries out one cue of the wall Space Pirates' animations. A climb continues at the opposite climb after terrain
    /// contact and a random direction choice always continues at the chosen climb, as the cues return those
    /// animations; every other cue runs on.
    /// </summary>
    private WallSpacePirateSegment? ApplyWallSpacePirateCue(RoomEnemySlot slot, WallSpacePirateCue cue, RoomLevelData? level)
    {
        WallSpacePirateEnemyState state = slot.RequireSpeciesState<WallSpacePirateEnemyState>();
        switch (cue)
        {
            case WallSpacePirateCue.ClimbLeftWall climbLeftWall:
                return MoveWallSpacePirateAndReverseOnCollision(slot, state, level, climbLeftWall.ClimbPixels, wall: HorizontalDirection.Left);
            case WallSpacePirateCue.ClimbRightWall climbRightWall:
                return MoveWallSpacePirateAndReverseOnCollision(slot, state, level, climbRightWall.ClimbPixels, wall: HorizontalDirection.Right);
            case WallSpacePirateCue.Signal { Kind: WallSpacePirateCueKind.ChooseLeftWallDirection }:
                return RandomizeWallSpacePirateClimbDirection(state, wall: HorizontalDirection.Left);
            case WallSpacePirateCue.Signal { Kind: WallSpacePirateCueKind.ChooseRightWallDirection }:
                return RandomizeWallSpacePirateClimbDirection(state, wall: HorizontalDirection.Right);
            case WallSpacePirateCue.Signal { Kind: WallSpacePirateCueKind.PrepareJumpRight }:
                PrepareWallSpacePirateJump(slot, state, direction: HorizontalDirection.Right);
                return null;
            case WallSpacePirateCue.Signal { Kind: WallSpacePirateCueKind.PrepareJumpLeft }:
                PrepareWallSpacePirateJump(slot, state, direction: HorizontalDirection.Left);
                return null;
            case WallSpacePirateCue.Signal { Kind: WallSpacePirateCueKind.FireLaserLeft }:
                FireWallSpacePirateLaser(slot, direction: HorizontalDirection.Left);
                return null;
            case WallSpacePirateCue.Signal { Kind: WallSpacePirateCueKind.FireLaserRight }:
                FireWallSpacePirateLaser(slot, direction: HorizontalDirection.Right);
                return null;
            case WallSpacePirateCue.SetAction setAction:
                state.Action = setAction.Action;
                return null;
            case WallSpacePirateCue.Signal { Kind: WallSpacePirateCueKind.PlayAttackSound }:
                _walkingSpacePirateBehavior.RequestSpacePirateSound(EnemySoundEffectDefinitions.WallPirateJumpSound);
                return null;
            default:
                throw new InvalidOperationException($"The wall Space Pirates' animations have no {cue} cue.");
        }
    }

    /// <summary>Runs the wall Space Pirate's main AI.</summary>
    internal static void RunWallSpacePirateMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunWallSpacePirateMain(
            slot,
            slot.RequireSpeciesState<WallSpacePirateEnemyState>(),
            frame.Samus);
    }

    /// <summary>Advances the wall Space Pirate's typed animation.</summary>
    internal void AdvanceWallSpacePirateTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(slot, WallSpacePirateAnimations.Segments,
            static (WallSpacePirateEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyWallSpacePirateCue(slot, cue, context.Level) is { } continuesAt
                ? new AnimationContinuation<WallSpacePirateSegment>(WallSpacePirateAnimations.StartingWith(continuesAt), AnimationPlace.StartOfAnimation)
                : null);
}

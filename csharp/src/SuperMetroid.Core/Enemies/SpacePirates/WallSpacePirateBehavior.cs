using SuperMetroid.Core.Programs;
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
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.PirateGoldWall, EnemyDefinitionId.PirateGreenWall, EnemyDefinitionId.PirateGreyWall, EnemyDefinitionId.PirateMagentaWall, EnemyDefinitionId.PirateRedWall, EnemyDefinitionId.PirateSilverWall];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new WallSpacePirateEntity(this, slot, common);

    /// <summary>Advances the shared random source when a climbing cue chooses a new direction.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Clamps vertical climbing to room blocks before a collision reverses direction.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Runs common Samus contact for the ordinary Pirate family.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Handles Pirate death animation and Gold Ninja's special pickup scatter.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Shares laser spawning and hitbox-shot behavior with walking Pirates.</summary>
    private readonly WalkingSpacePirateBehavior _walkingSpacePirateBehavior;

    /// <summary>Binds wall climbing, jump arcs, and family combat reactions to room services.</summary>
    internal WallSpacePirateBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops, WalkingSpacePirateBehavior walkingSpacePirateBehavior)
    {
        _walkingSpacePirateBehavior = walkingSpacePirateBehavior;
        _room = room;
        _terrain = terrain;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>Vertical distance below which a climbing Pirate starts its fire-and-jump list.</summary>
    private const int WallPirateSamusDetectionBand = 32;
    /// <summary>Pixels above the actor origin where the shared Pirate laser is emitted.</summary>
    private const int WallPirateLaserMuzzleYOffset = 16;

    /// <summary>Recognizes the six color variants that use wall-climbing Pirate AI.</summary>
    internal static bool IsWallSpacePirateDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.PirateGreyWall or
        EnemyDefinitionId.PirateGreenWall or
        EnemyDefinitionId.PirateRedWall or
        EnemyDefinitionId.PirateGoldWall or
        EnemyDefinitionId.PirateMagentaWall or
        EnemyDefinitionId.PirateSilverWall;

    /// <summary>Groups wall, walking, and Ninja Pirates that share ordinary Pirate combat callbacks.</summary>
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
            ? WallSpacePirateProgram.MovingDownRightWall
            : WallSpacePirateProgram.MovingDownLeftWall), AnimationPlace.StartOfAnimation);
        state.Function = startsOnRight
            ? WallSpacePirateFunction.ClimbingRightWall
            : WallSpacePirateFunction.ClimbingLeftWall;
        state.ClimbDirection = WallSpacePirateClimbDirection.Down;

        // The USA/Japan build measures angles in 256ths of a turn. A slow jump takes 63 two-step frames; a fast
        // one takes 32 four-step frames with adjusted endpoints. These values are not approximations: they are the
        // literal 190/66/2 and 192/64/4 branches.
        state.RightJumpTargetAngle = RotationAngle.FromSteps(slowJump ? 190 : 192);
        state.LeftJumpTargetAngle = RotationAngle.FromSteps(slowJump ? 66 : 64);
        state.JumpAngleDelta = slowJump ? 2 : 4;

        SnapWallSpacePirateXToTile(slot);
    }

    /// <summary>Runs a wall Space Pirate's per-frame behaviour and its six functions.</summary>
    private static void RunWallSpacePirateMain(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        SamusState? samus)
    {
        switch (state.Function)
        {
            case WallSpacePirateFunction.ClimbingLeftWall:
                if (WallSpacePirateCanAttack(slot, RequireSamus()))
                {
                    InstallWallSpacePirateInstruction(
                        slot,
                        state,
                        WallSpacePirateProgram.FireAndJumpRight);
                }
                return;

            case WallSpacePirateFunction.ClimbingRightWall:
                if (WallSpacePirateCanAttack(slot, RequireSamus()))
                {
                    InstallWallSpacePirateInstruction(
                        slot,
                        state,
                        WallSpacePirateProgram.FireAndJumpLeft);
                }
                return;

            case WallSpacePirateFunction.AttackAnimationOnLeftWall:
            case WallSpacePirateFunction.AttackAnimationOnRightWall:
                // These behaviours do nothing. Actor presentation and timing are owned
                // entirely by the currently-running animation.
                return;

            case WallSpacePirateFunction.WallJumpingRight:
                StepWallSpacePirateJump(slot, state, direction: HorizontalDirection.Right);
                return;

            case WallSpacePirateFunction.WallJumpingLeft:
                StepWallSpacePirateJump(slot, state, direction: HorizontalDirection.Left);
                return;

            default:
                throw new InvalidDataException(
                    $"Wall Space Pirate function {state.Function?.ToString() ?? "none"} is not translated.");
        }

        SamusState RequireSamus() => samus ?? throw new InvalidOperationException(
            "Wall Space Pirate detection requires the active Samus position.");
    }

    /// <summary>
    /// Moves along the jump arc (the angle falls jumping right and rises jumping left) and installs the landing list for
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

        InstallWallSpacePirateInstruction(
            slot,
            state,
            direction == HorizontalDirection.Right ? WallSpacePirateProgram.LandedOnRightWall : WallSpacePirateProgram.LandedOnLeftWall);
        SnapWallSpacePirateXToTile(slot);
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
    private static void SnapWallSpacePirateXToTile(RoomEnemySlot slot)
    {
        // The wall alignment deliberately has two alignment grids. Nibbles 0..10 round down to an
        // eight-pixel boundary; 11..15 round up to the next sixteen-pixel boundary.
        int x = slot.Position.X.Whole;
        slot.Position = slot.Position.WithWholeX(RoomGridConversions.OffsetWithinBlock(x) < 11
            ? RoomGridConversions.HalfBlockStart(x)
            : CoordinateAxis.Wrap(RoomGridConversions.BlockStart(x) + 16));
    }

    /// <summary>Starts a selected wall-Pirate list on its first animation frame.</summary>
    private static void InstallWallSpacePirateInstruction(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        WallSpacePirateProgram program)
    {
        state.Playback = new(WallSpacePirateAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Climbs, or after terrain contact returns the opposite climb to continue at; null runs on.</summary>
    private WallSpacePirateProgram? MoveWallSpacePirateAndReverseOnCollision(
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

        // A collision retains the collision helper's clamped position, flips only bit zero,
        // and continues at a fresh climb. It does not resume after the cue.
        state.ClimbDirection = state.ClimbDirection == WallSpacePirateClimbDirection.Down
            ? WallSpacePirateClimbDirection.Up
            : WallSpacePirateClimbDirection.Down;
        return SelectWallSpacePirateClimbList(wall == HorizontalDirection.Right, state.ClimbDirection);
    }

    /// <summary>Consumes one random value and returns the chosen up or down climb to continue at.</summary>
    private WallSpacePirateProgram RandomizeWallSpacePirateClimbDirection(
        WallSpacePirateEnemyState state,
        HorizontalDirection wall)
    {
        state.ClimbDirection = SpacePirateDecisions.ClimbUp.IsSetIn(_room.Random.Generate())
            ? WallSpacePirateClimbDirection.Up
            : WallSpacePirateClimbDirection.Down;
        return SelectWallSpacePirateClimbList(wall == HorizontalDirection.Right, state.ClimbDirection);
    }

    /// <summary>Selects the climb animation for a wall side and vertical climb direction.</summary>
    private static WallSpacePirateProgram SelectWallSpacePirateClimbList(
        bool onRightWall,
        WallSpacePirateClimbDirection direction) => new WallSpacePirateClimb(onRightWall, direction) switch
    {
        (false, WallSpacePirateClimbDirection.Down) =>
            WallSpacePirateProgram.MovingDownLeftWall,
        (false, WallSpacePirateClimbDirection.Up) =>
            WallSpacePirateProgram.MovingUpLeftWall,
        (true, WallSpacePirateClimbDirection.Down) =>
            WallSpacePirateProgram.MovingDownRightWall,
        (true, WallSpacePirateClimbDirection.Up) =>
            WallSpacePirateProgram.MovingUpRightWall,
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
        state.WallJumpDestination = new(CoordinateAxis.Wrap(slot.Position.X.Whole + signedSpan), 0);
        state.WallJumpArcCenter = new Position16<RoomPixels>(
            CoordinateAxis.Wrap(slot.Position.X.Whole + signedHalfSpan),
            slot.Position.Y.Whole);
        state.WallJumpArcAngle = direction == HorizontalDirection.Right ? RotationAngle.QuarterTurn : RotationAngle.ThreeQuarterTurn;
    }

    /// <summary>Emits a wall laser through the shared Pirate owner and queues sound only if allocation succeeds.</summary>
    private void FireWallSpacePirateLaser(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        HorizontalDirection direction)
    {
        // The laser initializer queues the laser sound for every successful allocation. The later wall-jump cue
        // independently queues the wall-jump sound; both are observable, distinct requests.
        if (_walkingSpacePirateBehavior.SpawnSpacePirateLaser(slot, direction == HorizontalDirection.Right, WallPirateLaserMuzzleYOffset))
        {
            state.SpawnedLaserCount++;
            _walkingSpacePirateBehavior.RequestSpacePirateSound(EnemySoundEffectDefinitions.WallPirateLaserSound);
        }
    }

    /// <summary>
    /// Carries out one cue of the wall Space Pirates' animations. A climb continues at the opposite climb after terrain
    /// contact and a random direction choice always continues at the chosen climb, as the cues return those
    /// animations; every other cue runs on.
    /// </summary>
    private WallSpacePirateProgram? ApplyWallSpacePirateCue(RoomEnemySlot slot, WallSpacePirateCue cue, RoomLevelData? level)
    {
        WallSpacePirateEnemyState state = slot.RequireSpeciesState<WallSpacePirateEnemyState>();
        switch (cue)
        {
            case WallSpacePirateCue.ClimbLeftWall climbLeftWall:
                return MoveWallSpacePirateAndReverseOnCollision(slot, state, level, climbLeftWall.ClimbPixels, wall: HorizontalDirection.Left);
            case WallSpacePirateCue.ClimbRightWall climbRightWall:
                return MoveWallSpacePirateAndReverseOnCollision(slot, state, level, climbRightWall.ClimbPixels, wall: HorizontalDirection.Right);
            case WallSpacePirateCue.Command { Kind: WallSpacePirateCueKind.ChooseLeftWallDirection }:
                return RandomizeWallSpacePirateClimbDirection(state, wall: HorizontalDirection.Left);
            case WallSpacePirateCue.Command { Kind: WallSpacePirateCueKind.ChooseRightWallDirection }:
                return RandomizeWallSpacePirateClimbDirection(state, wall: HorizontalDirection.Right);
            case WallSpacePirateCue.Command { Kind: WallSpacePirateCueKind.PrepareJumpRight }:
                PrepareWallSpacePirateJump(slot, state, direction: HorizontalDirection.Right);
                return null;
            case WallSpacePirateCue.Command { Kind: WallSpacePirateCueKind.PrepareJumpLeft }:
                PrepareWallSpacePirateJump(slot, state, direction: HorizontalDirection.Left);
                return null;
            case WallSpacePirateCue.Command { Kind: WallSpacePirateCueKind.FireLaserLeft }:
                FireWallSpacePirateLaser(slot, state, direction: HorizontalDirection.Left);
                return null;
            case WallSpacePirateCue.Command { Kind: WallSpacePirateCueKind.FireLaserRight }:
                FireWallSpacePirateLaser(slot, state, direction: HorizontalDirection.Right);
                return null;
            case WallSpacePirateCue.SetFunction setFunction:
                state.Function = setFunction.Function;
                return null;
            case WallSpacePirateCue.Command { Kind: WallSpacePirateCueKind.PlayAttackSound }:
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
    internal void AdvanceWallSpacePirateTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context) =>
        EnemyProgramAnimation.Advance(slot, WallSpacePirateAnimations.Segments,
            static (WallSpacePirateEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyWallSpacePirateCue(slot, cue, context.Level) is { } continuesAt
                ? new AnimationContinuation<WallSpacePirateProgram>(WallSpacePirateAnimations.StartingWith(continuesAt), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Applies the ordinary Pirate power-bomb callback across wall, walking, and Ninja variants.</summary>
    internal static void AddWallSpacePiratePowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => IsOrdinarySpacePirateDefinition(enemy.DefinitionId) &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyReaction.SpacePiratePowerBomb));
    }

    /// <summary>Uses shared Pirate shot logic, including hitbox quake, big death explosion, and Gold Ninja drops.</summary>
    internal void AddWallSpacePirateShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => IsOrdinarySpacePirateDefinition(enemy.DefinitionId) &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.SpacePirateShot)
        {
            AfterSuperMissileQuake = _walkingSpacePirateBehavior.ResolveSpacePirateHitboxShot,
            ReplacesGenericDeath = c =>
            {
                if (c.HitboxShotAi == EnemyReaction.NormalEnemyShot)
                    return false;
                // The Gold Ninja's death records its position as the drop origin before the enemy's death clears the
                // slot; it then spawns the death explosion before the metal ninja Pirate's item drop spawns the drops.
                bool spawnsGoldNinjaDrops = c.Enemy.DefinitionId == EnemyDefinitionId.PirateGoldNinja;
                Position16<RoomPixels> dropOrigin = c.Enemy.Position.Whole;
                _deathsAndDrops.StartGenericEnemyDeath(
                    c.Enemy,
                    EnemyDeathsAndDrops.SelectNormalShotDeathAnimation(c.Enemy, c.ProjectileType, rule: ShotDeathAnimationRule.PirateBigExplosion));
                if (spawnsGoldNinjaDrops)
                    _deathsAndDrops.SpawnEnemyDropScatterAround(EnemyDefinitionId.PirateGoldNinja, count: 5, dropOrigin);
                return true;
            },
        });
    }

    /// <summary>Skips frozen contact and rejects unsupported extended hitbox callbacks before normal touch damage.</summary>
    internal void AddWallSpacePirateTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => IsOrdinarySpacePirateDefinition(slot.DefinitionId) &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.SpacePirateTouch,
            context =>
            {
                // Space Pirate extended maps currently use the shared Pirate touch
                // callback in every retail hitbox. Keep the callback check explicit: silently
                // treating a later family-specific attack box as ordinary body contact would
                // recreate the exact radius-flattening bug this path is intended to remove.
                if (context.UsesFrameHitboxes && context.HitboxTouchAi != EnemyReaction.SpacePirateTouch)
                {
                    throw new InvalidDataException(
                        $"Space Pirate hitbox touch AI {context.HitboxTouchAi?.ToString() ?? "(unnamed)"} is not translated.");
                }
                // The shared Pirate touch returns before common touch AI while frozen. The overlap was
                // still found, but neither Samus nor the Pirate receives contact damage.
                if (context.Slot.FrozenTimer != 0)
                    return;
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
            });
    }
}

using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The wall space pirate species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class WallSpacePirateBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.PirateGoldWall, EnemyDefinitionId.PirateGreenWall, EnemyDefinitionId.PirateGreyWall, EnemyDefinitionId.PirateMagentaWall, EnemyDefinitionId.PirateRedWall, EnemyDefinitionId.PirateSilverWall];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new WallSpacePirateEntity(this, slot, common);

    /// <summary>Advances the shared random source when a climbing opcode chooses a new direction.</summary>
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

    /// <summary>Ports <c>InitAI_PirateWall</c> at <c>$B2:EF9F</c>.</summary>
    internal static void InitializeWallSpacePirate(RoomEnemySlot slot)
    {
        var state = new WallSpacePirateEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        WallSpacePirateParameterFlags flags = slot.SpawnSettings<WallSpacePirateSpawnSettings>().Options;
        bool startsOnRight = (flags & WallSpacePirateParameterFlags.StartsOnRightWall) != WallSpacePirateParameterFlags.None;
        bool slowJump = (flags & WallSpacePirateParameterFlags.SlowJumpAndLaser) != WallSpacePirateParameterFlags.None;

        state.Program = AnimationProgramCursor<WallSpacePirateProgram>.Start(startsOnRight
            ? WallSpacePirateProgram.MovingDownRightWall
            : WallSpacePirateProgram.MovingDownLeftWall);
        state.Function = startsOnRight
            ? WallSpacePirateFunction.ClimbingRightWall
            : WallSpacePirateFunction.ClimbingLeftWall;
        state.ClimbDirection = WallSpacePirateClimbDirection.Down;

        // The USA/Japan build uses byte angles. Bit fifteen selects 63 two-step frames;
        // clearing it selects 32 four-step frames with adjusted endpoints. These values are
        // not approximations: they are the literal $BE/$42/$02 and $C0/$40/$04 branches.
        state.RightJumpTargetAngle = RotationAngle.FromTableIndex(slowJump ? (byte)190 : (byte)192);
        state.LeftJumpTargetAngle = RotationAngle.FromTableIndex(slowJump ? (byte)66 : (byte)64);
        state.JumpAngleDelta = slowJump ? 2 : 4;

        SnapWallSpacePirateXToTile(slot);
    }

    /// <summary>Ports <c>MainAI_PirateWall</c> and its six function targets.</summary>
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
                // These are literal RTS functions. Actor presentation and timing are owned
                // entirely by the currently-running instruction list.
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
        state.WallJumpArcAngle = state.WallJumpArcAngle.AddTableUnits(direction == HorizontalDirection.Right ? -state.JumpAngleDelta : state.JumpAngleDelta);
        if (state.WallJumpArcAngle != (direction == HorizontalDirection.Right ? state.RightJumpTargetAngle : state.LeftJumpTargetAngle))
            return;

        InstallWallSpacePirateInstruction(
            slot,
            state,
            direction == HorizontalDirection.Right ? WallSpacePirateProgram.LandedOnRightWall : WallSpacePirateProgram.LandedOnLeftWall);
        SnapWallSpacePirateXToTile(slot);
    }

    /// <summary>Uses native sine and cosine products to place the Pirate on a parameter-two-sized jump ellipse.</summary>
    private static void PositionWallSpacePirateOnArc(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state)
    {
        // Native math rewrites both whole positions every frame ($B2:F0F9/$F113) and leaves
        // the fractions untouched. The jump ellipse is parameter2 pixels wide and
        // parameter2/4 pixels tall.
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(
            state.WallJumpArcCenter.X +
            EnemyMath.ReadEightBitNegativeSineProduct(
                state.WallJumpArcAngle.ToByteAngle(),
                (ushort)(slot.SpawnSettings<WallSpacePirateSpawnSettings>().JumpSpan >> 1)))));
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(
            state.WallJumpArcCenter.Y -
            EnemyMath.ReadEightBitCosineProduct(state.WallJumpArcAngle.ToByteAngle(), (ushort)(slot.SpawnSettings<WallSpacePirateSpawnSettings>().JumpSpan >> 2)))));
    }

    /// <summary>Allows the fire-and-jump sequence when Samus is within the strict vertical detection band.</summary>
    private static bool WallSpacePirateCanAttack(RoomEnemySlot slot, SamusState samus) =>
        Math.Abs(unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))) <
            WallPirateSamusDetectionBand;

    /// <summary>Applies the native split alignment rule when a Pirate starts or lands on a wall.</summary>
    private static void SnapWallSpacePirateXToTile(RoomEnemySlot slot)
    {
        // `$B2:EFFF` deliberately has two alignment grids. Nibbles 0..10 round down to an
        // eight-pixel boundary; 11..15 round up to the next sixteen-pixel boundary.
        int x = slot.Position.X.Whole;
        slot.Position = slot.Position.WithWholeX(RoomGridConversions.OffsetWithinBlock(x) < 11
            ? RoomGridConversions.HalfBlockStart(x)
            : unchecked((ushort)(RoomGridConversions.BlockStart(x) + 16)));
    }

    /// <summary>Starts a selected wall-Pirate list on its first instruction frame.</summary>
    private static void InstallWallSpacePirateInstruction(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        WallSpacePirateProgram program)
    {
        state.Program = AnimationProgramCursor<WallSpacePirateProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
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

        int displacement = SixteenSixteen.FromWhole(downwardPixels);
        if (!_terrain.MoveEnemyVertically(level, slot, displacement))
            return null;

        // A collision retains the collision helper's clamped position, flips only bit zero,
        // and continues at a fresh climb. It does not resume after the opcode operand.
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
        state.ClimbDirection = (_room.Random.Generate() & 1) == 0
            ? WallSpacePirateClimbDirection.Down
            : WallSpacePirateClimbDirection.Up;
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

    /// <summary>Captures the wall-to-wall destination, arc center, and starting angle from parameter two.</summary>
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
        state.WallJumpDestination = new(unchecked((ushort)(slot.Position.X.Whole + signedSpan)), 0);
        state.WallJumpArcCenter = new Position16<RoomPixels>(
            unchecked((ushort)(slot.Position.X.Whole + signedHalfSpan)),
            slot.Position.Y.Whole);
        state.WallJumpArcAngle = direction == HorizontalDirection.Right ? RotationAngle.QuarterTurn : RotationAngle.ThreeQuarterTurn;
    }

    /// <summary>Emits a wall laser through the shared Pirate owner and queues sound only if allocation succeeds.</summary>
    private void FireWallSpacePirateLaser(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        HorizontalDirection direction)
    {
        // `$86:A009` queues $67 for every successful allocation. The later wall-jump opcode
        // independently queues $66; both are observable, distinct library-two requests.
        if (_walkingSpacePirateBehavior.SpawnSpacePirateLaser(slot, direction == HorizontalDirection.Right, WallPirateLaserMuzzleYOffset))
        {
            state.SpawnedLaserCount++;
            _walkingSpacePirateBehavior.LastSpacePirateSoundEffect = EnemySoundEffectDefinitions.WallPirateLaserSound;
        }
    }

    /// <summary>
    /// Carries out one cue of the wall Space Pirates' animations. A climb continues at the opposite climb after terrain
    /// contact and a random direction choice always continues at the chosen climb, as the native opcodes return those
    /// lists; every other cue runs on.
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
                _walkingSpacePirateBehavior.LastSpacePirateSoundEffect = EnemySoundEffectDefinitions.WallPirateJumpSound;
                return null;
            default:
                throw new InvalidOperationException($"The wall Space Pirates' animations have no {cue} cue.");
        }
    }

    /// <summary>Ports WallSpacePirate's main AI.</summary>
    internal static void RunWallSpacePirateMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunWallSpacePirateMain(
            slot,
            slot.RequireSpeciesState<WallSpacePirateEnemyState>(),
            frame.Samus);
    }

    /// <summary>Advances WallSpacePirate's typed animation.</summary>
    internal void AdvanceWallSpacePirateTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context) =>
        EnemyProgramAnimation.Advance(slot, WallSpacePirateAnimations.Programs, WallSpacePirateFrameDefinitions.ShownFrames.Of,
            static (WallSpacePirateEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyWallSpacePirateCue(slot, cue, context.Level));

    /// <summary>Applies the ordinary Pirate power-bomb callback across wall, walking, and Ninja variants.</summary>
    internal static void AddWallSpacePiratePowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => IsOrdinarySpacePirateDefinition(enemy.DefinitionId) &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.SpacePiratePowerBomb));
    }

    /// <summary>Uses shared Pirate shot logic, including hitbox quake, big death explosion, and Gold Ninja drops.</summary>
    internal void AddWallSpacePirateShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => IsOrdinarySpacePirateDefinition(enemy.DefinitionId) &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.SpacePirateShot)
        {
            AfterSuperMissileQuake = _walkingSpacePirateBehavior.ResolveSpacePirateHitboxShot,
            ReplacesGenericDeath = c =>
            {
                if (c.HitboxShotAi == EnemyInteractionCallback.NormalEnemyShot)
                    return false;
                // The shared Pirate shot tail clears one value before requesting death animation
                // variant four: a wall Pirate's jump destination (used only as a debug destination, so its fatal clear
                // is observable) or a Ninja's spin-jump speed; a walking Pirate keeps nothing there.
                ClearPirateMoveOnShotDeath(c.Enemy);
                // $B2:878F records the Gold Ninja's position as the drop origin before EnemyDeath clears the slot;
                // $B2:87B4 then spawns the death explosion before MetalNinjaPirateDeathItemDropRoutine ($B2:87B8)
                // spawns the drops.
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
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.SpacePirateTouch,
            context =>
            {
                // Space Pirate extended maps currently use the shared `$B2:876C` touch
                // callback in every retail hitbox. Keep the pointer check explicit: silently
                // treating a later family-specific attack box as ordinary body contact would
                // recreate the exact radius-flattening bug this path is intended to remove.
                if (context.UsesExtendedHitboxes && context.HitboxTouchAi != EnemyInteractionCallback.SpacePirateTouch)
                {
                    throw new InvalidDataException(
                        $"Space Pirate hitbox touch AI {context.HitboxTouchAi?.ToString() ?? "(unnamed)"} is not translated.");
                }
                // `$B2:876C` returns before common touch AI while frozen. The overlap was
                // still found, but neither Samus nor the Pirate receives contact damage.
                if (!context.Slot.FrozenTimer.IsZero)
                    return;
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
            });
    }

    /// <summary>Clears the value the shared Pirate shot tail empties, in the state of the family owning the slot.</summary>
    private static void ClearPirateMoveOnShotDeath(RoomEnemySlot enemy)
    {
        if (enemy.Entity is WallSpacePirateEntity && enemy.SpeciesState<WallSpacePirateEnemyState>() is { } wall)
            wall.WallJumpDestination = Position16<RoomPixels>.Zero;
        else if (enemy.Entity is NinjaSpacePirateEntity && enemy.SpeciesState<NinjaSpacePirateEnemyState>() is { } ninja)
            ninja.CalculatedSpinJumpSpeed = 0;
    }
}

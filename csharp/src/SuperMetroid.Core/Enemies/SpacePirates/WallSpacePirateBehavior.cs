using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The wall space pirate species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class WallSpacePirateBehavior : IEnemySpecies
{
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

    /// <summary>Climb direction, arc geometry, and animation state indexed by physical enemy slot.</summary>
    internal readonly WallSpacePirateEnemyState?[] _wallSpacePirateStates =
        new WallSpacePirateEnemyState?[RoomEnemySystem.MaximumEnemyCount];

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
    internal void InitializeWallSpacePirate(RoomEnemySlot slot)
    {
        var state = new WallSpacePirateEnemyState(slot);
        _wallSpacePirateStates[slot.SlotIndex] = state;

        var flags = (WallSpacePirateParameterFlags)slot.Parameter1;
        bool startsOnRight = (flags & WallSpacePirateParameterFlags.StartsOnRightWall) != 0;
        bool slowJump = (flags & WallSpacePirateParameterFlags.SlowJumpAndLaser) != 0;

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
        state.RightJumpTargetAngle = slowJump ? (ushort)190 : (ushort)192;
        state.LeftJumpTargetAngle = slowJump ? (ushort)66 : (ushort)64;
        state.JumpAngleDelta = slowJump ? (ushort)2 : (ushort)4;

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
                StepWallSpacePirateJumpRight(slot, state);
                return;

            case WallSpacePirateFunction.WallJumpingLeft:
                StepWallSpacePirateJumpLeft(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Wall Space Pirate function $B2:{(ushort)state.Function:X4} is not translated.");
        }

        SamusState RequireSamus() => samus ?? throw new InvalidOperationException(
            "Wall Space Pirate detection requires the active Samus position.");
    }

    /// <summary>Moves along the rightward jump arc and installs the right-wall landing list at its target angle.</summary>
    private static void StepWallSpacePirateJumpRight(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state)
    {
        PositionWallSpacePirateOnArc(slot, state);
        state.WallJumpArcAngle = unchecked((byte)(
            state.WallJumpArcAngle - state.JumpAngleDelta));
        if (state.WallJumpArcAngle != state.RightJumpTargetAngle)
            return;

        InstallWallSpacePirateInstruction(
            slot,
            state,
            WallSpacePirateProgram.LandedOnRightWall);
        SnapWallSpacePirateXToTile(slot);
    }

    /// <summary>Moves along the leftward jump arc and installs the left-wall landing list at its target angle.</summary>
    private static void StepWallSpacePirateJumpLeft(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state)
    {
        PositionWallSpacePirateOnArc(slot, state);
        state.WallJumpArcAngle = unchecked((byte)(
            state.WallJumpArcAngle + state.JumpAngleDelta));
        if (state.WallJumpArcAngle != state.LeftJumpTargetAngle)
            return;

        InstallWallSpacePirateInstruction(
            slot,
            state,
            WallSpacePirateProgram.LandedOnLeftWall);
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
                state.WallJumpArcAngle,
                (ushort)(slot.Parameter2 >> 1)))));
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(
            state.WallJumpArcCenter.Y -
            EnemyMath.ReadEightBitCosineProduct(state.WallJumpArcAngle, (ushort)(slot.Parameter2 >> 2)))));
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
        slot.Position = slot.Position.WithWholeX((slot.Position.X.Whole & RoomPixelMasks.WithinBlock) < 11
            ? unchecked((ushort)(slot.Position.X.Whole & RoomPixelMasks.HalfBlockStart))
            : unchecked((ushort)((slot.Position.X.Whole & RoomPixelMasks.BlockStart) + 16)));
    }

    /// <summary>Starts a selected wall-Pirate list on its first instruction frame.</summary>
    private static void InstallWallSpacePirateInstruction(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        WallSpacePirateProgram program)
    {
        state.Program = AnimationProgramCursor<WallSpacePirateProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Climbs, or after terrain contact returns the opposite climb to continue at; null runs on.</summary>
    private WallSpacePirateProgram? MoveWallSpacePirateAndReverseOnCollision(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        RoomLevelData? level,
        ushort signedPixelDisplacement,
        bool onRightWall)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                "Wall Space Pirate climbing requires room collision data.");
        }

        int displacement = SnesSixteenSixteen.FromWhole((short)signedPixelDisplacement);
        if (!_terrain.MoveEnemyVertically(level, slot, displacement))
            return null;

        // A collision retains the collision helper's clamped position, flips only bit zero,
        // and continues at a fresh climb. It does not resume after the opcode operand.
        state.ClimbDirection = state.ClimbDirection == WallSpacePirateClimbDirection.Down
            ? WallSpacePirateClimbDirection.Up
            : WallSpacePirateClimbDirection.Down;
        return SelectWallSpacePirateClimbList(onRightWall, state.ClimbDirection);
    }

    /// <summary>Consumes one random value and returns the chosen up or down climb to continue at.</summary>
    private WallSpacePirateProgram RandomizeWallSpacePirateClimbDirection(
        WallSpacePirateEnemyState state,
        bool onRightWall)
    {
        state.ClimbDirection = (_room.Random.Generate() & 1) == 0
            ? WallSpacePirateClimbDirection.Down
            : WallSpacePirateClimbDirection.Up;
        return SelectWallSpacePirateClimbList(onRightWall, state.ClimbDirection);
    }

    /// <summary>Selects the climb animation for a wall side and vertical climb direction.</summary>
    private static WallSpacePirateProgram SelectWallSpacePirateClimbList(
        bool onRightWall,
        WallSpacePirateClimbDirection direction) => (onRightWall, direction) switch
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
            $"Wall Pirate climb direction ${(ushort)direction} is not binary."),
    };

    /// <summary>Captures the wall-to-wall destination, arc center, and starting angle from parameter two.</summary>
    private static void PrepareWallSpacePirateJump(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        bool jumpingRight)
    {
        int signedSpan = jumpingRight ? slot.Parameter2 : -slot.Parameter2;
        int signedHalfSpan = jumpingRight
            ? slot.Parameter2 >> 1
            : -(slot.Parameter2 >> 1);
        state.WallJumpDestinationX = unchecked((ushort)(slot.Position.X.Whole + signedSpan));
        state.WallJumpArcCenter = new Position16<RoomPixels>(
            unchecked((ushort)(slot.Position.X.Whole + signedHalfSpan)),
            slot.Position.Y.Whole);
        state.WallJumpArcAngle = jumpingRight ? (ushort)64 : (ushort)192;
    }

    /// <summary>Emits a wall laser through the shared Pirate owner and queues sound only if allocation succeeds.</summary>
    private void FireWallSpacePirateLaser(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        bool movingRight)
    {
        // `$86:A009` queues $67 for every successful allocation. The later wall-jump opcode
        // independently queues $66; both are observable, distinct library-two requests.
        if (_walkingSpacePirateBehavior.SpawnSpacePirateLaser(slot, movingRight, WallPirateLaserMuzzleYOffset))
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
        WallSpacePirateEnemyState state = RequireWallSpacePirateState(slot);
        switch (cue.Kind)
        {
            case WallSpacePirateCueKind.ClimbLeftWall:
                return MoveWallSpacePirateAndReverseOnCollision(slot, state, level, cue.Value, onRightWall: false);
            case WallSpacePirateCueKind.ClimbRightWall:
                return MoveWallSpacePirateAndReverseOnCollision(slot, state, level, cue.Value, onRightWall: true);
            case WallSpacePirateCueKind.ChooseLeftWallDirection:
                return RandomizeWallSpacePirateClimbDirection(state, onRightWall: false);
            case WallSpacePirateCueKind.ChooseRightWallDirection:
                return RandomizeWallSpacePirateClimbDirection(state, onRightWall: true);
            case WallSpacePirateCueKind.PrepareJumpRight:
                PrepareWallSpacePirateJump(slot, state, jumpingRight: true);
                return null;
            case WallSpacePirateCueKind.PrepareJumpLeft:
                PrepareWallSpacePirateJump(slot, state, jumpingRight: false);
                return null;
            case WallSpacePirateCueKind.FireLaserLeft:
                FireWallSpacePirateLaser(slot, state, movingRight: false);
                return null;
            case WallSpacePirateCueKind.FireLaserRight:
                FireWallSpacePirateLaser(slot, state, movingRight: true);
                return null;
            case WallSpacePirateCueKind.SetFunction:
                state.Function = cue.Function;
                return null;
            case WallSpacePirateCueKind.PlayAttackSound:
                _walkingSpacePirateBehavior.LastSpacePirateSoundEffect = EnemySoundEffectDefinitions.WallPirateJumpSound;
                return null;
            default:
                throw new InvalidOperationException($"The wall Space Pirates' animations have no {cue.Kind} cue.");
        }
    }

    /// <summary>Rejects a wall-Pirate callback before its slot has initialized species state.</summary>
    private WallSpacePirateEnemyState RequireWallSpacePirateState(RoomEnemySlot slot) =>
        _wallSpacePirateStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized wall Space Pirate state.");

    /// <summary>Registers the native wall-Pirate initializer for all six color definitions.</summary>
    internal void RegisterWallSpacePirateInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_PirateWall, slot => IsWallSpacePirateDefinition(slot.EnemyDefinitionPointer), (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeWallSpacePirate(slot);
        });
    }

    /// <summary>Dispatches climbing, attack-list waits, and wall-jump movement for wall Pirates.</summary>
    internal void RegisterWallSpacePirateMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_PirateWall, slot => IsWallSpacePirateDefinition(slot.EnemyDefinitionPointer), (slot, frame, nmiFrameCounter8) =>
        {
            RunWallSpacePirateMain(
                slot,
                RequireWallSpacePirateState(slot),
                frame.Samus);
        });
    }

    /// <summary>Runs the wall Space Pirates' animations as typed programs (#641).</summary>
    internal void RegisterWallSpacePirateTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => IsWallSpacePirateDefinition(slot.EnemyDefinitionPointer), (slot, context) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireWallSpacePirateState(slot);
            AnimationProgramCursor<WallSpacePirateProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, WallSpacePirateAnimations.Programs, ref cursor, WallSpacePirateFrameDefinitions.Spritemap,
                    (cue, _) => ApplyWallSpacePirateCue(slot, cue, context.Level));
            state.Program = cursor;
        });


    /// <summary>Applies the ordinary Pirate power-bomb callback across wall, walking, and Ninja variants.</summary>
    internal static void RegisterWallSpacePiratePowerBombReactions(EnemyAiTable table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => IsOrdinarySpacePirateDefinition(enemy.EnemyDefinitionPointer) &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.SpacePiratePowerBomb));
    }

    /// <summary>Uses shared Pirate shot logic, including hitbox quake, big death explosion, and Gold Ninja drops.</summary>
    internal void RegisterWallSpacePirateShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => IsOrdinarySpacePirateDefinition(enemy.EnemyDefinitionPointer) &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.SpacePirateShot)
        {
            AfterSuperMissileQuake = _walkingSpacePirateBehavior.ResolveSpacePirateHitboxShot,
            ReplacesGenericDeath = c =>
            {
                if (c.HitboxShotAi == EnemyInteractionCallback.NormalEnemyShot)
                    return false;
                // The shared Pirate shot tail clears native variable B before requesting death animation variant
                // four. Wall AI uses B only as a debug jump destination, so its fatal clear is observable too.
                c.Enemy.VariableB = 0;
                // $B2:878F records the Gold Ninja's position as the drop origin before EnemyDeath clears the slot;
                // $B2:87B4 then spawns the death explosion before MetalNinjaPirateDeathItemDropRoutine ($B2:87B8)
                // spawns the drops.
                bool spawnsGoldNinjaDrops = c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.PirateGoldNinja;
                Position16<RoomPixels> dropOrigin = c.Enemy.Position.Whole;
                _deathsAndDrops.StartGenericEnemyDeath(
                    c.Enemy,
                    EnemyDeathsAndDrops.SelectNormalShotDeathAnimation(c.Enemy, c.ProjectileType, forcePirateBigExplosion: true));
                if (spawnsGoldNinjaDrops)
                    _deathsAndDrops.SpawnEnemyDropScatterAround(EnemyDefinitionId.PirateGoldNinja, count: 5, dropOrigin);
                return true;
            },
        });
    }

    /// <summary>Skips frozen contact and rejects unsupported extended hitbox callbacks before normal touch damage.</summary>
    internal void RegisterWallSpacePirateTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => IsOrdinarySpacePirateDefinition(slot.EnemyDefinitionPointer) &&
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
                        $"Space Pirate hitbox touch AI $B2:{context.HitboxTouchAiPointer:X4} is not translated.");
                }
                // `$B2:876C` returns before common touch AI while frozen. The overlap was
                // still found, but neither Samus nor the Pirate receives contact damage.
                if (!context.Slot.FrozenTimer.IsZero)
                    return;
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
            });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_wallSpacePirateStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}

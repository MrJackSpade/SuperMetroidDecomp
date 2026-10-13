using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The wall space pirate species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class WallSpacePirateBehavior : IEnemySpecies
{
    private readonly EnemyRoomBinding _room;
    private readonly EnemyTerrain _terrain;
    private readonly EnemyCollision _collision;
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    private readonly WalkingSpacePirateBehavior _walkingSpacePirateBehavior;

    internal WallSpacePirateBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops, WalkingSpacePirateBehavior walkingSpacePirateBehavior)
    {
        _walkingSpacePirateBehavior = walkingSpacePirateBehavior;
        _room = room;
        _terrain = terrain;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    private const int WallPirateSamusDetectionBand = 32;
    private const int WallPirateLaserMuzzleYOffset = 16;
    private const ushort WallPirateLaserSound = 0x0067;
    private const ushort WallPirateJumpSound = 0x0066;

    internal readonly WallSpacePirateEnemyState?[] _wallSpacePirateStates =
        new WallSpacePirateEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    internal static bool IsWallSpacePirateDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.PirateGreyWall or
        EnemyDefinitionId.PirateGreenWall or
        EnemyDefinitionId.PirateRedWall or
        EnemyDefinitionId.PirateGoldWall or
        EnemyDefinitionId.PirateMagentaWall or
        EnemyDefinitionId.PirateSilverWall;

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

        slot.CurrentInstruction = startsOnRight
            ? WallSpacePirateInstructionProgramDefinitions.MovingDownRightWall
            : WallSpacePirateInstructionProgramDefinitions.MovingDownLeftWall;
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
                        WallSpacePirateInstructionProgramDefinitions.FireAndJumpRight);
                }
                return;

            case WallSpacePirateFunction.ClimbingRightWall:
                if (WallSpacePirateCanAttack(slot, RequireSamus()))
                {
                    InstallWallSpacePirateInstruction(
                        slot,
                        WallSpacePirateInstructionProgramDefinitions.FireAndJumpLeft);
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
            WallSpacePirateInstructionProgramDefinitions.LandedOnRightWall);
        SnapWallSpacePirateXToTile(slot);
    }

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
            WallSpacePirateInstructionProgramDefinitions.LandedOnLeftWall);
        SnapWallSpacePirateXToTile(slot);
    }

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

    private static bool WallSpacePirateCanAttack(RoomEnemySlot slot, SamusState samus) =>
        Math.Abs(unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))) <
            WallPirateSamusDetectionBand;

    private static void SnapWallSpacePirateXToTile(RoomEnemySlot slot)
    {
        // `$B2:EFFF` deliberately has two alignment grids. Nibbles 0..10 round down to an
        // eight-pixel boundary; 11..15 round up to the next sixteen-pixel boundary.
        slot.Position = slot.Position.WithWholeX((slot.Position.X.Whole & 0x000f) < 11
            ? unchecked((ushort)(slot.Position.X.Whole & 0xfff8))
            : unchecked((ushort)((slot.Position.X.Whole & 0xfff0) + 16)));
    }

    private static void InstallWallSpacePirateInstruction(
        RoomEnemySlot slot,
        ushort instructionPointer)
    {
        slot.CurrentInstruction = instructionPointer;
        slot.InstructionTimer = NativeCountdown.Start(1);
    }

    private void MoveWallSpacePirateAndReverseOnCollision(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        RoomLevelData? level,
        ushort signedPixelDisplacement,
        bool onRightWall,
        ref ushort cursor)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                "Wall Space Pirate climbing requires room collision data.");
        }

        int displacement = SnesSixteenSixteen.FromWhole((short)signedPixelDisplacement);
        if (!_terrain.MoveEnemyVertically(level, slot, displacement))
        {
            cursor = unchecked((ushort)(cursor + 4));
            return;
        }

        // A collision retains the collision helper's clamped position, flips only bit zero,
        // and returns a fresh list pointer. It does not resume after the opcode operand.
        state.ClimbDirection = state.ClimbDirection == WallSpacePirateClimbDirection.Down
            ? WallSpacePirateClimbDirection.Up
            : WallSpacePirateClimbDirection.Down;
        cursor = SelectWallSpacePirateClimbList(onRightWall, state.ClimbDirection);
    }

    private void RandomizeWallSpacePirateClimbDirection(
        WallSpacePirateEnemyState state,
        bool onRightWall,
        ref ushort cursor)
    {
        state.ClimbDirection = (_room.Random.Generate() & 1) == 0
            ? WallSpacePirateClimbDirection.Down
            : WallSpacePirateClimbDirection.Up;
        cursor = SelectWallSpacePirateClimbList(onRightWall, state.ClimbDirection);
    }

    private static ushort SelectWallSpacePirateClimbList(
        bool onRightWall,
        WallSpacePirateClimbDirection direction) => (onRightWall, direction) switch
    {
        (false, WallSpacePirateClimbDirection.Down) =>
            WallSpacePirateInstructionProgramDefinitions.MovingDownLeftWall,
        (false, WallSpacePirateClimbDirection.Up) =>
            WallSpacePirateInstructionProgramDefinitions.MovingUpLeftWall,
        (true, WallSpacePirateClimbDirection.Down) =>
            WallSpacePirateInstructionProgramDefinitions.MovingDownRightWall,
        (true, WallSpacePirateClimbDirection.Up) =>
            WallSpacePirateInstructionProgramDefinitions.MovingUpRightWall,
        _ => throw new InvalidOperationException(
            $"Wall Pirate climb direction ${(ushort)direction} is not binary."),
    };

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
            _walkingSpacePirateBehavior.LastSpacePirateSoundEffect = WallPirateLaserSound;
        }
    }

    /// <summary>
    /// Executes the nine wall-Pirate-specific instruction opcodes. Keeping family bytecode
    /// here prevents the common interpreter from becoming a second monolithic AI switch.
    /// </summary>
    private bool TryProcessWallSpacePirateInstruction(
        RoomEnemySlot slot,
        RoomLevelData? level,
        ushort opcode,
        ref ushort cursor)
    {
        if (!IsWallSpacePirateDefinition(slot.EnemyDefinitionPointer))
            return false;

        // The shared interpreter hands every remaining negative word here; only
        // these wall-Pirate opcodes belong to this handler.
        if ((SpacePirateInstruction)opcode is not (
            SpacePirateInstruction.PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Left or
            SpacePirateInstruction.PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Right or
            SpacePirateInstruction.PirateWall_RandomlyChooseADirection_LeftWall or
            SpacePirateInstruction.PirateWall_RandomlyChooseADirection_RightWall or
            SpacePirateInstruction.PirateWall_PrepareWallJumpToRight or
            SpacePirateInstruction.PirateWall_PrepareWallJumpToLeft or
            SpacePirateInstruction.PirateWall_FireLaserLeft or
            SpacePirateInstruction.PirateWall_FireLaserRight or
            SpacePirateInstruction.PirateWall_FunctionInY or
            SpacePirateInstruction.PirateWall_QueueSpacePirateAttackSFX))
            return false;

        WallSpacePirateEnemyState state = RequireWallSpacePirateState(slot);
        switch ((SpacePirateInstruction)opcode)
        {
            case SpacePirateInstruction.PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Left:
                MoveWallSpacePirateAndReverseOnCollision(
                    slot,
                    state,
                    level,
                    EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2))),
                    onRightWall: false,
                    ref cursor);
                return true;

            case SpacePirateInstruction.PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Right:
                MoveWallSpacePirateAndReverseOnCollision(
                    slot,
                    state,
                    level,
                    EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2))),
                    onRightWall: true,
                    ref cursor);
                return true;

            case SpacePirateInstruction.PirateWall_RandomlyChooseADirection_LeftWall:
                RandomizeWallSpacePirateClimbDirection(state, onRightWall: false, ref cursor);
                return true;

            case SpacePirateInstruction.PirateWall_RandomlyChooseADirection_RightWall:
                RandomizeWallSpacePirateClimbDirection(state, onRightWall: true, ref cursor);
                return true;

            case SpacePirateInstruction.PirateWall_PrepareWallJumpToRight:
                PrepareWallSpacePirateJump(slot, state, jumpingRight: true);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case SpacePirateInstruction.PirateWall_PrepareWallJumpToLeft:
                PrepareWallSpacePirateJump(slot, state, jumpingRight: false);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case SpacePirateInstruction.PirateWall_FireLaserLeft:
                FireWallSpacePirateLaser(slot, state, movingRight: false);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case SpacePirateInstruction.PirateWall_FireLaserRight:
                FireWallSpacePirateLaser(slot, state, movingRight: true);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case SpacePirateInstruction.PirateWall_FunctionInY:
                state.Function = (WallSpacePirateFunction)EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                    slot,
                    unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case SpacePirateInstruction.PirateWall_QueueSpacePirateAttackSFX:
                _walkingSpacePirateBehavior.LastSpacePirateSoundEffect = WallPirateJumpSound;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            default:
                throw new InvalidOperationException($"Wall Space Pirate does not own instruction ${opcode:X4}.");
        }
    }

    private WallSpacePirateEnemyState RequireWallSpacePirateState(RoomEnemySlot slot) =>
        _wallSpacePirateStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized wall Space Pirate state.");

    internal void RegisterWallSpacePirateInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_PirateWall, slot => IsWallSpacePirateDefinition(slot.EnemyDefinitionPointer), (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeWallSpacePirate(slot);
        });
    }

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

    internal void RegisterWallSpacePirateOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessWallSpacePirateInstruction(slot, context.Level, word, ref cursor);
        });
    }

    internal static void RegisterWallSpacePiratePowerBombReactions(EnemyAiTable table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => IsOrdinarySpacePirateDefinition(enemy.EnemyDefinitionPointer) &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.SpacePiratePowerBomb));
    }

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

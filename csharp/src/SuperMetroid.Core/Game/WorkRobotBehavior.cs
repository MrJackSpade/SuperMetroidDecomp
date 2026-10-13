using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The work robot species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class WorkRobotBehavior : IEnemySpecies
{
    /// <summary>Checks Phantoon's defeat flag and samples the room random word for laser timing.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Moves the robot along floors and probes its next foothold.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates lasers from the common enemy projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Writes the shared animated robot palette into its OBJ color row.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Binds robot locomotion, power state, lasers, and palette animation to room services.</summary>
    internal WorkRobotBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyPresentation presentation)
    {
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _presentation = presentation;
    }

    /// <summary>Native signed eight-eight leftward laser speed.</summary>
    private const ushort WorkRobotFacingLeftVelocity = 0xfe00;
    /// <summary>Native signed eight-eight rightward laser speed.</summary>
    private const ushort WorkRobotFacingRightVelocity = 0x0200;
    /// <summary>Pixels advanced by one robot walk instruction.</summary>
    private const int WorkRobotStepPixels = 4;
    /// <summary>Library-two cue requested by the footstep opcode.</summary>
    private const ushort WorkRobotFootstepSound = 0x0068;

    /// <summary>Movement, recoil, and laser cooldown state at native enemy slot indices.</summary>
    internal readonly WorkRobotEnemyState?[] _workRobotStates =
        new WorkRobotEnemyState?[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Shared countdown to the next robot palette animation record.</summary>
    internal NativeCountdown _workRobotPaletteAnimationTimer;
    /// <summary>Offset of the next native robot palette record.</summary>
    internal ushort _workRobotPaletteAnimationTableOffset;
    /// <summary>OBJ palette index selected by the robot graphics-drawn hook.</summary>
    internal ushort _workRobotPaletteAnimationPaletteIndex;

    /// <summary>
    /// Most recent library-two sound requested by robot feet ($68) or a robot laser ($67).
    /// It is cleared at the beginning of every enemy frame, like the other audio seams.
    /// </summary>
    public ushort? LastWorkRobotSoundEffect { get; private set; }

    /// <summary>Matches powered and unpowered Wrecked Ship robot population definitions.</summary>
    internal static bool IsWorkRobotDefinition(EnemyDefinitionId definitionPointer) =>
        definitionPointer is EnemyDefinitionId.Robot or EnemyDefinitionId.RobotNoPower;

    /// <summary>Ports <c>InitAI_Robot</c> at $A8:CB77.</summary>
    private void InitializeWorkRobot(RoomEnemySlot slot)
    {
        var state = new WorkRobotEnemyState(slot);
        _workRobotStates[slot.SlotIndex] = state;

        bool areaBossDefeated = _room.Services.Bosses.IsDefeated(BossBits.AreaBoss);
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Robot || !areaBossDefeated)
        {
            InitializeWorkRobotNoPower(slot, state);
            return;
        }

        state.Powered = true;

        // ORA #$A000 installs the common instruction bit and unknown-but-observable bit
        // $8000. Population records already carry process-off-screen $0800 in retail rooms.
        slot.Properties = slot.Properties.With(
            EnemyProperties.SolidToSamus | EnemyProperties.ProcessInstructions);
        slot.InstructionTimer = NativeCountdown.Start(4);
        slot.Timer = NativeCountdown.Start(0);
        slot.CurrentInstruction = WorkRobotInstructionProgramDefinitions.Initial;
        state.LaserXVelocity = WorkRobotFacingLeftVelocity;
        state.LaserCooldown = NativeCountdown.Start(0);

        // The graphics-drawn hook is global. Every powered initialization resets its shared
        // clock/table and remembers this actor's OBJ palette exactly as $A8:CB83-$CBCB does.
        _workRobotPaletteAnimationTimer = NativeCountdown.Start(1);
        _workRobotPaletteAnimationTableOffset = 0;
        _workRobotPaletteAnimationPaletteIndex = slot.PaletteIndex;
    }

    /// <summary>Ports <c>InitAI_RobotNoPower</c> at $A8:CBCC.</summary>
    internal void InitializeWorkRobotNoPower(RoomEnemySlot slot, WorkRobotEnemyState state)
    {
        state.Powered = false;

        // The native range check accidentally accepts parameter three even though the
        // pointer table has only three entries. Retain rather than sanitize that bug:
        // retail populations use zero/one, while a corrupt value three observes the
        // compiled first code word at $CC36 just as the cartridge would.
        if (unchecked((short)slot.Parameter1) < 0 || slot.Parameter1 >= 4)
            slot.Parameter1 = 0;
        slot.CurrentInstruction =
            WorkRobotInitializationDefinitions.GetInitialInstruction(slot.Parameter1);
        slot.Properties = slot.Properties.With(EnemyProperties.SolidToSamus);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);

        // This global write disables an earlier robot's palette hook too. Mixed powered and
        // unpowered sets are unusual, but retaining the shared timer makes order observable.
        _workRobotPaletteAnimationTimer = NativeCountdown.Start(0);
        state.YSubvelocity = 0;
        state.YVelocity = 1;

        // $A8:CC1C writes four colors to active palette RAM through a stale global palette
        // index. Room fade immediately overwrites them, and no retail deactivated robot can
        // select its own palette there. The deliberately ineffective writes are documented
        // rather than synthesized into CGRAM, matching the visible cartridge result.
    }

    /// <summary>Ports the gravity-only <c>MainAI_Robot</c> at $A8:CC36.</summary>
    private void RunWorkRobotMain(
        RoomEnemySlot slot,
        WorkRobotEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Work Robot gravity requires room collision data.");

        int displacement = YardEnemyBehavior.ComposeSignedFixed(state.YVelocity, state.YSubvelocity);
        if (_terrain.MoveEnemyVertically(level, slot, displacement))
            return;

        // Enemy instruction processing will decrement the timer later in this same frame.
        // Incrementing here cancels that tick, freezing the current art while airborne.
        slot.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(slot.InstructionTimer.RawValue + 1)));
        uint subvelocity = (uint)state.YSubvelocity + 0x8000;
        state.YSubvelocity = unchecked((ushort)subvelocity);
        state.YVelocity = unchecked((ushort)(state.YVelocity + (SnesSixteenSixteen.WholeOf(subvelocity))));
    }

    /// <summary>
    /// Handles all eighteen custom Work Robot opcodes. <paramref name="cursor"/> points at
    /// the opcode on entry and is rewritten to native Y (already advanced or redirected).
    /// </summary>
    private bool TryProcessWorkRobotInstruction(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level,
        ushort opcode,
        ref ushort cursor,
        Position16<RoomPixels> camera)
    {
        if (!IsWorkRobotDefinition(slot.EnemyDefinitionPointer))
            return false;

        WorkRobotEnemyState state = RequireWorkRobotState(slot);
        ushort next = unchecked((ushort)(cursor + 2));
        if (!Enum.IsDefined((WorkRobotInstruction)opcode))
            return false;
        WorkRobotInstruction instruction = (WorkRobotInstruction)opcode;
        switch (instruction)
        {
            case WorkRobotInstruction.FacingLeft_MoveForward_HandleWallOrFall:
                cursor = MoveWorkRobot(slot, state, samus, level, next, new WorkRobotMoveStep { HorizontalPixels = -WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingLeftVelocity, WallInstruction = WorkRobotInstructionProgramDefinitions.FacingLeftHitWallMovingForwards, CheckLedge = true, LedgeProbeDirection = -1, FallInstruction = WorkRobotInstructionProgramDefinitions.ApproachingFallLeft, FallLaserVelocity = WorkRobotFacingRightVelocity });
                return true;
            case WorkRobotInstruction.FacingLeft_MoveForward_HandleHittingWall:
                cursor = MoveWorkRobot(slot, state, samus, level, next, new WorkRobotMoveStep { HorizontalPixels = -WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingLeftVelocity, WallInstruction = WorkRobotInstructionProgramDefinitions.FacingLeftHitWallMovingForwards, CheckLedge = false, LedgeProbeDirection = 0, FallInstruction = 0, FallLaserVelocity = 0 });
                return true;
            case WorkRobotInstruction.FacingLeft_MoveBackward_HandleWallOrFall:
                cursor = MoveWorkRobot(slot, state, samus, level, next, new WorkRobotMoveStep { HorizontalPixels = WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingLeftVelocity, WallInstruction = WorkRobotInstructionProgramDefinitions.FacingLeftWalkingForwards, CheckLedge = true, LedgeProbeDirection = 1, FallInstruction = WorkRobotInstructionProgramDefinitions.ApproachingFallRight, FallLaserVelocity = WorkRobotFacingLeftVelocity });
                return true;
            case WorkRobotInstruction.FacingLeft_MoveBackward_HandleHittingWall:
                cursor = MoveWorkRobot(slot, state, samus, level, next, new WorkRobotMoveStep { HorizontalPixels = WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingLeftVelocity, WallInstruction = WorkRobotInstructionProgramDefinitions.FacingLeftWalkingForwards, CheckLedge = false, LedgeProbeDirection = 0, FallInstruction = 0, FallLaserVelocity = 0 });
                return true;
            case WorkRobotInstruction.SetInstListTo_FacingRight_WalkingForwards:
                cursor = WorkRobotInstructionProgramDefinitions.FacingRightWalkingForwards;
                return true;
            case WorkRobotInstruction.FacingRight_MoveForward_HandleWallOrFall:
                cursor = MoveWorkRobot(slot, state, samus, level, next, new WorkRobotMoveStep { HorizontalPixels = WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingRightVelocity, WallInstruction = WorkRobotInstructionProgramDefinitions.FacingRightHitWallMovingForwards, CheckLedge = true, LedgeProbeDirection = 1, FallInstruction = WorkRobotInstructionProgramDefinitions.ApproachingFallRight, FallLaserVelocity = WorkRobotFacingLeftVelocity });
                return true;
            case WorkRobotInstruction.FacingRight_MoveForward_HandleHittingWall:
                cursor = MoveWorkRobot(slot, state, samus, level, next, new WorkRobotMoveStep { HorizontalPixels = WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingRightVelocity, WallInstruction = WorkRobotInstructionProgramDefinitions.FacingRightHitWallMovingForwards, CheckLedge = false, LedgeProbeDirection = 0, FallInstruction = 0, FallLaserVelocity = 0 });
                return true;
            case WorkRobotInstruction.FacingRight_MoveBackward_HandleWallOrFall:
                cursor = MoveWorkRobot(slot, state, samus, level, next, new WorkRobotMoveStep { HorizontalPixels = -WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingRightVelocity, WallInstruction = WorkRobotInstructionProgramDefinitions.FacingLeftWalkingForwards, CheckLedge = true, LedgeProbeDirection = -1, FallInstruction = WorkRobotInstructionProgramDefinitions.ApproachingFallLeft, FallLaserVelocity = WorkRobotFacingRightVelocity });
                return true;
            case WorkRobotInstruction.FacingRight_MoveBackward_HandleHittingWall:
                cursor = MoveWorkRobot(slot, state, samus, level, next, new WorkRobotMoveStep { HorizontalPixels = -WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingRightVelocity, WallInstruction = WorkRobotInstructionProgramDefinitions.FacingLeftWalkingForwards, CheckLedge = false, LedgeProbeDirection = 0, FallInstruction = 0, FallLaserVelocity = 0 });
                return true;
            case WorkRobotInstruction.PlaySFXIfOnScreen:
                if (IsWorkRobotOriginStrictlyOnScreen(slot, camera))
                    LastWorkRobotSoundEffect = WorkRobotFootstepSound;
                cursor = next;
                return true;
            case WorkRobotInstruction.Goto_FacingLeft_WalkingForwards:
                cursor = WorkRobotInstructionProgramDefinitions.FacingLeftWalkingForwards;
                return true;
            case WorkRobotInstruction.TryShootingLaserUpRight:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaser.UpRight,
                    WorkRobotInstructionProgramDefinitions.FacingRightShotLaserUpRight,
                    camera);
                return true;
            case WorkRobotInstruction.TryShootingLaserUpLeft:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaser.UpLeft,
                    WorkRobotInstructionProgramDefinitions.FacingLeftShotLaserUpLeft,
                    camera);
                return true;
            case WorkRobotInstruction.TryShootingLaserRight:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaser.Horizontal,
                    WorkRobotInstructionProgramDefinitions.FacingRightShotLaserRight,
                    camera);
                return true;
            case WorkRobotInstruction.TryShootingLaserLeft:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaser.Horizontal,
                    WorkRobotInstructionProgramDefinitions.FacingLeftShotLaserLeft,
                    camera);
                return true;
            case WorkRobotInstruction.TryShootingLaserDownRight:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaser.DownRight,
                    WorkRobotInstructionProgramDefinitions.FacingRightShotLaserDownRight,
                    camera);
                return true;
            case WorkRobotInstruction.TryShootingLaserDownLeft:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaser.DownLeft,
                    WorkRobotInstructionProgramDefinitions.FacingLeftShotLaserDownLeft,
                    camera);
                return true;
            case WorkRobotInstruction.DecrementLaserCooldown:
                DecrementWorkRobotLaserCooldown(state);
                cursor = next;
                return true;
            default:
                throw new InvalidOperationException($"Undefined WorkRobotInstruction {instruction}.");
        }
    }

    /// <summary>Applies one walk step, carries Samus on contact, and branches to wall or fall lists when support ends.</summary>
    private ushort MoveWorkRobot(RoomEnemySlot slot, WorkRobotEnemyState state, SamusState? samus, RoomLevelData? level, ushort nextCursor, WorkRobotMoveStep step)
    {
        if (level is null)
            throw new InvalidOperationException("Work Robot movement instruction requires room collision data.");

        DecrementWorkRobotLaserCooldown(state);
        state.LaserXVelocity = step.FacingLaserVelocity;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                SnesSixteenSixteen.FromWhole(step.HorizontalPixels)))
        {
            state.LaserCooldown = NativeCountdown.FromRaw(unchecked((ushort)(state.LaserCooldown.RawValue + 8)));
            return step.WallInstruction;
        }

        if (samus is not null && EnemyCollision.IsEnemyTouchingSamusFromBelow(slot, samus))
        {
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(SnesSignedSixteenSixteen.FromParts(unchecked((short)step.HorizontalPixels), 0));
        }

        if (!step.CheckLedge)
            return nextCursor;

        // The ROM moves only whole X/Y words for this temporary foot probe. In particular,
        // MoveEnemyDown's collision-produced Y subposition survives after Y is restored.
        state.BackupPosition = slot.Position.Whole;
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(
            slot.Position.X.Whole + step.LedgeProbeDirection * slot.Radius.X * 2)));
        bool floorFound = _terrain.MoveEnemyVertically(level, slot, 1 << 16);
        slot.Position = slot.Position.WithWhole(state.BackupPosition);
        if (floorFound)
            return nextCursor;

        state.LaserCooldown = NativeCountdown.FromRaw(unchecked((ushort)(state.LaserCooldown.RawValue + 8)));
        state.LaserXVelocity = step.FallLaserVelocity;
        return step.FallInstruction;
    }

    /// <summary>Checks the robot origin against the native screen rectangle with exclusive top and left edges.</summary>
    private static bool IsWorkRobotOriginStrictlyOnScreen(
        RoomEnemySlot robot,
        Position16<RoomPixels> camera) =>
        unchecked((short)(robot.Position.X.Whole - camera.X)) > 0 &&
        unchecked((short)(camera.X + 256 - robot.Position.X.Whole)) >= 0 &&
        unchecked((short)(robot.Position.Y.Whole - camera.Y)) > 0 &&
        unchecked((short)(camera.Y + 224 - robot.Position.Y.Whole)) >= 0;

    /// <summary>Uses the existing random word to reset cooldown and enters the firing list when ready.</summary>
    private ushort TryFireWorkRobotLaser(
        RoomEnemySlot robot,
        WorkRobotEnemyState state,
        ushort nextCursor,
        WorkRobotLaser projectileDefinition,
        ushort firingInstruction,
        Position16<RoomPixels> camera)
    {
        if (!state.LaserCooldown.IsZero)
        {
            DecrementWorkRobotLaserCooldown(state);
            return nextCursor;
        }

        Func<ushort> readRandomNumber = _room.Random.Read;
        state.LaserCooldown = NativeCountdown.FromRaw(unchecked((ushort)((readRandomNumber() & 0x001f) + 0x0010)));
        SpawnWorkRobotLaser(robot, state, projectileDefinition, camera);
        return firingInstruction;
    }

    /// <summary>Counts a nonzero laser cooldown down without unsigned wraparound.</summary>
    private static void DecrementWorkRobotLaserCooldown(WorkRobotEnemyState state)
    {
        state.LaserCooldown = state.LaserCooldown.DecrementToZero();
    }

    /// <summary>
    /// Ports custom touch $A8:D174. Work Robots are moving solids: body contact publishes a
    /// four-pixel external push away from their center and deliberately deals no touch damage.
    /// </summary>
    private static void ResolveWorkRobotTouch(RoomEnemySlot robot, SamusState samus)
    {
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(SnesSignedSixteenSixteen.FromParts(
            robot.Position.X.Whole < samus.Position.X.Whole ? (short)4 : (short)-4, 0));
    }

    /// <summary>
    /// Ports the private tail of powered shot AI $A8:D192 after common shot handling. The
    /// indestructible vulnerability table leaves health unchanged, but the accepted impact
    /// still chooses an ahead/behind recoil list and delays the next laser by 64 frames.
    /// </summary>
    private void ResolveWorkRobotShotAfterCommon(RoomEnemySlot robot, SamusState? samus)
    {
        if (robot.EnemyDefinitionPointer != EnemyDefinitionId.Robot || robot.Health == 0)
            return;
        if (!_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
            return;
        if (samus is null)
            throw new InvalidOperationException("Work Robot shot recoil requires the active Samus actor.");

        WorkRobotEnemyState state = RequireWorkRobotState(robot);
        bool facingLeft = unchecked((short)state.LaserXVelocity) < 0;
        if (facingLeft)
        {
            robot.CurrentInstruction = samus.Position.X.Whole < robot.Position.X.Whole
                ? WorkRobotInstructionProgramDefinitions.FacingLeftShotSamusAhead
                : WorkRobotInstructionProgramDefinitions.FacingLeftShotSamusBehind;
        }
        else
        {
            robot.CurrentInstruction = samus.Position.X.Whole >= robot.Position.X.Whole
                ? WorkRobotInstructionProgramDefinitions.FacingRightShotSamusAhead
                : WorkRobotInstructionProgramDefinitions.FacingRightShotSamusBehind;
        }
        robot.InstructionTimer = NativeCountdown.Start(1);
        state.LaserCooldown = NativeCountdown.FromRaw(unchecked((ushort)(state.LaserCooldown.RawValue + 0x0040)));
    }

    /// <summary>Ports the global graphics-drawn palette hook at $A8:CC67.</summary>
    internal void StepWorkRobotPaletteAnimation()
    {
        if (_workRobotPaletteAnimationTimer.IsZero)
            return;
        _workRobotPaletteAnimationTimer = _workRobotPaletteAnimationTimer.Decrement().Remaining;
        if (!_workRobotPaletteAnimationTimer.IsZero)
            return;

        _workRobotPaletteAnimationTableOffset =
            WorkRobotPaletteTimingDefinitions.NormalizeByteOffset(
                _workRobotPaletteAnimationTableOffset);
        int record = _workRobotPaletteAnimationTableOffset /
            WorkRobotPaletteTimingDefinitions.RecordByteCount;

        int destination = 128 +
            ((_workRobotPaletteAnimationPaletteIndex >> 9) & 7) * 16 +
            WorkRobotPaletteRomData.FirstAnimatedColor;
        (_room.Artwork?.WorkRobotPaletteCycle ?? throw new InvalidDataException(
            "Work Robot palette animation requires installed artwork."))
            .ApplyFrame(_presentation.Cgram, record, destination);
        _workRobotPaletteAnimationTimer =
            NativeCountdown.FromRaw(WorkRobotPaletteTimingDefinitions.DurationForByteOffset(
                _workRobotPaletteAnimationTableOffset));
        _workRobotPaletteAnimationTableOffset = unchecked((ushort)(
            _workRobotPaletteAnimationTableOffset + 10));
    }

    /// <summary>Rejects a robot callback before its slot initializes movement and laser state.</summary>
    private WorkRobotEnemyState RequireWorkRobotState(RoomEnemySlot slot) =>
        _workRobotStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Work Robot state.");

    /// <summary>Rejects powered-robot bomb damage before Phantoon's defeat and applies recoil after admitted hits.</summary>
    internal void RegisterWorkRobotBombReactions(EnemyAiTable table)
    {
        // Powered Work Robot returns until Phantoon is dead. Bank $A0 has already marked the bomb, so callback
        // rejection belongs here rather than in collision admission.
        table.AddBombCommon(new EnemyBombReaction(c => IsWorkRobotDefinition(c.Enemy.EnemyDefinitionPointer) &&
                                                       c.SelectedShotAi is EnemyInteractionCallback.WorkRobotShot or EnemyInteractionCallback.WorkRobotNoPowerShot)
        {
            Rejects = c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Robot &&
                           c.SelectedShotAi == EnemyInteractionCallback.WorkRobotShot &&
                           !_room.Services.Bosses.IsDefeated(BossBits.AreaBoss),
            AfterCommon = c => ResolveWorkRobotShotAfterCommon(c.Enemy, c.Samus),
        });
    }

    /// <summary>Connects robot lasers to collision-first movement in their projectile pre-instruction.</summary>
    internal static void RegisterWorkRobotProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunWorkRobotLaserPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.WreckedShipRobotLaser);
    }

    /// <summary>Shares native initialization between powered and unpowered robot definitions.</summary>
    internal void RegisterWorkRobotInitializationAi(EnemyAiTable table)
    {

        EnemyInitializationAiHandler shared74 = (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeWorkRobot(slot);
        };

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Robot, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Robot, shared74);

        table.RegisterInitialization(EnemyAiRoutine.InitAI_RobotNoPower, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.RobotNoPower, shared74);
    }

    /// <summary>Runs powered robot gravity and movement AI with current room terrain.</summary>
    internal void RegisterWorkRobotMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Robot, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Robot, (slot, frame, nmiFrameCounter8) =>
        {
            RunWorkRobotMain(slot, RequireWorkRobotState(slot), frame.Level);
        });
    }

    /// <summary>Claims robot movement, recoil, footstep, and laser opcodes before common animation dispatch.</summary>
    internal void RegisterWorkRobotOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessWorkRobotInstruction(slot, context.Samus, context.Level, word, ref cursor, context.Camera);
        });
    }

    /// <summary>Applies the powered robot's accepted-impact recoil tail after common projectile shots.</summary>
    internal void RegisterWorkRobotShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => IsWorkRobotDefinition(enemy.EnemyDefinitionPointer) &&
                                                     EnemyCollision.ShotAi(enemy) is EnemyInteractionCallback.WorkRobotShot or EnemyInteractionCallback.WorkRobotNoPowerShot)
        {
            AfterCommon = (c, _) => ResolveWorkRobotShotAfterCommon(c.Enemy, c.Samus),
        });
    }

    /// <summary>Routes robot contact to its four-pixel external push without Samus damage.</summary>
    internal static void RegisterWorkRobotTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => IsWorkRobotDefinition(slot.EnemyDefinitionPointer) &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.WorkRobotTouch,
            context => ResolveWorkRobotTouch(context.Slot, context.Samus));
    }
    /// <summary>Library-two cue requested after a robot laser projectile is allocated.</summary>
    private const ushort WorkRobotLaserSound = 0x0067;

    /// <summary>
    /// Allocates one of bank $86's five Work Robot laser definitions. Horizontal left/right
    /// share $D2B4; the sign in the owner's laser velocity supplies the sixth direction.
    /// </summary>
    internal void SpawnWorkRobotLaser(
        RoomEnemySlot robot,
        WorkRobotEnemyState state,
        WorkRobotLaser definition,
        Position16<RoomPixels> camera)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        // SpawnEnemyProjectileY_ParameterA_XGraphics installs the complete seven-word
        // definition before entering the family initializer. Keeping that division here is
        // important: the five records do not share radii or damage, and the common helper
        // also supplies the native $8000 pre-draw map sentinel and all three property bits.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            (RoomEnemyProjectileKind)definition,
            unchecked((ushort)(robot.VramTilesIndex | robot.PaletteIndex)));
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(state.LaserXVelocity));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(definition switch
        {
            WorkRobotLaser.UpLeft or WorkRobotLaser.UpRight =>
                unchecked((ushort)-0x0080),
            WorkRobotLaser.DownLeft or WorkRobotLaser.DownRight => 0x0080,
            WorkRobotLaser.Horizontal => 0,
            _ => throw new InvalidOperationException(
                $"Undefined Work Robot laser definition {definition}."),
        }));

        // All five initializers place the muzzle four pixels in the signed facing direction
        // and sixteen pixels above the robot. The temporary graphics word comes from the
        // owner; pre-instruction $D3BF clears it before the first draw, matching the ROM's
        // use of palette/tile bits embedded directly in the bank-$8D laser spritemaps.
        projectile.Position = new(new(unchecked((ushort)(robot.Position.X.Whole +
            (unchecked((short)state.LaserXVelocity) < 0 ? -4 : 4))), 0), new(unchecked((ushort)(robot.Position.Y.Whole - 16)), 0));
        // $86:D326 clears the graphics word in the down-left/right initializer itself.
        // Upward and horizontal variants retain the owner's word until common
        // pre-instruction $D3BF clears it on their first projectile pass.
        if (definition is
            WorkRobotLaser.DownLeft or WorkRobotLaser.DownRight)
            projectile.GraphicsIndex = 0;

        // Preserve the original $86:D35B viewport bug. Its final Y comparison omits
        // `CMP Layer1YPosition`, so lasers only request sound while their owner's absolute
        // top edge is above scanline 224, even when a lower room row is on screen.
        bool horizontallyVisible =
            unchecked((short)(robot.Position.X.Whole + robot.Radius.X - camera.X)) >= 0 &&
            unchecked((short)(robot.Position.X.Whole - robot.Radius.X - 0x0101 - camera.X)) < 0;
        bool verticallyVisible =
            unchecked((short)(robot.Position.Y.Whole + robot.Radius.Y - camera.Y)) >= 0 &&
            unchecked((short)(robot.Position.Y.Whole - robot.Radius.Y - 0x00e0)) < 0;
        if (horizontallyVisible && verticallyVisible)
            LastWorkRobotSoundEffect = WorkRobotLaserSound;
    }

    /// <summary>Ports pre-instruction $86:D3BF: X collision, then Y collision, else move.</summary>
    internal static void RunWorkRobotLaserPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        projectile.GraphicsIndex = 0;
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Clear();
        }
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastWorkRobotSoundEffect = null;
        Array.Clear(_workRobotStates);
        _workRobotPaletteAnimationTimer = NativeCountdown.Start(0);
        _workRobotPaletteAnimationTableOffset = 0;
        _workRobotPaletteAnimationPaletteIndex = 0;
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastWorkRobotSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastWorkRobotSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }


    /// <summary>Registers the unpowered robot's native RTS main routine.</summary>
    internal static void RegisterUnpoweredWorkRobotMainAi(EnemyAiTable table)
    {
        table.RegisterMain(EnemyAiRoutine.RTL_A8CC66, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.RobotNoPower, (slot, frame, nmiFrameCounter8) =>
        { });
    }
}

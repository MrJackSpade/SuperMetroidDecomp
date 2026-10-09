using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-visible names for the six common enemy words aliased by bank-$A8's Work Robot.
/// The values deliberately remain native words: laser speed is signed 8.8, while falling
/// speed is split into the same signed-whole/fraction pair consumed by bank $A0 movement.
/// </summary>
public sealed class WorkRobotEnemyState
{
    /// <summary>Room slot containing the native common words exposed by this state view.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a typed view over the common variables of an initialized Work Robot slot.</summary>
    /// <param name="slot">Live enemy slot whose cartridge variables back this state.</param>
    internal WorkRobotEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// True only when definition $E8FF saw the area's boss bit during initialization.
    /// Definition $E8FF can therefore own a deactivated state before Phantoon is defeated.
    /// </summary>
    public bool Powered { get; internal set; }

    /// <summary>Gets or sets variable A, the signed 8.8 horizontal velocity assigned to newly fired robot lasers.</summary>
    public ushort LaserXVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Gets or sets variable B, the wrapping countdown that gates the next laser attempt.</summary>
    public ushort LaserCooldown
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Gets or sets variable C, the whole-pixel X coordinate saved during a temporary ledge probe.</summary>
    public ushort BackupXPosition
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Gets or sets variable D, the whole-pixel Y coordinate saved during a temporary ledge probe.</summary>
    public ushort BackupYPosition
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Gets or sets variable E, the fractional low word of the robot's signed 16.16 falling velocity.</summary>
    public ushort YSubvelocity
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Gets or sets variable F, the signed whole-pixel word of the robot's 16.16 falling velocity.</summary>
    public ushort YVelocity
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }
}

/// <summary>
/// Literal translation of powered Work Robot $E8FF and deactivated Work Robot $E93F from
/// <c>$A8:C6B3-$D1EF</c>. Locomotion really is animation bytecode: main AI only applies
/// gravity, and the instruction lists decide when each four-pixel step, turn, shot, recoil,
/// footstep, wall check, and ledge check occurs.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Bank-$A8 enemy definition pointer for the powered Work Robot.</summary>
    internal const ushort WorkRobotDefinition = 0xe8ff;

    /// <summary>Bank-$A8 enemy definition pointer for the Work Robot initialized without power.</summary>
    internal const ushort WorkRobotNoPowerDefinition = 0xe93f;

    /// <summary>Signed 8.8 laser X velocity assigned while the robot faces left.</summary>
    private const ushort WorkRobotFacingLeftVelocity = 0xfe00;

    /// <summary>Signed 8.8 laser X velocity assigned while the robot faces right.</summary>
    private const ushort WorkRobotFacingRightVelocity = 0x0200;

    /// <summary>Whole-pixel distance moved by each scripted walking step.</summary>
    private const int WorkRobotStepPixels = 4;

    /// <summary>Library-two sound requested when an on-screen robot footstep executes.</summary>
    private const ushort WorkRobotFootstepSound = 0x0068;

    /// <summary>Typed variable views indexed by native enemy slot, cleared between rooms.</summary>
    private readonly WorkRobotEnemyState?[] _workRobotStates =
        new WorkRobotEnemyState?[MaximumEnemyCount];

    /// <summary>Shared countdown for the active Work Robot palette animation hook.</summary>
    private ushort _workRobotPaletteAnimationTimer;

    /// <summary>Byte offset of the next timing record in the shared palette animation sequence.</summary>
    private ushort _workRobotPaletteAnimationTableOffset;

    /// <summary>OBJ palette index captured by powered initialization for the shared animation hook.</summary>
    private ushort _workRobotPaletteAnimationPaletteIndex;

    /// <summary>
    /// Most recent library-two sound requested by robot feet ($68) or a robot laser ($67).
    /// It is cleared at the beginning of every enemy frame, like the other audio seams.
    /// </summary>
    public ushort? LastWorkRobotSoundEffect { get; private set; }

    /// <summary>Reports whether a population record uses either powered or unpowered Work Robot data.</summary>
    /// <param name="definitionPointer">Enemy definition pointer read from the population record.</param>
    private static bool IsWorkRobotDefinition(ushort definitionPointer) =>
        definitionPointer is WorkRobotDefinition or WorkRobotNoPowerDefinition;

    /// <summary>Ports <c>InitAI_Robot</c> at $A8:CB77.</summary>
    private void InitializeWorkRobot(RoomEnemySlot slot)
    {
        var state = new WorkRobotEnemyState(slot);
        _workRobotStates[slot.SlotIndex] = state;

        bool areaBossDefeated = RequireAreaBossDefeated();
        if (slot.EnemyDefinitionPointer != WorkRobotDefinition || !areaBossDefeated)
        {
            InitializeWorkRobotNoPower(slot, state);
            return;
        }

        state.Powered = true;

        // ORA #$A000 installs the common instruction bit and unknown-but-observable bit
        // $8000. Population records already carry process-off-screen $0800 in retail rooms.
        slot.Properties = slot.Properties.With(
            EnemyProperties.SolidToSamus | EnemyProperties.ProcessInstructions);
        slot.InstructionTimer = 4;
        slot.Timer = 0;
        slot.CurrentInstruction = WorkRobotInstructionProgramDefinitions.Initial;
        state.LaserXVelocity = WorkRobotFacingLeftVelocity;
        state.LaserCooldown = 0;

        // The graphics-drawn hook is global. Every powered initialization resets its shared
        // clock/table and remembers this actor's OBJ palette exactly as $A8:CB83-$CBCB does.
        _workRobotPaletteAnimationTimer = 1;
        _workRobotPaletteAnimationTableOffset = 0;
        _workRobotPaletteAnimationPaletteIndex = slot.PaletteIndex;
    }

    /// <summary>Ports <c>InitAI_RobotNoPower</c> at $A8:CBCC.</summary>
    private void InitializeWorkRobotNoPower(RoomEnemySlot slot, WorkRobotEnemyState state)
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
        slot.InstructionTimer = 1;
        slot.Timer = 0;

        // This global write disables an earlier robot's palette hook too. Mixed powered and
        // unpowered sets are unusual, but retaining the shared timer makes order observable.
        _workRobotPaletteAnimationTimer = 0;
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

        int displacement = ComposeSignedFixed(state.YVelocity, state.YSubvelocity);
        if (MoveEnemyVertically(level, slot, displacement))
            return;

        // Enemy instruction processing will decrement the timer later in this same frame.
        // Incrementing here cancels that tick, freezing the current art while airborne.
        slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer + 1));
        uint subvelocity = (uint)state.YSubvelocity + 0x8000;
        state.YSubvelocity = unchecked((ushort)subvelocity);
        state.YVelocity = unchecked((ushort)(state.YVelocity + (subvelocity >> 16)));
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
        ushort cameraX,
        ushort cameraY)
    {
        if (!IsWorkRobotDefinition(slot.EnemyDefinitionPointer))
            return false;

        WorkRobotEnemyState state = RequireWorkRobotState(slot);
        ushort next = unchecked((ushort)(cursor + 2));
        switch (opcode)
        {
            case WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveForward_HandleWallOrFall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, -WorkRobotStepPixels,
                    WorkRobotFacingLeftVelocity,
                    WorkRobotInstructionProgramDefinitions.FacingLeftHitWallMovingForwards,
                    checkLedge: true, ledgeProbeDirection: -1,
                    WorkRobotInstructionProgramDefinitions.ApproachingFallLeft,
                    WorkRobotFacingRightVelocity);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveForward_HandleHittingWall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, -WorkRobotStepPixels,
                    WorkRobotFacingLeftVelocity,
                    WorkRobotInstructionProgramDefinitions.FacingLeftHitWallMovingForwards,
                    checkLedge: false, 0, 0, 0);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveBackward_HandleWallOrFall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, WorkRobotStepPixels,
                    WorkRobotFacingLeftVelocity,
                    WorkRobotInstructionProgramDefinitions.FacingLeftWalkingForwards,
                    checkLedge: true, ledgeProbeDirection: 1,
                    WorkRobotInstructionProgramDefinitions.ApproachingFallRight,
                    WorkRobotFacingLeftVelocity);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveBackward_HandleHittingWall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, WorkRobotStepPixels,
                    WorkRobotFacingLeftVelocity,
                    WorkRobotInstructionProgramDefinitions.FacingLeftWalkingForwards,
                    checkLedge: false, 0, 0, 0);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_SetInstListTo_FacingRight_WalkingForwards:
                cursor = WorkRobotInstructionProgramDefinitions.FacingRightWalkingForwards;
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveForward_HandleWallOrFall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, WorkRobotStepPixels,
                    WorkRobotFacingRightVelocity,
                    WorkRobotInstructionProgramDefinitions.FacingRightHitWallMovingForwards,
                    checkLedge: true, ledgeProbeDirection: 1,
                    WorkRobotInstructionProgramDefinitions.ApproachingFallRight,
                    WorkRobotFacingLeftVelocity);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveForward_HandleHittingWall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, WorkRobotStepPixels,
                    WorkRobotFacingRightVelocity,
                    WorkRobotInstructionProgramDefinitions.FacingRightHitWallMovingForwards,
                    checkLedge: false, 0, 0, 0);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveBackward_HandleWallOrFall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, -WorkRobotStepPixels,
                    WorkRobotFacingRightVelocity,
                    WorkRobotInstructionProgramDefinitions.FacingLeftWalkingForwards,
                    checkLedge: true, ledgeProbeDirection: -1,
                    WorkRobotInstructionProgramDefinitions.ApproachingFallLeft,
                    WorkRobotFacingRightVelocity);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveBackward_HandleHittingWall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, -WorkRobotStepPixels,
                    WorkRobotFacingRightVelocity,
                    WorkRobotInstructionProgramDefinitions.FacingLeftWalkingForwards,
                    checkLedge: false, 0, 0, 0);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_PlaySFXIfOnScreen:
                if (IsWorkRobotOriginStrictlyOnScreen(slot, cameraX, cameraY))
                    LastWorkRobotSoundEffect = WorkRobotFootstepSound;
                cursor = next;
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_Goto_FacingLeft_WalkingForwards:
                cursor = WorkRobotInstructionProgramDefinitions.FacingLeftWalkingForwards;
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserUpRight:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaserDefinitions.UpRight,
                    WorkRobotInstructionProgramDefinitions.FacingRightShotLaserUpRight,
                    cameraX, cameraY);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserUpLeft:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaserDefinitions.UpLeft,
                    WorkRobotInstructionProgramDefinitions.FacingLeftShotLaserUpLeft,
                    cameraX, cameraY);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserRight:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaserDefinitions.Horizontal,
                    WorkRobotInstructionProgramDefinitions.FacingRightShotLaserRight,
                    cameraX, cameraY);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserLeft:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaserDefinitions.Horizontal,
                    WorkRobotInstructionProgramDefinitions.FacingLeftShotLaserLeft,
                    cameraX, cameraY);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserDownRight:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaserDefinitions.DownRight,
                    WorkRobotInstructionProgramDefinitions.FacingRightShotLaserDownRight,
                    cameraX, cameraY);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserDownLeft:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaserDefinitions.DownLeft,
                    WorkRobotInstructionProgramDefinitions.FacingLeftShotLaserDownLeft,
                    cameraX, cameraY);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_DecrementLaserCooldown:
                DecrementWorkRobotLaserCooldown(state);
                cursor = next;
                return true;
            default:
                return false;
        }
    }

    /// <summary>Executes one scripted horizontal step, including wall response and the optional ledge probe.</summary>
    /// <param name="slot">Robot slot whose position and instruction state are updated.</param>
    /// <param name="state">Robot variables used for facing velocity, cooldown, and temporary probe coordinates.</param>
    /// <param name="samus">Active Samus state for moving-solid contact, when present.</param>
    /// <param name="level">Room collision data required to move and probe the robot.</param>
    /// <param name="nextCursor">Instruction pointer to resume when the step continues normally.</param>
    /// <param name="horizontalPixels">Signed whole-pixel movement for this step.</param>
    /// <param name="facingLaserVelocity">Laser X velocity corresponding to the robot's current facing.</param>
    /// <param name="wallInstruction">Instruction list selected when horizontal movement hits a wall.</param>
    /// <param name="checkLedge">Whether to probe for floor beyond the robot's feet after moving.</param>
    /// <param name="ledgeProbeDirection">Horizontal direction used by the temporary foot probe.</param>
    /// <param name="fallInstruction">Instruction list selected when the ledge probe finds no floor.</param>
    /// <param name="fallLaserVelocity">Laser X velocity installed when the robot begins a fall.</param>
    /// <returns>The next instruction pointer chosen by normal movement, wall collision, or the ledge check.</returns>
    private ushort MoveWorkRobot(
        RoomEnemySlot slot,
        WorkRobotEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        ushort nextCursor,
        int horizontalPixels,
        ushort facingLaserVelocity,
        ushort wallInstruction,
        bool checkLedge,
        int ledgeProbeDirection,
        ushort fallInstruction,
        ushort fallLaserVelocity)
    {
        if (level is null)
            throw new InvalidOperationException("Work Robot movement instruction requires room collision data.");

        DecrementWorkRobotLaserCooldown(state);
        state.LaserXVelocity = facingLaserVelocity;
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                horizontalPixels << 16))
        {
            state.LaserCooldown = unchecked((ushort)(state.LaserCooldown + 8));
            return wallInstruction;
        }

        if (samus is not null && IsEnemyTouchingSamusFromBelow(slot, samus))
        {
            samus.Kinematics.ExtraXSubdisplacement = 0;
            samus.Kinematics.ExtraXDisplacement = unchecked((ushort)horizontalPixels);
        }

        if (!checkLedge)
            return nextCursor;

        // The ROM moves only whole X/Y words for this temporary foot probe. In particular,
        // MoveEnemyDown's collision-produced Y subposition survives after Y is restored.
        state.BackupYPosition = slot.YPosition;
        state.BackupXPosition = slot.XPosition;
        slot.XPosition = unchecked((ushort)(
            slot.XPosition + ledgeProbeDirection * slot.XRadius * 2));
        bool floorFound = MoveEnemyVertically(level, slot, 1 << 16);
        slot.XPosition = state.BackupXPosition;
        slot.YPosition = state.BackupYPosition;
        if (floorFound)
            return nextCursor;

        state.LaserCooldown = unchecked((ushort)(state.LaserCooldown + 8));
        state.LaserXVelocity = fallLaserVelocity;
        return fallInstruction;
    }

    /// <summary>Tests the robot origin against the strict left/top and inclusive right/bottom camera bounds.</summary>
    /// <param name="robot">Robot slot whose world position is tested.</param>
    /// <param name="cameraX">Horizontal world coordinate of the camera's left edge.</param>
    /// <param name="cameraY">Vertical world coordinate of the camera's top edge.</param>
    /// <returns><see langword="true"/> when the origin lies inside the visible 256-by-224-pixel bounds.</returns>
    private static bool IsWorkRobotOriginStrictlyOnScreen(
        RoomEnemySlot robot,
        ushort cameraX,
        ushort cameraY) =>
        unchecked((short)(robot.XPosition - cameraX)) > 0 &&
        unchecked((short)(cameraX + 256 - robot.XPosition)) >= 0 &&
        unchecked((short)(robot.YPosition - cameraY)) > 0 &&
        unchecked((short)(cameraY + 224 - robot.YPosition)) >= 0;

    /// <summary>Consumes a laser attempt, decrementing cooldown or spawning a projectile and selecting its firing list.</summary>
    /// <param name="robot">Robot slot that may create the projectile.</param>
    /// <param name="state">Robot firing velocity and cooldown state.</param>
    /// <param name="nextCursor">Instruction pointer used when the attempt is still cooling down.</param>
    /// <param name="projectileDefinition">Compiled projectile definition to spawn after a successful attempt.</param>
    /// <param name="firingInstruction">Instruction list entered when the laser is fired.</param>
    /// <param name="cameraX">Camera X used for projectile spawn positioning.</param>
    /// <param name="cameraY">Camera Y used for projectile spawn positioning.</param>
    /// <returns>The instruction pointer for either the cooldown continuation or firing animation.</returns>
    private ushort TryFireWorkRobotLaser(
        RoomEnemySlot robot,
        WorkRobotEnemyState state,
        ushort nextCursor,
        ushort projectileDefinition,
        ushort firingInstruction,
        ushort cameraX,
        ushort cameraY)
    {
        if (state.LaserCooldown != 0)
        {
            DecrementWorkRobotLaserCooldown(state);
            return nextCursor;
        }

        Func<ushort> readRandomNumber = _readRandomNumber ??
            throw new InvalidOperationException(
                "Work Robot laser instruction requires a non-advancing random-seed reader.");
        state.LaserCooldown = unchecked((ushort)((readRandomNumber() & 0x001f) + 0x0010));
        SpawnWorkRobotLaser(robot, state, projectileDefinition, cameraX, cameraY);
        return firingInstruction;
    }

    /// <summary>Decrements a nonzero laser cooldown using the robot's wrapping word counter.</summary>
    /// <param name="state">Robot firing state whose cooldown is advanced toward zero.</param>
    private static void DecrementWorkRobotLaserCooldown(WorkRobotEnemyState state)
    {
        if (state.LaserCooldown != 0)
            state.LaserCooldown = unchecked((ushort)(state.LaserCooldown - 1));
    }

    /// <summary>
    /// Ports custom touch $A8:D174. Work Robots are moving solids: body contact publishes a
    /// four-pixel external push away from their center and deliberately deals no touch damage.
    /// </summary>
    private static void ResolveWorkRobotTouch(RoomEnemySlot robot, SamusState samus)
    {
        samus.Kinematics.ExtraXSubdisplacement = 0;
        samus.Kinematics.ExtraXDisplacement = robot.XPosition < samus.XPosition
            ? (ushort)4
            : unchecked((ushort)-4);
    }

    /// <summary>
    /// Ports the private tail of powered shot AI $A8:D192 after common shot handling. The
    /// indestructible vulnerability table leaves health unchanged, but the accepted impact
    /// still chooses an ahead/behind recoil list and delays the next laser by 64 frames.
    /// </summary>
    private void ResolveWorkRobotShotAfterCommon(RoomEnemySlot robot, SamusState? samus)
    {
        if (robot.EnemyDefinitionPointer != WorkRobotDefinition || robot.Health == 0)
            return;
        if (!RequireAreaBossDefeated())
            return;
        if (samus is null)
            throw new InvalidOperationException("Work Robot shot recoil requires the active Samus actor.");

        WorkRobotEnemyState state = RequireWorkRobotState(robot);
        bool facingLeft = unchecked((short)state.LaserXVelocity) < 0;
        if (facingLeft)
        {
            robot.CurrentInstruction = samus.XPosition < robot.XPosition
                ? WorkRobotInstructionProgramDefinitions.FacingLeftShotSamusAhead
                : WorkRobotInstructionProgramDefinitions.FacingLeftShotSamusBehind;
        }
        else
        {
            robot.CurrentInstruction = samus.XPosition >= robot.XPosition
                ? WorkRobotInstructionProgramDefinitions.FacingRightShotSamusAhead
                : WorkRobotInstructionProgramDefinitions.FacingRightShotSamusBehind;
        }
        robot.InstructionTimer = 1;
        state.LaserCooldown = unchecked((ushort)(state.LaserCooldown + 0x0040));
    }

    /// <summary>Ports the global graphics-drawn palette hook at $A8:CC67.</summary>
    private void StepWorkRobotPaletteAnimation()
    {
        if (_workRobotPaletteAnimationTimer == 0)
            return;
        _workRobotPaletteAnimationTimer = unchecked((ushort)(
            _workRobotPaletteAnimationTimer - 1));
        if (_workRobotPaletteAnimationTimer != 0)
            return;

        _workRobotPaletteAnimationTableOffset =
            WorkRobotPaletteTimingDefinitions.NormalizeByteOffset(
                _workRobotPaletteAnimationTableOffset);
        int record = _workRobotPaletteAnimationTableOffset /
            WorkRobotPaletteTimingDefinitions.RecordByteCount;

        int destination = 128 +
            ((_workRobotPaletteAnimationPaletteIndex >> 9) & 7) * 16 +
            WorkRobotPaletteRomData.FirstAnimatedColor;
        (TileArtwork?.WorkRobotPaletteCycle ?? throw new InvalidDataException(
            "Work Robot palette animation requires installed artwork."))
            .ApplyFrame(_cgram!, record, destination);
        _workRobotPaletteAnimationTimer =
            WorkRobotPaletteTimingDefinitions.DurationForByteOffset(
                _workRobotPaletteAnimationTableOffset);
        _workRobotPaletteAnimationTableOffset = unchecked((ushort)(
            _workRobotPaletteAnimationTableOffset + 10));
    }

    /// <summary>Gets the state view installed for a robot slot or reports an initialization-order violation.</summary>
    /// <param name="slot">Work Robot slot whose typed state is required.</param>
    /// <returns>The state view created during Work Robot initialization.</returns>
    /// <exception cref="InvalidOperationException">No Work Robot state has been initialized for the slot.</exception>
    private WorkRobotEnemyState RequireWorkRobotState(RoomEnemySlot slot) =>
        _workRobotStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Work Robot state.");
}

using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-visible names for the six common enemy words aliased by bank-$A8's Work Robot.
/// The values deliberately remain native words: laser speed is signed 8.8, while falling
/// speed is split into the same signed-whole/fraction pair consumed by bank $A0 movement.
/// </summary>
public sealed class WorkRobotEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal WorkRobotEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// True only when definition $E8FF saw the area's boss bit during initialization.
    /// Definition $E8FF can therefore own a deactivated state before Phantoon is defeated.
    /// </summary>
    public bool Powered { get; internal set; }

    public ushort LaserXVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    public ushort LaserCooldown
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    public ushort BackupXPosition
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    public ushort BackupYPosition
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    public ushort YSubvelocity
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

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
    internal const ushort WorkRobotDefinition = 0xe8ff;
    internal const ushort WorkRobotNoPowerDefinition = 0xe93f;

    private const ushort WorkRobotNoPowerNeutral = 0xc6d3;
    private const ushort WorkRobotInitial = 0xc6e5;
    private const ushort WorkRobotWalkLeft = 0xc6e9;
    private const ushort WorkRobotLeftHitWall = 0xc73f;
    private const ushort WorkRobotLeftShotAhead = 0xc7bb;
    private const ushort WorkRobotLeftShotBehind = 0xc833;
    private const ushort WorkRobotLaserDownLeftAnimation = 0xc8b1;
    private const ushort WorkRobotLaserLeftAnimation = 0xc8bd;
    private const ushort WorkRobotLaserUpLeftAnimation = 0xc8d1;
    private const ushort WorkRobotApproachFallRight = 0xc91b;
    private const ushort WorkRobotWalkRight = 0xc92d;
    private const ushort WorkRobotRightHitWall = 0xc985;
    private const ushort WorkRobotRightShotAhead = 0xca01;
    private const ushort WorkRobotRightShotBehind = 0xca7d;
    private const ushort WorkRobotLaserDownRightAnimation = 0xcafd;
    private const ushort WorkRobotLaserRightAnimation = 0xcb09;
    private const ushort WorkRobotLaserUpRightAnimation = 0xcb1d;
    private const ushort WorkRobotApproachFallLeft = 0xcb65;

    private const ushort WorkRobotFacingLeftVelocity = 0xfe00;
    private const ushort WorkRobotFacingRightVelocity = 0x0200;
    private const int WorkRobotStepPixels = 4;
    private const ushort WorkRobotFootstepSound = 0x0068;

    // Five projectile definitions encode six directions: the horizontal definition uses
    // the robot's signed facing velocity to choose left or right.
    private const ushort WorkRobotLaserUpLeft = 0xd2a6;
    private const ushort WorkRobotLaserHorizontal = 0xd2b4;
    private const ushort WorkRobotLaserDownLeft = 0xd2c2;
    private const ushort WorkRobotLaserUpRight = 0xd2d0;
    private const ushort WorkRobotLaserDownRight = 0xd2de;

    private readonly WorkRobotEnemyState?[] _workRobotStates =
        new WorkRobotEnemyState?[MaximumEnemyCount];
    private ushort _workRobotPaletteAnimationTimer;
    private ushort _workRobotPaletteAnimationTableOffset;
    private ushort _workRobotPaletteAnimationPaletteIndex;

    /// <summary>Typed state for powered and deactivated Work Robots in all physical slots.</summary>
    public IReadOnlyList<WorkRobotEnemyState?> WorkRobotStates => _workRobotStates;

    /// <summary>
    /// Most recent library-two sound requested by robot feet ($68) or a robot laser ($67).
    /// It is cleared at the beginning of every enemy frame, like the other audio seams.
    /// </summary>
    public ushort? LastWorkRobotSoundEffect { get; private set; }

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
        slot.Properties = unchecked((ushort)(slot.Properties | 0xa000));
        slot.InstructionTimer = 4;
        slot.Timer = 0;
        slot.CurrentInstruction = WorkRobotInitial;
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
        // pointer table has only three entries. Read through ROM instead of sanitizing that
        // bug: retail populations use zero/one, while a corrupt value three observes the
        // first code word at $CC36 just as the cartridge would.
        if (unchecked((short)slot.Parameter1) < 0 || slot.Parameter1 >= 4)
            slot.Parameter1 = 0;
        slot.CurrentInstruction = ReadWord(
            _bus!,
            EnemyRomTablePointers.WorkRobot.InitialInstructionListWords + slot.Parameter1 * 2);
        slot.Properties = unchecked((ushort)(slot.Properties | 0x8000));
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
                    WorkRobotFacingLeftVelocity, WorkRobotLeftHitWall,
                    checkLedge: true, ledgeProbeDirection: -1,
                    WorkRobotApproachFallLeft, WorkRobotFacingRightVelocity);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveForward_HandleHittingWall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, -WorkRobotStepPixels,
                    WorkRobotFacingLeftVelocity, WorkRobotLeftHitWall,
                    checkLedge: false, 0, 0, 0);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveBackward_HandleWallOrFall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, WorkRobotStepPixels,
                    WorkRobotFacingLeftVelocity, WorkRobotWalkLeft,
                    checkLedge: true, ledgeProbeDirection: 1,
                    WorkRobotApproachFallRight, WorkRobotFacingLeftVelocity);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveBackward_HandleHittingWall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, WorkRobotStepPixels,
                    WorkRobotFacingLeftVelocity, WorkRobotWalkLeft,
                    checkLedge: false, 0, 0, 0);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_SetInstListTo_FacingRight_WalkingForwards:
                cursor = WorkRobotWalkRight;
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveForward_HandleWallOrFall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, WorkRobotStepPixels,
                    WorkRobotFacingRightVelocity, WorkRobotRightHitWall,
                    checkLedge: true, ledgeProbeDirection: 1,
                    WorkRobotApproachFallRight, WorkRobotFacingLeftVelocity);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveForward_HandleHittingWall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, WorkRobotStepPixels,
                    WorkRobotFacingRightVelocity, WorkRobotRightHitWall,
                    checkLedge: false, 0, 0, 0);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveBackward_HandleWallOrFall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, -WorkRobotStepPixels,
                    WorkRobotFacingRightVelocity, WorkRobotWalkLeft,
                    checkLedge: true, ledgeProbeDirection: -1,
                    WorkRobotApproachFallLeft, WorkRobotFacingRightVelocity);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveBackward_HandleHittingWall:
                cursor = MoveWorkRobot(
                    slot, state, samus, level, next, -WorkRobotStepPixels,
                    WorkRobotFacingRightVelocity, WorkRobotWalkLeft,
                    checkLedge: false, 0, 0, 0);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_PlaySFXIfOnScreen:
                if (IsWorkRobotOriginStrictlyOnScreen(slot, cameraX, cameraY))
                    LastWorkRobotSoundEffect = WorkRobotFootstepSound;
                cursor = next;
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_Goto_FacingLeft_WalkingForwards:
                cursor = WorkRobotWalkLeft;
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserUpRight:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaserUpRight,
                    WorkRobotLaserUpRightAnimation, cameraX, cameraY);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserUpLeft:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaserUpLeft,
                    WorkRobotLaserUpLeftAnimation, cameraX, cameraY);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserRight:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaserHorizontal,
                    WorkRobotLaserRightAnimation, cameraX, cameraY);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserLeft:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaserHorizontal,
                    WorkRobotLaserLeftAnimation, cameraX, cameraY);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserDownRight:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaserDownRight,
                    WorkRobotLaserDownRightAnimation, cameraX, cameraY);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserDownLeft:
                cursor = TryFireWorkRobotLaser(
                    slot, state, next, WorkRobotLaserDownLeft,
                    WorkRobotLaserDownLeftAnimation, cameraX, cameraY);
                return true;
            case WorkRobotInstructionCodes.Instruction_Robot_DecrementLaserCooldown:
                DecrementWorkRobotLaserCooldown(state);
                cursor = next;
                return true;
            default:
                return false;
        }
    }

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

        if (samus is not null && WorkRobotIsTouchingSamusFromBelow(slot, samus))
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

    /// <summary>Exact asymmetric support test from <c>$A0:ABE7</c>.</summary>
    private static bool WorkRobotIsTouchingSamusFromBelow(
        RoomEnemySlot robot,
        SamusState samus)
    {
        ushort xDistance = WrappedMagnitude(unchecked((ushort)(samus.XPosition - robot.XPosition)));
        ushort horizontalGap = unchecked((ushort)(xDistance - samus.Kinematics.XRadius));
        bool horizontalOverlap = xDistance < samus.Kinematics.XRadius ||
            horizontalGap < robot.XRadius;
        if (!horizontalOverlap)
            return false;

        ushort biasedYDifference = unchecked((ushort)(samus.YPosition + 3 - robot.YPosition));
        if (unchecked((short)biasedYDifference) >= 0)
            return false;
        ushort yDistance = unchecked((ushort)-biasedYDifference);
        ushort verticalGap = unchecked((ushort)(yDistance - samus.Kinematics.YRadius));
        return yDistance < samus.Kinematics.YRadius || verticalGap <= robot.YRadius;
    }

    private static bool IsWorkRobotOriginStrictlyOnScreen(
        RoomEnemySlot robot,
        ushort cameraX,
        ushort cameraY) =>
        unchecked((short)(robot.XPosition - cameraX)) > 0 &&
        unchecked((short)(cameraX + 256 - robot.XPosition)) >= 0 &&
        unchecked((short)(robot.YPosition - cameraY)) > 0 &&
        unchecked((short)(cameraY + 224 - robot.YPosition)) >= 0;

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
                ? WorkRobotLeftShotAhead
                : WorkRobotLeftShotBehind;
        }
        else
        {
            robot.CurrentInstruction = samus.XPosition >= robot.XPosition
                ? WorkRobotRightShotAhead
                : WorkRobotRightShotBehind;
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

        int recordAddress = 0xa8ccc1 + _workRobotPaletteAnimationTableOffset;
        if (ReadWord(_bus!, recordAddress) == 0xffff)
        {
            _workRobotPaletteAnimationTableOffset = 0;
            recordAddress = 0xa8ccc1;
        }

        int destination = 128 +
            ((_workRobotPaletteAnimationPaletteIndex >> 9) & 7) * 16 + 9;
        for (int color = 0; color < 4; color++)
            _cgram!.SetColor(destination + color, ReadWord(_bus!, recordAddress + color * 2));
        _workRobotPaletteAnimationTimer = ReadWord(_bus!, recordAddress + 8);
        _workRobotPaletteAnimationTableOffset = unchecked((ushort)(
            _workRobotPaletteAnimationTableOffset + 10));
    }

    private WorkRobotEnemyState RequireWorkRobotState(RoomEnemySlot slot) =>
        _workRobotStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Work Robot state.");
}

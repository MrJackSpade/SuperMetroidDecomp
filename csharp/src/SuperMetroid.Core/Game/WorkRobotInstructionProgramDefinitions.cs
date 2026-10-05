namespace SuperMetroid.Core.Game;

/// <summary>One compiled Work Robot control word at its bank-$A8 address.</summary>
internal readonly record struct WorkRobotInstructionMechanicsWord(
    ushort Address, ushort Value);

internal static class WorkRobotInstructionProgramDefinitions
{
    /// <summary><c>InstList_RobotNoPower_Neutral</c> at $A8:C6D3.</summary>
    public const ushort NoPowerNeutral = 0xc6d3;
    /// <summary><c>InstList_RobotNoPower_LeaningLeft</c> at $A8:C6D9.</summary>
    public const ushort NoPowerLeaningLeft = 0xc6d9;
    /// <summary><c>InstList_RobotNoPower_LeaningRight</c> at $A8:C6DF.</summary>
    public const ushort NoPowerLeaningRight = 0xc6df;
    /// <summary><c>InstList_Robot_Initial</c> at $A8:C6E5.</summary>
    public const ushort Initial = 0xc6e5;
    /// <summary><c>InstList_Robot_FacingLeft_WalkingForwards</c> at $A8:C6E9.</summary>
    public const ushort FacingLeftWalkingForwards = 0xc6e9;
    /// <summary><c>InstList_Robot_FacingLeft_HitWallMovingForwards</c> at $A8:C73F.</summary>
    public const ushort FacingLeftHitWallMovingForwards = 0xc73f;
    /// <summary><c>InstList_Robot_FacingLeft_Shot_SamusAhead</c> at $A8:C7BB.</summary>
    public const ushort FacingLeftShotSamusAhead = 0xc7bb;
    /// <summary><c>InstList_Robot_FacingLeft_Shot_SamusBehind</c> at $A8:C833.</summary>
    public const ushort FacingLeftShotSamusBehind = 0xc833;
    /// <summary><c>InstList_Robot_FacingLeft_ShotLaserDownLeft</c> at $A8:C8B1.</summary>
    public const ushort FacingLeftShotLaserDownLeft = 0xc8b1;
    /// <summary><c>InstList_Robot_FacingLeft_ShotLaserLeft</c> at $A8:C8BD.</summary>
    public const ushort FacingLeftShotLaserLeft = 0xc8bd;
    /// <summary><c>InstList_Robot_FacingLeft_ShotLaserUpLeft</c> at $A8:C8D1.</summary>
    public const ushort FacingLeftShotLaserUpLeft = 0xc8d1;
    /// <summary><c>InstList_Robot_FacingLeft_LaserShotRecoil</c> at $A8:C8E9.</summary>
    public const ushort FacingLeftLaserShotRecoil = 0xc8e9;
    /// <summary><c>InstList_Robot_ApproachingFallRight</c> at $A8:C91B.</summary>
    public const ushort ApproachingFallRight = 0xc91b;
    /// <summary><c>InstList_Robot_FacingRight_WalkingForwards</c> at $A8:C92D.</summary>
    public const ushort FacingRightWalkingForwards = 0xc92d;
    /// <summary><c>InstList_Robot_FacingRight_HitWallMovingForwards</c> at $A8:C985.</summary>
    public const ushort FacingRightHitWallMovingForwards = 0xc985;
    /// <summary><c>InstList_Robot_FacingRight_Shot_SamusAhead</c> at $A8:CA01.</summary>
    public const ushort FacingRightShotSamusAhead = 0xca01;
    /// <summary><c>InstList_Robot_FacingRight_Shot_SamusBehind</c> at $A8:CA7D.</summary>
    public const ushort FacingRightShotSamusBehind = 0xca7d;
    /// <summary><c>InstList_Robot_FacingRight_ShotLaserDownRight</c> at $A8:CAFD.</summary>
    public const ushort FacingRightShotLaserDownRight = 0xcafd;
    /// <summary><c>InstList_Robot_FacingRight_ShotLaserRight</c> at $A8:CB09.</summary>
    public const ushort FacingRightShotLaserRight = 0xcb09;
    /// <summary><c>InstList_Robot_FacingRight_ShotLaserUpRight</c> at $A8:CB1D.</summary>
    public const ushort FacingRightShotLaserUpRight = 0xcb1d;
    /// <summary><c>InstList_Robot_FacingRight_LaserShotRecoil</c> at $A8:CB35.</summary>
    public const ushort FacingRightLaserShotRecoil = 0xcb35;
    /// <summary><c>InstList_Robot_ApproachingFallLeft</c> at $A8:CB65.</summary>
    public const ushort ApproachingFallLeft = 0xcb65;

    private const int PresentationOperand = -1;
    /// <summary>$A8:CB77, first code after the complete robot instruction region.</summary>
    private const ushort EndAddress = 0xcb77;
    /// <summary>$A8:C6D3/D9/DF: unpowered pose hold before sleep; the maximum positive timer choice remains pending.</summary>
    private const ushort UnresolvedUnpoweredTicks = (ushort)short.MaxValue;
    /// <summary>$A8:C6E9/C73F/C92D/C985/CA01: entry pose scheduling before the ongoing gait; pending.</summary>
    private const ushort UnresolvedEntryTicks = 1;
    /// <summary>$A8:C6F1/C6FB/C731 and mirrored shot checks: delay before the callback within a gait pose; pending.</summary>
    private const ushort UnresolvedShootingOpportunityTicks = 1;
    /// <summary>$A8:C6E5, initial stationary pose before walking; independent hold remains pending.</summary>
    private const ushort UnresolvedInitialTicks = 32;
    /// <summary>$A8:C6ED and mirrored gait: common walking cadence; its independent magnitude remains pending.</summary>
    private const ushort UnresolvedWalkTicks = 10;
    /// <summary>$A8:C7BB/CA05 shot response traverses the same gait poses at twice normal cadence.</summary>
    private const ushort ShotResponseTicks = UnresolvedWalkTicks / 2;
    /// <summary>$A8:C8B1/BD/D1 and mirrored laser aims: initial firing pose hold, pending.</summary>
    private const ushort UnresolvedLaserPoseTicks = 5;
    /// <summary>$A8:C8B5/C8C1/C8D5/C8D9 and mirrors: laser transition pose cadence, pending.</summary>
    private const ushort UnresolvedLaserTransitionTicks = 2;
    /// <summary>$A8:C8DD/E5 and mirrors: upward-shot stepping poses, pending.</summary>
    private const ushort UnresolvedUpwardStepTicks = 4;
    /// <summary>$A8:C8EB/CB37: initial recoil kick hold, pending.</summary>
    private const ushort UnresolvedRecoilKickTicks = 16;
    /// <summary>$A8:C911/CB5D: final recoil recovery hold, pending.</summary>
    private const ushort UnresolvedRecoilRecoveryTicks = 96;
    /// <summary>$A8:C91B/CB65: ledge-approach hold before the two return poses, pending.</summary>
    private const ushort UnresolvedLedgeTicks = 128;
    /// <summary>$A8:CA61: right-facing shot retreat's second contact pose differs from its five-tick peers; pending.</summary>
    private const ushort UnresolvedRightRetreatContactTicks = 10;

    internal const int MechanicsWordCount = 367;
    internal static int PresentationWordCount => 227;

    internal static WorkRobotInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        for (int address = NoPowerNeutral; address < EndAddress; address += 2)
        {
            int value = ProgramWord((ushort)address);
            if (value != PresentationOperand && index-- == 0) return new((ushort)address, (ushort)value);
        }
        throw new InvalidOperationException("Work Robot mechanics-word index is inconsistent.");
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (!IsWordAddress(address)) throw NotCompiled(address);
        int value = ProgramWord(address);
        return value == PresentationOperand ? throw NotCompiled(address) : (ushort)value;
    }
    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        for (int address = NoPowerNeutral; address < EndAddress; address += 2)
            if (ProgramWord((ushort)address) == PresentationOperand && index-- == 0) return (ushort)address;
        throw new InvalidOperationException("Work Robot presentation-word index is inconsistent.");
    }
    internal static bool IsPresentationWordAddress(ushort address) =>
        IsWordAddress(address) && ProgramWord(address) == PresentationOperand;
    private static bool IsWordAddress(ushort address) => address >= NoPowerNeutral && address < EndAddress &&
        ((address - NoPowerNeutral) & 1) == 0;
    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        ushort bankAddress = (ushort)address;
        if (bankAddress < NoPowerNeutral || bankAddress >= EndAddress) return false;
        ushort wordAddress = (ushort)(bankAddress - ((bankAddress - NoPowerNeutral) & 1));
        return ProgramWord(wordAddress) != PresentationOperand;
    }

    // Each phase emits its executable instruction shape into a scalar selector. No
    // generated words or presentation-address table are retained between calls.
    private static int ProgramWord(ushort address)
    {
        ushort start;
        if (address < Initial)
        {
            start = (ushort)(NoPowerNeutral + (address - NoPowerNeutral) / 6 * 6);
            var idle = new WordSelector(address, start);
            idle.Timed(UnresolvedUnpoweredTicks);
            idle.Command(CommonEnemyInstructionCodes.Sleep);
            return idle.Value;
        }
        if (address < FacingLeftWalkingForwards)
        {
            var initial = new WordSelector(address, Initial);
            initial.Timed(UnresolvedInitialTicks);
            return initial.Value;
        }
        bool right = address >= FacingRightWalkingForwards;
        ushort walking = right ? FacingRightWalkingForwards : FacingLeftWalkingForwards;
        ushort wall = right ? FacingRightHitWallMovingForwards : FacingLeftHitWallMovingForwards;
        ushort ahead = right ? FacingRightShotSamusAhead : FacingLeftShotSamusAhead;
        ushort behind = right ? FacingRightShotSamusBehind : FacingLeftShotSamusBehind;
        ushort down = right ? FacingRightShotLaserDownRight : FacingLeftShotLaserDownLeft;
        ushort horizontal = right ? FacingRightShotLaserRight : FacingLeftShotLaserLeft;
        ushort up = right ? FacingRightShotLaserUpRight : FacingLeftShotLaserUpLeft;
        ushort recoil = right ? FacingRightLaserShotRecoil : FacingLeftLaserShotRecoil;
        ushort ledge = right ? ApproachingFallLeft : ApproachingFallRight;
        start = address < wall ? walking : address < ahead ? wall : address < behind ? ahead
            : address < down ? behind : address < horizontal ? down : address < up ? horizontal
            : address < recoil ? up : address < ledge ? recoil : ledge;
        var writer = new WordSelector(address, start);
        if (start == walking) Walk(ref writer, right);
        else if (start == wall) Retreat(ref writer, right, wallResponse: true);
        else if (start == ahead) Retreat(ref writer, right, wallResponse: false);
        else if (start == behind) AdvanceAfterShot(ref writer, right);
        else if (start == down) Laser(ref writer, right, LaserAim.Down);
        else if (start == horizontal) Laser(ref writer, right, LaserAim.Horizontal);
        else if (start == up) Laser(ref writer, right, LaserAim.Up);
        else if (start == recoil) Recoil(ref writer, right);
        else Ledge(ref writer, right);
        return writer.Value;
    }

    /// <summary>The native shooting programs select exactly one of down, horizontal or up.</summary>
    private enum LaserAim { Down, Horizontal, Up }

    private static void Walk(ref WordSelector writer, bool right)
    {
        ushort move = MoveForward(right, hitWallOnly: false);
        writer.Timed(UnresolvedEntryTicks);
        if (right)
        {
            ShootingOpportunity(ref writer, WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserDownRight);
            writer.Command(WorkRobotInstructionCodes.Instruction_Robot_DecrementLaserCooldown);
        }
        writer.Timed(UnresolvedWalkTicks);
        ShootingOpportunity(ref writer, right ? WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserRight
            : WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserLeft);
        ShootingOpportunity(ref writer, right ? WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserUpRight
            : WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserUpLeft);
        writer.Timed(UnresolvedWalkTicks);
        SoundThenMove(ref writer, move);
        writer.Timed(UnresolvedWalkTicks);
        writer.Command(move);
        writer.Timed(UnresolvedWalkTicks, 5);
        MoveThenSound(ref writer, move);
        writer.Timed(UnresolvedWalkTicks);
        writer.Command(move);
        if (!right) ShootingOpportunity(ref writer, WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserDownLeft);
        writer.Goto((ushort)((right ? FacingRightWalkingForwards : FacingLeftWalkingForwards) + 4));
    }
    private static void ShootingOpportunity(ref WordSelector writer, ushort callback)
    {
        // The callback is scheduled within a pose; the remainder completes
        // the same ten-tick gait exposure if no shooting branch replaces the list.
        writer.Timed(UnresolvedShootingOpportunityTicks);
        writer.Command(callback);
        writer.Timed(UnresolvedWalkTicks - UnresolvedShootingOpportunityTicks);
    }
    private static void Retreat(ref WordSelector writer, bool right, bool wallResponse)
    {
        ushort cadence = wallResponse ? UnresolvedWalkTicks : ShotResponseTicks;
        ushort move = MoveBackward(right, hitWallOnly: !wallResponse);
        if (wallResponse || right) writer.Timed(UnresolvedEntryTicks);
        for (int stride = 0; stride < 2; stride++)
        {
            if (stride == 0)
            {
                writer.Timed(cadence);
                SoundThenMove(ref writer, move);
            }
            else
            {
                if (right && wallResponse) SoundThenMove(ref writer, move);
                else MoveThenSound(ref writer, move);
                writer.Timed(cadence);
                writer.Command(move);
            }
            writer.Timed(cadence, 5);
            SoundThenMove(ref writer, move);
            writer.Timed(right && !wallResponse && stride == 1 ? UnresolvedRightRetreatContactTicks : cadence);
            writer.Command(move);
            writer.Timed(cadence, 5);
        }
        writer.Command(right ? WorkRobotInstructionCodes.Instruction_Robot_Goto_FacingLeft_WalkingForwards
            : WorkRobotInstructionCodes.Instruction_Robot_SetInstListTo_FacingRight_WalkingForwards);
    }
    private static void AdvanceAfterShot(ref WordSelector writer, bool right)
    {
        ushort move = MoveForward(right, hitWallOnly: true);
        if (right) MoveThenSound(ref writer, move); else SoundThenMove(ref writer, move);
        writer.Timed(ShotResponseTicks, right ? 5 : 4);
        for (int stride = 0; stride < 2; stride++)
        {
            SoundThenMove(ref writer, move);
            writer.Timed(ShotResponseTicks);
            writer.Command(move);
            if (right && stride == 0) writer.Command(WorkRobotInstructionCodes.Instruction_Robot_PlaySFXIfOnScreen);
            writer.Timed(ShotResponseTicks, 5);
            MoveThenSound(ref writer, move);
            writer.Timed(ShotResponseTicks);
            writer.Command(move);
            if (stride == 0) writer.Timed(ShotResponseTicks, 5);
            else if (!right) writer.Timed(ShotResponseTicks);
        }
        writer.Command(right ? WorkRobotInstructionCodes.Instruction_Robot_SetInstListTo_FacingRight_WalkingForwards
            : WorkRobotInstructionCodes.Instruction_Robot_Goto_FacingLeft_WalkingForwards);
    }
    private static void Laser(ref WordSelector writer, bool right, LaserAim aim)
    {
        writer.Timed(UnresolvedLaserPoseTicks);
        writer.Timed(UnresolvedLaserTransitionTicks, aim == LaserAim.Up ? 2 : 1);
        if (aim == LaserAim.Up) writer.Timed(UnresolvedUpwardStepTicks);
        if (aim != LaserAim.Down)
        {
            ushort move = MoveBackward(right, hitWallOnly: false);
            if (right) SoundThenMove(ref writer, move); else MoveThenSound(ref writer, move);
            writer.Timed(aim == LaserAim.Up ? UnresolvedUpwardStepTicks : UnresolvedWalkTicks);
        }
        if (aim != LaserAim.Up) writer.Goto(right ? FacingRightLaserShotRecoil : FacingLeftLaserShotRecoil);
    }
    private static void Recoil(ref WordSelector writer, bool right)
    {
        ushort move = MoveBackward(right, hitWallOnly: false);
        writer.Command(move);
        writer.Timed(UnresolvedRecoilKickTicks);
        writer.Timed(ShotResponseTicks, 4);
        SoundThenMove(ref writer, move);
        writer.Timed(UnresolvedWalkTicks);
        writer.Command(move);
        writer.Timed(UnresolvedWalkTicks, 2);
        writer.Timed(UnresolvedRecoilRecoveryTicks);
        if (!right) writer.Command(WorkRobotInstructionCodes.Instruction_Robot_DecrementLaserCooldown);
        writer.Goto(right ? FacingRightWalkingForwards : FacingLeftWalkingForwards);
    }
    private static void Ledge(ref WordSelector writer, bool right)
    {
        writer.Timed(UnresolvedLedgeTicks);
        writer.Timed(UnresolvedWalkTicks, 2);
        writer.Command(WorkRobotInstructionCodes.Instruction_Robot_DecrementLaserCooldown);
        writer.Goto(right ? FacingRightWalkingForwards : FacingLeftWalkingForwards);
    }
    private static ushort MoveForward(bool right, bool hitWallOnly) => right
        ? hitWallOnly ? WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveForward_HandleHittingWall
            : WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveForward_HandleWallOrFall
        : hitWallOnly ? WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveForward_HandleHittingWall
            : WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveForward_HandleWallOrFall;
    private static ushort MoveBackward(bool right, bool hitWallOnly) => right
        ? hitWallOnly ? WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveBackward_HandleHittingWall
            : WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveBackward_HandleWallOrFall
        : hitWallOnly ? WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveBackward_HandleHittingWall
            : WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveBackward_HandleWallOrFall;
    private static void SoundThenMove(ref WordSelector writer, ushort move)
    {
        writer.Command(WorkRobotInstructionCodes.Instruction_Robot_PlaySFXIfOnScreen);
        writer.Command(move);
    }
    private static void MoveThenSound(ref WordSelector writer, ushort move)
    {
        writer.Command(move);
        writer.Command(WorkRobotInstructionCodes.Instruction_Robot_PlaySFXIfOnScreen);
    }
    private struct WordSelector(ushort address, ushort start)
    {
        private int remaining = (address - start) / 2;
        private int selected = int.MinValue;
        public readonly int Value => selected == int.MinValue
            ? throw new InvalidOperationException("Work Robot semantic program shape is incomplete.") : selected;
        public void Command(ushort command) => Emit(command);
        public void Timed(ushort duration, int poses = 1)
        {
            for (int pose = 0; pose < poses; pose++) { Emit(duration); Emit(PresentationOperand); }
        }
        public void Goto(ushort target) { Emit(CommonEnemyInstructionCodes.Goto); Emit(target); }
        private void Emit(int value) { if (remaining-- == 0) selected = value; }
    }
    private static InvalidDataException NotCompiled(ushort address) =>
        new($"Work Robot instruction mechanics pointer $A8:{address:X4} is not compiled.");
}

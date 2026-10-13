namespace SuperMetroid.Core.Game;

/// <summary>Compiled bank-$A8 Work Robot instruction mechanics, preserving authored pose timing and branches.</summary>
internal abstract class WorkRobotInstructionProgramDefinitions
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

    /// <summary>Marks spritemap operands that must be resolved by the presentation owner.</summary>
    internal const int PresentationOperand = -1;
    /// <summary>$A8:CB77, first code after the complete robot instruction region.</summary>
    internal const ushort EndAddress = 0xcb77;
    /// <summary>$A8:C6D3/D9/DF: unpowered pose hold before sleep: the maximum positive timer, holding indefinitely (InstructionItem.IndefiniteDuration) rather than a chosen cadence.</summary>
    private const ushort UnpoweredTicks = InstructionItem.IndefiniteDuration;
    /// <summary>$A8:C6E9/C73F/C92D/C985/CA01: entry pose scheduling before the ongoing gait. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort EntryTicks = 1;
    /// <summary>$A8:C6F1/C6FB/C731 and mirrored shot checks: delay before the callback within a gait pose. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort ShootingOpportunityTicks = 1;
    /// <summary>$A8:C6E5, initial stationary pose before walking; independent hold. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort InitialTicks = 32;
    /// <summary>$A8:C6ED and mirrored gait: common walking cadence; its independent magnitude. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort WalkTicks = 10;
    /// <summary>$A8:C7BB/CA05 shot response traverses the same gait poses at twice normal cadence.</summary>
    private const ushort ShotResponseTicks = WalkTicks / 2;
    /// <summary>$A8:C8B1/BD/D1 and mirrored laser aims: initial firing pose hold. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort LaserPoseTicks = 5;
    /// <summary>$A8:C8B5/C8C1/C8D5/C8D9 and mirrors: laser transition pose cadence. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort LaserTransitionTicks = 2;
    /// <summary>$A8:C8DD/E5 and mirrors: upward-shot stepping poses. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort UpwardStepTicks = 4;
    /// <summary>$A8:C8EB/CB37: initial recoil kick hold. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort RecoilKickTicks = 16;
    /// <summary>$A8:C911/CB5D: final recoil recovery hold. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort RecoilRecoveryTicks = 96;
    /// <summary>$A8:C91B/CB65: ledge-approach hold before the two return poses. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort LedgeTicks = 128;
    /// <summary>$A8:CA61: right-facing shot retreat's second contact pose differs from its five-tick peers. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort RightRetreatContactTicks = 10;
    /// <summary>Reads a compiled instruction or timing word and rejects presentation-only addresses.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (!IsWordAddress(address)) throw NotCompiled(address);
        int value = ProgramWord(address);
        return value == PresentationOperand ? throw NotCompiled(address) : (ushort)value;
    }
    /// <summary>Identifies pose operands inside a compiled program without reading sprite artwork.</summary>
    internal static bool IsPresentationWordAddress(ushort address) =>
        IsWordAddress(address) && ProgramWord(address) == PresentationOperand;
    /// <summary>Bounds aligned instruction words to the complete Work Robot program interval.</summary>
    private static bool IsWordAddress(ushort address) => address >= NoPowerNeutral && address < EndAddress &&
        ((address - NoPowerNeutral) & 1) == 0;

    /// <summary>
    /// Selects one executable word by emitting the relevant native program phase into a
    /// scalar cursor. No word array or presentation-address table survives the call.
    /// </summary>
    internal static int ProgramWord(ushort address)
    {
        ushort start;
        if (address < Initial)
        {
            start = (ushort)(NoPowerNeutral + (address - NoPowerNeutral) / 6 * 6);
            var idle = new WordSelector(address, start);
            idle.Timed(UnpoweredTicks);
            idle.Command((ushort)CommonEnemyInstruction.Sleep);
            return idle.Value;
        }
        if (address < FacingLeftWalkingForwards)
        {
            var initial = new WordSelector(address, Initial);
            initial.Timed(InitialTicks);
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

    /// <summary>The three mutually exclusive laser trajectories used by authored firing lists.</summary>
    private enum LaserAim
    {
        /// <summary>Fires below the robot's horizontal line.</summary>
        Down,
        /// <summary>Fires along the robot's facing direction.</summary>
        Horizontal,
        /// <summary>Fires above the robot's horizontal line.</summary>
        Up,
    }

    /// <summary>Emits the direction-specific gait with callback windows and movement sounds.</summary>
    private static void Walk(ref WordSelector writer, bool right)
    {
        ushort move = MoveForward(right, hitWallOnly: false);
        writer.Timed(EntryTicks);
        if (right)
        {
            ShootingOpportunity(ref writer, (ushort)WorkRobotInstruction.TryShootingLaserDownRight);
            writer.Command((ushort)WorkRobotInstruction.DecrementLaserCooldown);
        }
        writer.Timed(WalkTicks);
        ShootingOpportunity(ref writer, right ? (ushort)WorkRobotInstruction.TryShootingLaserRight
            : (ushort)WorkRobotInstruction.TryShootingLaserLeft);
        ShootingOpportunity(ref writer, right ? (ushort)WorkRobotInstruction.TryShootingLaserUpRight
            : (ushort)WorkRobotInstruction.TryShootingLaserUpLeft);
        writer.Timed(WalkTicks);
        SoundThenMove(ref writer, move);
        writer.Timed(WalkTicks);
        writer.Command(move);
        writer.Timed(WalkTicks, 5);
        MoveThenSound(ref writer, move);
        writer.Timed(WalkTicks);
        writer.Command(move);
        if (!right) ShootingOpportunity(ref writer, (ushort)WorkRobotInstruction.TryShootingLaserDownLeft);
        writer.Goto((ushort)((right ? FacingRightWalkingForwards : FacingLeftWalkingForwards) + 4));
    }
    /// <summary>Places a shot callback early in a gait pose and retains its remaining duration.</summary>
    private static void ShootingOpportunity(ref WordSelector writer, ushort callback)
    {
        // The callback is scheduled within a pose; the remainder completes
        // the same ten-tick gait exposure if no shooting branch replaces the list.
        writer.Timed(ShootingOpportunityTicks);
        writer.Command(callback);
        writer.Timed(WalkTicks - ShootingOpportunityTicks);
    }
    /// <summary>Emits two backward strides at wall or post-shot cadence before returning to walking.</summary>
    private static void Retreat(ref WordSelector writer, bool right, bool wallResponse)
    {
        ushort cadence = wallResponse ? WalkTicks : ShotResponseTicks;
        ushort move = MoveBackward(right, hitWallOnly: !wallResponse);
        if (wallResponse || right) writer.Timed(EntryTicks);
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
            writer.Timed(right && !wallResponse && stride == 1 ? RightRetreatContactTicks : cadence);
            writer.Command(move);
            writer.Timed(cadence, 5);
        }
        writer.Command(right ? (ushort)WorkRobotInstruction.Goto_FacingLeft_WalkingForwards
            : (ushort)WorkRobotInstruction.SetInstListTo_FacingRight_WalkingForwards);
    }
    /// <summary>Emits the forward response after Samus shoots from behind the robot.</summary>
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
            if (right && stride == 0) writer.Command((ushort)WorkRobotInstruction.PlaySFXIfOnScreen);
            writer.Timed(ShotResponseTicks, 5);
            MoveThenSound(ref writer, move);
            writer.Timed(ShotResponseTicks);
            writer.Command(move);
            if (stride == 0) writer.Timed(ShotResponseTicks, 5);
            else if (!right) writer.Timed(ShotResponseTicks);
        }
        writer.Command(right ? (ushort)WorkRobotInstruction.SetInstListTo_FacingRight_WalkingForwards
            : (ushort)WorkRobotInstruction.Goto_FacingLeft_WalkingForwards);
    }
    /// <summary>Emits the selected laser aim and its direction-specific transition into recoil.</summary>
    private static void Laser(ref WordSelector writer, bool right, LaserAim aim)
    {
        writer.Timed(LaserPoseTicks);
        writer.Timed(LaserTransitionTicks, aim == LaserAim.Up ? 2 : 1);
        if (aim == LaserAim.Up) writer.Timed(UpwardStepTicks);
        if (aim != LaserAim.Down)
        {
            ushort move = MoveBackward(right, hitWallOnly: false);
            if (right) SoundThenMove(ref writer, move); else MoveThenSound(ref writer, move);
            writer.Timed(aim == LaserAim.Up ? UpwardStepTicks : WalkTicks);
        }
        if (aim != LaserAim.Up) writer.Goto(right ? FacingRightLaserShotRecoil : FacingLeftLaserShotRecoil);
    }
    /// <summary>Emits kick, recovery, cooldown, and return-to-walking phases after firing.</summary>
    private static void Recoil(ref WordSelector writer, bool right)
    {
        ushort move = MoveBackward(right, hitWallOnly: false);
        writer.Command(move);
        writer.Timed(RecoilKickTicks);
        writer.Timed(ShotResponseTicks, 4);
        SoundThenMove(ref writer, move);
        writer.Timed(WalkTicks);
        writer.Command(move);
        writer.Timed(WalkTicks, 2);
        writer.Timed(RecoilRecoveryTicks);
        if (!right) writer.Command((ushort)WorkRobotInstruction.DecrementLaserCooldown);
        writer.Goto(right ? FacingRightWalkingForwards : FacingLeftWalkingForwards);
    }
    /// <summary>Holds at a ledge, reduces laser cooldown, and resumes the walking list.</summary>
    private static void Ledge(ref WordSelector writer, bool right)
    {
        writer.Timed(LedgeTicks);
        writer.Timed(WalkTicks, 2);
        writer.Command((ushort)WorkRobotInstruction.DecrementLaserCooldown);
        writer.Goto(right ? FacingRightWalkingForwards : FacingLeftWalkingForwards);
    }
    /// <summary>Selects the native forward callback with either wall-only or wall-and-fall handling.</summary>
    private static ushort MoveForward(bool right, bool hitWallOnly) => right
        ? hitWallOnly ? (ushort)WorkRobotInstruction.FacingRight_MoveForward_HandleHittingWall
            : (ushort)WorkRobotInstruction.FacingRight_MoveForward_HandleWallOrFall
        : hitWallOnly ? (ushort)WorkRobotInstruction.FacingLeft_MoveForward_HandleHittingWall
            : (ushort)WorkRobotInstruction.FacingLeft_MoveForward_HandleWallOrFall;
    /// <summary>Selects the native backward callback with the requested collision behavior.</summary>
    private static ushort MoveBackward(bool right, bool hitWallOnly) => right
        ? hitWallOnly ? (ushort)WorkRobotInstruction.FacingRight_MoveBackward_HandleHittingWall
            : (ushort)WorkRobotInstruction.FacingRight_MoveBackward_HandleWallOrFall
        : hitWallOnly ? (ushort)WorkRobotInstruction.FacingLeft_MoveBackward_HandleHittingWall
            : (ushort)WorkRobotInstruction.FacingLeft_MoveBackward_HandleWallOrFall;
    /// <summary>Places the on-screen movement sound before the movement callback.</summary>
    private static void SoundThenMove(ref WordSelector writer, ushort move)
    {
        writer.Command((ushort)WorkRobotInstruction.PlaySFXIfOnScreen);
        writer.Command(move);
    }
    /// <summary>Places the on-screen movement sound after the movement callback.</summary>
    private static void MoveThenSound(ref WordSelector writer, ushort move)
    {
        writer.Command(move);
        writer.Command((ushort)WorkRobotInstruction.PlaySFXIfOnScreen);
    }
    /// <summary>Retains only the requested word while a phase emits its instruction shape.</summary>
    private struct WordSelector(ushort address, ushort start)
    {
        /// <summary>Counts emitted words until the caller's address is reached.</summary>
        private int remaining = (address - start) / 2;

        /// <summary>The selected instruction word, requiring the phase to have emitted it.</summary>
        public int Value { get => field == int.MinValue
            ? throw new InvalidOperationException("Work Robot semantic program shape is incomplete.") : field; private set; } = int.MinValue;
        /// <summary>Offers one mechanic opcode to the requested-address selector.</summary>
        public void Command(ushort command) => Emit(command);
        /// <summary>Emits timed poses with presentation operands in their native interleaving.</summary>
        public void Timed(ushort duration, int poses = 1)
        {
            for (int pose = 0; pose < poses; pose++) { Emit(duration); Emit(PresentationOperand); }
        }
        /// <summary>Emits the native goto opcode followed by its list destination.</summary>
        public void Goto(ushort target) { Emit((ushort)CommonEnemyInstruction.Goto); Emit(target); }
        /// <summary>Captures the emitted word only when it occupies the requested address.</summary>
        private void Emit(int value) { if (remaining-- == 0) Value = value; }
    }
    /// <summary>Identifies an address outside the compiled mechanics or inside presentation data.</summary>
    private static InvalidDataException NotCompiled(ushort address) =>
        new($"Work Robot instruction mechanics pointer $A8:{address:X4} is not compiled.");
}

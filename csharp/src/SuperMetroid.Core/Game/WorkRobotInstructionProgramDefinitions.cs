namespace SuperMetroid.Core.Game;

/// <summary>Reconstructs the executable word at a Work Robot instruction pointer from its authored movement and firing phases.</summary>
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

    /// <summary>Marks a timed instruction's presentation word, which holds the selected pose without executing a command.</summary>
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
    /// <summary>Reads an executable mechanics word and rejects pointers outside the compiled program or into pose data.</summary>
    /// <param name="address">The bank-$A8 instruction pointer to resolve.</param>
    /// <returns>The command or duration word at that pointer.</returns>
    /// <exception cref="InvalidDataException">The pointer is not part of the compiled instruction region.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (!IsWordAddress(address)) throw NotCompiled(address);
        int value = ProgramWord(address);
        return value == PresentationOperand ? throw NotCompiled(address) : (ushort)value;
    }
    /// <summary>Determines whether an in-range instruction pointer selects a timed pose's presentation operand.</summary>
    /// <param name="address">The bank-$A8 instruction pointer to inspect.</param>
    /// <returns><see langword="true"/> when the selected word represents presentation data rather than executable mechanics.</returns>
    internal static bool IsPresentationWordAddress(ushort address) =>
        IsWordAddress(address) && ProgramWord(address) == PresentationOperand;
    /// <summary>Checks the aligned address interval occupied by the authored Work Robot instruction programs.</summary>
    /// <param name="address">The bank-local word pointer to validate.</param>
    /// <returns><see langword="true"/> when the address lies within the region and is word-aligned from its start.</returns>
    private static bool IsWordAddress(ushort address) => address >= NoPowerNeutral && address < EndAddress &&
        ((address - NoPowerNeutral) & 1) == 0;

    // Each phase emits its executable instruction shape into a scalar selector. No
    // generated words or presentation-address table are retained between calls.
    /// <summary>Resolves one instruction pointer by walking the matching authored phase and selecting its corresponding word.</summary>
    /// <param name="address">The bank-$A8 address whose semantic program word is requested.</param>
    /// <returns>The command, timer, presentation marker, or target word at that position.</returns>
    /// <exception cref="InvalidOperationException">A phase does not emit a complete instruction shape for the requested address.</exception>
    internal static int ProgramWord(ushort address)
    {
        ushort start;
        if (address < Initial)
        {
            start = (ushort)(NoPowerNeutral + (address - NoPowerNeutral) / 6 * 6);
            var idle = new WordSelector(address, start);
            idle.Timed(UnpoweredTicks);
            idle.Command(CommonEnemyInstructionCodes.Sleep);
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

    /// <summary>The native shooting programs select exactly one of down, horizontal or up.</summary>
    private enum LaserAim
    {
        /// <summary>Places the shot sequence in the downward aim pose.</summary>
        Down,
        /// <summary>Places the shot sequence in the horizontal aim pose.</summary>
        Horizontal,
        /// <summary>Places the shot sequence in the upward aim pose.</summary>
        Up
    }

    /// <summary>Emits the repeating forward gait, scheduling directional laser opportunities between movement poses.</summary>
    /// <param name="writer">The selector positioned at the requested address within the walking program.</param>
    /// <param name="right">Whether to emit right-facing callbacks and instructions.</param>
    private static void Walk(ref WordSelector writer, bool right)
    {
        ushort move = MoveForward(right, hitWallOnly: false);
        writer.Timed(EntryTicks);
        if (right)
        {
            ShootingOpportunity(ref writer, WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserDownRight);
            writer.Command(WorkRobotInstructionCodes.Instruction_Robot_DecrementLaserCooldown);
        }
        writer.Timed(WalkTicks);
        ShootingOpportunity(ref writer, right ? WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserRight
            : WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserLeft);
        ShootingOpportunity(ref writer, right ? WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserUpRight
            : WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserUpLeft);
        writer.Timed(WalkTicks);
        SoundThenMove(ref writer, move);
        writer.Timed(WalkTicks);
        writer.Command(move);
        writer.Timed(WalkTicks, 5);
        MoveThenSound(ref writer, move);
        writer.Timed(WalkTicks);
        writer.Command(move);
        if (!right) ShootingOpportunity(ref writer, WorkRobotInstructionCodes.Instruction_Robot_TryShootingLaserDownLeft);
        writer.Goto((ushort)((right ? FacingRightWalkingForwards : FacingLeftWalkingForwards) + 4));
    }
    /// <summary>Places a shooting callback inside one gait pose and fills the rest of that pose's normal cadence.</summary>
    /// <param name="writer">The selector receiving the timed callback sequence.</param>
    /// <param name="callback">The directional instruction that attempts to fire.</param>
    private static void ShootingOpportunity(ref WordSelector writer, ushort callback)
    {
        // The callback is scheduled within a pose; the remainder completes
        // the same ten-tick gait exposure if no shooting branch replaces the list.
        writer.Timed(ShootingOpportunityTicks);
        writer.Command(callback);
        writer.Timed(WalkTicks - ShootingOpportunityTicks);
    }
    /// <summary>Emits the backward strides used after a wall contact or a shot at Samus.</summary>
    /// <param name="writer">The selector positioned within the selected retreat instruction list.</param>
    /// <param name="right">Whether the robot faces right, which affects native command ordering and cadence.</param>
    /// <param name="wallResponse">Selects the wall-hit gait timing instead of the faster shot-response timing.</param>
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
        writer.Command(right ? WorkRobotInstructionCodes.Instruction_Robot_Goto_FacingLeft_WalkingForwards
            : WorkRobotInstructionCodes.Instruction_Robot_SetInstListTo_FacingRight_WalkingForwards);
    }
    /// <summary>Emits the forward recovery gait used when Samus was behind the robot at the shot check.</summary>
    /// <param name="writer">The selector positioned within the post-shot instruction list.</param>
    /// <param name="right">Whether to use right-facing movement and its authored command ordering.</param>
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
    /// <summary>Emits pose timing and movement for a laser aim, then transfers non-upward shots to recoil.</summary>
    /// <param name="writer">The selector positioned within the chosen laser instruction list.</param>
    /// <param name="right">Whether the emitted aim and movement instructions face right.</param>
    /// <param name="aim">The vertical or horizontal direction of the authored shot sequence.</param>
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
    /// <summary>Emits the backward kick and timed recovery after firing before returning to the forward gait.</summary>
    /// <param name="writer">The selector positioned within the recoil instruction list.</param>
    /// <param name="right">Whether to use the right-facing authored recoil sequence.</param>
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
        if (!right) writer.Command(WorkRobotInstructionCodes.Instruction_Robot_DecrementLaserCooldown);
        writer.Goto(right ? FacingRightWalkingForwards : FacingLeftWalkingForwards);
    }
    /// <summary>Emits the ledge-fall approach hold and return to the facing-specific walking list.</summary>
    /// <param name="writer">The selector positioned within the ledge instruction list.</param>
    /// <param name="right">Whether the robot faces right as it reaches the ledge.</param>
    private static void Ledge(ref WordSelector writer, bool right)
    {
        writer.Timed(LedgeTicks);
        writer.Timed(WalkTicks, 2);
        writer.Command(WorkRobotInstructionCodes.Instruction_Robot_DecrementLaserCooldown);
        writer.Goto(right ? FacingRightWalkingForwards : FacingLeftWalkingForwards);
    }
    /// <summary>Selects a facing-specific forward movement instruction and its wall or wall-and-fall handling mode.</summary>
    /// <param name="right">Selects the right-facing instruction family when <see langword="true"/>.</param>
    /// <param name="hitWallOnly">When <see langword="true"/>, handle wall contact without the fall check.</param>
    /// <returns>The instruction code for the requested movement behavior.</returns>
    private static ushort MoveForward(bool right, bool hitWallOnly) => right
        ? hitWallOnly ? WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveForward_HandleHittingWall
            : WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveForward_HandleWallOrFall
        : hitWallOnly ? WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveForward_HandleHittingWall
            : WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveForward_HandleWallOrFall;
    /// <summary>Selects a facing-specific backward movement instruction and its wall or wall-and-fall handling mode.</summary>
    /// <param name="right">Selects the right-facing instruction family when <see langword="true"/>.</param>
    /// <param name="hitWallOnly">When <see langword="true"/>, handle wall contact without the fall check.</param>
    /// <returns>The instruction code for the requested movement behavior.</returns>
    private static ushort MoveBackward(bool right, bool hitWallOnly) => right
        ? hitWallOnly ? WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveBackward_HandleHittingWall
            : WorkRobotInstructionCodes.Instruction_Robot_FacingRight_MoveBackward_HandleWallOrFall
        : hitWallOnly ? WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveBackward_HandleHittingWall
            : WorkRobotInstructionCodes.Instruction_Robot_FacingLeft_MoveBackward_HandleWallOrFall;
    /// <summary>Emits an on-screen movement sound before the selected movement command.</summary>
    /// <param name="writer">The selector receiving the two command words.</param>
    /// <param name="move">The movement instruction to emit after the sound check.</param>
    private static void SoundThenMove(ref WordSelector writer, ushort move)
    {
        writer.Command(WorkRobotInstructionCodes.Instruction_Robot_PlaySFXIfOnScreen);
        writer.Command(move);
    }
    /// <summary>Emits the selected movement command before checking whether its sound should play on screen.</summary>
    /// <param name="writer">The selector receiving the two command words.</param>
    /// <param name="move">The movement instruction to emit before the sound check.</param>
    private static void MoveThenSound(ref WordSelector writer, ushort move)
    {
        writer.Command(move);
        writer.Command(WorkRobotInstructionCodes.Instruction_Robot_PlaySFXIfOnScreen);
    }
    /// <summary>Selects one word from a sequentially emitted instruction program without retaining a generated program table.</summary>
    /// <param name="address">The requested word address within the authored program.</param>
    /// <param name="start">The start address of the phase being emitted.</param>
    private struct WordSelector(ushort address, ushort start)
    {
        /// <summary>Number of emitted words remaining before reaching the requested address.</summary>
        private int remaining = (address - start) / 2;
        /// <summary>The requested word, or the sentinel until emission reaches that position.</summary>
        private int selected = int.MinValue;
        /// <summary>Gets the selected word after the phase has emitted through its address.</summary>
        /// <exception cref="InvalidOperationException">No emitted word matched the requested address.</exception>
        public readonly int Value => selected == int.MinValue
            ? throw new InvalidOperationException("Work Robot semantic program shape is incomplete.") : selected;
        /// <summary>Emits one executable instruction word.</summary>
        /// <param name="command">The instruction code to contribute to the sequence.</param>
        public void Command(ushort command) => Emit(command);
        /// <summary>Emits one or more timed pose pairs, each containing a duration and presentation marker.</summary>
        /// <param name="duration">The number of updates for each emitted pose.</param>
        /// <param name="poses">The number of identical duration-marker pairs to emit.</param>
        public void Timed(ushort duration, int poses = 1)
        {
            for (int pose = 0; pose < poses; pose++) { Emit(duration); Emit(PresentationOperand); }
        }
        /// <summary>Emits a jump instruction and its destination pointer.</summary>
        /// <param name="target">The bank-local instruction address to jump to.</param>
        public void Goto(ushort target) { Emit(CommonEnemyInstructionCodes.Goto); Emit(target); }
        /// <summary>Counts an emitted word and captures it if it occupies the requested address.</summary>
        /// <param name="value">The instruction, timing, presentation, or target word being emitted.</param>
        private void Emit(int value) { if (remaining-- == 0) selected = value; }
    }
    /// <summary>Creates the data error used when a mechanics lookup addresses presentation data or an uncompiled instruction.</summary>
    /// <param name="address">The bank-$A8 address that could not be resolved as executable mechanics.</param>
    /// <returns>An exception identifying the unsupported instruction pointer.</returns>
    private static InvalidDataException NotCompiled(ushort address) =>
        new($"Work Robot instruction mechanics pointer $A8:{address:X4} is not compiled.");
}

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for every Shaktool instruction program reachable from
/// its initialization, orientation, collision-recovery, and dormant-attack selectors.
/// Interleaved spritemap operands select installed presentation frames.
/// </summary>
internal abstract class ShaktoolInstructionProgramDefinitions
{
    /// <summary><c>UNUSED_InstList_Shaktool_SawHand_Attack_PrimaryPiece_AAD9EA</c> at $AA:D9EA.</summary>
    internal const ushort SawHandAttackPrimaryPiece = 0xd9ea;
    /// <summary><c>UNUSED_InstList_Shaktool_SawHand_Attack_FinalPiece_AAD9F2</c> at $AA:D9F2.</summary>
    internal const ushort SawHandAttackFinalPiece = 0xd9f2;
    /// <summary><c>InstList_Shaktool_SawHand_HeadBob_PrimaryPiece</c> at $AA:D9FC.</summary>
    internal const ushort SawHandHeadBobPrimaryPiece = 0xd9fc;
    /// <summary><c>InstList_Shaktool_SawHand_HeadBob_FinalPiece</c> at $AA:DA04.</summary>
    internal const ushort SawHandHeadBobFinalPiece = 0xda04;
    /// <summary><c>InstList_Shaktool_SawHand_PrimaryPiece</c> at $AA:DA0E.</summary>
    internal const ushort SawHandPrimaryPiece = 0xda0e;
    /// <summary><c>InstList_Shaktool_SawHand_FinalPiece</c> at $AA:DA1E.</summary>
    internal const ushort SawHandFinalPiece = 0xda1e;
    /// <summary><c>UNUSED_InstList_Shaktool_ArmPiece_Attack_Back_AADA2E</c> at $AA:DA2E.</summary>
    internal const ushort ArmPieceAttackBack = 0xda2e;
    /// <summary><c>UNUSED_InstList_Shaktool_ArmPiece_Attack_Front_AADA42</c> at $AA:DA42.</summary>
    internal const ushort ArmPieceAttackFront = 0xda42;
    /// <summary><c>InstList_Shaktool_ArmPiece_HeadBob_Back</c> at $AA:DA56.</summary>
    internal const ushort ArmPieceHeadBobBack = 0xda56;
    /// <summary><c>InstList_Shaktool_ArmPiece_HeadBob_Front</c> at $AA:DA62.</summary>
    internal const ushort ArmPieceHeadBobFront = 0xda62;
    /// <summary><c>InstList_Shaktool_ArmPiece_Normal</c> at $AA:DA72.</summary>
    internal const ushort ArmPieceNormal = 0xda72;
    /// <summary><c>UNUSED_InstList_Shaktool_Head_Attack_AADA7A</c> at $AA:DA7A.</summary>
    internal const ushort HeadAttack = 0xda7a;
    /// <summary><c>InstList_Shaktool_Head_HeadBob</c> at $AA:DA90.</summary>
    internal const ushort HeadHeadBob = 0xda90;
    /// <summary><c>InstList_Shaktool_Head_AimingLeft</c> at $AA:DAA4.</summary>
    internal const ushort HeadAimingLeft = 0xdaa4;
    /// <summary><c>InstList_Shaktool_Head_AimingDown</c> at $AA:DAD4.</summary>
    internal const ushort HeadAimingDown = 0xdad4;

    /// <summary><c>RTS_AADAE4</c>, the first adjacent code routine at $AA:DAE4.</summary>
    internal const ushort FirstAdjacentCodeRoutine = 0xdae4;

    /// <summary>Marker emitted for interleaved words supplied by the installed Shaktool presentation.</summary>
    internal const int PresentationOperand = -1;
    /// <summary>$AA:D9EC/D9F4: total dormant attack duration shared by the saw pieces. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort AttackTicks = 576;
    /// <summary>$AA:DA36/DA4A/DA7C/DA84: shared attack displacement interval. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort AttackDisplacementTicks = 128;
    /// <summary>$AA:DA2E/DA42/DA7A: successive head, back-arm and front-arm activation starts differ by64 ticks. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort AttackStaggerTicks = 64;
    /// <summary>$AA:D9FE/DA06/DA5A: complete symmetric collision bob lasts20 ticks. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort BobTicks = 20;
    /// <summary>$AA:DA64/DA70: each inward body layer starts/ends four ticks nearer the bob center. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort BobStaggerTicks = 4;
    /// <summary>$AA:DA0E/12/16: primary saw's three-pose cadence. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort PrimarySawTicks = 10;
    /// <summary>$AA:DA1E/22/26: final saw's three-pose cadence after collision. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort FinalSawTicks = 3;
    /// <summary>$AA:DA72: stationary arm's repeated-pose hold. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort ArmHoldTicks = 119;
    /// <summary>$AA:DAA4: first head-facing hold; each next eighth-turn increases it by one. Base. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort FirstFacingTicks = 0x0774;
    /// <summary>$AA:DA8E/DAA2: final one-tick waits before head-program fallthrough; independent scheduling choice. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort HeadFallthroughTicks = 1;
    /// <summary>Recognizes OAM operands interleaved with Shaktool's compiled instruction words.</summary>
    /// <param name="address">Bank-$AA word address being classified.</param>
    /// <returns><see langword="true"/> when the address selects an installed presentation value.</returns>
    internal static bool IsPresentationWord(ushort address)
    {
        if (address >= SawHandPrimaryPiece && address < ArmPieceAttackBack)
        {
            int offset = (address - SawHandPrimaryPiece) % 16;
            return offset < 12 && (offset & 3) == 2;
        }
        return address == ArmPieceNormal + 2 || address >= HeadAimingLeft && address < FirstAdjacentCodeRoutine &&
            ((address - HeadAimingLeft) & 7) == 2;
    }
    /// <summary>Returns a compiled mechanics word and rejects addresses that belong to presentation data.</summary>
    /// <param name="address">Even bank-$AA address of a compiled engine-control operand.</param>
    /// <returns>The selected instruction or operand word.</returns>
    /// <exception cref="InvalidDataException">The address is outside compiled Shaktool programs or selects a presentation word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address < SawHandAttackPrimaryPiece || address >= FirstAdjacentCodeRoutine || (address & 1) != 0 || IsPresentationWord(address))
            throw new InvalidDataException($"Shaktool instruction mechanics pointer $AA:{address:X4} is not compiled.");
        return (ushort)ProgramWord(address);
    }
    /// <summary>Semantically selects the compiled instruction word at a bank-$AA address.</summary>
    /// <param name="address">Address within one of the supported Shaktool instruction lists.</param>
    /// <returns>The mechanics word, or <see cref="PresentationOperand"/> where the visual stream supplies the operand.</returns>
    internal static int ProgramWord(ushort address)
    {
        if (address < SawHandPrimaryPiece)
        {
            bool attack = address < SawHandHeadBobPrimaryPiece;
            bool final = attack ? address >= SawHandAttackFinalPiece : address >= SawHandHeadBobFinalPiece;
            ushort start = attack ? final ? SawHandAttackFinalPiece : SawHandAttackPrimaryPiece
                : final ? SawHandHeadBobFinalPiece : SawHandHeadBobPrimaryPiece;
            var writer = new WordSelector(address, start);
            writer.Wait(attack ? AttackTicks : BobTicks);
            if (final) writer.Command(ShaktoolInstructionCodes.Instruction_Shaktool_ResetShaktoolFunctions);
            writer.Goto(final ? SawHandFinalPiece : SawHandPrimaryPiece);
            return writer.Value;
        }
        if (address < ArmPieceAttackBack)
        {
            bool final = address >= SawHandFinalPiece;
            ushort start = final ? SawHandFinalPiece : SawHandPrimaryPiece;
            var writer = new WordSelector(address, start);
            for (int pose = 0; pose < 3; pose++) writer.Timed(final ? FinalSawTicks : PrimarySawTicks);
            writer.Goto(start);
            return writer.Value;
        }
        if (address < ArmPieceHeadBobBack)
        {
            bool front = address >= ArmPieceAttackFront;
            var writer = new WordSelector(address, front ? ArmPieceAttackFront : ArmPieceAttackBack);
            ushort lead = (ushort)(AttackDisplacementTicks + AttackStaggerTicks * (front ? 2 : 1));
            writer.Wait(lead);
            writer.Command(ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Lower1PixelAwayFromProj_AAD931);
            writer.Wait(AttackDisplacementTicks);
            writer.Command(ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Raise1PixelTowardsProj_AAD93F);
            writer.Wait((ushort)(AttackTicks - lead - AttackDisplacementTicks));
            writer.Goto(ArmPieceNormal);
            return writer.Value;
        }
        if (address < ArmPieceNormal)
        {
            bool front = address >= ArmPieceHeadBobFront;
            var writer = new WordSelector(address, front ? ArmPieceHeadBobFront : ArmPieceHeadBobBack);
            Bob(ref writer, front ? 1 : 0);
            if (!front) writer.Goto(ArmPieceNormal);
            return writer.Value;
        }
        if (address < HeadAttack)
        {
            var writer = new WordSelector(address, ArmPieceNormal);
            writer.Timed(ArmHoldTicks);
            writer.Goto(ArmPieceNormal);
            return writer.Value;
        }
        if (address < HeadHeadBob)
        {
            var writer = new WordSelector(address, HeadAttack);
            writer.Wait(AttackDisplacementTicks);
            writer.Command(ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Lower1PixelAwayFromProj_AAD931);
            writer.Command(ShaktoolInstructionCodes.RTL_AAD99F);
            writer.Wait(AttackDisplacementTicks);
            writer.Command(ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Raise1PixelTowardsProj_AAD93F);
            writer.Wait(AttackTicks - 2 * AttackDisplacementTicks);
            writer.Wait(HeadFallthroughTicks);
            return writer.Value;
        }
        if (address < HeadAimingLeft)
        {
            var writer = new WordSelector(address, HeadHeadBob);
            Bob(ref writer, 2);
            writer.Wait(HeadFallthroughTicks);
            return writer.Value;
        }
        int direction = (address - HeadAimingLeft) / 8;
        ushort facing = (ushort)(HeadAimingLeft + direction * 8);
        var aiming = new WordSelector(address, facing);
        aiming.Timed((ushort)(FirstFacingTicks + direction));
        aiming.Goto(facing);
        return aiming.Value;
    }
    /// <summary>Emits the staggered lower-and-raise sequence used by a Shaktool body layer's collision bob.</summary>
    /// <param name="writer">Selector positioned at the layer's first instruction word.</param>
    /// <param name="inwardLayer">Layer distance from the outer body, determining its lead-in and lead-out waits.</param>
    private static void Bob(ref WordSelector writer, int inwardLayer)
    {
        ushort lead = (ushort)(inwardLayer * BobStaggerTicks);
        if (lead != 0) writer.Wait(lead);
        writer.Command(ShaktoolInstructionCodes.Instruction_Shaktool_Lower1Pixel);
        writer.Wait((ushort)(BobTicks - 2 * lead));
        writer.Command(ShaktoolInstructionCodes.Instruction_Shaktool_Raise1Pixel);
        if (lead != 0) writer.Wait(lead);
    }
    /// <summary>Walks a semantic instruction sequence and captures the word at one requested program address.</summary>
    /// <param name="address">Target bank-relative word address whose emitted value is selected.</param>
    /// <param name="start">First word address represented by the semantic program writer.</param>
    private struct WordSelector(ushort address, ushort start)
    {
        /// <summary>Remaining emitted-word offset before the writer reaches the requested address.</summary>
        private int remaining = (address - start) / 2;
        /// <summary>Selected word value, or the sentinel indicating that no emitted word has reached the target.</summary>
        private int selected = int.MinValue;
        /// <summary>Gets the selected word once the semantic writer has emitted the target position.</summary>
        public readonly int Value => selected == int.MinValue
            ? throw new InvalidOperationException("Shaktool semantic program shape is incomplete.") : selected;
        /// <summary>Emits a one-word native instruction at the current semantic position.</summary>
        /// <param name="command">Instruction word to emit.</param>
        public void Command(ushort command) => Emit(command);
        /// <summary>Emits an animation duration followed by the presentation-operand marker.</summary>
        /// <param name="duration">Frame count loaded by the native instruction stream.</param>
        public void Timed(ushort duration) { Emit(duration); Emit(PresentationOperand); }
        /// <summary>Emits the native wait opcode and its frame-count operand.</summary>
        /// <param name="duration">Number of frames the instruction stream waits.</param>
        public void Wait(ushort duration) { Emit(CommonEnemyInstructionCodes.WaitFrames); Emit(duration); }
        /// <summary>Emits a native goto opcode and its target address.</summary>
        /// <param name="target">Instruction-list address selected by the branch.</param>
        public void Goto(ushort target) { Emit(CommonEnemyInstructionCodes.Goto); Emit(target); }
        /// <summary>Accounts for one semantic word and captures it when the writer reaches the requested address.</summary>
        /// <param name="value">Mechanics word or presentation marker being emitted.</param>
        private void Emit(int value) { if (remaining-- == 0) selected = value; }
    }
}

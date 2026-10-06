namespace SuperMetroid.Core.Game;

/// <summary>One compiled Shaktool mechanics word at its bank-$AA address.</summary>
internal readonly record struct ShaktoolInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for every Shaktool instruction program reachable from
/// its initialization, orientation, collision-recovery, and dormant-attack selectors.
/// Interleaved spritemap operands select installed presentation frames.
/// </summary>
internal static class ShaktoolInstructionProgramDefinitions
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
    /// <summary><c>InstList_Shaktool_Head_AimingUpLeft</c> at $AA:DAAC.</summary>
    internal const ushort HeadAimingUpLeft = 0xdaac;
    /// <summary><c>InstList_Shaktool_Head_AimingUp</c> at $AA:DAB4.</summary>
    internal const ushort HeadAimingUp = 0xdab4;
    /// <summary><c>InstList_Shaktool_Head_AimingUpRight</c> at $AA:DABC.</summary>
    internal const ushort HeadAimingUpRight = 0xdabc;
    /// <summary><c>InstList_Shaktool_Head_AimingRight</c> at $AA:DAC4.</summary>
    internal const ushort HeadAimingRight = 0xdac4;
    /// <summary><c>InstList_Shaktool_Head_AimingDownRight</c> at $AA:DACC.</summary>
    internal const ushort HeadAimingDownRight = 0xdacc;
    /// <summary><c>InstList_Shaktool_Head_AimingDown</c> at $AA:DAD4.</summary>
    internal const ushort HeadAimingDown = 0xdad4;
    /// <summary><c>InstList_Shaktool_Head_AimingDownLeft</c> at $AA:DADC.</summary>
    internal const ushort HeadAimingDownLeft = 0xdadc;

    /// <summary><c>RTS_AADAE4</c>, the first adjacent code routine at $AA:DAE4.</summary>
    internal const ushort FirstAdjacentCodeRoutine = 0xdae4;

    private const int PresentationOperand = -1;
    /// <summary>$AA:D9EC/D9F4: total dormant attack duration shared by the saw pieces; pending magnitude.</summary>
    private const ushort UnresolvedAttackTicks = 576;
    /// <summary>$AA:DA36/DA4A/DA7C/DA84: shared attack displacement interval; pending magnitude.</summary>
    private const ushort UnresolvedAttackDisplacementTicks = 128;
    /// <summary>$AA:DA2E/DA42/DA7A: successive head, back-arm and front-arm activation starts differ by64 ticks; pending stagger.</summary>
    private const ushort UnresolvedAttackStaggerTicks = 64;
    /// <summary>$AA:D9FE/DA06/DA5A: complete symmetric collision bob lasts20 ticks; pending magnitude.</summary>
    private const ushort UnresolvedBobTicks = 20;
    /// <summary>$AA:DA64/DA70: each inward body layer starts/ends four ticks nearer the bob center; pending stagger.</summary>
    private const ushort UnresolvedBobStaggerTicks = 4;
    /// <summary>$AA:DA0E/12/16: primary saw's three-pose cadence, pending.</summary>
    private const ushort UnresolvedPrimarySawTicks = 10;
    /// <summary>$AA:DA1E/22/26: final saw's three-pose cadence after collision, pending.</summary>
    private const ushort UnresolvedFinalSawTicks = 3;
    /// <summary>$AA:DA72: stationary arm's repeated-pose hold, pending.</summary>
    private const ushort UnresolvedArmHoldTicks = 119;
    /// <summary>$AA:DAA4: first head-facing hold; each next eighth-turn increases it by one. Base remains pending.</summary>
    private const ushort UnresolvedFirstFacingTicks = 0x0774;
    /// <summary>$AA:DA8E/DAA2: final one-tick waits before head-program fallthrough; independent scheduling choice remains pending.</summary>
    private const ushort UnresolvedHeadFallthroughTicks = 1;

    internal static int MechanicsWordCount => 110;
    internal static int PresentationWordCount => 15;
    internal static ShaktoolInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        for (int address = SawHandAttackPrimaryPiece; address < FirstAdjacentCodeRoutine; address += 2)
        {
            int value = ProgramWord((ushort)address);
            if (value != PresentationOperand && index-- == 0) return new((ushort)address, (ushort)value);
        }
        throw new InvalidOperationException("Shaktool mechanics-word index is inconsistent.");
    }
    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 6) return (ushort)((index < 3 ? SawHandPrimaryPiece : SawHandFinalPiece) + (index % 3) * 4 + 2);
        if (index == 6) return ArmPieceNormal + 2;
        return (ushort)(HeadAimingLeft + (index - 7) * 8 + 2);
    }
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
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address < SawHandAttackPrimaryPiece || address >= FirstAdjacentCodeRoutine || (address & 1) != 0 || IsPresentationWord(address))
            throw new InvalidDataException($"Shaktool instruction mechanics pointer $AA:{address:X4} is not compiled.");
        return (ushort)ProgramWord(address);
    }
    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000) return false;
        ushort bankAddress = (ushort)(address & 0xfffe);
        return bankAddress >= SawHandAttackPrimaryPiece && bankAddress < FirstAdjacentCodeRoutine && !IsPresentationWord(bankAddress);
    }
    private static int ProgramWord(ushort address)
    {
        if (address < SawHandPrimaryPiece)
        {
            bool attack = address < SawHandHeadBobPrimaryPiece;
            bool final = attack ? address >= SawHandAttackFinalPiece : address >= SawHandHeadBobFinalPiece;
            ushort start = attack ? final ? SawHandAttackFinalPiece : SawHandAttackPrimaryPiece
                : final ? SawHandHeadBobFinalPiece : SawHandHeadBobPrimaryPiece;
            var writer = new WordSelector(address, start);
            writer.Wait(attack ? UnresolvedAttackTicks : UnresolvedBobTicks);
            if (final) writer.Command(ShaktoolInstructionCodes.Instruction_Shaktool_ResetShaktoolFunctions);
            writer.Goto(final ? SawHandFinalPiece : SawHandPrimaryPiece);
            return writer.Value;
        }
        if (address < ArmPieceAttackBack)
        {
            bool final = address >= SawHandFinalPiece;
            ushort start = final ? SawHandFinalPiece : SawHandPrimaryPiece;
            var writer = new WordSelector(address, start);
            for (int pose = 0; pose < 3; pose++) writer.Timed(final ? UnresolvedFinalSawTicks : UnresolvedPrimarySawTicks);
            writer.Goto(start);
            return writer.Value;
        }
        if (address < ArmPieceHeadBobBack)
        {
            bool front = address >= ArmPieceAttackFront;
            var writer = new WordSelector(address, front ? ArmPieceAttackFront : ArmPieceAttackBack);
            ushort lead = (ushort)(UnresolvedAttackDisplacementTicks + UnresolvedAttackStaggerTicks * (front ? 2 : 1));
            writer.Wait(lead);
            writer.Command(ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Lower1PixelAwayFromProj_AAD931);
            writer.Wait(UnresolvedAttackDisplacementTicks);
            writer.Command(ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Raise1PixelTowardsProj_AAD93F);
            writer.Wait((ushort)(UnresolvedAttackTicks - lead - UnresolvedAttackDisplacementTicks));
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
            writer.Timed(UnresolvedArmHoldTicks);
            writer.Goto(ArmPieceNormal);
            return writer.Value;
        }
        if (address < HeadHeadBob)
        {
            var writer = new WordSelector(address, HeadAttack);
            writer.Wait(UnresolvedAttackDisplacementTicks);
            writer.Command(ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Lower1PixelAwayFromProj_AAD931);
            writer.Command(ShaktoolInstructionCodes.RTL_AAD99F);
            writer.Wait(UnresolvedAttackDisplacementTicks);
            writer.Command(ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Raise1PixelTowardsProj_AAD93F);
            writer.Wait(UnresolvedAttackTicks - 2 * UnresolvedAttackDisplacementTicks);
            writer.Wait(UnresolvedHeadFallthroughTicks);
            return writer.Value;
        }
        if (address < HeadAimingLeft)
        {
            var writer = new WordSelector(address, HeadHeadBob);
            Bob(ref writer, 2);
            writer.Wait(UnresolvedHeadFallthroughTicks);
            return writer.Value;
        }
        int direction = (address - HeadAimingLeft) / 8;
        ushort facing = (ushort)(HeadAimingLeft + direction * 8);
        var aiming = new WordSelector(address, facing);
        aiming.Timed((ushort)(UnresolvedFirstFacingTicks + direction));
        aiming.Goto(facing);
        return aiming.Value;
    }
    private static void Bob(ref WordSelector writer, int inwardLayer)
    {
        ushort lead = (ushort)(inwardLayer * UnresolvedBobStaggerTicks);
        if (lead != 0) writer.Wait(lead);
        writer.Command(ShaktoolInstructionCodes.Instruction_Shaktool_Lower1Pixel);
        writer.Wait((ushort)(UnresolvedBobTicks - 2 * lead));
        writer.Command(ShaktoolInstructionCodes.Instruction_Shaktool_Raise1Pixel);
        if (lead != 0) writer.Wait(lead);
    }
    private struct WordSelector(ushort address, ushort start)
    {
        private int remaining = (address - start) / 2;
        private int selected = int.MinValue;
        public readonly int Value => selected == int.MinValue
            ? throw new InvalidOperationException("Shaktool semantic program shape is incomplete.") : selected;
        public void Command(ushort command) => Emit(command);
        public void Timed(ushort duration) { Emit(duration); Emit(PresentationOperand); }
        public void Wait(ushort duration) { Emit(CommonEnemyInstructionCodes.WaitFrames); Emit(duration); }
        public void Goto(ushort target) { Emit(CommonEnemyInstructionCodes.Goto); Emit(target); }
        private void Emit(int value) { if (remaining-- == 0) selected = value; }
    }
}

namespace SuperMetroid.Core.Game;

/// <summary>One compiled Shaktool mechanics word at its bank-$AA address.</summary>
internal readonly record struct ShaktoolInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for every Shaktool instruction program reachable from
/// its initialization, orientation, collision-recovery, and dormant-attack selectors.
/// Interleaved spritemap operands remain cartridge presentation data.
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

    private static readonly ShaktoolInstructionMechanicsWord[] Words =
    [
        new(SawHandAttackPrimaryPiece, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd9ec, 0x0240), new(0xd9ee, CommonEnemyInstructionCodes.Goto),
        new(0xd9f0, SawHandPrimaryPiece),
        new(SawHandAttackFinalPiece, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd9f4, 0x0240),
        new(0xd9f6, ShaktoolInstructionCodes.Instruction_Shaktool_ResetShaktoolFunctions),
        new(0xd9f8, CommonEnemyInstructionCodes.Goto), new(0xd9fa, SawHandFinalPiece),
        new(SawHandHeadBobPrimaryPiece, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd9fe, 0x0014), new(0xda00, CommonEnemyInstructionCodes.Goto),
        new(0xda02, SawHandPrimaryPiece),
        new(SawHandHeadBobFinalPiece, CommonEnemyInstructionCodes.WaitFrames),
        new(0xda06, 0x0014),
        new(0xda08, ShaktoolInstructionCodes.Instruction_Shaktool_ResetShaktoolFunctions),
        new(0xda0a, CommonEnemyInstructionCodes.Goto), new(0xda0c, SawHandFinalPiece),

        new(SawHandPrimaryPiece, 0x000a), new(0xda12, 0x000a),
        new(0xda16, 0x000a), new(0xda1a, CommonEnemyInstructionCodes.Goto),
        new(0xda1c, SawHandPrimaryPiece),
        new(SawHandFinalPiece, 0x0003), new(0xda22, 0x0003),
        new(0xda26, 0x0003), new(0xda2a, CommonEnemyInstructionCodes.Goto),
        new(0xda2c, SawHandFinalPiece),

        new(ArmPieceAttackBack, CommonEnemyInstructionCodes.WaitFrames),
        new(0xda30, 0x00c0),
        new(0xda32, ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Lower1PixelAwayFromProj_AAD931),
        new(0xda34, CommonEnemyInstructionCodes.WaitFrames), new(0xda36, 0x0080),
        new(0xda38, ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Raise1PixelTowardsProj_AAD93F),
        new(0xda3a, CommonEnemyInstructionCodes.WaitFrames), new(0xda3c, 0x0100),
        new(0xda3e, CommonEnemyInstructionCodes.Goto), new(0xda40, ArmPieceNormal),
        new(ArmPieceAttackFront, CommonEnemyInstructionCodes.WaitFrames),
        new(0xda44, 0x0100),
        new(0xda46, ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Lower1PixelAwayFromProj_AAD931),
        new(0xda48, CommonEnemyInstructionCodes.WaitFrames), new(0xda4a, 0x0080),
        new(0xda4c, ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Raise1PixelTowardsProj_AAD93F),
        new(0xda4e, CommonEnemyInstructionCodes.WaitFrames), new(0xda50, 0x00c0),
        new(0xda52, CommonEnemyInstructionCodes.Goto), new(0xda54, ArmPieceNormal),

        new(ArmPieceHeadBobBack, ShaktoolInstructionCodes.Instruction_Shaktool_Lower1Pixel),
        new(0xda58, CommonEnemyInstructionCodes.WaitFrames), new(0xda5a, 0x0014),
        new(0xda5c, ShaktoolInstructionCodes.Instruction_Shaktool_Raise1Pixel),
        new(0xda5e, CommonEnemyInstructionCodes.Goto), new(0xda60, ArmPieceNormal),
        new(ArmPieceHeadBobFront, CommonEnemyInstructionCodes.WaitFrames),
        new(0xda64, 0x0004),
        new(0xda66, ShaktoolInstructionCodes.Instruction_Shaktool_Lower1Pixel),
        new(0xda68, CommonEnemyInstructionCodes.WaitFrames), new(0xda6a, 0x000c),
        new(0xda6c, ShaktoolInstructionCodes.Instruction_Shaktool_Raise1Pixel),
        new(0xda6e, CommonEnemyInstructionCodes.WaitFrames), new(0xda70, 0x0004),
        new(ArmPieceNormal, 0x0077), new(0xda76, CommonEnemyInstructionCodes.Goto),
        new(0xda78, ArmPieceNormal),

        new(HeadAttack, CommonEnemyInstructionCodes.WaitFrames), new(0xda7c, 0x0080),
        new(0xda7e, ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Lower1PixelAwayFromProj_AAD931),
        new(0xda80, ShaktoolInstructionCodes.RTL_AAD99F),
        new(0xda82, CommonEnemyInstructionCodes.WaitFrames), new(0xda84, 0x0080),
        new(0xda86, ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Raise1PixelTowardsProj_AAD93F),
        new(0xda88, CommonEnemyInstructionCodes.WaitFrames), new(0xda8a, 0x0140),
        new(0xda8c, CommonEnemyInstructionCodes.WaitFrames), new(0xda8e, 0x0001),
        new(HeadHeadBob, CommonEnemyInstructionCodes.WaitFrames), new(0xda92, 0x0008),
        new(0xda94, ShaktoolInstructionCodes.Instruction_Shaktool_Lower1Pixel),
        new(0xda96, CommonEnemyInstructionCodes.WaitFrames), new(0xda98, 0x0004),
        new(0xda9a, ShaktoolInstructionCodes.Instruction_Shaktool_Raise1Pixel),
        new(0xda9c, CommonEnemyInstructionCodes.WaitFrames), new(0xda9e, 0x0008),
        new(0xdaa0, CommonEnemyInstructionCodes.WaitFrames), new(0xdaa2, 0x0001),

        new(HeadAimingLeft, 0x0774), new(0xdaa8, CommonEnemyInstructionCodes.Goto),
        new(0xdaaa, HeadAimingLeft),
        new(HeadAimingUpLeft, 0x0775), new(0xdab0, CommonEnemyInstructionCodes.Goto),
        new(0xdab2, HeadAimingUpLeft),
        new(HeadAimingUp, 0x0776), new(0xdab8, CommonEnemyInstructionCodes.Goto),
        new(0xdaba, HeadAimingUp),
        new(HeadAimingUpRight, 0x0777), new(0xdac0, CommonEnemyInstructionCodes.Goto),
        new(0xdac2, HeadAimingUpRight),
        new(HeadAimingRight, 0x0778), new(0xdac8, CommonEnemyInstructionCodes.Goto),
        new(0xdaca, HeadAimingRight),
        new(HeadAimingDownRight, 0x0779), new(0xdad0, CommonEnemyInstructionCodes.Goto),
        new(0xdad2, HeadAimingDownRight),
        new(HeadAimingDown, 0x077a), new(0xdad8, CommonEnemyInstructionCodes.Goto),
        new(0xdada, HeadAimingDown),
        new(HeadAimingDownLeft, 0x077b), new(0xdae0, CommonEnemyInstructionCodes.Goto),
        new(0xdae2, HeadAimingDownLeft),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xda10, 0xda14, 0xda18,
        0xda20, 0xda24, 0xda28,
        0xda74,
        0xdaa6, 0xdaae, 0xdab6, 0xdabe, 0xdac6, 0xdace, 0xdad6, 0xdade,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ShaktoolInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            ShaktoolInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Shaktool instruction mechanics pointer $AA:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

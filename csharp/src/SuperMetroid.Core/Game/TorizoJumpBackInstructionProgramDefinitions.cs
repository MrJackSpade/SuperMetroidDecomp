namespace SuperMetroid.Core.Game;

internal readonly record struct TorizoJumpBackMechanicsWord(
    ushort Address, ushort Value);

/// <summary>
/// Engine-owned bank-$AA control words for the two shared Bomb/Golden Torizo
/// right-facing backward-jump lists at $AA:C110-C187. Their eight interleaved
/// selectors use three editable visual frames; link targets, movement callbacks,
/// durations and branch operands remain compiled cartridge mechanics.
/// </summary>
internal static class TorizoJumpBackInstructionProgramDefinitions
{
    internal const ushort Start = 0xc110;
    internal const ushort End = 0xc188;

    private static readonly TorizoJumpBackMechanicsWord[] Words =
    [
        new(0xc110, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xc112, 0xc82c),
        new(0xc114, TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY),
        new(0xc116, 0xc128),
        new(0xc118, 0x0005),
        new(0xc11c, 0x0005),
        new(0xc120, 0x0001),
        new(0xc124, TorizoInstructionCodes.Instruction_Torizo_GotoY_IfRising),
        new(0xc126, 0xc120),
        new(0xc128, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xc12a, 0xc82c),
        new(0xc12c, TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY),
        new(0xc12e, 0xc138),
        new(0xc130, 0x0005),
        new(0xc134, CommonEnemyInstructionCodes.Goto),
        new(0xc136, 0xc130),
        new(0xc138, TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        new(0xc13a, TorizoInstructionCodes.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        new(0xc13c, TorizoInstructionCodes.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden),
        new(0xc13e, 0xc192),
        new(0xc140, 0xcdc3),
        new(0xc142, TorizoInstructionCodes.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack),
        new(0xc144, 0xbec0),
        new(0xc146, 0xbf6c),
        new(0xc148, CommonEnemyInstructionCodes.Goto),
        new(0xc14a, 0xbde2),
        new(0xc14c, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xc14e, 0xc82c),
        new(0xc150, TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY),
        new(0xc152, 0xc164),
        new(0xc154, 0x0005),
        new(0xc158, 0x0005),
        new(0xc15c, 0x0001),
        new(0xc160, TorizoInstructionCodes.Instruction_Torizo_GotoY_IfRising),
        new(0xc162, 0xc15c),
        new(0xc164, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xc166, 0xc82c),
        new(0xc168, TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY),
        new(0xc16a, 0xc174),
        new(0xc16c, 0x0005),
        new(0xc170, CommonEnemyInstructionCodes.Goto),
        new(0xc172, 0xc16c),
        new(0xc174, TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        new(0xc176, TorizoInstructionCodes.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        new(0xc178, TorizoInstructionCodes.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden),
        new(0xc17a, 0xc1cc),
        new(0xc17c, 0xcdcd),
        new(0xc17e, TorizoInstructionCodes.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack),
        new(0xc180, 0xbe7e),
        new(0xc182, 0xbf02),
        new(0xc184, CommonEnemyInstructionCodes.Goto),
        new(0xc186, 0xbe30),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xc11a, 0xc11e, 0xc122, 0xc132,
        0xc156, 0xc15a, 0xc15e, 0xc16e,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static TorizoJumpBackMechanicsWord MechanicsWord(int index) => Words[index];
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (TorizoJumpBackMechanicsWord word in Words)
        {
            if (word.Address != address) continue;
            value = word.Value;
            return true;
        }
        value = 0;
        return false;
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
            return false;
        ushort offset = unchecked((ushort)address);
        foreach (TorizoJumpBackMechanicsWord word in Words)
            if (offset == word.Address ||
                offset == unchecked((ushort)(word.Address + 1)))
                return true;
        return false;
    }
}

namespace SuperMetroid.Core.Game;

/// <summary>
/// Engine-owned bank-$AA control words for the two shared Bomb/Golden Torizo
/// left-facing backward-jump lists at $AA:BC96-BD0D. Eight interleaved frame
/// pointers remain presentation selectors, independent of the instruction
/// callbacks, durations, and branch destinations compiled here.
/// </summary>
internal static class TorizoJumpBackLeftInstructionProgramDefinitions
{
    internal const ushort Start = 0xbc96;
    internal const ushort End = 0xbd0e;

    private static readonly TorizoJumpBackMechanicsWord[] Words =
    [
        new(0xbc96, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xbc98, 0xc82c),
        new(0xbc9a, TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY),
        new(0xbc9c, 0xbcae),
        new(0xbc9e, 0x0005),
        new(0xbca2, 0x0005),
        new(0xbca6, 0x0001),
        new(0xbcaa, TorizoInstructionCodes.Instruction_Torizo_GotoY_IfRising),
        new(0xbcac, 0xbca6),
        new(0xbcae, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xbcb0, 0xc82c),
        new(0xbcb2, TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY),
        new(0xbcb4, 0xbcbe),
        new(0xbcb6, 0x0005),
        new(0xbcba, CommonEnemyInstructionCodes.Goto),
        new(0xbcbc, 0xbcb6),
        new(0xbcbe, TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        new(0xbcc0, TorizoInstructionCodes.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        new(0xbcc2, TorizoInstructionCodes.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden),
        new(0xbcc4, 0xbd18),
        new(0xbcc6, 0xcdaf),
        new(0xbcc8, TorizoInstructionCodes.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack),
        new(0xbcca, 0xba46),
        new(0xbccc, 0xbaf2),
        new(0xbcce, CommonEnemyInstructionCodes.Goto),
        new(0xbcd0, 0xb96c),
        new(0xbcd2, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xbcd4, 0xc82c),
        new(0xbcd6, TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY),
        new(0xbcd8, 0xbcea),
        new(0xbcda, 0x0005),
        new(0xbcde, 0x0005),
        new(0xbce2, 0x0001),
        new(0xbce6, TorizoInstructionCodes.Instruction_Torizo_GotoY_IfRising),
        new(0xbce8, 0xbce2),
        new(0xbcea, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xbcec, 0xc82c),
        new(0xbcee, TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY),
        new(0xbcf0, 0xbcfa),
        new(0xbcf2, 0x0005),
        new(0xbcf6, CommonEnemyInstructionCodes.Goto),
        new(0xbcf8, 0xbcf2),
        new(0xbcfa, TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        new(0xbcfc, TorizoInstructionCodes.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        new(0xbcfe, TorizoInstructionCodes.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden),
        new(0xbd00, 0xbd52),
        new(0xbd02, 0xcdb9),
        new(0xbd04, TorizoInstructionCodes.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack),
        new(0xbd06, 0xba04),
        new(0xbd08, 0xba88),
        new(0xbd0a, CommonEnemyInstructionCodes.Goto),
        new(0xbd0c, 0xb9b6),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xbca0, 0xbca4, 0xbca8, 0xbcb8,
        0xbcdc, 0xbce0, 0xbce4, 0xbcf4,
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
            if (offset == word.Address || offset == unchecked((ushort)(word.Address + 1)))
                return true;
        return false;
    }
}

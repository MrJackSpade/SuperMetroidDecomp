namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoAwakeningMechanicsWord(
    ushort Address, ushort Value);

/// <summary>
/// The bounded continuation of <c>InstList_GoldenTorizo_Initial_0</c> at
/// $AA:C9E2-CACD: fall, sitting-down tile swaps, standing-up animation,
/// palette change, and the handoff to the ordinary walking program. The
/// interleaved extended-spritemap selectors are presentation data; the eight
/// $814B upload descriptors and their pixels have separate installed owners.
/// </summary>
internal static class GoldenTorizoAwakeningInstructionProgramDefinitions
{
    /// <summary>First instruction after the Samus-position sleep at $AA:C9E2.</summary>
    internal const ushort Start = 0xc9e2;

    /// <summary>First word after the awakening handoff at $AA:CACE.</summary>
    internal const ushort End = 0xcace;

    private static readonly GoldenTorizoAwakeningMechanicsWord[] Words =
    [
        new(0xc9e2, TorizoInstructionCodes.Instruction_Torizo_FunctionInY),
        new(0xc9e4, 0xc6bf),
        new(0xc9e6, 0x0001),
        new(0xc9ea, TorizoInstructionCodes.Instruction_Torizo_GotoY_IfNotHitGround),
        new(0xc9ec, 0xc9e6),
        new(0xc9ee, TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        new(0xc9f0, 0x0003),
        new(0xc9f4, TorizoInstructionCodes.Instruction_Torizo_SittingDownMovement_IndexInY),
        new(0xc9f6, 0x0004),
        new(0xc9f8, 0x0004),
        new(0xc9fc, TorizoInstructionCodes.Instruction_Torizo_SittingDownMovement_IndexInY),
        new(0xc9fe, 0x0002),
        new(0xca00, 0x0005),
        new(0xca04, TorizoInstructionCodes.Instruction_Torizo_SittingDownMovement_IndexInY),
        new(0xca06, 0x0000),
        new(0xca08, TorizoInstructionCodes.Instruction_Torizo_FunctionInY),
        new(0xca0a, 0xc6ab),
        new(0xca0c, 0x0030),
        new(0xca10, CommonEnemyInstructionCodes.CopyToVram),
        new(0xca19, 0x0020),
        new(0xca1d, CommonEnemyInstructionCodes.CopyToVram),
        new(0xca26, 0x0010),
        new(0xca2a, CommonEnemyInstructionCodes.CopyToVram),
        new(0xca33, 0x0008),
        new(0xca37, CommonEnemyInstructionCodes.CopyToVram),
        new(0xca40, 0x0020),
        new(0xca44, CommonEnemyInstructionCodes.SetTimer),
        new(0xca46, 0x0002),
        new(0xca48, 0x0004),
        new(0xca4c, CommonEnemyInstructionCodes.CopyToVram),
        new(0xca55, 0x0004),
        new(0xca59, CommonEnemyInstructionCodes.CopyToVram),
        new(0xca62, 0x0004),
        new(0xca66, CommonEnemyInstructionCodes.CopyToVram),
        new(0xca6f, 0x0004),
        new(0xca73, CommonEnemyInstructionCodes.CopyToVram),
        new(0xca7c, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xca7e, 0xca48),
        new(0xca80, 0x0020),
        new(0xca84, TorizoInstructionCodes.Instruction_Torizo_StandingUpMovement_IndexInY),
        new(0xca86, 0x0000),
        new(0xca88, 0x000c),
        new(0xca8c, TorizoInstructionCodes.Instruction_Torizo_StandingUpMovement_IndexInY),
        new(0xca8e, 0x0002),
        new(0xca90, 0x0008),
        new(0xca94, TorizoInstructionCodes.Instruction_Torizo_StandingUpMovement_IndexInY),
        new(0xca96, 0x0004),
        new(0xca98, 0x0008),
        new(0xca9c, TorizoInstructionCodes.Instruction_Torizo_StandingUpMovement_IndexInY),
        new(0xca9e, 0x0006),
        new(0xcaa0, 0x0008),
        new(0xcaa4, TorizoInstructionCodes.Instruction_Torizo_StandingUpMovement_IndexInY),
        new(0xcaa6, 0x0008),
        new(0xcaa8, 0x0008),
        new(0xcaac, TorizoInstructionCodes.Instruction_Torizo_StandingUpMovement_IndexInY),
        new(0xcaae, 0x000a),
        new(0xcab0, TorizoInstructionCodes.Instruction_Torizo_LoadGoldenTorizoPalettes),
        new(0xcab2, CommonEnemyInstructionCodes.SetTimer),
        new(0xcab4, 0x0010),
        new(0xcab6, 0x0004),
        new(0xcaba, TorizoInstructionCodes.Instruction_Torizo_AdvanceGradualColorChange),
        new(0xcabc, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xcabe, 0xcab6),
        new(0xcac0, TorizoInstructionCodes.RTL_AAC2C8),
        new(0xcac2, TorizoInstructionCodes.Instruction_Torizo_ClearAnimationLock),
        new(0xcac4, TorizoInstructionCodes.Inst_Torizo_StartFightMusic_GoldenTorizoBellyPaletteFX),
        new(0xcac6, 0x0010),
        new(0xcaca, CommonEnemyInstructionCodes.Goto),
        new(0xcacc, 0xd259),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xc9e8, 0xc9f2, 0xc9fa, 0xca02, 0xca0e, 0xca1b, 0xca28,
        0xca35, 0xca42, 0xca4a, 0xca57, 0xca64, 0xca71, 0xca82,
        0xca8a, 0xca92, 0xca9a, 0xcaa2, 0xcaaa, 0xcab8, 0xcac8,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static GoldenTorizoAwakeningMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ushort PresentationWordAddress(int index) =>
        PresentationWords[index];

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (GoldenTorizoAwakeningMechanicsWord word in Words)
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
        foreach (GoldenTorizoAwakeningMechanicsWord word in Words)
            if (offset == word.Address ||
                offset == unchecked((ushort)(word.Address + 1)))
                return true;
        return false;
    }
}

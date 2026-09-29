namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoLeftOrbMechanicsWord(
    ushort Address, ushort Value);

/// <summary>
/// The two callable left-facing Golden Torizo Chozo-orb lists at
/// $AA:CAFF-CB82. Their 20 interleaved extended-frame selectors are editable
/// presentation; callbacks, timers, loop operands, and movement functions are
/// fixed cartridge mechanics.
/// </summary>
internal static class GoldenTorizoLeftOrbInstructionProgramDefinitions
{
    internal const ushort Start = 0xcaff;
    internal const ushort LeftFootForward = 0xcb41;
    internal const ushort End = 0xcb83;

    private static readonly GoldenTorizoLeftOrbMechanicsWord[] Words =
    [
        new(0xcaff, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xcb01, 0xd5ed),
        new(0xcb03, 0x0006),
        new(0xcb07, 0x0003),
        new(0xcb0b, 0x0003),
        new(0xcb0f, 0x0003),
        new(0xcb13, 0x0003),
        new(0xcb17, 0x0006),
        new(0xcb1b, CommonEnemyInstructionCodes.SetTimer),
        new(0xcb1d, 0x0006),
        new(0xcb1f, TorizoInstructionCodes.Instruction_Torizo_PlayShotTorizoSFX),
        new(0xcb21, TorizoInstructionCodes.Instruction_GoldenTorizo_SpawnChozoOrbs),
        new(0xcb23, CommonEnemyInstructionCodes.WaitFrames),
        new(0xcb25, 0x0006),
        new(0xcb27, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xcb29, 0xcb1f),
        new(0xcb2b, 0x0003),
        new(0xcb2f, 0x0003),
        new(0xcb33, 0x0003),
        new(0xcb37, 0x0003),
        new(0xcb3b, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xcb3d, 0xd5f1),
        new(0xcb3f, TorizoInstructionCodes.Instruction_Torizo_Return),
        new(0xcb41, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xcb43, 0xd5ed),
        new(0xcb45, 0x0006),
        new(0xcb49, 0x0003),
        new(0xcb4d, 0x0003),
        new(0xcb51, 0x0003),
        new(0xcb55, 0x0003),
        new(0xcb59, 0x0006),
        new(0xcb5d, CommonEnemyInstructionCodes.SetTimer),
        new(0xcb5f, 0x0006),
        new(0xcb61, TorizoInstructionCodes.Instruction_Torizo_PlayShotTorizoSFX),
        new(0xcb63, TorizoInstructionCodes.Instruction_GoldenTorizo_SpawnChozoOrbs),
        new(0xcb65, CommonEnemyInstructionCodes.WaitFrames),
        new(0xcb67, 0x0006),
        new(0xcb69, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xcb6b, 0xcb61),
        new(0xcb6d, 0x0003),
        new(0xcb71, 0x0003),
        new(0xcb75, 0x0003),
        new(0xcb79, 0x0003),
        new(0xcb7d, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xcb7f, 0xd5f1),
        new(0xcb81, TorizoInstructionCodes.Instruction_Torizo_Return),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xcb05, 0xcb09, 0xcb0d, 0xcb11, 0xcb15,
        0xcb19, 0xcb2d, 0xcb31, 0xcb35, 0xcb39,
        0xcb47, 0xcb4b, 0xcb4f, 0xcb53, 0xcb57,
        0xcb5b, 0xcb6f, 0xcb73, 0xcb77, 0xcb7b,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static GoldenTorizoLeftOrbMechanicsWord MechanicsWord(int index) => Words[index];
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (GoldenTorizoLeftOrbMechanicsWord word in Words)
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
        foreach (GoldenTorizoLeftOrbMechanicsWord word in Words)
            if (offset == word.Address || offset == unchecked((ushort)(word.Address + 1)))
                return true;
        return false;
    }
}

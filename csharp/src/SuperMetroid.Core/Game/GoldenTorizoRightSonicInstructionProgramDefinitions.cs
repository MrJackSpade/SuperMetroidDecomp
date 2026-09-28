namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoRightSonicMechanicsWord(
    ushort Address, ushort Value);

/// <summary>
/// Bank-$AA Golden Torizo's two right-facing sonic-boom attack lists at
/// $AA:CCDB-CDAE. Their 66 timers, callbacks, movement and loop words are
/// immutable mechanics; 40 interleaved sprite selectors are presentation.
/// </summary>
internal static class GoldenTorizoRightSonicInstructionProgramDefinitions
{
    internal const ushort Start = 0xccdb;
    internal const ushort RightFootForward = 0xcd45;
    internal const ushort End = 0xcdaf;

    private static readonly GoldenTorizoRightSonicMechanicsWord[] Words =
    [
        new(0xccdb, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xccdd, 0xd5ed),
        new(0xccdf, CommonEnemyInstructionCodes.SetTimer),
        new(0xcce1, 0x0004),
        new(0xcce3, 0x0003),
        new(0xcce7, 0x0003),
        new(0xcceb, 0x0003),
        new(0xccef, 0x0003),
        new(0xccf3, 0x0003),
        new(0xccf7, 0x0003),
        new(0xccfb, 0x0001),
        new(0xccff, 0x0001),
        new(0xcd03, TorizoInstructionCodes.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY),
        new(0xcd05, 0x0000),
        new(0xcd07, 0x0001),
        new(0xcd0b, 0x0004),
        new(0xcd0f, 0x0003),
        new(0xcd13, 0x0003),
        new(0xcd17, 0x0003),
        new(0xcd1b, 0x0003),
        new(0xcd1f, 0x0003),
        new(0xcd23, 0x0003),
        new(0xcd27, 0x0001),
        new(0xcd2b, 0x0001),
        new(0xcd2f, TorizoInstructionCodes.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY),
        new(0xcd31, 0x0001),
        new(0xcd33, 0x0001),
        new(0xcd37, 0x0004),
        new(0xcd3b, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xcd3d, 0xcce3),
        new(0xcd3f, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xcd41, 0xd5f1),
        new(0xcd43, TorizoInstructionCodes.Instruction_Torizo_Return),
        new(0xcd45, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xcd47, 0xd5ed),
        new(0xcd49, CommonEnemyInstructionCodes.SetTimer),
        new(0xcd4b, 0x0004),
        new(0xcd4d, 0x0003),
        new(0xcd51, 0x0003),
        new(0xcd55, 0x0003),
        new(0xcd59, 0x0003),
        new(0xcd5d, 0x0003),
        new(0xcd61, 0x0003),
        new(0xcd65, 0x0001),
        new(0xcd69, 0x0001),
        new(0xcd6d, TorizoInstructionCodes.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY),
        new(0xcd6f, 0x0000),
        new(0xcd71, 0x0001),
        new(0xcd75, 0x0004),
        new(0xcd79, 0x0003),
        new(0xcd7d, 0x0003),
        new(0xcd81, 0x0003),
        new(0xcd85, 0x0003),
        new(0xcd89, 0x0003),
        new(0xcd8d, 0x0003),
        new(0xcd91, 0x0001),
        new(0xcd95, 0x0001),
        new(0xcd99, TorizoInstructionCodes.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY),
        new(0xcd9b, 0x0001),
        new(0xcd9d, 0x0001),
        new(0xcda1, 0x0004),
        new(0xcda5, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xcda7, 0xcd4d),
        new(0xcda9, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xcdab, 0xd5f1),
        new(0xcdad, TorizoInstructionCodes.Instruction_Torizo_Return),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xcce5, 0xcce9, 0xcced, 0xccf1, 0xccf5, 0xccf9, 0xccfd, 0xcd01,
        0xcd09, 0xcd0d, 0xcd11, 0xcd15, 0xcd19, 0xcd1d, 0xcd21, 0xcd25,
        0xcd29, 0xcd2d, 0xcd35, 0xcd39,
        0xcd4f, 0xcd53, 0xcd57, 0xcd5b, 0xcd5f, 0xcd63, 0xcd67, 0xcd6b,
        0xcd73, 0xcd77, 0xcd7b, 0xcd7f, 0xcd83, 0xcd87, 0xcd8b, 0xcd8f,
        0xcd93, 0xcd97, 0xcd9f, 0xcda3,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static GoldenTorizoRightSonicMechanicsWord MechanicsWord(int index) => Words[index];
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (GoldenTorizoRightSonicMechanicsWord word in Words)
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
        foreach (GoldenTorizoRightSonicMechanicsWord word in Words)
            if (offset == word.Address ||
                offset == unchecked((ushort)(word.Address + 1)))
                return true;
        return false;
    }
}

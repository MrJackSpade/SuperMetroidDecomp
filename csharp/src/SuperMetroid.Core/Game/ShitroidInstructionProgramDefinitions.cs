namespace SuperMetroid.Core.Game;

internal readonly record struct ShitroidInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the Tourian Shitroid's finish-draining,
/// normal, latched, and remorse programs. Their thirty spritemap operands remain
/// live cartridge presentation data.
/// </summary>
internal static class ShitroidInstructionProgramDefinitions
{
    /// <summary><c>InstList_BabyMetroid_FinishDraining</c> at $A9:F906.</summary>
    internal const ushort FinishDraining = 0xf906;

    /// <summary><c>InstList_BabyMetroid_Normal</c> at $A9:F90E.</summary>
    internal const ushort Normal = 0xf90e;

    /// <summary><c>InstList_BabyMetroid_LatchedOn</c> at $A9:F924.</summary>
    internal const ushort LatchedOn = 0xf924;

    /// <summary><c>InstList_BabyMetroid_Remorse</c> at $A9:F93A.</summary>
    internal const ushort Remorse = 0xf93a;

    /// <summary>The random remorse branch callback word at $A9:F95A.</summary>
    internal const ushort RemorseRandomBranchOpcode = 0xf95a;

    /// <summary>The final unconditional remorse-loop callback word at $A9:F98E.</summary>
    internal const ushort RemorseLoopOpcode = 0xf98e;

    /// <summary>The first callback implementation after the programs, at $A9:F990.</summary>
    internal const ushort FirstAdjacentCallbackCode = 0xf990;

    private static readonly ShitroidInstructionMechanicsWord[] Words =
    [
        new(FinishDraining, 0x0080), new(0xf90a, 0x0010),

        new(Normal, 0x0010), new(0xf912, 0x0010),
        new(0xf916, 0x0010), new(0xf91a, 0x0010),
        new(0xf91e, EnemyInstructionCodePointers.Instruction_BabyMetroid_GotoNormal),

        new(LatchedOn, 0x0008), new(0xf928, 0x0008),
        new(0xf92c, 0x0005), new(0xf930, 0x0002),
        new(0xf934, EnemyInstructionCodePointers.Instruction_GotoLatchedOn),

        new(Remorse, 0x000a), new(0xf93e, 0x000a),
        new(0xf942, 0x000a), new(0xf946, 0x000a),
        new(0xf94a, 0x000a), new(0xf94e, 0x000a),
        new(0xf952, 0x000a), new(0xf956, 0x000a),
        new(RemorseRandomBranchOpcode,
            EnemyInstructionCodePointers.Instruction_BabyMetroid_GotoY_OrPlayRemorseSFX),
        new(0xf95c, Remorse),
        new(0xf95e, 0x0006), new(0xf962, 0x0005),
        new(0xf966, 0x0004), new(0xf96a, 0x0003),
        new(0xf96e, 0x0002), new(0xf972, 0x0003),
        new(0xf976, 0x0004), new(0xf97a, 0x0005),
        new(0xf97e, 0x0006), new(0xf982, 0x0007),
        new(0xf986, 0x0008), new(0xf98a, 0x0009),
        new(RemorseLoopOpcode,
            EnemyInstructionCodePointers.Instruction_BabyMetroid_GotoRemorse),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xf908, 0xf90c,
        0xf910, 0xf914, 0xf918, 0xf91c,
        0xf926, 0xf92a, 0xf92e, 0xf932,
        0xf93c, 0xf940, 0xf944, 0xf948,
        0xf94c, 0xf950, 0xf954, 0xf958,
        0xf960, 0xf964, 0xf968, 0xf96c,
        0xf970, 0xf974, 0xf978, 0xf97c,
        0xf980, 0xf984, 0xf988, 0xf98c,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ShitroidInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Shitroid instruction mechanics pointer $A9:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa90000)
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

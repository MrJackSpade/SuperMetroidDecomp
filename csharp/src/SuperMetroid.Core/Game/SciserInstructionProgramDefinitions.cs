namespace SuperMetroid.Core.Game;

/// <summary>One compiled Sciser mechanics word at its native bank-$A3 address.</summary>
internal readonly record struct SciserInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Sciser's four surface loops. The sixteen interleaved
/// spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class SciserInstructionProgramDefinitions
{
    /// <summary><c>InstList_Sciser_UpsideRight_0</c> at $A3:967B.</summary>
    internal const ushort UpsideRight = 0x967b;
    /// <summary><c>InstList_Sciser_UpsideLeft_0</c> at $A3:9693.</summary>
    internal const ushort UpsideLeft = 0x9693;
    /// <summary><c>InstList_Sciser_UpsideDown_0</c> at $A3:96AB.</summary>
    internal const ushort UpsideDown = 0x96ab;
    /// <summary><c>InstList_Sciser_UpsideUp_0</c> at $A3:96C3.</summary>
    internal const ushort UpsideUp = 0x96c3;
    /// <summary>The final elevator return opcode immediately before Sciser's palette.</summary>
    internal const ushort AdjacentPreviousCode = 0x95eb;

    private static readonly SciserInstructionMechanicsWord[] Words =
    [
        new(UpsideRight, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0x967d, (ushort)CrawlerEnemyFunction.CrawlingVertically),
        new(0x967f, 8), new(0x9683, 8), new(0x9687, 8), new(0x968b, 8),
        new(0x968f, CommonEnemyInstructionCodes.Goto), new(0x9691, 0x967f),

        new(UpsideLeft, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0x9695, (ushort)CrawlerEnemyFunction.CrawlingVertically),
        new(0x9697, 8), new(0x969b, 8), new(0x969f, 8), new(0x96a3, 8),
        new(0x96a7, CommonEnemyInstructionCodes.Goto), new(0x96a9, 0x9697),

        new(UpsideDown, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0x96ad, (ushort)CrawlerEnemyFunction.CrawlingHorizontally),
        new(0x96af, 8), new(0x96b3, 8), new(0x96b7, 8), new(0x96bb, 8),
        new(0x96bf, CommonEnemyInstructionCodes.Goto), new(0x96c1, 0x96af),

        new(UpsideUp, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0x96c5, (ushort)CrawlerEnemyFunction.CrawlingHorizontally),
        new(0x96c7, 8), new(0x96cb, 8), new(0x96cf, 8), new(0x96d3, 8),
        new(0x96d7, CommonEnemyInstructionCodes.Goto), new(0x96d9, 0x96c7),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x9681, 0x9685, 0x9689, 0x968d,
        0x9699, 0x969d, 0x96a1, 0x96a5,
        0x96b1, 0x96b5, 0x96b9, 0x96bd,
        0x96c9, 0x96cd, 0x96d1, 0x96d5,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static SciserInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            SciserInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Sciser instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
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

namespace SuperMetroid.Core.Game;

/// <summary>One compiled Zero mechanics word at its native bank-$A3 address.</summary>
internal readonly record struct ZeroInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Zero's four production-selected surface loops. The
/// twenty-four interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class ZeroInstructionProgramDefinitions
{
    /// <summary><c>InstList_Zero_UpsideRight_FacingDown_0</c> at $A3:984B.</summary>
    internal const ushort UpsideRight = 0x984b;
    /// <summary><c>InstList_Zero_UpsideLeft_FacingUp_0</c> at $A3:988B.</summary>
    internal const ushort UpsideLeft = 0x988b;
    /// <summary><c>InstList_Zero_UpsideDown_FacingLeft_0</c> at $A3:98AB.</summary>
    internal const ushort UpsideDown = 0x98ab;
    /// <summary><c>UNUSED_InstList_Zero_UpsideUp_FacingRight_A3990B</c> at $A3:990B.</summary>
    internal const ushort UpsideUp = 0x990b;
    /// <summary>The retail-unused alternate upside-right program at $A3:982B.</summary>
    internal const ushort UnusedAlternateUpsideRight = 0x982b;

    private static readonly ZeroInstructionMechanicsWord[] Words =
    [
        new(UpsideRight, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0x984d, (ushort)CrawlerEnemyFunction.CrawlingVertically),
        new(0x984f, 4), new(0x9853, 4), new(0x9857, 4), new(0x985b, 4),
        new(0x985f, 4), new(0x9863, 4),
        new(0x9867, CommonEnemyInstructionCodes.Goto), new(0x9869, 0x984f),

        new(UpsideLeft, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0x988d, (ushort)CrawlerEnemyFunction.CrawlingVertically),
        new(0x988f, 4), new(0x9893, 4), new(0x9897, 4), new(0x989b, 4),
        new(0x989f, 4), new(0x98a3, 4),
        new(0x98a7, CommonEnemyInstructionCodes.Goto), new(0x98a9, 0x988f),

        new(UpsideDown, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0x98ad, (ushort)CrawlerEnemyFunction.CrawlingHorizontally),
        new(0x98af, 4), new(0x98b3, 4), new(0x98b7, 4), new(0x98bb, 4),
        new(0x98bf, 4), new(0x98c3, 4),
        new(0x98c7, CommonEnemyInstructionCodes.Goto), new(0x98c9, 0x98af),

        new(UpsideUp, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0x990d, (ushort)CrawlerEnemyFunction.CrawlingHorizontally),
        new(0x990f, 4), new(0x9913, 4), new(0x9917, 4), new(0x991b, 4),
        new(0x991f, 4), new(0x9923, 4),
        new(0x9927, CommonEnemyInstructionCodes.Goto), new(0x9929, 0x990f),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x9851, 0x9855, 0x9859, 0x985d, 0x9861, 0x9865,
        0x9891, 0x9895, 0x9899, 0x989d, 0x98a1, 0x98a5,
        0x98b1, 0x98b5, 0x98b9, 0x98bd, 0x98c1, 0x98c5,
        0x9911, 0x9915, 0x9919, 0x991d, 0x9921, 0x9925,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ZeroInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            ZeroInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Zero instruction mechanics pointer $A3:{address:X4} is not compiled.");
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

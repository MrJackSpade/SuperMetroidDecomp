namespace SuperMetroid.Core.Game;

/// <summary>One compiled shared-crawler mechanics word at its native bank-$A3 address.</summary>
internal readonly record struct SharedCrawlerInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words shared by Zeela, Sova, Zoomer, and Stone Zoomer. The
/// twenty interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class SharedCrawlerInstructionProgramDefinitions
{
    /// <summary><c>InstList_Zeela_Zoomer_UpsideRight_0</c> at $A3:E25C.</summary>
    internal const ushort UpsideRight = 0xe25c;
    /// <summary><c>InstList_Zeela_Zoomer_UpsideLeft_0</c> at $A3:E278.</summary>
    internal const ushort UpsideLeft = 0xe278;
    /// <summary><c>InstList_Zeela_Zoomer_UpsideDown_0</c> at $A3:E294.</summary>
    internal const ushort UpsideDown = 0xe294;
    /// <summary><c>InstList_Zeela_Zoomer_UpsideUp_0</c> at $A3:E2B0.</summary>
    internal const ushort UpsideUp = 0xe2b0;
    /// <summary>The first word of the adjacent initial-list pointer table at $A3:E2CC.</summary>
    internal const ushort AdjacentInitialSelectorTable = 0xe2cc;

    private static readonly SharedCrawlerInstructionMechanicsWord[] Words =
    [
        new(UpsideRight, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0xe25e, (ushort)CrawlerEnemyFunction.CrawlingVertically),
        new(0xe260, 3), new(0xe264, 3), new(0xe268, 3), new(0xe26c, 3),
        new(0xe270, 3),
        new(0xe274, CommonEnemyInstructionCodes.Goto), new(0xe276, 0xe260),

        new(UpsideLeft, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0xe27a, (ushort)CrawlerEnemyFunction.CrawlingVertically),
        new(0xe27c, 3), new(0xe280, 3), new(0xe284, 3), new(0xe288, 3),
        new(0xe28c, 3),
        new(0xe290, CommonEnemyInstructionCodes.Goto), new(0xe292, 0xe27c),

        new(UpsideDown, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0xe296, (ushort)CrawlerEnemyFunction.CrawlingHorizontally),
        new(0xe298, 3), new(0xe29c, 3), new(0xe2a0, 3), new(0xe2a4, 3),
        new(0xe2a8, 3),
        new(0xe2ac, CommonEnemyInstructionCodes.Goto), new(0xe2ae, 0xe298),

        new(UpsideUp, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0xe2b2, (ushort)CrawlerEnemyFunction.CrawlingHorizontally),
        new(0xe2b4, 3), new(0xe2b8, 3), new(0xe2bc, 3), new(0xe2c0, 3),
        new(0xe2c4, 3),
        new(0xe2c8, CommonEnemyInstructionCodes.Goto), new(0xe2ca, 0xe2b4),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xe262, 0xe266, 0xe26a, 0xe26e, 0xe272,
        0xe27e, 0xe282, 0xe286, 0xe28a, 0xe28e,
        0xe29a, 0xe29e, 0xe2a2, 0xe2a6, 0xe2aa,
        0xe2b6, 0xe2ba, 0xe2be, 0xe2c2, 0xe2c6,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static SharedCrawlerInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            SharedCrawlerInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Shared-crawler instruction mechanics pointer $A3:{address:X4} is not compiled.");
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

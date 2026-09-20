namespace SuperMetroid.Core.Game;

internal readonly record struct BeetomInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Beetom's crawling, hopping, and draining programs.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class BeetomInstructionProgramDefinitions
{
    /// <summary><c>InstList_Beetom_Crawling_FacingLeft_0</c> at $A8:B696.</summary>
    internal const ushort CrawlingLeft = 0xb696;

    /// <summary><c>InstList_Beetom_Hop_FacingLeft</c> at $A8:B6AC.</summary>
    internal const ushort HopLeft = 0xb6ac;

    /// <summary><c>InstList_Beetom_DrainingSamus_FacingLeft_0</c> at $A8:B6CC.</summary>
    internal const ushort DrainingLeft = 0xb6cc;

    /// <summary><c>InstList_Beetom_Crawling_FacingRight_0</c> at $A8:B6F2.</summary>
    internal const ushort CrawlingRight = 0xb6f2;

    /// <summary><c>InstList_Beetom_Hop_FacingRight</c> at $A8:B708.</summary>
    internal const ushort HopRight = 0xb708;

    /// <summary><c>InstList_Beetom_DrainingSamus_FacingRight_0</c> at $A8:B728.</summary>
    internal const ushort DrainingRight = 0xb728;

    /// <summary>The repeating left-crawl frame list at $A8:B698.</summary>
    internal const ushort CrawlingLeftLoop = 0xb698;

    /// <summary>The terminal left-hop sleep instruction at $A8:B6BE.</summary>
    internal const ushort HopLeftSleep = 0xb6be;

    /// <summary>The repeating left-drain frame list at $A8:B6DE.</summary>
    internal const ushort DrainingLeftLoop = 0xb6de;

    /// <summary>The repeating right-crawl frame list at $A8:B6F4.</summary>
    internal const ushort CrawlingRightLoop = 0xb6f4;

    /// <summary>The terminal right-hop sleep instruction at $A8:B71A.</summary>
    internal const ushort HopRightSleep = 0xb71a;

    /// <summary>The repeating right-drain frame list at $A8:B73A.</summary>
    internal const ushort DrainingRightLoop = 0xb73a;

    private static readonly BeetomInstructionMechanicsWord[] Words =
    [
        new(0xb696, CommonEnemyInstructionCodes.DisableOffScreenProcessing),
        new(0xb698, 10), new(0xb69c, 10), new(0xb6a0, 10), new(0xb6a4, 10),
        new(0xb6a8, CommonEnemyInstructionCodes.Goto), new(0xb6aa, CrawlingLeftLoop),

        new(0xb6ac, CommonEnemyInstructionCodes.EnableOffScreenProcessing),
        new(0xb6ae, 4), new(0xb6b2, 8), new(0xb6b6, 4), new(0xb6ba, 1),
        new(0xb6be, CommonEnemyInstructionCodes.Sleep),

        new(0xb6cc, 5), new(0xb6d0, 5), new(0xb6d4, 5), new(0xb6d8, 0x30),
        new(0xb6dc, EnemyInstructionCodePointers.Instruction_Beetom_Nothing),
        new(0xb6de, 5), new(0xb6e2, 5), new(0xb6e6, 5), new(0xb6ea, 5),
        new(0xb6ee, CommonEnemyInstructionCodes.Goto), new(0xb6f0, DrainingLeftLoop),

        new(0xb6f2, CommonEnemyInstructionCodes.DisableOffScreenProcessing),
        new(0xb6f4, 10), new(0xb6f8, 10), new(0xb6fc, 10), new(0xb700, 10),
        new(0xb704, CommonEnemyInstructionCodes.Goto), new(0xb706, CrawlingRightLoop),

        new(0xb708, CommonEnemyInstructionCodes.EnableOffScreenProcessing),
        new(0xb70a, 4), new(0xb70e, 8), new(0xb712, 4), new(0xb716, 1),
        new(0xb71a, CommonEnemyInstructionCodes.Sleep),

        new(0xb728, 5), new(0xb72c, 5), new(0xb730, 5), new(0xb734, 0x30),
        new(0xb738, EnemyInstructionCodePointers.Instruction_Beetom_Nothing),
        new(0xb73a, 5), new(0xb73e, 5), new(0xb742, 5), new(0xb746, 5),
        new(0xb74a, CommonEnemyInstructionCodes.Goto), new(0xb74c, DrainingRightLoop),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xb69a, 0xb69e, 0xb6a2, 0xb6a6,
        0xb6b0, 0xb6b4, 0xb6b8, 0xb6bc,
        0xb6ce, 0xb6d2, 0xb6d6, 0xb6da, 0xb6e0, 0xb6e4, 0xb6e8, 0xb6ec,
        0xb6f6, 0xb6fa, 0xb6fe, 0xb702,
        0xb70c, 0xb710, 0xb714, 0xb718,
        0xb72a, 0xb72e, 0xb732, 0xb736, 0xb73c, 0xb740, 0xb744, 0xb748,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static BeetomInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            BeetomInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Beetom instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
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

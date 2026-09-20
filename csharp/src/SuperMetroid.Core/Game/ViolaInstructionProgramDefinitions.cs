namespace SuperMetroid.Core.Game;

/// <summary>One compiled Viola mechanics word at its native bank-$A3 address.</summary>
internal readonly record struct ViolaInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Viola's four surface entry programs and shared normal
/// loop. The fourteen interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class ViolaInstructionProgramDefinitions
{
    /// <summary><c>InstList_Viola_UpsideRight</c> at $A3:B5E3.</summary>
    internal const ushort UpsideRight = 0xb5e3;
    /// <summary><c>InstList_Viola_UpsideLeft</c> at $A3:B5EB.</summary>
    internal const ushort UpsideLeft = 0xb5eb;
    /// <summary><c>InstList_Viola_UpsideDown</c> at $A3:B5D3.</summary>
    internal const ushort UpsideDown = 0xb5d3;
    /// <summary><c>InstList_Viola_UpsideUp</c> at $A3:B5DB.</summary>
    internal const ushort UpsideUp = 0xb5db;
    /// <summary><c>InstList_Viola_Normal</c> at $A3:B5EF.</summary>
    internal const ushort NormalLoop = 0xb5ef;
    /// <summary>The retail-unused X-flipped Viola program at $A3:B62B.</summary>
    internal const ushort UnusedXFlipped = 0xb62b;

    private static readonly ViolaInstructionMechanicsWord[] Words =
    [
        new(UpsideDown, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0xb5d5, (ushort)CrawlerEnemyFunction.CrawlingHorizontally),
        new(0xb5d7, CommonEnemyInstructionCodes.Goto), new(0xb5d9, NormalLoop),

        new(UpsideUp, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0xb5dd, (ushort)CrawlerEnemyFunction.CrawlingHorizontally),
        new(0xb5df, CommonEnemyInstructionCodes.Goto), new(0xb5e1, NormalLoop),

        new(UpsideRight, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0xb5e5, (ushort)CrawlerEnemyFunction.CrawlingVertically),
        new(0xb5e7, CommonEnemyInstructionCodes.Goto), new(0xb5e9, NormalLoop),

        new(UpsideLeft, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
        new(0xb5ed, (ushort)CrawlerEnemyFunction.CrawlingVertically),

        new(0xb5ef, 10), new(0xb5f3, 10), new(0xb5f7, 10), new(0xb5fb, 10),
        new(0xb5ff, 10), new(0xb603, 10), new(0xb607, 10), new(0xb60b, 10),
        new(0xb60f, 10), new(0xb613, 10), new(0xb617, 10), new(0xb61b, 10),
        new(0xb61f, 10), new(0xb623, 10),
        new(0xb627, CommonEnemyInstructionCodes.Goto), new(0xb629, NormalLoop),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xb5f1, 0xb5f5, 0xb5f9, 0xb5fd, 0xb601, 0xb605, 0xb609,
        0xb60d, 0xb611, 0xb615, 0xb619, 0xb61d, 0xb621, 0xb625,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ViolaInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            ViolaInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Viola instruction mechanics pointer $A3:{address:X4} is not compiled.");
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

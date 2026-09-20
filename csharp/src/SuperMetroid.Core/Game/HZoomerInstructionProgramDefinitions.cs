namespace SuperMetroid.Core.Game;

/// <summary>One compiled HZoomer mechanics word at its native bank-$A3 address.</summary>
internal readonly record struct HZoomerInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the Wrecked Ship orange Zoomer's four surface loops.
/// The twenty interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class HZoomerInstructionProgramDefinitions
{
    /// <summary><c>InstList_HZoomer_UpsideRight_0</c> at $A3:DFCB.</summary>
    internal const ushort UpsideRight = 0xdfcb;
    /// <summary><c>InstList_HZoomer_UpsideLeft_0</c> at $A3:DFE7.</summary>
    internal const ushort UpsideLeft = 0xdfe7;
    /// <summary><c>InstList_HZoomer_UpsideDown_0</c> at $A3:E003.</summary>
    internal const ushort UpsideDown = 0xe003;
    /// <summary><c>InstList_HZoomer_UpsideUp_0</c> at $A3:E01F.</summary>
    internal const ushort UpsideUp = 0xe01f;
    /// <summary><c>Instruction_HZoomer_FunctionInY</c> immediately before the programs.</summary>
    internal const ushort AdjacentFunctionCode = 0xdfc2;

    private static readonly HZoomerInstructionMechanicsWord[] Words =
    [
        new(UpsideRight, EnemyInstructionCodePointers.Instruction_HZoomer_FunctionInY),
        new(0xdfcd, (ushort)CrawlerEnemyFunction.HZoomerCrawlingVertically),
        new(0xdfcf, 3), new(0xdfd3, 3), new(0xdfd7, 3), new(0xdfdb, 3),
        new(0xdfdf, 3), new(0xdfe3, CommonEnemyInstructionCodes.Goto),
        new(0xdfe5, 0xdfcf),

        new(UpsideLeft, EnemyInstructionCodePointers.Instruction_HZoomer_FunctionInY),
        new(0xdfe9, (ushort)CrawlerEnemyFunction.HZoomerCrawlingVertically),
        new(0xdfeb, 3), new(0xdfef, 3), new(0xdff3, 3), new(0xdff7, 3),
        new(0xdffb, 3), new(0xdfff, CommonEnemyInstructionCodes.Goto),
        new(0xe001, 0xdfeb),

        new(UpsideDown, EnemyInstructionCodePointers.Instruction_HZoomer_FunctionInY),
        new(0xe005, (ushort)CrawlerEnemyFunction.HZoomerCrawlingHorizontally),
        new(0xe007, 3), new(0xe00b, 3), new(0xe00f, 3), new(0xe013, 3),
        new(0xe017, 3), new(0xe01b, CommonEnemyInstructionCodes.Goto),
        new(0xe01d, 0xe007),

        new(UpsideUp, EnemyInstructionCodePointers.Instruction_HZoomer_FunctionInY),
        new(0xe021, (ushort)CrawlerEnemyFunction.HZoomerCrawlingHorizontally),
        new(0xe023, 3), new(0xe027, 3), new(0xe02b, 3), new(0xe02f, 3),
        new(0xe033, 3), new(0xe037, CommonEnemyInstructionCodes.Goto),
        new(0xe039, 0xe023),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xdfd1, 0xdfd5, 0xdfd9, 0xdfdd, 0xdfe1,
        0xdfed, 0xdff1, 0xdff5, 0xdff9, 0xdffd,
        0xe009, 0xe00d, 0xe011, 0xe015, 0xe019,
        0xe025, 0xe029, 0xe02d, 0xe031, 0xe035,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static HZoomerInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            HZoomerInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"HZoomer instruction mechanics pointer $A3:{address:X4} is not compiled.");
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

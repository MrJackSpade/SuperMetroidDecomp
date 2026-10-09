namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Viola's four surface entry programs and shared normal
/// loop. The fourteen interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal abstract class ViolaInstructionProgramDefinitions
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

    /// <summary>Gets the 30 compiled engine-control words across the four entry programs and shared normal loop.</summary>
    public static int MechanicsWordCount => 30;

    /// <summary>Gets the 14 live spritemap operands interleaved with the normal-loop frame durations.</summary>
    public static int PresentationWordCount => 14;

    /// <summary>Four axis-setting entries converge on fourteen ten-tick frames; the last entry falls through.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        if (index < 14)
        {
            ushort address = (ushort)(UpsideDown + 2 * index);
            ushort value = (index % 4) switch
            {
                0 => EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY,
                1 => (ushort)(index / 4 < 2 ? CrawlerEnemyFunction.CrawlingHorizontally : CrawlerEnemyFunction.CrawlingVertically),
                2 => CommonEnemyInstructionCodes.Goto,
                _ => NormalLoop,
            };
            return new(address, value);
        }
        int word = index - 14;
        return word < 14 ? new((ushort)(NormalLoop + 4 * word), 10)
            : new((ushort)(NormalLoop + 56 + 2 * (word - 14)), word == 14 ? CommonEnemyInstructionCodes.Goto : NormalLoop);
    }

    /// <summary>Returns the address of a spritemap operand in Viola's shared normal loop.</summary>
    /// <param name="index">Zero-based index of one of the 14 normal-loop frames.</param>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the presentation operands.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(NormalLoop + 2 + 4 * index);
    }
    /// <summary>Resolves a bank-$A3 address to its compiled Viola mechanics word.</summary>
    /// <param name="address">Address of the mechanics word to read.</param>
    /// <returns>The compiled value at the exact matching address.</returns>
    /// <exception cref="InvalidDataException">The address is not among the compiled mechanics words.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
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
}

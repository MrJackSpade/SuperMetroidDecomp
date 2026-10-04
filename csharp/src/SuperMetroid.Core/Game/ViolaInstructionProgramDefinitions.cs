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

    internal static int MechanicsWordCount => 30;
    internal static int PresentationWordCount => 14;

    /// <summary>Four axis-setting entries converge on fourteen ten-tick frames; the last entry falls through.</summary>
    internal static ViolaInstructionMechanicsWord MechanicsWord(int index)
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

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(NormalLoop + 2 + 4 * index);
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            ViolaInstructionMechanicsWord candidate = MechanicsWord(middle);
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
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

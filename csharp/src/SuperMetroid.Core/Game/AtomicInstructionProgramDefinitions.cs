namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A8 address.</summary>
internal readonly record struct AtomicInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>Compiled mechanics words from Atomic's four directional animation loops.</summary>
/// <remarks>
/// The six durations and terminal goto in each list are immutable simulation control. The
/// interleaved visual selectors are compiled in
/// <see cref="Assets.EnemySpritemapDefinitions"/>.
/// </remarks>
internal static class AtomicInstructionProgramDefinitions
{
    /// <summary><c>$A8:E310</c>, spinning up-right.</summary>
    internal const ushort UpRight = 0xe310;

    /// <summary><c>$A8:E32C</c>, spinning up-left.</summary>
    internal const ushort UpLeft = 0xe32c;

    /// <summary><c>$A8:E348</c>, spinning down-left.</summary>
    internal const ushort DownLeft = 0xe348;

    /// <summary><c>$A8:E364</c>, spinning down-right.</summary>
    internal const ushort DownRight = 0xe364;

    private static readonly AtomicInstructionMechanicsWord[] Words =
    [
        new(0xe310, 0x0008), new(0xe314, 0x0008), new(0xe318, 0x0008),
        new(0xe31c, 0x0008), new(0xe320, 0x0008), new(0xe324, 0x0008),
        new(0xe328, 0x80ed), new(0xe32a, 0xe310),
        new(0xe32c, 0x0008), new(0xe330, 0x0008), new(0xe334, 0x0008),
        new(0xe338, 0x0008), new(0xe33c, 0x0008), new(0xe340, 0x0008),
        new(0xe344, 0x80ed), new(0xe346, 0xe32c),
        new(0xe348, 0x0008), new(0xe34c, 0x0008), new(0xe350, 0x0008),
        new(0xe354, 0x0008), new(0xe358, 0x0008), new(0xe35c, 0x0008),
        new(0xe360, 0x80ed), new(0xe362, 0xe348),
        new(0xe364, 0x0008), new(0xe368, 0x0008), new(0xe36c, 0x0008),
        new(0xe370, 0x0008), new(0xe374, 0x0008), new(0xe378, 0x0008),
        new(0xe37c, 0x80ed), new(0xe37e, 0xe364),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xe312, 0xe316, 0xe31a, 0xe31e, 0xe322, 0xe326,
        0xe32e, 0xe332, 0xe336, 0xe33a, 0xe33e, 0xe342,
        0xe34a, 0xe34e, 0xe352, 0xe356, 0xe35a, 0xe35e,
        0xe366, 0xe36a, 0xe36e, 0xe372, 0xe376, 0xe37a,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static AtomicInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns one fixed control word or rejects pointers outside all four loops.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            AtomicInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Atomic instruction mechanics pointer $A8:{address:X4} is not compiled.");
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

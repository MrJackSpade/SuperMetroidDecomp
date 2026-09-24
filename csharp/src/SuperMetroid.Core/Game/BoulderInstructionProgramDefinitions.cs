namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A6 address.</summary>
internal readonly record struct BoulderInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics words from Boulder's mirrored rolling programs.</summary>
/// <remarks>
/// Frame durations and loop control are immutable simulation data. The sixteen
/// interleaved visual selectors are compiled in
/// <see cref="Assets.EnemySpritemapDefinitions"/>.
/// </remarks>
internal static class BoulderInstructionProgramDefinitions
{
    /// <summary><c>$A6:86A7</c>, the eight-frame left-moving rolling loop.</summary>
    internal const ushort Left = 0x86a7;

    /// <summary><c>$A6:86CB</c>, the eight-frame right-moving rolling loop.</summary>
    internal const ushort Right = 0x86cb;

    private static readonly BoulderInstructionMechanicsWord[] Words =
    [
        new(0x86a7, 0x0008), new(0x86ab, 0x0008),
        new(0x86af, 0x0008), new(0x86b3, 0x0008),
        new(0x86b7, 0x0008), new(0x86bb, 0x0008),
        new(0x86bf, 0x0008), new(0x86c3, 0x0008),
        new(0x86c7, 0x80ed), new(0x86c9, 0x86a7),
        new(0x86cb, 0x0008), new(0x86cf, 0x0008),
        new(0x86d3, 0x0008), new(0x86d7, 0x0008),
        new(0x86db, 0x0008), new(0x86df, 0x0008),
        new(0x86e3, 0x0008), new(0x86e7, 0x0008),
        new(0x86eb, 0x80ed), new(0x86ed, 0x86cb),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x86a9, 0x86ad, 0x86b1, 0x86b5,
        0x86b9, 0x86bd, 0x86c1, 0x86c5,
        0x86cd, 0x86d1, 0x86d5, 0x86d9,
        0x86dd, 0x86e1, 0x86e5, 0x86e9,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static BoulderInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Boulder control or rejects pointers outside both loops.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            BoulderInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Boulder instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
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

namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A2 address.</summary>
internal readonly record struct BoyonInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics words from Boyon's idle and bouncing programs.</summary>
/// <remarks>
/// Property commands, callbacks, durations, and loop control are immutable simulation
/// data. The ten interleaved visual selectors are compiled separately in
/// <see cref="Assets.EnemySpritemapDefinitions"/>; neither belongs in editable artwork.
/// </remarks>
internal static class BoyonInstructionProgramDefinitions
{
    /// <summary><c>$A2:86A7</c>, the four-frame idle loop.</summary>
    internal const ushort Idle = 0x86a7;

    /// <summary><c>$A2:86BF</c>, the six-frame bouncing loop.</summary>
    internal const ushort Bouncing = 0x86bf;

    private static readonly BoyonInstructionMechanicsWord[] Words =
    [
        new(0x86a7, 0x817d), new(0x86a9, 0x88c5),
        new(0x86ab, 0x000a), new(0x86af, 0x000a),
        new(0x86b3, 0x000a), new(0x86b7, 0x000a),
        new(0x86bb, 0x80ed), new(0x86bd, 0x86ab),
        new(0x86bf, 0x8173), new(0x86c1, 0x88c6),
        new(0x86c3, 0x0005), new(0x86c7, 0x0005),
        new(0x86cb, 0x0005), new(0x86cf, 0x0005),
        new(0x86d3, 0x0005), new(0x86d7, 0x0005),
        new(0x86db, 0x80ed), new(0x86dd, 0x86c3),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x86ad, 0x86b1, 0x86b5, 0x86b9,
        0x86c5, 0x86c9, 0x86cd, 0x86d1, 0x86d5, 0x86d9,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static BoyonInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Boyon control or rejects pointers outside both programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            BoyonInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Boyon instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
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

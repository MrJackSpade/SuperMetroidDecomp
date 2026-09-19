namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A6 address.</summary>
internal readonly record struct NuclearWaffleInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics words from Nuclear Waffle's body animation loop.</summary>
/// <remarks>
/// Frame durations and terminal loop control are immutable simulation data. The twelve
/// interleaved spritemap pointers remain live cartridge presentation data.
/// </remarks>
internal static class NuclearWaffleInstructionProgramDefinitions
{
    /// <summary><c>$A6:9490</c>, the twelve-frame body animation loop.</summary>
    internal const ushort BodyLoop = 0x9490;

    private static readonly NuclearWaffleInstructionMechanicsWord[] Words =
    [
        new(0x9490, 0x0003), new(0x9494, 0x0003), new(0x9498, 0x0003),
        new(0x949c, 0x0003), new(0x94a0, 0x0003), new(0x94a4, 0x0003),
        new(0x94a8, 0x0003), new(0x94ac, 0x0003), new(0x94b0, 0x0003),
        new(0x94b4, 0x0003), new(0x94b8, 0x0003), new(0x94bc, 0x0003),
        new(0x94c0, 0x80ed), new(0x94c2, 0x9490),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x9492, 0x9496, 0x949a, 0x949e, 0x94a2, 0x94a6,
        0x94aa, 0x94ae, 0x94b2, 0x94b6, 0x94ba, 0x94be,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static NuclearWaffleInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns one fixed control word or rejects pointers outside the body loop.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            NuclearWaffleInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Nuclear Waffle instruction mechanics pointer $A6:{address:X4} is not compiled.");
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

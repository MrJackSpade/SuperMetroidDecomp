namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A8 address.</summary>
internal readonly record struct BlueBrinstarFaceBlockInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics words from all three Blue Brinstar face-block programs.</summary>
/// <remarks>
/// Frame durations and terminal sleeps are immutable simulation data. The seven
/// interleaved spritemap pointers remain live cartridge presentation data.
/// </remarks>
internal static class BlueBrinstarFaceBlockInstructionProgramDefinitions
{
    /// <summary><c>$A8:E80C</c>, the three-frame animation when Samus approaches from the left.</summary>
    internal const ushort SamusLeft = 0xe80c;

    /// <summary><c>$A8:E81A</c>, the three-frame animation when Samus approaches from the right.</summary>
    internal const ushort SamusRight = 0xe81a;

    /// <summary><c>$A8:E828</c>, the one-frame neutral program installed at initialization.</summary>
    internal const ushort Initial = 0xe828;

    private static readonly BlueBrinstarFaceBlockInstructionMechanicsWord[] Words =
    [
        new(0xe80c, 0x0030), new(0xe810, 0x0010),
        new(0xe814, 0x0010), new(0xe818, 0x812f),
        new(0xe81a, 0x0030), new(0xe81e, 0x0010),
        new(0xe822, 0x0010), new(0xe826, 0x812f),
        new(0xe828, 0x0001), new(0xe82c, 0x812f),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xe80e, 0xe812, 0xe816,
        0xe81c, 0xe820, 0xe824,
        0xe82a,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static BlueBrinstarFaceBlockInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed face-block control or rejects pointers outside its programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            BlueBrinstarFaceBlockInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Blue Brinstar face-block instruction mechanics pointer " +
            $"$A8:{address:X4} is not compiled.");
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

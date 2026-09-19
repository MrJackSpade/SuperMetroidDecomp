namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A3 address.</summary>
internal readonly record struct WaverInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics words from Waver's steady and spinning programs.</summary>
/// <remarks>
/// Durations, spin completion, and terminal sleeps are immutable simulation data. The
/// ten interleaved spritemap pointers remain live cartridge presentation data.
/// </remarks>
internal static class WaverInstructionProgramDefinitions
{
    /// <summary><c>$A3:86A7</c>, steady animation facing left.</summary>
    internal const ushort SteadyFacingLeft = 0x86a7;

    /// <summary><c>$A3:86AD</c>, steady animation facing right.</summary>
    internal const ushort SteadyFacingRight = 0x86ad;

    /// <summary><c>$A3:86B3</c>, four-frame spin facing left.</summary>
    internal const ushort SpinningFacingLeft = 0x86b3;

    /// <summary><c>$A3:86C7</c>, four-frame spin facing right.</summary>
    internal const ushort SpinningFacingRight = 0x86c7;

    private static readonly WaverInstructionMechanicsWord[] Words =
    [
        new(0x86a7, 0x0001), new(0x86ab, 0x812f),
        new(0x86ad, 0x0001), new(0x86b1, 0x812f),
        new(0x86b3, 0x0008), new(0x86b7, 0x0008),
        new(0x86bb, 0x0008), new(0x86bf, 0x0008),
        new(0x86c3, 0x86e3), new(0x86c5, 0x812f),
        new(0x86c7, 0x0008), new(0x86cb, 0x0008),
        new(0x86cf, 0x0008), new(0x86d3, 0x0008),
        new(0x86d7, 0x86e3), new(0x86d9, 0x812f),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x86a9, 0x86af,
        0x86b5, 0x86b9, 0x86bd, 0x86c1,
        0x86c9, 0x86cd, 0x86d1, 0x86d5,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static WaverInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Waver control or rejects pointers outside all four programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            WaverInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Waver instruction mechanics pointer $A3:{address:X4} is not compiled.");
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

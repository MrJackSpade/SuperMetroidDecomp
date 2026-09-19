namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A3 address.</summary>
internal readonly record struct ZoaInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics words from Zoa's shooting and rising programs.</summary>
/// <remarks>
/// Speed callbacks, durations, and loop control are immutable simulation data. The twelve
/// interleaved spritemap pointers remain live cartridge presentation data.
/// </remarks>
internal static class ZoaInstructionProgramDefinitions
{
    /// <summary><c>$A3:B3C1</c>, left-facing horizontal launch.</summary>
    internal const ushort FacingLeftShooting = 0xb3c1;

    /// <summary><c>$A3:B3D7</c>, left-facing vertical rise.</summary>
    internal const ushort FacingLeftRising = 0xb3d7;

    /// <summary><c>$A3:B3E7</c>, right-facing horizontal launch.</summary>
    internal const ushort FacingRightShooting = 0xb3e7;

    /// <summary><c>$A3:B3FD</c>, right-facing vertical rise.</summary>
    internal const ushort FacingRightRising = 0xb3fd;

    private static readonly ZoaInstructionMechanicsWord[] Words =
    [
        new(0xb3c1, 0xb429), new(0xb3c3, 0x0040),
        new(0xb3c7, 0xb434), new(0xb3c9, 0x0008),
        new(0xb3cd, 0xb43f), new(0xb3cf, 0x0030),
        new(0xb3d3, 0x80ed), new(0xb3d5, 0xb3c1),

        new(0xb3d7, 0x0004), new(0xb3db, 0x0004), new(0xb3df, 0x0004),
        new(0xb3e3, 0x80ed), new(0xb3e5, 0xb3d7),

        new(0xb3e7, 0xb429), new(0xb3e9, 0x0040),
        new(0xb3ed, 0xb434), new(0xb3ef, 0x0008),
        new(0xb3f3, 0xb43f), new(0xb3f5, 0x0030),
        new(0xb3f9, 0x80ed), new(0xb3fb, 0xb3e7),

        new(0xb3fd, 0x0004), new(0xb401, 0x0004), new(0xb405, 0x0004),
        new(0xb409, 0x80ed), new(0xb40b, 0xb3fd),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xb3c5, 0xb3cb, 0xb3d1,
        0xb3d9, 0xb3dd, 0xb3e1,
        0xb3eb, 0xb3f1, 0xb3f7,
        0xb3ff, 0xb403, 0xb407,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ZoaInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Zoa control or rejects pointers outside all four programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            ZoaInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Zoa instruction mechanics pointer $A3:{address:X4} is not compiled.");
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

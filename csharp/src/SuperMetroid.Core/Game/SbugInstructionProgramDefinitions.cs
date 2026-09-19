namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A3 address.</summary>
internal readonly record struct SbugInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>Compiled mechanics words from Sbug's eight directional animation loops.</summary>
/// <remarks>
/// Each list's frame durations and terminal goto are immutable simulation control. The
/// interleaved spritemap pointers remain live cartridge presentation data.
/// </remarks>
internal static class SbugInstructionProgramDefinitions
{
    /// <summary><c>$A3:A071</c>, right-facing animation loop.</summary>
    internal const ushort Right = 0xa071;

    /// <summary><c>$A3:A085</c>, up-right-facing animation loop.</summary>
    internal const ushort UpRight = 0xa085;

    /// <summary><c>$A3:A099</c>, up-facing animation loop.</summary>
    internal const ushort Up = 0xa099;

    /// <summary><c>$A3:A0AD</c>, up-left-facing animation loop.</summary>
    internal const ushort UpLeft = 0xa0ad;

    /// <summary><c>$A3:A0C1</c>, left-facing animation loop.</summary>
    internal const ushort Left = 0xa0c1;

    /// <summary><c>$A3:A0D5</c>, down-left-facing animation loop.</summary>
    internal const ushort DownLeft = 0xa0d5;

    /// <summary><c>$A3:A0E9</c>, down-facing animation loop.</summary>
    internal const ushort Down = 0xa0e9;

    /// <summary><c>$A3:A0FD</c>, down-right-facing animation loop.</summary>
    internal const ushort DownRight = 0xa0fd;

    private static readonly SbugInstructionMechanicsWord[] Words =
    [
        new(0xa071, 0x0005), new(0xa075, 0x0005), new(0xa079, 0x0005),
        new(0xa07d, 0x0005), new(0xa081, 0x80ed), new(0xa083, 0xa071),
        new(0xa085, 0x0005), new(0xa089, 0x0005), new(0xa08d, 0x0005),
        new(0xa091, 0x0005), new(0xa095, 0x80ed), new(0xa097, 0xa085),
        new(0xa099, 0x0005), new(0xa09d, 0x0005), new(0xa0a1, 0x0005),
        new(0xa0a5, 0x0005), new(0xa0a9, 0x80ed), new(0xa0ab, 0xa099),
        new(0xa0ad, 0x0005), new(0xa0b1, 0x0005), new(0xa0b5, 0x0005),
        new(0xa0b9, 0x0005), new(0xa0bd, 0x80ed), new(0xa0bf, 0xa0ad),
        new(0xa0c1, 0x0005), new(0xa0c5, 0x0005), new(0xa0c9, 0x0005),
        new(0xa0cd, 0x0005), new(0xa0d1, 0x80ed), new(0xa0d3, 0xa0c1),
        new(0xa0d5, 0x0005), new(0xa0d9, 0x0005), new(0xa0dd, 0x0005),
        new(0xa0e1, 0x0005), new(0xa0e5, 0x80ed), new(0xa0e7, 0xa0d5),
        new(0xa0e9, 0x0005), new(0xa0ed, 0x0005), new(0xa0f1, 0x0005),
        new(0xa0f5, 0x0005), new(0xa0f9, 0x80ed), new(0xa0fb, 0xa0e9),
        new(0xa0fd, 0x0005), new(0xa101, 0x0005), new(0xa105, 0x0005),
        new(0xa109, 0x0005), new(0xa10d, 0x80ed), new(0xa10f, 0xa0fd),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xa073, 0xa077, 0xa07b, 0xa07f,
        0xa087, 0xa08b, 0xa08f, 0xa093,
        0xa09b, 0xa09f, 0xa0a3, 0xa0a7,
        0xa0af, 0xa0b3, 0xa0b7, 0xa0bb,
        0xa0c3, 0xa0c7, 0xa0cb, 0xa0cf,
        0xa0d7, 0xa0db, 0xa0df, 0xa0e3,
        0xa0eb, 0xa0ef, 0xa0f3, 0xa0f7,
        0xa0ff, 0xa103, 0xa107, 0xa10b,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static SbugInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns one fixed control word or rejects pointers outside all eight loops.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            SbugInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Sbug instruction mechanics pointer $A3:{address:X4} is not compiled.");
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

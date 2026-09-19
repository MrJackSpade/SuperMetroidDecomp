namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A8 address.</summary>
internal readonly record struct SparkInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>Compiled mechanics words from Wrecked Ship Spark's four animation programs.</summary>
/// <remarks>
/// Callback identities, durations, terminal control, and branch targets are immutable
/// simulation data. Interleaved spritemap pointers remain live cartridge presentation data.
/// </remarks>
internal static class SparkInstructionProgramDefinitions
{
    /// <summary><c>$A8:E5A7</c>, make tangible and flicker into the active loop.</summary>
    internal const ushort FlickerOn = 0xe5a7;

    /// <summary><c>$A8:E5D1</c>, continuously active four-frame loop.</summary>
    internal const ushort Active = 0xe5d1;

    /// <summary><c>$A8:E5E5</c>, flicker out, become intangible, and sleep.</summary>
    internal const ushort FlickerOut = 0xe5e5;

    /// <summary><c>$A8:E609</c>, stationary falling-spark emitter loop.</summary>
    internal const ushort Emitter = 0xe609;

    private static readonly SparkInstructionMechanicsWord[] Words =
    [
        new(0xe5a7, 0xe62a),
        new(0xe5a9, 0x0001), new(0xe5ad, 0x0002), new(0xe5b1, 0x0001),
        new(0xe5b5, 0x0002), new(0xe5b9, 0x0001), new(0xe5bd, 0x0002),
        new(0xe5c1, 0x0001), new(0xe5c5, 0x0001), new(0xe5c9, 0x0002),
        new(0xe5cd, 0x0002),
        new(0xe5d1, 0x0003), new(0xe5d5, 0x0003), new(0xe5d9, 0x0003),
        new(0xe5dd, 0x0003), new(0xe5e1, 0x80ed), new(0xe5e3, 0xe5d1),
        new(0xe5e5, 0x0001), new(0xe5e9, 0x0001), new(0xe5ed, 0x0001),
        new(0xe5f1, 0x0001), new(0xe5f5, 0x0001), new(0xe5f9, 0x0001),
        new(0xe5fd, 0x0001), new(0xe601, 0x0001),
        new(0xe605, 0xe61d), new(0xe607, 0x812f),
        new(0xe609, 0x0003), new(0xe60d, 0x0003), new(0xe611, 0x0003),
        new(0xe615, 0x0003), new(0xe619, 0x80ed), new(0xe61b, 0xe609),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xe5ab, 0xe5af, 0xe5b3, 0xe5b7, 0xe5bb, 0xe5bf, 0xe5c3, 0xe5c7,
        0xe5cb, 0xe5cf,
        0xe5d3, 0xe5d7, 0xe5db, 0xe5df,
        0xe5e7, 0xe5eb, 0xe5ef, 0xe5f3, 0xe5f7, 0xe5fb, 0xe5ff, 0xe603,
        0xe60b, 0xe60f, 0xe613, 0xe617,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static SparkInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns one fixed control word or rejects pointers outside the four programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            SparkInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Wrecked Ship Spark instruction mechanics pointer $A8:{address:X4} is not compiled.");
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

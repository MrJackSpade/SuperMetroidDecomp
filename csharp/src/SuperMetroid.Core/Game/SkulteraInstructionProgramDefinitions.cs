namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A3 address.</summary>
internal readonly record struct SkulteraInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics words from Skultera's swimming and turning programs.</summary>
/// <remarks>
/// Layer callbacks, durations, turn completion, sleeps, and loop control are immutable
/// simulation data. The twenty-two interleaved spritemap pointers are compiled
/// separately from their editable composition assets.
/// </remarks>
internal static class SkulteraInstructionProgramDefinitions
{
    /// <summary><c>$A3:902A</c>, the layer callback and three-frame left-swimming loop.</summary>
    internal const ushort SwimmingLeft = 0x902a;

    /// <summary><c>$A3:903C</c>, the eight-frame turn from left to right.</summary>
    internal const ushort TurningRight = 0x903c;

    /// <summary><c>$A3:9060</c>, the layer callback and three-frame right-swimming loop.</summary>
    internal const ushort SwimmingRight = 0x9060;

    /// <summary><c>$A3:9072</c>, the eight-frame turn from right to left.</summary>
    internal const ushort TurningLeft = 0x9072;

    private static readonly SkulteraInstructionMechanicsWord[] Words =
    [
        new(0x902a, 0x90a0),
        new(0x902c, 0x000e), new(0x9030, 0x000e), new(0x9034, 0x000e),
        new(0x9038, 0x80ed), new(0x903a, 0x902c),

        new(0x903c, 0x000d), new(0x9040, 0x000a),
        new(0x9044, 0x0008), new(0x9048, 0x0006),
        new(0x904c, 0x0006), new(0x9050, 0x0008),
        new(0x9054, 0x000a), new(0x9058, 0x000d),
        new(0x905c, 0x90aa), new(0x905e, 0x812f),

        new(0x9060, 0x9096),
        new(0x9062, 0x000e), new(0x9066, 0x000e), new(0x906a, 0x000e),
        new(0x906e, 0x80ed), new(0x9070, 0x9062),

        new(0x9072, 0x000d), new(0x9076, 0x000a),
        new(0x907a, 0x0008), new(0x907e, 0x0006),
        new(0x9082, 0x0006), new(0x9086, 0x0008),
        new(0x908a, 0x000a), new(0x908e, 0x000d),
        new(0x9092, 0x90aa), new(0x9094, 0x812f),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x902e, 0x9032, 0x9036,
        0x903e, 0x9042, 0x9046, 0x904a, 0x904e, 0x9052, 0x9056, 0x905a,
        0x9064, 0x9068, 0x906c,
        0x9074, 0x9078, 0x907c, 0x9080, 0x9084, 0x9088, 0x908c, 0x9090,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static SkulteraInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool IsPresentationWord(ushort address) =>
        Array.BinarySearch(PresentationWords, address) >= 0;

    /// <summary>Returns fixed Skultera control or rejects pointers outside all four programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            SkulteraInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Skultera instruction mechanics pointer $A3:{address:X4} is not compiled.");
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

namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A2 address.</summary>
internal readonly record struct RinkaInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled mechanics words from the ordinary and Mother Brain Rinka instruction programs.
/// </summary>
/// <remarks>
/// Both native lists interleave engine state with presentation data. Callback identities,
/// frame durations, the common goto opcode, and its loop targets affect simulation and live
/// here. The word following every duration is a spritemap pointer; those eighteen words
/// deliberately remain live cartridge reads.
/// </remarks>
internal static class RinkaInstructionProgramDefinitions
{
    /// <summary><c>$A2:B9E0</c>, ordinary room-Rinka animation program.</summary>
    internal const ushort OrdinaryInitial = 0xb9e0;

    /// <summary><c>$A2:BA0C</c>, off-screen Mother Brain Rinka animation program.</summary>
    internal const ushort SpecialInitial = 0xba0c;

    private static readonly RinkaInstructionMechanicsWord[] Words =
    [
        new(0xb9e0, 0xb9b3), new(0xb9e2, 0x0040), new(0xb9e6, 0xb9c7),
        new(0xb9e8, 0x0010), new(0xb9ec, 0x0008), new(0xb9f0, 0x0007),
        new(0xb9f4, 0x0006), new(0xb9f8, 0x0005), new(0xb9fc, 0x0006),
        new(0xba00, 0x0007), new(0xba04, 0x0008), new(0xba08, 0x80ed),
        new(0xba0a, 0xb9e8), new(0xba0c, 0xb9bd), new(0xba0e, 0x0040),
        new(0xba12, 0xb9c7), new(0xba14, 0x0010), new(0xba18, 0x0008),
        new(0xba1c, 0x0007), new(0xba20, 0x0006), new(0xba24, 0x0005),
        new(0xba28, 0x0006), new(0xba2c, 0x0007), new(0xba30, 0x0008),
        new(0xba34, 0x80ed), new(0xba36, 0xba14),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xb9e4, 0xb9ea, 0xb9ee, 0xb9f2, 0xb9f6, 0xb9fa, 0xb9fe, 0xba02, 0xba06,
        0xba10, 0xba16, 0xba1a, 0xba1e, 0xba22, 0xba26, 0xba2a, 0xba2e, 0xba32,
    ];

    /// <summary>Number of mechanics words compiled from the two native programs.</summary>
    internal static int MechanicsWordCount => Words.Length;

    /// <summary>Number of interleaved presentation words deliberately left ROM-backed.</summary>
    internal static int PresentationWordCount => PresentationWords.Length;

    /// <summary>Returns one mechanics definition for cartridge-equivalence verification.</summary>
    internal static RinkaInstructionMechanicsWord MechanicsWord(int index) => Words[index];

    /// <summary>Returns one live spritemap-word address for boundary verification.</summary>
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>
    /// Reads one mechanics word and rejects presentation addresses or pointers outside the
    /// two authored programs. A restored invalid cursor must not resume arbitrary bank data.
    /// </summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            RinkaInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Rinka instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    /// <summary>True when an absolute address names a byte owned by compiled mechanics.</summary>
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

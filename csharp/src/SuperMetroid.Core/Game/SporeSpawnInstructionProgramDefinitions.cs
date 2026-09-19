namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A5 address.</summary>
internal readonly record struct SporeSpawnInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled mechanics words from Spore Spawn's five bank-$A5 instruction programs.
/// </summary>
/// <remarks>
/// These native lists interleave simulation state with presentation data. Durations,
/// callbacks, callback operands, loop counters, and branch targets affect gameplay and
/// therefore live here. The word following every duration is a spritemap pointer; those
/// forty-one presentation words deliberately remain live cartridge reads.
/// </remarks>
internal static class SporeSpawnInstructionProgramDefinitions
{
    /// <summary><c>$A5:E6B9</c>, defeated-room initialization program.</summary>
    internal const ushort InitialDead = 0xe6b9;

    /// <summary><c>$A5:E6C7</c>, living-room descent initialization program.</summary>
    internal const ushort InitialAlive = 0xe6c7;

    /// <summary><c>$A5:E6D5</c>, first open-and-moving combat program.</summary>
    internal const ushort FightStarted = 0xe6d5;

    /// <summary><c>$A5:E729</c>, close-head and resume-motion program.</summary>
    internal const ushort CloseAndMove = 0xe729;

    /// <summary><c>$A5:E77D</c>, complete death and hardening program.</summary>
    internal const ushort Death = 0xe77d;

    private static readonly SporeSpawnInstructionMechanicsWord[] Words =
    [
        new(0xe6b9, 0xe91c), new(0xe6bb, 0x00c0), new(0xe6bd, 0xe8ba),
        new(0xe6bf, 0xeb1a), new(0xe6c1, 0x0001), new(0xe6c5, 0x812f),
        new(0xe6c7, 0x0100), new(0xe6cb, 0xe8ba), new(0xe6cd, 0xeb1b),
        new(0xe6cf, 0x0001), new(0xe6d3, 0x812f), new(0xe6d5, 0xe82d),
        new(0xe6d7, 0x0040), new(0xe6d9, 0x0001), new(0xe6db, 0xe8ba),
        new(0xe6dd, 0xeb52), new(0xe6df, 0x0300), new(0xe6e3, 0xe872),
        new(0xe6e5, 0x0001), new(0xe6e7, 0xe895), new(0xe6e9, 0x002c),
        new(0xe6eb, 0x0001), new(0xe6ef, 0x0008), new(0xe6f3, 0x0008),
        new(0xe6f7, 0x0008), new(0xe6fb, 0x0007), new(0xe6ff, 0x0007),
        new(0xe703, 0x0006), new(0xe707, 0x0001), new(0xe70b, 0xe771),
        new(0xe70d, 0xe8ba), new(0xe70f, 0xeb1a), new(0xe711, 0x8123),
        new(0xe713, 0x0005), new(0xe715, 0x0008), new(0xe719, 0x0008),
        new(0xe71d, 0x0008), new(0xe721, 0x0008), new(0xe725, 0x8110),
        new(0xe727, 0xe715), new(0xe729, 0x0008), new(0xe72d, 0x0008),
        new(0xe731, 0x0008), new(0xe735, 0x0008), new(0xe739, 0x0008),
        new(0xe73d, 0x0008), new(0xe741, 0x0001), new(0xe745, 0xe8ba),
        new(0xe747, 0xeb52), new(0xe749, 0xe872), new(0xe74b, 0x0000),
        new(0xe74d, 0xe75f), new(0xe74f, 0x0200), new(0xe753, 0xe872),
        new(0xe755, 0x0001), new(0xe757, 0x00d0), new(0xe75b, 0x80ed),
        new(0xe75d, 0xe6e3), new(0xe77d, 0xe8ba), new(0xe77f, 0xeb9b),
        new(0xe781, 0x0001), new(0xe785, 0xe8ba), new(0xe787, 0xebee),
        new(0xe789, 0x8123), new(0xe78b, 0x000a), new(0xe78d, 0x0001),
        new(0xe791, 0xe9b1), new(0xe793, 0x813a), new(0xe795, 0x0008),
        new(0xe797, 0x8110), new(0xe799, 0xe78d), new(0xe79b, 0x0008),
        new(0xe79f, 0x0008), new(0xe7a3, 0x0008), new(0xe7a7, 0x0008),
        new(0xe7ab, 0x0008), new(0xe7af, 0x0008), new(0xe7b3, 0x0001),
        new(0xe7b7, 0xe87c), new(0xe7b9, 0x8123), new(0xe7bb, 0x000a),
        new(0xe7bd, 0xe96e), new(0xe7bf, 0x813a), new(0xe7c1, 0x0008),
        new(0xe7c3, 0x8110), new(0xe7c5, 0xe7bd), new(0xe7c7, 0xe8ca),
        new(0xe7c9, 0x0000), new(0xe7cb, 0xe96e), new(0xe7cd, 0x0010),
        new(0xe7d1, 0xe8ca), new(0xe7d3, 0x0020), new(0xe7d5, 0xe96e),
        new(0xe7d7, 0x0010), new(0xe7db, 0xe8ca), new(0xe7dd, 0x0040),
        new(0xe7df, 0xe96e), new(0xe7e1, 0x0010), new(0xe7e5, 0xe8ca),
        new(0xe7e7, 0x0060), new(0xe7e9, 0xe96e), new(0xe7eb, 0x0010),
        new(0xe7ef, 0xe8ca), new(0xe7f1, 0x0080), new(0xe7f3, 0xe96e),
        new(0xe7f5, 0x0010), new(0xe7f9, 0xe8ca), new(0xe7fb, 0x00a0),
        new(0xe7fd, 0xe96e), new(0xe7ff, 0x0010), new(0xe803, 0xe8ca),
        new(0xe805, 0x00c0), new(0xe807, 0xe96e), new(0xe809, 0x0010),
        new(0xe80d, 0xe8b1), new(0xe80f, 0x812f),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xe6c3, 0xe6c9, 0xe6d1, 0xe6e1, 0xe6ed, 0xe6f1, 0xe6f5,
        0xe6f9, 0xe6fd, 0xe701, 0xe705, 0xe709, 0xe717, 0xe71b,
        0xe71f, 0xe723, 0xe72b, 0xe72f, 0xe733, 0xe737, 0xe73b,
        0xe73f, 0xe743, 0xe751, 0xe759, 0xe783, 0xe78f, 0xe79d,
        0xe7a1, 0xe7a5, 0xe7a9, 0xe7ad, 0xe7b1, 0xe7b5, 0xe7cf,
        0xe7d9, 0xe7e3, 0xe7ed, 0xe7f7, 0xe801, 0xe80b,
    ];

    /// <summary>Number of mechanics words compiled from the five native programs.</summary>
    internal static int MechanicsWordCount => Words.Length;

    /// <summary>Number of interleaved presentation words deliberately left ROM-backed.</summary>
    internal static int PresentationWordCount => PresentationWords.Length;

    /// <summary>Returns one mechanics definition for cartridge-equivalence verification.</summary>
    internal static SporeSpawnInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];

    /// <summary>Returns one live spritemap-word address for boundary verification.</summary>
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>
    /// Reads one mechanics word and rejects presentation addresses or pointers outside the
    /// translated family. A restored invalid cursor must not silently resume ROM execution.
    /// </summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            SporeSpawnInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Spore Spawn instruction mechanics pointer $A5:{address:X4} is not compiled.");
    }

    /// <summary>True when an absolute address names a byte owned by compiled mechanics.</summary>
    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa50000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }
}

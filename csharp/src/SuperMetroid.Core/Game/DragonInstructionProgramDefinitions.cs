namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A2 address.</summary>
internal readonly record struct DragonInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics words from Dragon's body, wing, and attack programs.</summary>
/// <remarks>
/// Durations, loop control, sleeps, and the attack-completion callback are immutable
/// simulation data. The sixteen interleaved spritemap pointers remain live cartridge
/// presentation data.
/// </remarks>
internal static class DragonInstructionProgramDefinitions
{
    /// <summary><c>$A2:E59B</c>, sleeping body facing left.</summary>
    internal const ushort IdleFacingLeft = 0xe59b;

    /// <summary><c>$A2:E5AD</c>, sleeping body facing right.</summary>
    internal const ushort IdleFacingRight = 0xe5ad;

    /// <summary><c>$A2:E5A1</c>, cosmetic wing loop facing left.</summary>
    internal const ushort WingsFacingLeft = 0xe5a1;

    /// <summary><c>$A2:E5B3</c>, cosmetic wing loop facing right.</summary>
    internal const ushort WingsFacingRight = 0xe5b3;

    /// <summary><c>$A2:E5BF</c>, five-frame attack body sequence facing left.</summary>
    internal const ushort AttackingFacingLeft = 0xe5bf;

    /// <summary><c>$A2:E5D7</c>, five-frame attack body sequence facing right.</summary>
    internal const ushort AttackingFacingRight = 0xe5d7;

    /// <summary><c>$A2:E5FB</c>, marks the current body attack animation complete.</summary>
    internal const ushort AttackFinishedCallback = 0xe5fb;

    private static readonly DragonInstructionMechanicsWord[] Words =
    [
        new(0xe59b, 0x0001), new(0xe59f, 0x812f),

        new(0xe5a1, 0x0005), new(0xe5a5, 0x0005),
        new(0xe5a9, 0x80ed), new(0xe5ab, 0xe5a1),

        new(0xe5ad, 0x0001), new(0xe5b1, 0x812f),

        new(0xe5b3, 0x0005), new(0xe5b7, 0x0005),
        new(0xe5bb, 0x80ed), new(0xe5bd, 0xe5b3),

        new(0xe5bf, 0x0020), new(0xe5c3, 0x0003),
        new(0xe5c7, 0x0007), new(0xe5cb, 0x0003),
        new(0xe5cf, 0x0001), new(0xe5d3, AttackFinishedCallback),
        new(0xe5d5, 0x812f),

        new(0xe5d7, 0x0020), new(0xe5db, 0x0003),
        new(0xe5df, 0x0007), new(0xe5e3, 0x0003),
        new(0xe5e7, 0x0001), new(0xe5eb, AttackFinishedCallback),
        new(0xe5ed, 0x812f),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xe59d,
        0xe5a3, 0xe5a7,
        0xe5af,
        0xe5b5, 0xe5b9,
        0xe5c1, 0xe5c5, 0xe5c9, 0xe5cd, 0xe5d1,
        0xe5d9, 0xe5dd, 0xe5e1, 0xe5e5, 0xe5e9,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static DragonInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Dragon control or rejects pointers outside all six programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            DragonInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Dragon instruction mechanics pointer $A2:{address:X4} is not compiled.");
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

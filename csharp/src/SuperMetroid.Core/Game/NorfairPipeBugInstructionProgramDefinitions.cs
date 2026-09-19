namespace SuperMetroid.Core.Game;

internal readonly record struct NorfairPipeBugInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled control words for Norfair Pipe Bug rising and flight loops.</summary>
internal static class NorfairPipeBugInstructionProgramDefinitions
{
    /// <summary><c>$B3:8AE1</c>, rising while facing left.</summary>
    internal const ushort RisingLeft = 0x8ae1;
    /// <summary><c>$B3:8B05</c>, horizontal flight facing left.</summary>
    internal const ushort FlyingLeft = 0x8b05;
    /// <summary><c>$B3:8B21</c>, rising while facing right.</summary>
    internal const ushort RisingRight = 0x8b21;
    /// <summary><c>$B3:8B45</c>, horizontal flight facing right.</summary>
    internal const ushort FlyingRight = 0x8b45;

    private static readonly NorfairPipeBugInstructionMechanicsWord[] Words =
    [
        new(0x8ae1, 2), new(0x8ae5, 2), new(0x8ae9, 2), new(0x8aed, 2),
        new(0x8af1, 2), new(0x8af5, 2), new(0x8af9, 2), new(0x8afd, 2),
        new(0x8b01, 0x80ed), new(0x8b03, RisingLeft),
        new(0x8b05, 1), new(0x8b09, 1), new(0x8b0d, 1), new(0x8b11, 1),
        new(0x8b15, 1), new(0x8b19, 1), new(0x8b1d, 0x80ed),
        new(0x8b1f, FlyingLeft),
        new(0x8b21, 2), new(0x8b25, 2), new(0x8b29, 2), new(0x8b2d, 2),
        new(0x8b31, 2), new(0x8b35, 2), new(0x8b39, 2), new(0x8b3d, 2),
        new(0x8b41, 0x80ed), new(0x8b43, RisingRight),
        new(0x8b45, 1), new(0x8b49, 1), new(0x8b4d, 1), new(0x8b51, 1),
        new(0x8b55, 1), new(0x8b59, 1), new(0x8b5d, 0x80ed),
        new(0x8b5f, FlyingRight),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x8ae3, 0x8ae7, 0x8aeb, 0x8aef, 0x8af3, 0x8af7, 0x8afb, 0x8aff,
        0x8b07, 0x8b0b, 0x8b0f, 0x8b13, 0x8b17, 0x8b1b,
        0x8b23, 0x8b27, 0x8b2b, 0x8b2f, 0x8b33, 0x8b37, 0x8b3b, 0x8b3f,
        0x8b47, 0x8b4b, 0x8b4f, 0x8b53, 0x8b57, 0x8b5b,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static NorfairPipeBugInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            NorfairPipeBugInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }
        throw new InvalidDataException(
            $"Norfair Pipe Bug instruction mechanics pointer $B3:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb30000)
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

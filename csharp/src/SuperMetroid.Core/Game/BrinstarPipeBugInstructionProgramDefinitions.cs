namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$B3 address.</summary>
internal readonly record struct BrinstarPipeBugInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics for normal and strong Brinstar Pipe Bug programs.</summary>
internal static class BrinstarPipeBugInstructionProgramDefinitions
{
    internal const ushort NormalRisingLeft = 0x87ab;
    internal const ushort NormalShootingLeft = 0x87cf;
    internal const ushort NormalRisingRight = 0x87eb;
    internal const ushort NormalShootingRight = 0x880f;
    internal const ushort StrongRisingLeft = 0x8a1d;
    internal const ushort StrongShootingLeft = 0x8a31;
    internal const ushort StrongRisingRight = 0x8a45;
    internal const ushort StrongShootingRight = 0x8a59;

    private static readonly BrinstarPipeBugInstructionMechanicsWord[] Words =
    [
        new(0x87ab, 2), new(0x87af, 2), new(0x87b3, 2), new(0x87b7, 2),
        new(0x87bb, 2), new(0x87bf, 2), new(0x87c3, 2), new(0x87c7, 2),
        new(0x87cb, 0x80ed), new(0x87cd, NormalRisingLeft),
        new(0x87cf, 1), new(0x87d3, 1), new(0x87d7, 1), new(0x87db, 1),
        new(0x87df, 1), new(0x87e3, 1), new(0x87e7, 0x80ed),
        new(0x87e9, NormalShootingLeft),
        new(0x87eb, 2), new(0x87ef, 2), new(0x87f3, 2), new(0x87f7, 2),
        new(0x87fb, 2), new(0x87ff, 2), new(0x8803, 2), new(0x8807, 2),
        new(0x880b, 0x80ed), new(0x880d, NormalRisingRight),
        new(0x880f, 1), new(0x8813, 1), new(0x8817, 1), new(0x881b, 1),
        new(0x881f, 1), new(0x8823, 1), new(0x8827, 0x80ed),
        new(0x8829, NormalShootingRight),

        new(0x8a1d, 2), new(0x8a21, 1), new(0x8a25, 2), new(0x8a29, 1),
        new(0x8a2d, 0x80ed), new(0x8a2f, StrongRisingLeft),
        new(0x8a31, 3), new(0x8a35, 3), new(0x8a39, 3), new(0x8a3d, 3),
        new(0x8a41, 0x80ed), new(0x8a43, StrongShootingLeft),
        new(0x8a45, 2), new(0x8a49, 1), new(0x8a4d, 2), new(0x8a51, 1),
        new(0x8a55, 0x80ed), new(0x8a57, StrongRisingRight),
        new(0x8a59, 3), new(0x8a5d, 3), new(0x8a61, 3), new(0x8a65, 3),
        new(0x8a69, 0x80ed), new(0x8a6b, StrongShootingRight),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x87ad, 0x87b1, 0x87b5, 0x87b9, 0x87bd, 0x87c1, 0x87c5, 0x87c9,
        0x87d1, 0x87d5, 0x87d9, 0x87dd, 0x87e1, 0x87e5,
        0x87ed, 0x87f1, 0x87f5, 0x87f9, 0x87fd, 0x8801, 0x8805, 0x8809,
        0x8811, 0x8815, 0x8819, 0x881d, 0x8821, 0x8825,
        0x8a1f, 0x8a23, 0x8a27, 0x8a2b, 0x8a33, 0x8a37, 0x8a3b, 0x8a3f,
        0x8a47, 0x8a4b, 0x8a4f, 0x8a53, 0x8a5b, 0x8a5f, 0x8a63, 0x8a67,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static BrinstarPipeBugInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            BrinstarPipeBugInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }
        throw new InvalidDataException(
            $"Brinstar Pipe Bug instruction mechanics pointer $B3:{address:X4} is not compiled.");
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

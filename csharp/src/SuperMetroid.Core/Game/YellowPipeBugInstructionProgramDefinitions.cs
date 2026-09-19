namespace SuperMetroid.Core.Game;

internal readonly record struct YellowPipeBugInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled control words for Yellow Brinstar Pipe Bug straight and arc loops.</summary>
internal static class YellowPipeBugInstructionProgramDefinitions
{
    /// <summary><c>$B3:8EFC</c>, straight flight facing left.</summary>
    internal const ushort FlyingLeft = 0x8efc;
    /// <summary><c>$B3:8F10</c>, arcing flight facing left.</summary>
    internal const ushort ArcingLeft = 0x8f10;
    /// <summary><c>$B3:8F24</c>, straight flight facing right.</summary>
    internal const ushort FlyingRight = 0x8f24;
    /// <summary><c>$B3:8F38</c>, arcing flight facing right.</summary>
    internal const ushort ArcingRight = 0x8f38;

    private static readonly YellowPipeBugInstructionMechanicsWord[] Words =
    [
        new(0x8efc, 4), new(0x8f00, 4), new(0x8f04, 4), new(0x8f08, 4),
        new(0x8f0c, 0x80ed), new(0x8f0e, FlyingLeft),
        new(0x8f10, 1), new(0x8f14, 1), new(0x8f18, 1), new(0x8f1c, 1),
        new(0x8f20, 0x80ed), new(0x8f22, ArcingLeft),
        new(0x8f24, 4), new(0x8f28, 4), new(0x8f2c, 4), new(0x8f30, 4),
        new(0x8f34, 0x80ed), new(0x8f36, FlyingRight),
        new(0x8f38, 1), new(0x8f3c, 1), new(0x8f40, 1), new(0x8f44, 1),
        new(0x8f48, 0x80ed), new(0x8f4a, ArcingRight),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x8efe, 0x8f02, 0x8f06, 0x8f0a,
        0x8f12, 0x8f16, 0x8f1a, 0x8f1e,
        0x8f26, 0x8f2a, 0x8f2e, 0x8f32,
        0x8f3a, 0x8f3e, 0x8f42, 0x8f46,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static YellowPipeBugInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            YellowPipeBugInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }
        throw new InvalidDataException(
            $"Yellow Pipe Bug instruction mechanics pointer $B3:{address:X4} is not compiled.");
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

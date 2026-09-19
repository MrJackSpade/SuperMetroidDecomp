namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A6 address.</summary>
internal readonly record struct HibashiInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics words from Hibashi's paired graphics and hitbox programs.</summary>
/// <remarks>
/// Durations and instruction callbacks are immutable simulation data. The 24 interleaved
/// spritemap pointers remain live cartridge presentation data.
/// </remarks>
internal static class HibashiInstructionProgramDefinitions
{
    /// <summary><c>$A6:8D1B</c>, the visible eruption graphics program.</summary>
    internal const ushort GraphicsProgram = 0x8d1b;

    /// <summary><c>$A6:8DA9</c>, the invisible collision-part program.</summary>
    internal const ushort HitboxProgram = 0x8da9;

    private static readonly HibashiInstructionMechanicsWord[] Words =
    [
        new(0x8d1b, 0x8daf),
        new(0x8d1d, 0x0002), new(0x8d21, 0x8e13),
        new(0x8d23, 0x0002), new(0x8d27, 0x8e2d),
        new(0x8d29, 0x0002), new(0x8d2d, 0x8e41),
        new(0x8d2f, 0x0002), new(0x8d33, 0x8e55),
        new(0x8d35, 0x0001), new(0x8d39, 0x8e69),
        new(0x8d3b, 0x0001), new(0x8d3f, 0x8e7d),
        new(0x8d41, 0x0001), new(0x8d45, 0x8e91),
        new(0x8d47, 0x0001), new(0x8d4b, 0x8ea5),
        new(0x8d4d, 0x0002), new(0x8d51, 0x8eb9),
        new(0x8d53, 0x0002), new(0x8d57, 0x8ecd),
        new(0x8d59, 0x0002), new(0x8d5d, 0x8ee1),
        new(0x8d5f, 0x0002), new(0x8d63, 0x8ef5),
        new(0x8d65, 0x0002), new(0x8d69, 0x8f09),
        new(0x8d6b, 0x0002), new(0x8d6f, 0x8f1d),
        new(0x8d71, 0x0002), new(0x8d75, 0x8f31),
        new(0x8d77, 0x0002), new(0x8d7b, 0x8f45),
        new(0x8d7d, 0x0004), new(0x8d81, 0x8f59),
        new(0x8d83, 0x0004), new(0x8d87, 0x8f6d),
        new(0x8d89, 0x0004), new(0x8d8d, 0x8f81),
        new(0x8d8f, 0x0004), new(0x8d93, 0x8f95),
        new(0x8d95, 0x0004), new(0x8d99, 0x8fa9),
        new(0x8d9b, 0x0004), new(0x8d9f, 0x8fbd),
        new(0x8da1, 0x0004), new(0x8da5, 0x8fd1),
        new(0x8da7, 0x812f),
        new(0x8da9, 0x0002), new(0x8dad, 0x812f),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x8d1f, 0x8d25, 0x8d2b, 0x8d31, 0x8d37, 0x8d3d,
        0x8d43, 0x8d49, 0x8d4f, 0x8d55, 0x8d5b, 0x8d61,
        0x8d67, 0x8d6d, 0x8d73, 0x8d79, 0x8d7f, 0x8d85,
        0x8d8b, 0x8d91, 0x8d97, 0x8d9d, 0x8da3, 0x8dab,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static HibashiInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Hibashi control or rejects pointers outside both programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            HibashiInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Hibashi instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
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

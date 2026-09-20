namespace SuperMetroid.Core.Game;

internal readonly record struct MultiviolaInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Multiviola's production animation loop. The
/// interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class MultiviolaInstructionProgramDefinitions
{
    /// <summary><c>InstList_Multiviola</c> at $A2:B2DC.</summary>
    internal const ushort Flying = 0xb2dc;

    private static readonly MultiviolaInstructionMechanicsWord[] Words =
    [
        new(0xb2dc, 10), new(0xb2e0, 10), new(0xb2e4, 10), new(0xb2e8, 10),
        new(0xb2ec, 10), new(0xb2f0, 10), new(0xb2f4, 10), new(0xb2f8, 10),
        new(0xb2fc, 10), new(0xb300, 10), new(0xb304, 10), new(0xb308, 10),
        new(0xb30c, 10), new(0xb310, 10),
        new(0xb314, CommonEnemyInstructionCodes.Goto), new(0xb316, Flying),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xb2de, 0xb2e2, 0xb2e6, 0xb2ea, 0xb2ee, 0xb2f2, 0xb2f6,
        0xb2fa, 0xb2fe, 0xb302, 0xb306, 0xb30a, 0xb30e, 0xb312,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static MultiviolaInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            MultiviolaInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Multiviola instruction mechanics pointer $A2:{address:X4} is not compiled.");
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

namespace SuperMetroid.Core.Game;

internal readonly record struct HorizontalShutterInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the horizontal shutter's stationary program.
/// Its interleaved spritemap operand remains live cartridge presentation data.
/// </summary>
internal static class HorizontalShutterInstructionProgramDefinitions
{
    /// <summary><c>InstList_ShutterHorizontal</c> at $A2:E9D4.</summary>
    internal const ushort Stationary = 0xe9d4;

    private static readonly HorizontalShutterInstructionMechanicsWord[] Words =
    [
        new(0xe9d4, 1),
        new(0xe9d8, CommonEnemyInstructionCodes.Sleep),
    ];

    private const ushort PresentationWord = 0xe9d6;

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => 1;
    internal static HorizontalShutterInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => index == 0
        ? PresentationWord
        : throw new ArgumentOutOfRangeException(nameof(index));

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Horizontal-shutter instruction mechanics pointer $A2:{address:X4} is not compiled.");
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

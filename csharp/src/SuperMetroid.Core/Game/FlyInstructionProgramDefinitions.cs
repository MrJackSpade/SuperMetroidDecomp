namespace SuperMetroid.Core.Game;

internal readonly record struct FlyInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// Compiled engine-control words for the shared Mellow, Mella, and Memu animation loop.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class FlyInstructionProgramDefinitions
{
    /// <summary><c>InstList_Mellow_Mella_Menu</c> at $A2:B013.</summary>
    internal const ushort Flight = 0xb013;

    private static readonly FlyInstructionMechanicsWord[] Words =
    [
        new(0xb013, 2), new(0xb017, 2), new(0xb01b, 2), new(0xb01f, 2),
        new(0xb023, CommonEnemyInstructionCodes.Goto), new(0xb025, Flight),
    ];

    private static readonly ushort[] PresentationWords = [0xb015, 0xb019, 0xb01d, 0xb021];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static FlyInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Fly-family instruction mechanics pointer $A2:{address:X4} is not compiled.");
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

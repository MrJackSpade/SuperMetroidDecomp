namespace SuperMetroid.Core.Game;

internal readonly record struct KzanInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the Kzan spike-platform animation program.
/// Its interleaved spritemap operand remains live cartridge presentation data.
/// </summary>
internal static class KzanInstructionProgramDefinitions
{
    /// <summary><c>InstList_Kzan</c> at $A6:8B29.</summary>
    internal const ushort Idle = 0x8b29;

    private static readonly KzanInstructionMechanicsWord[] Words =
    [
        new(0x8b29, 1),
        new(0x8b2d, CommonEnemyInstructionCodes.Sleep),
    ];

    /// <summary>Address of the live <c>Spritemap_Kzan</c> operand at $A6:8B2B.</summary>
    internal const ushort PresentationWord = 0x8b2b;

    internal static int MechanicsWordCount => Words.Length;
    internal static KzanInstructionMechanicsWord MechanicsWord(int index) => Words[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Kzan instruction mechanics pointer $A6:{address:X4} is not compiled.");
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

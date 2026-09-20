namespace SuperMetroid.Core.Game;

internal readonly record struct PolypInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// Compiled engine-control words for Polyp's stationary program. Its spritemap operand
/// remains live cartridge presentation data.
/// </summary>
internal static class PolypInstructionProgramDefinitions
{
    /// <summary><c>InstList_Polyp</c> at $A2:B51A.</summary>
    internal const ushort Stationary = 0xb51a;

    private static readonly PolypInstructionMechanicsWord[] Words =
    [
        new(0xb51a, 1),
        new(0xb51e, CommonEnemyInstructionCodes.Sleep),
    ];

    /// <summary>The live <c>Spritemap_Polyp</c> operand at $A2:B51C.</summary>
    internal const ushort PresentationWord = 0xb51c;

    internal static int MechanicsWordCount => Words.Length;
    internal static PolypInstructionMechanicsWord MechanicsWord(int index) => Words[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }
        throw new InvalidDataException(
            $"Polyp instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return bankAddress is 0xb51a or 0xb51b or 0xb51e or 0xb51f;
    }
}

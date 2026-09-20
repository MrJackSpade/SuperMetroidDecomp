namespace SuperMetroid.Core.Game;

internal readonly record struct MochtroidInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// Compiled engine-control words for Mochtroid's free-flight and attached animation loops.
/// Their eight spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class MochtroidInstructionProgramDefinitions
{
    /// <summary><c>InstList_Mochtroid_NotTouchingSamus</c> at $A3:A745.</summary>
    internal const ushort FreeFlight = 0xa745;
    /// <summary><c>InstList_Mochtroid_TouchingSamus</c> at $A3:A759.</summary>
    internal const ushort Attached = 0xa759;
    /// <summary>The shake-velocity table immediately after the programs, at $A3:A76D.</summary>
    internal const ushort FirstAdjacentMechanicsData = 0xa76d;

    private static readonly MochtroidInstructionMechanicsWord[] Words =
    [
        new(FreeFlight, 0x0e), new(0xa749, 0x0e),
        new(0xa74d, 0x0e), new(0xa751, 0x0e),
        new(0xa755, CommonEnemyInstructionCodes.Goto), new(0xa757, FreeFlight),
        new(Attached, 5), new(0xa75d, 5), new(0xa761, 5), new(0xa765, 5),
        new(0xa769, CommonEnemyInstructionCodes.Goto), new(0xa76b, Attached),
    ];

    private static readonly ushort[] PresentationWords =
        [0xa747, 0xa74b, 0xa74f, 0xa753, 0xa75b, 0xa75f, 0xa763, 0xa767];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static MochtroidInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }
        throw new InvalidDataException(
            $"Mochtroid instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
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

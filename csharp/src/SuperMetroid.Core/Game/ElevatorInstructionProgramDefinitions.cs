namespace SuperMetroid.Core.Game;

internal readonly record struct ElevatorInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// Compiled engine-control words for the ordinary elevator's two-frame animation loop.
/// Its two spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class ElevatorInstructionProgramDefinitions
{
    /// <summary><c>InstList_Elevator</c> at $A3:94D6.</summary>
    internal const ushort Loop = 0x94d6;

    /// <summary>The controller-input table immediately after the program, at $A3:94E2.</summary>
    internal const ushort FirstAdjacentMechanicsData = 0x94e2;

    private static readonly ElevatorInstructionMechanicsWord[] Words =
    [
        new(Loop, 2),
        new(0x94da, 2),
        new(0x94de, CommonEnemyInstructionCodes.Goto),
        new(0x94e0, Loop),
    ];

    private static readonly ushort[] PresentationWords = [0x94d8, 0x94dc];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ElevatorInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }
        throw new InvalidDataException(
            $"Elevator instruction mechanics pointer $A3:{address:X4} is not compiled.");
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

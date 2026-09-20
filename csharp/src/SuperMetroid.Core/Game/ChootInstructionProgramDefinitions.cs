namespace SuperMetroid.Core.Game;

internal readonly record struct ChootInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// Compiled engine-control words for Choot's idle, jumping, and falling programs.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class ChootInstructionProgramDefinitions
{
    /// <summary><c>InstructionList_Choot_Idle</c> at $A2:D82C.</summary>
    internal const ushort Idle = 0xd82c;

    /// <summary><c>InstructionList_Choot_Jumping</c> at $A2:D834.</summary>
    internal const ushort Jumping = 0xd834;

    /// <summary><c>InstructionList_Choot_Falling</c> at $A2:D840.</summary>
    internal const ushort Falling = 0xd840;

    private static readonly ChootInstructionMechanicsWord[] Words =
    [
        new(0xd82c, CommonEnemyInstructionCodes.DisableOffScreenProcessing),
        new(0xd82e, 1),
        new(0xd832, CommonEnemyInstructionCodes.Sleep),
        new(0xd834, CommonEnemyInstructionCodes.EnableOffScreenProcessing),
        new(0xd836, 8),
        new(0xd83a, 1),
        new(0xd83e, CommonEnemyInstructionCodes.Sleep),
        new(0xd840, CommonEnemyInstructionCodes.EnableOffScreenProcessing),
        new(0xd842, 8),
        new(0xd846, 1),
        new(0xd84a, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xd830,
        0xd838,
        0xd83c,
        0xd844,
        0xd848,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ChootInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Choot instruction mechanics pointer $A2:{address:X4} is not compiled.");
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

namespace SuperMetroid.Core.Game;

internal readonly record struct OwtchInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Owtch's left/right animation programs.
/// Interleaved spritemap operands are selected by the installed visual catalog.
/// </summary>
internal static class OwtchInstructionProgramDefinitions
{
    /// <summary><c>InstList_Owtch_MovingLeft_0</c> at $A2:A3AB.</summary>
    internal const ushort MovingLeft = 0xa3ab;

    /// <summary><c>InstList_Owtch_MovingRight_0</c> at $A2:A3BD.</summary>
    internal const ushort MovingRight = 0xa3bd;

    private static readonly OwtchInstructionMechanicsWord[] Words =
    [
        new(0xa3ab, EnemyInstructionCodePointers.Instruction_Owtch_0),
        new(0xa3ad, 8),
        new(0xa3b1, 8),
        new(0xa3b5, 8),
        new(0xa3b9, CommonEnemyInstructionCodes.Goto),
        new(0xa3bb, 0xa3ad),
        new(0xa3bd, EnemyInstructionCodePointers.Instruction_Owtch_1),
        new(0xa3bf, 8),
        new(0xa3c3, 8),
        new(0xa3c7, 8),
        new(0xa3cb, CommonEnemyInstructionCodes.Goto),
        new(0xa3cd, 0xa3bf),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xa3af, 0xa3b3, 0xa3b7,
        0xa3c1, 0xa3c5, 0xa3c9,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static OwtchInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            OwtchInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Owtch instruction mechanics pointer $A2:{address:X4} is not compiled.");
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

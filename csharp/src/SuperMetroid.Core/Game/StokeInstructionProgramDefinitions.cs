namespace SuperMetroid.Core.Game;

internal readonly record struct StokeInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Stoke's walking and attack programs.
/// Interleaved spritemap operands are selected by the installed visual catalog.
/// </summary>
internal static class StokeInstructionProgramDefinitions
{
    /// <summary><c>InstList_Stoke_MovingLeft_0</c> at $A2:8932.</summary>
    internal const ushort MovingLeft = 0x8932;
    /// <summary><c>InstList_Stoke_AttackingLeft</c> at $A2:8948.</summary>
    internal const ushort AttackingLeft = 0x8948;
    /// <summary><c>InstList_Stoke_MovingRight_0</c> at $A2:8958.</summary>
    internal const ushort MovingRight = 0x8958;
    /// <summary><c>InstList_Stoke_AttackingRight</c> at $A2:896E.</summary>
    internal const ushort AttackingRight = 0x896e;

    private static readonly StokeInstructionMechanicsWord[] Words =
    [
        new(0x8932, EnemyInstructionCodePointers.Instruction_Stoke_SetMovingLeft),
        new(0x8934, 8), new(0x8938, 0x0010), new(0x893c, 8), new(0x8940, 8),
        new(0x8944, CommonEnemyInstructionCodes.Goto), new(0x8946, 0x8934),
        new(0x8948, 0x0010),
        new(0x894c, EnemyInstructionCodePointers.Instruction_Stoke_SpawnFireball),
        new(0x894e, 0), new(0x8950, 0x0010),
        new(0x8954, CommonEnemyInstructionCodes.Goto), new(0x8956, MovingLeft),
        new(0x8958, EnemyInstructionCodePointers.Instruction_Stoke_SetMovingRight),
        new(0x895a, 8), new(0x895e, 0x0010), new(0x8962, 8), new(0x8966, 8),
        new(0x896a, CommonEnemyInstructionCodes.Goto), new(0x896c, 0x895a),
        new(0x896e, 0x0010),
        new(0x8972, EnemyInstructionCodePointers.Instruction_Stoke_SpawnFireball),
        new(0x8974, 1), new(0x8976, 0x0010),
        new(0x897a, CommonEnemyInstructionCodes.Goto), new(0x897c, MovingRight),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x8936, 0x893a, 0x893e, 0x8942, 0x894a, 0x8952,
        0x895c, 0x8960, 0x8964, 0x8968, 0x8970, 0x8978,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static StokeInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            StokeInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Stoke instruction mechanics pointer $A2:{address:X4} is not compiled.");
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

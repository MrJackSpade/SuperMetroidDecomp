namespace SuperMetroid.Core.Game;

/// <summary>One compiled Eye Door sweat mechanics word at its bank-$86 address.</summary>
internal readonly record struct EyeDoorSweatInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for an Eye Door sweat drop's falling loop and floor-impact animation.
/// Interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal static class EyeDoorSweatInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_EyeDoorSweat</c> at $86:B615.</summary>
    internal const ushort Initial = 0xb615;

    /// <summary><c>InstList_EnemyProjectile_EyeDoorSweat_Impact</c> at $86:B61D.</summary>
    internal const ushort Impact = 0xb61d;

    internal static int MechanicsWordCount => 8;
    internal static int PresentationWordCount => 4;

    /// <summary>Falling draws once and loops; impact clears movement, draws three frames and deletes.</summary>
    internal static EyeDoorSweatInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index is >= 4 and <= 6) return new((ushort)(Impact + 2 + 4 * (index - 4)), 6);
        return index switch
        {
            0 => new(Initial, 6),
            1 => new(Initial + 4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
            2 => new(Initial + 6, Initial),
            3 => new(Impact, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
            _ => new(Impact + 14, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        };
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index == 0 ? Initial + 2 : Impact + 4 + 4 * (index - 1));
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            var word = MechanicsWord(index);
            if (word.Address == address) return word.Value;
        }
        throw new InvalidDataException(
            $"Eye Door sweat instruction mechanics pointer $86:{address:X4} " +
            "is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}

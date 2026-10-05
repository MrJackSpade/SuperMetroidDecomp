namespace SuperMetroid.Core.Game;

internal readonly record struct TorizoExplosiveSwipeInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Bomb Torizo's explosive-swipe projectile at $86:A4AA-$A4C1.
/// Its five spritemap operands use extracted presentation art; its packed sound ID
/// remains cartridge audio data.
/// </summary>
internal static class TorizoExplosiveSwipeInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_BombTorizoExplosionSwipe</c> at $86:A4AA.</summary>
    internal const ushort Initial = 0xa4aa;

    internal static int MechanicsWordCount => 7;
    internal static int PresentationWordCount => 5;

    /// <summary>One packed sound command precedes five five-tick poses and deletion.</summary>
    internal static TorizoExplosiveSwipeInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index == 0)
            return new(Initial, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max1);
        return new((ushort)(Initial + 3 + 4 * (index - 1)), index == 6
            ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete : (ushort)5);
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Initial + 5 + 4 * index);
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            TorizoExplosiveSwipeInstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Torizo explosive-swipe mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            var word = MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}

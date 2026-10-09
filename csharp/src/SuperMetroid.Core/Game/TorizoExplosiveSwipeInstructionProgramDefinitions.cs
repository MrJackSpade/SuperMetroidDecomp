namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Bomb Torizo's explosive-swipe projectile at $86:A4AA-$A4C1.
/// Its five spritemap operands use extracted presentation art; its packed sound ID
/// remains cartridge audio data.
/// </summary>
internal abstract class TorizoExplosiveSwipeInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_BombTorizoExplosionSwipe</c> at $86:A4AA.</summary>
    internal const ushort Initial = 0xa4aa;

    /// <summary>Number of compiled sound, duration, and deletion mechanics words in this instruction list.</summary>
    public static int MechanicsWordCount => 7;

    /// <summary>Number of spritemap operands whose visual identities are supplied by presentation art.</summary>
    public static int PresentationWordCount => 5;

    /// <summary>One packed sound command precedes five five-tick poses and deletion.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index == 0)
            return new(Initial, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max1);
        return new((ushort)(Initial + 3 + 4 * (index - 1)), index == 6
            ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete : (ushort)5);
    }

    /// <summary>Returns the bank-$86 address of one editable spritemap operand.</summary>
    /// <param name="index">Zero-based presentation slot index.</param>
    /// <returns>Address of the selected spritemap word in the instruction stream.</returns>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Initial + 5 + 4 * index);
    }
    /// <summary>Finds the compiled mechanics operand stored at an exact instruction address.</summary>
    /// <param name="address">Bank-relative address to resolve.</param>
    /// <returns>The native command or duration value stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not one of this program's compiled mechanics words.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Torizo explosive-swipe mechanics pointer $86:{address:X4} is not compiled.");
    }
}

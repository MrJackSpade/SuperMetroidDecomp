namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the two-frame Stoke projectile animation loop.
/// Interleaved spritemap operands identify compiled presentation.
/// </summary>
internal abstract class StokeProjectileInstructionProgramDefinitions
{
    /// <summary><c>UNUSED_InstList_EnemyProjectile_StokeProjectile_86DB0B</c> at $86:DB0C.</summary>
    internal const ushort Initial = 0xdb0c;

    // The shared 16-tick pose cadence is authored animation timing (reviewed under #1165); the interpreter only loads it into the instruction timer.
    /// <summary>Number of compiled delay and control words in the two-frame projectile loop.</summary>
    public static int MechanicsWordCount => 4;

    /// <summary>Number of frame operands interleaved with the loop's instruction words.</summary>
    public static int PresentationWordCount => 2;

    /// <summary>Resolves one ordinal loop-control entry to its ROM address and compiled value.</summary>
    /// <param name="index">Zero-based mechanics-word index.</param>
    /// <returns>The instruction address and its delay, branch, or loop value.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the four compiled entries.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return new((ushort)(Initial + (index < 3 ? 4 * index : 10)),
            index < 2 ? (ushort)16 : index == 2 ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY : Initial);
    }
    /// <summary>Returns the address of one of the two spritemap operands embedded in the loop.</summary>
    /// <param name="index">Zero-based frame-operand index.</param>
    /// <returns>The bank-relative address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the two compiled operands.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Initial + 2 + 4 * index);
    }
    /// <summary>Looks up a compiled delay or control word by its bank-relative instruction address.</summary>
    /// <param name="address">Address to resolve within the Stoke projectile loop.</param>
    /// <returns>The compiled instruction value at the matching address.</returns>
    /// <exception cref="InvalidDataException">The address does not identify a compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Stoke-projectile instruction mechanics pointer $86:{address:X4} is not compiled.");
    }
}

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the two-frame Stoke projectile animation loop.
/// Interleaved spritemap operands identify compiled presentation.
/// </summary>
internal abstract class StokeProjectileInstructionProgramDefinitions
{
    /// <summary><c>UNUSED_InstList_EnemyProjectile_StokeProjectile_86DB0B</c> at $86:DB0C.</summary>
    internal const ushort Initial = 0xdb0c;

    /// <summary>Two authored sixteen-tick poses plus a goto opcode and loop target.</summary>
    public static int MechanicsWordCount => 4;
    /// <summary>Each timed projectile pose retains one separate visual operand.</summary>
    public static int PresentationWordCount => 2;
    /// <summary>Exposes a timed hold or terminal goto word at its bank-$86 address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return new((ushort)(Initial + (index < 3 ? 4 * index : 10)),
            index < 2 ? (ushort)16 : index == 2 ? (ushort)EnemyProjectileInstruction.GotoY : Initial);
    }
    /// <summary>Locates the spritemap pointer after a pose's sixteen-tick duration.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Initial + 2 + 4 * index);
    }
    /// <summary>Searches the four compiled control addresses while rejecting interleaved visuals.</summary>
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

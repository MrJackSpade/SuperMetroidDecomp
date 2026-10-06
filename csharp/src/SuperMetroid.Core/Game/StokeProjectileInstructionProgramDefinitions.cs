namespace SuperMetroid.Core.Game;

/// <summary>One compiled Stoke-projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct StokeProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the two-frame Stoke projectile animation loop.
/// Interleaved spritemap operands identify compiled presentation.
/// </summary>
internal static class StokeProjectileInstructionProgramDefinitions
{
    /// <summary><c>UNUSED_InstList_EnemyProjectile_StokeProjectile_86DB0B</c> at $86:DB0C.</summary>
    internal const ushort Initial = 0xdb0c;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the projectile loop at $86:DB14.
    /// </summary>
    internal const ushort LoopCommand = 0xdb14;

    // The shared 16-tick pose cadence is authored animation timing (reviewed under #1165); the interpreter only loads it into the instruction timer.
    internal static int MechanicsWordCount => 4;
    internal static int PresentationWordCount => 2;
    internal static StokeProjectileInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return new((ushort)(Initial + (index < 3 ? 4 * index : 10)),
            index < 2 ? (ushort)16 : index == 2 ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY : Initial);
    }
    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Initial + 2 + 4 * index);
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            StokeProjectileInstructionMechanicsWord candidate = MechanicsWord(middle);
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

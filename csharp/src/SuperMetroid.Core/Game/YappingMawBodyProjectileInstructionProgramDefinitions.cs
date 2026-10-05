namespace SuperMetroid.Core.Game;

/// <summary>One compiled Yapping Maw body-projectile word at its bank-$86 address.</summary>
internal readonly record struct YappingMawBodyProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing and terminal sleep control for the two Yapping Maw body-link poses.
/// Their interleaved sprite operands select installed presentation artwork.
/// </summary>
internal static class YappingMawBodyProjectileInstructionProgramDefinitions
{
    /// <summary>
    /// <c>InstList_EnemyProjectile_YappingMawsBody_FacingDown</c> at $86:EC56.
    /// </summary>
    internal const ushort FacingDown = 0xec56;

    /// <summary>
    /// <c>InstList_EnemyProjectile_YappingMawsBody_FacingUp</c> at $86:EC5C.
    /// </summary>
    internal const ushort FacingUp = 0xec5c;

    internal static int MechanicsWordCount => 4;
    internal static int PresentationWordCount => 2;

    /// <summary>$86:EC56-EC61 contains two six-byte single-pose/sleep programs, down then up.</summary>
    internal static YappingMawBodyProjectileInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int operation = index & 1;
        return new((ushort)(FacingDown + index / 2 * 6 + operation * 4),
            operation == 0 ? (ushort)1 : EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep);
    }
    internal static ushort PresentationWordAddress(int index) => (uint)index < PresentationWordCount
        ? (ushort)(FacingDown + index * 6 + sizeof(ushort)) : throw new IndexOutOfRangeException();
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            YappingMawBodyProjectileInstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Yapping Maw body-projectile mechanics pointer $86:{address:X4} is not compiled.");
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

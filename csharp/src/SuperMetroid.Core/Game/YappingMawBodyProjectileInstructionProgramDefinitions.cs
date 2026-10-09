namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled timing and terminal sleep control for the two Yapping Maw body-link poses.
/// Their interleaved sprite operands select installed presentation artwork.
/// </summary>
internal abstract class YappingMawBodyProjectileInstructionProgramDefinitions
{
    /// <summary>
    /// <c>InstList_EnemyProjectile_YappingMawsBody_FacingDown</c> at $86:EC56.
    /// </summary>
    internal const ushort FacingDown = 0xec56;

    /// <summary>
    /// <c>InstList_EnemyProjectile_YappingMawsBody_FacingUp</c> at $86:EC5C.
    /// </summary>
    internal const ushort FacingUp = 0xec5c;

    /// <summary>Number of frame-duration and sleep-command words in the two body-pose lists.</summary>
    public static int MechanicsWordCount => 4;

    /// <summary>Number of spritemap operands selecting the down- and up-facing body poses.</summary>
    public static int PresentationWordCount => 2;

    /// <summary>$86:EC56-EC61 contains two six-byte single-pose/sleep programs, down then up.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int operation = index & 1;
        return new((ushort)(FacingDown + index / 2 * 6 + operation * 4),
            operation == 0 ? (ushort)1 : EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep);
    }

    /// <summary>Gets the bank-$86 address of a down- or up-pose spritemap operand.</summary>
    /// <param name="index">Zero-based index selecting one of the two pose presentation words.</param>
    /// <returns>The instruction-list address of the selected spritemap operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the two-pose range.</exception>
    public static ushort PresentationWordAddress(int index) => (uint)index < PresentationWordCount
        ? (ushort)(FacingDown + index * 6 + sizeof(ushort)) : throw new IndexOutOfRangeException();

    /// <summary>Resolves a frame duration or sleep opcode from either compiled body-pose list.</summary>
    /// <param name="address">Bank-$86 address of the mechanics word to resolve.</param>
    /// <returns>The duration or sleep-command value stored at that address.</returns>
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
            $"Yapping Maw body-projectile mechanics pointer $86:{address:X4} is not compiled.");
    }
}

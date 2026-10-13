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

    /// <summary>Each facing program has one duration word and one terminal sleep word.</summary>
    public static int MechanicsWordCount => 4;
    /// <summary>Each facing program has one interleaved spritemap operand.</summary>
    public static int PresentationWordCount => 2;

    /// <summary>$86:EC56-EC61 contains two six-byte single-pose/sleep programs, down then up.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int operation = index & 1;
        return new((ushort)(FacingDown + index / 2 * 6 + operation * 4),
            operation == 0 ? (ushort)1 : (ushort)EnemyProjectileInstruction.Sleep);
    }
    /// <summary>Locates the pose operand following the duration word of each facing list.</summary>
    public static ushort PresentationWordAddress(int index) => (uint)index < PresentationWordCount
        ? (ushort)(FacingDown + index * 6 + sizeof(ushort)) : throw new IndexOutOfRangeException();
    /// <summary>Reads a compiled duration or sleep word and rejects any other bank-$86 address.</summary>
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

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for an Eye Door sweat drop's falling loop and floor-impact animation.
/// Interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class EyeDoorSweatInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_EyeDoorSweat</c> at $86:B615.</summary>
    internal const ushort Initial = 0xb615;

    /// <summary><c>InstList_EnemyProjectile_EyeDoorSweat_Impact</c> at $86:B61D.</summary>
    internal const ushort Impact = 0xb61d;

    /// <summary>Number of address/value entries in the falling and impact mechanics program.</summary>
    public static int MechanicsWordCount => 8;
    /// <summary>Number of presentation operand words resolved from extracted spritemap data.</summary>
    public static int PresentationWordCount => 4;

    /// <summary>Falling draws once and loops; impact clears movement, draws three frames and deletes.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
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

    /// <summary>
    /// Maps a presentation operand's ordinal in the compiled lists to its native instruction address.
    /// </summary>
    /// <param name="index">Zero-based ordinal across the falling and impact spritemap operands.</param>
    /// <returns>The address whose value is supplied by extracted presentation artwork.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation-word list.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index == 0 ? Initial + 2 : Impact + 4 + 4 * (index - 1));
    }
    /// <summary>
    /// Resolves a compiled mechanics word by instruction address for bus-backed instruction execution.
    /// </summary>
    /// <param name="address">Instruction address to look up in the falling and impact mechanics program.</param>
    /// <returns>The compiled duration or operation word at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not one of the compiled mechanics-word addresses.</exception>
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
}

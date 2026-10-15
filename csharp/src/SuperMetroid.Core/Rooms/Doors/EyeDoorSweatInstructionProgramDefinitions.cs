using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Doors;

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

    /// <summary>The falling loop and floor impact contribute eight timing, branch, and terminal control words.</summary>
    public static int MechanicsWordCount => 8;
    /// <summary>The falling pose and three impact poses each select a spritemap.</summary>
    public static int PresentationWordCount => 4;

    /// <summary>Falling draws once and loops; impact clears movement, draws three frames and deletes.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index is >= 4 and <= 6) return new((ushort)(Impact + 2 + 4 * (index - 4)), 6);
        return index switch
        {
            0 => new(Initial, 6),
            1 => new(Initial + 4, (ushort)EnemyProjectileInstruction.GotoY),
            2 => new(Initial + 6, Initial),
            3 => new(Impact, (ushort)EnemyProjectileInstruction.ClearPreInstruction),
            _ => new(Impact + 14, (ushort)EnemyProjectileInstruction.Delete),
        };
    }

    /// <summary>Locates the falling spritemap operand or one of three consecutive impact-frame operands.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index == 0 ? Initial + 2 : Impact + 4 + 4 * (index - 1));
    }
    /// <summary>Reads only the compiled $86:B615-B62B control addresses, rejecting spritemap operands and unknown words.</summary>
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

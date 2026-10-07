namespace SuperMetroid.Core.Game;

/// <summary>One compiled pre-Phantoon room projectile word at its bank-$86 address.</summary>
internal readonly record struct PrePhantoonRoomProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for <c>InstList_EnemyProjectile_PrePhantoonRoom</c> ($86:A3AA): one
/// blank $20-frame hold, then deletion. The blank operand at $86:A3AC is a visual selector.
/// </summary>
internal static class PrePhantoonRoomProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_PrePhantoonRoom</c> at $86:A3AA.</summary>
    internal const ushort Initial = 0xa3aa;

    /// <summary>The single frame's duration word at $86:A3AA.</summary>
    private const ushort HoldFrames = 0x20;

    /// <summary><c>Instruction_EnemyProjectile_Delete</c> at $86:A3AE.</summary>
    private const ushort DeleteCommand = 0xa3ae;

    /// <summary>Bank of the program and its blank spritemap operand.</summary>
    internal const int Bank = 0x86;

    /// <summary>The blank spritemap operand at $86:A3AC.</summary>
    internal const ushort PresentationWord = 0xa3ac;

    internal static int MechanicsWordCount => 2;

    internal static PrePhantoonRoomProjectileInstructionMechanicsWord MechanicsWord(int index)
    {
        ushort address = index switch
        {
            0 => Initial,
            1 => DeleteCommand,
            _ => throw new IndexOutOfRangeException(),
        };
        return new(address, ReadMechanicsWord(address));
    }

    internal static ushort ReadMechanicsWord(ushort address) => address switch
    {
        Initial => HoldFrames,
        DeleteCommand => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
        _ => throw new InvalidDataException(
            $"Pre-Phantoon room projectile mechanics pointer $86:{address:X4} is not compiled."),
    };
}

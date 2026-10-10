namespace SuperMetroid.Core.Game;

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

    /// <summary>Resolves one control word by its byte position from <see cref="Initial"/>.</summary>
    internal static ushort ReadMechanicsWord(ushort address) => (address - Initial) switch
    {
        0 => HoldFrames,
        DeleteCommand - Initial => (ushort)EnemyProjectileInstruction.Delete,
        _ => throw new InvalidDataException(
            $"Pre-Phantoon room projectile mechanics pointer $86:{address:X4} is not compiled."),
    };
}

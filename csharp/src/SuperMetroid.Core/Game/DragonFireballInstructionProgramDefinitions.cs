namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Dragon's left/right rising and falling fireball loops.
/// Interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class DragonFireballInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Rising_Left</c> at $86:B4BF.</summary>
    internal const ushort RisingLeft = 0xb4bf;

    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Rising_Right</c> at $86:B4CB.</summary>
    internal const ushort RisingRight = 0xb4cb;

    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Falling_Left</c> at $86:B4D7.</summary>
    internal const ushort FallingLeft = 0xb4d7;

    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Falling_Right</c> at $86:B4E3.</summary>
    internal const ushort FallingRight = 0xb4e3;
    public static int PresentationWordCount => 8;

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(RisingLeft + index / 2 * 12 + index % 2 * 4 + 2);
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value)) return value;
        throw new InvalidDataException(
            $"Dragon-fireball instruction mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool TryRead(int address, out ushort value)
    {
        int offset = address - RisingLeft;
        value = 0;
        if (offset is < 0 or >= 48) return false;
        int step = offset % 12;
        value = step switch
        {
            0 or 4 => 5,
            8 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY,
            10 => (ushort)(RisingLeft + offset / 12 * 12),
            _ => 0,
        };
        return value != 0;
    }
}
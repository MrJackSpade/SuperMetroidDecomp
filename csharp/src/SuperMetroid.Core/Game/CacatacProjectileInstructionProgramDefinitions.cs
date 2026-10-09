namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for all ten Cacatac spike projectile programs.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal abstract class CacatacProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_CacatacSpike_Left_FacingUp</c> at $86:D92E.</summary>
    internal const ushort LeftFacingUp = 0xd92e;
    /// <summary><c>InstList_EnemyProjectile_CacatacSpike_UpLeft</c> at $86:D934.</summary>
    internal const ushort UpLeft = 0xd934;
    /// <summary><c>InstList_EnemyProjectile_CacatacSpike_Up</c> at $86:D93A.</summary>
    internal const ushort Up = 0xd93a;
    /// <summary><c>InstList_EnemyProjectile_CacatacSpike_UpRight</c> at $86:D940.</summary>
    internal const ushort UpRight = 0xd940;
    /// <summary><c>InstList_EnemyProjectile_CacatacSpike_Right_FacingUp</c> at $86:D946.</summary>
    internal const ushort RightFacingUp = 0xd946;
    /// <summary><c>InstList_EnemyProjectile_CacatacSpike_Left_FacingDown</c> at $86:D94C.</summary>
    internal const ushort LeftFacingDown = 0xd94c;
    /// <summary><c>InstList_EnemyProjectile_CacatacSpike_DownLeft</c> at $86:D952.</summary>
    internal const ushort DownLeft = 0xd952;
    /// <summary><c>InstList_EnemyProjectile_CacatacSpike_Down</c> at $86:D958.</summary>
    internal const ushort Down = 0xd958;
    /// <summary><c>InstList_EnemyProjectile_CacatacSpike_DownRight</c> at $86:D95E.</summary>
    internal const ushort DownRight = 0xd95e;
    /// <summary><c>InstList_EnemyProjectile_CacatacSpike_Down_FacingRight</c> at $86:D964.</summary>
    internal const ushort RightFacingDown = 0xd964;

    /// <summary>Ten six-byte direction programs: one-frame drawing followed by sleep.</summary>
    internal const int ProgramCount = 10;

    /// <summary>Gets the number of live spritemap operands interleaved with the ten direction programs.</summary>
    public static int PresentationWordCount => ProgramCount;

    /// <summary>Gets the bank-local address of a direction program's spritemap operand.</summary>
    /// <param name="index">The zero-based direction-program index.</param>
    /// <returns>The native address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index does not identify one of the ten programs.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= ProgramCount) throw new IndexOutOfRangeException();
        return (ushort)(LeftFacingUp + 6 * index + 2);
    }
    /// <summary>Reads a compiled one-tick duration or terminal sleep opcode, excluding spritemap operands.</summary>
    /// <param name="address">The bank-local instruction-word address to resolve.</param>
    /// <returns>The mechanics value stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address is outside these programs or identifies a presentation operand.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - LeftFacingUp;
        if ((uint)offset < 6 * ProgramCount)
        {
            if (offset % 6 == 0) return 1;
            if (offset % 6 == 4) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep;
        }
        throw new InvalidDataException(
            $"Cacatac spike instruction mechanics pointer $86:{address:X4} is not compiled.");
    }
}

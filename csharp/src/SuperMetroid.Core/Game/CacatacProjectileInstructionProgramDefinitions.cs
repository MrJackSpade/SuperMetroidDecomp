namespace SuperMetroid.Core.Game;

internal readonly record struct CacatacProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for all ten Cacatac spike projectile programs.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class CacatacProjectileInstructionProgramDefinitions
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
    private const int ProgramCount = 10;
    internal static int MechanicsWordCount => 2 * ProgramCount;
    internal static int PresentationWordCount => ProgramCount;

    internal static CacatacProjectileInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        bool sleep = (index & 1) != 0;
        return new((ushort)(LeftFacingUp + 6 * (index / 2) + (sleep ? 4 : 0)),
            sleep ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep : (ushort)1);
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= ProgramCount) throw new IndexOutOfRangeException();
        return (ushort)(LeftFacingUp + 6 * index + 2);
    }
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

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0x860000)
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

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

    /// <summary>
    /// Ten six-byte programs at $86:D92E-$86:D969. For program index i = 0..9,
    /// the start address is $D92E + 6*i, its duration word is exactly $0001,
    /// and its terminal instruction at start + 4 is $8159 (sleep). The
    /// intervening word at start + 2 is a live spritemap operand.
    /// Program order follows the named list pointers above; the native
    /// direction-selector table at $86:D96A uses a different order.
    /// </summary>
    private static readonly CacatacProjectileInstructionMechanicsWord[] Words =
    [
        new(LeftFacingUp, 1),
        new(LeftFacingUp + 4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(UpLeft, 1),
        new(UpLeft + 4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(Up, 1),
        new(Up + 4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(UpRight, 1),
        new(UpRight + 4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(RightFacingUp, 1),
        new(RightFacingUp + 4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(LeftFacingDown, 1),
        new(LeftFacingDown + 4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(DownLeft, 1),
        new(DownLeft + 4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(Down, 1),
        new(Down + 4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(DownRight, 1),
        new(DownRight + 4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(RightFacingDown, 1),
        new(RightFacingDown + 4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
    ];

    /// <summary>
    /// Live spritemap operands in native program order. For i = 0..9, the
    /// address is $86:D930 + 6*i and the pinned cartridge stores pointer
    /// $A908 + 7*i, ending at $86:D966 / $A947. The selector table at
    /// $86:D96A maps direction values to this physical program order.
    /// </summary>
    private static readonly ushort[] PresentationWords =
    [
        LeftFacingUp + 2,
        UpLeft + 2,
        Up + 2,
        UpRight + 2,
        RightFacingUp + 2,
        LeftFacingDown + 2,
        DownLeft + 2,
        Down + 2,
        DownRight + 2,
        RightFacingDown + 2,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static CacatacProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CacatacProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Cacatac spike instruction mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0x860000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}

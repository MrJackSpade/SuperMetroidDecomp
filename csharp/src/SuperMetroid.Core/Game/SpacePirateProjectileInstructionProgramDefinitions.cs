namespace SuperMetroid.Core.Game;

/// <summary>One compiled Space Pirate projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct SpacePirateProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the shared Pirate/Mother Brain laser and Ninja Pirate claw
/// programs. Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class SpacePirateProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_Pirate_MotherBrain_Laser_Left_0</c> at $86:9F41.</summary>
    internal const ushort LaserLeft = 0x9f41;

    /// <summary><c>InstList_EnemyProjectile_Pirate_MotherBrain_Laser_Left_1</c> at $86:9F71.</summary>
    internal const ushort LaserLeftLoop = 0x9f71;

    /// <summary><c>InstList_EnemyProjectile_Pirate_MotherBrain_Laser_Right_0</c> at $86:9F7D.</summary>
    internal const ushort LaserRight = 0x9f7d;

    /// <summary><c>InstList_EnemyProjectile_Pirate_MotherBrain_Laser_Right_1</c> at $86:9FAD.</summary>
    internal const ushort LaserRightLoop = 0x9fad;

    /// <summary><c>InstList_EnemyProjectile_PirateClaw_Left_0</c> at $86:9FB9.</summary>
    internal const ushort ClawLeft = 0x9fb9;

    /// <summary><c>InstList_EnemyProjectile_PirateClaw_Left_1</c> at $86:9FBD.</summary>
    internal const ushort ClawLeftLoop = 0x9fbd;

    /// <summary><c>InstList_EnemyProjectile_PirateClaw_Right_0</c> at $86:9FE1.</summary>
    internal const ushort ClawRight = 0x9fe1;

    /// <summary><c>InstList_EnemyProjectile_PirateClaw_Right_1</c> at $86:9FE5.</summary>
    internal const ushort ClawRightLoop = 0x9fe5;

    private static readonly SpacePirateProjectileInstructionMechanicsWord[] Words =
    [
        new(LaserLeft, 0x0002), new(0x9f45, 0x0002), new(0x9f49, 0x0002),
        new(0x9f4d, EnemyProjectileCodePointers.Instruction_PreInstructionInY_ExecuteY),
        new(0x9f4f,
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_Pirate_MotherBrain_Laser_Left),
        new(0x9f51, 0x0001), new(0x9f55, 0x0001), new(0x9f59, 0x0001),
        new(0x9f5d, 0x0001), new(0x9f61, 0x0001), new(0x9f65, 0x0001),
        new(0x9f69, 0x0001), new(0x9f6d, 0x0001),
        new(LaserLeftLoop, 0x0001), new(0x9f75, 0x0001),
        new(0x9f79, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x9f7b, LaserLeftLoop),

        new(LaserRight, 0x0002), new(0x9f81, 0x0002), new(0x9f85, 0x0002),
        new(0x9f89, EnemyProjectileCodePointers.Instruction_PreInstructionInY_ExecuteY),
        new(0x9f8b,
            EnemyProjectileCodePointers.PreInst_EnemyProjectile_Pirate_MotherBrain_Laser_Right),
        new(0x9f8d, 0x0001), new(0x9f91, 0x0001), new(0x9f95, 0x0001),
        new(0x9f99, 0x0001), new(0x9f9d, 0x0001), new(0x9fa1, 0x0001),
        new(0x9fa5, 0x0001), new(0x9fa9, 0x0001),
        new(LaserRightLoop, 0x0001), new(0x9fb1, 0x0001),
        new(0x9fb5, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x9fb7, LaserRightLoop),

        new(ClawLeft, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0x9fbb, EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_PirateClaw_Left),
        new(ClawLeftLoop, 0x0001), new(0x9fc1, 0x0001),
        new(0x9fc5, 0x0001), new(0x9fc9, 0x0001),
        new(0x9fcd, 0x0001), new(0x9fd1, 0x0001),
        new(0x9fd5, 0x0001), new(0x9fd9, 0x0001),
        new(0x9fdd, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x9fdf, ClawLeftLoop),

        new(ClawRight, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0x9fe3, EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_PirateClaw_Right),
        new(ClawRightLoop, 0x0001), new(0x9fe9, 0x0001),
        new(0x9fed, 0x0001), new(0x9ff1, 0x0001),
        new(0x9ff5, 0x0001), new(0x9ff9, 0x0001),
        new(0x9ffd, 0x0001), new(0xa001, 0x0001),
        new(0xa005, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xa007, ClawRightLoop),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x9f43, 0x9f47, 0x9f4b,
        0x9f53, 0x9f57, 0x9f5b, 0x9f5f, 0x9f63, 0x9f67, 0x9f6b, 0x9f6f,
        0x9f73, 0x9f77,
        0x9f7f, 0x9f83, 0x9f87,
        0x9f8f, 0x9f93, 0x9f97, 0x9f9b, 0x9f9f, 0x9fa3, 0x9fa7, 0x9fab,
        0x9faf, 0x9fb3,
        0x9fbf, 0x9fc3, 0x9fc7, 0x9fcb, 0x9fcf, 0x9fd3, 0x9fd7, 0x9fdb,
        0x9fe7, 0x9feb, 0x9fef, 0x9ff3, 0x9ff7, 0x9ffb, 0x9fff, 0xa003,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static SpacePirateProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.PirateMotherBrainLaser or
        RoomEnemyProjectileKind.PirateClaw;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            SpacePirateProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Space Pirate projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
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

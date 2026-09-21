namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$86 projectile-definition identities used by Work Robot fire commands.
/// The horizontal definition uses the owner's signed X velocity for both directions.
/// </summary>
internal static class WorkRobotLaserDefinitions
{
    /// <summary><c>EnemyProjectile_RobotLaser_UpLeft</c> at $86:D2A6.</summary>
    internal const ushort UpLeft = 0xd2a6;

    /// <summary><c>EnemyProjectile_RobotLaser_Horizontal</c> at $86:D2B4.</summary>
    internal const ushort Horizontal = 0xd2b4;

    /// <summary><c>EnemyProjectile_RobotLaser_DownLeft</c> at $86:D2C2.</summary>
    internal const ushort DownLeft = 0xd2c2;

    /// <summary><c>EnemyProjectile_RobotLaser_UpRight</c> at $86:D2D0.</summary>
    internal const ushort UpRight = 0xd2d0;

    /// <summary><c>EnemyProjectile_RobotLaser_DownRight</c> at $86:D2DE.</summary>
    internal const ushort DownRight = 0xd2de;
}

/// <summary>One compiled Work Robot laser mechanics word at its bank-$86 address.</summary>
internal readonly record struct WorkRobotLaserInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the shared Work Robot laser animation program.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class WorkRobotLaserInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_WreckedShipRobotLaser_0</c> at $86:D2EC.</summary>
    internal const ushort Initial = 0xd2ec;

    /// <summary><c>InstList_EnemyProjectile_WreckedShipRobotLaser_1</c> at $86:D2F8.</summary>
    internal const ushort Loop = 0xd2f8;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the laser loop at $86:D308.
    /// </summary>
    internal const ushort LoopCommand = 0xd308;

    private static readonly WorkRobotLaserInstructionMechanicsWord[] Words =
    [
        new(Initial, 0x0004),
        new(0xd2f0, 0x0004),
        new(0xd2f4, 0x0004),
        new(Loop, 0x0004),
        new(0xd2fc, 0x0004),
        new(0xd300, 0x0004),
        new(0xd304, 0x0004),
        new(LoopCommand, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xd30a, Loop),
    ];

    private static readonly ushort[] PresentationWords =
        [0xd2ee, 0xd2f2, 0xd2f6, 0xd2fa, 0xd2fe, 0xd302, 0xd306];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static WorkRobotLaserInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.WorkRobotLaserUpLeft or
        RoomEnemyProjectileKind.WorkRobotLaserHorizontal or
        RoomEnemyProjectileKind.WorkRobotLaserDownLeft or
        RoomEnemyProjectileKind.WorkRobotLaserUpRight or
        RoomEnemyProjectileKind.WorkRobotLaserDownRight;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            WorkRobotLaserInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Work Robot laser instruction mechanics pointer $86:{address:X4} is not compiled.");
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

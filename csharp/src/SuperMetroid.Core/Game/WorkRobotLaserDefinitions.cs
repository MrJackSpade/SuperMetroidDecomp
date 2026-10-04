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
/// Interleaved sprite operands select installed artwork bindings; only asset
/// import reads their native bank-$8D compositions from the cartridge.
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

    internal static int MechanicsWordCount => 9;
    internal static int PresentationWordCount => 7;

    internal static WorkRobotLaserInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        return index switch
        {
            7 => new(LoopCommand, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
            8 => new(LoopCommand + 2, Loop),
            _ => new((ushort)(Initial + 4 * index), 4),
        };
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(Initial + 4 * index + 2);
    }
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.WorkRobotLaserUpLeft or
        RoomEnemyProjectileKind.WorkRobotLaserHorizontal or
        RoomEnemyProjectileKind.WorkRobotLaserDownLeft or
        RoomEnemyProjectileKind.WorkRobotLaserUpRight or
        RoomEnemyProjectileKind.WorkRobotLaserDownRight;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address == LoopCommand)
            return EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY;
        if (address == LoopCommand + 2)
            return Loop;
        int frameOffset = address - Initial;
        if (frameOffset >= 0 && frameOffset < 28 && frameOffset % 4 == 0)
            return 4;
        throw new InvalidDataException(
            $"Work Robot laser instruction mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        int offset = unchecked((ushort)address) - Initial;
        return offset >= 0 && offset < 28 && offset % 4 < 2 || offset >= 28 && offset < 32;
    }
}

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

/// <summary>
/// Compiled control for the shared Work Robot laser animation program.
/// Interleaved sprite operands select installed artwork bindings; only asset
/// import reads their native bank-$8D compositions from the cartridge.
/// </summary>
internal abstract class WorkRobotLaserInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_WreckedShipRobotLaser_0</c> at $86:D2EC.</summary>
    internal const ushort Initial = 0xd2ec;

    /// <summary><c>InstList_EnemyProjectile_WreckedShipRobotLaser_1</c> at $86:D2F8.</summary>
    internal const ushort Loop = 0xd2f8;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the laser loop at $86:D308.
    /// </summary>
    internal const ushort LoopCommand = 0xd308;

    /// <summary>Number of interleaved sprite operands in the shared animation program.</summary>
    public static int PresentationWordCount => 7;

    /// <summary>Returns the bank-$86 address of one editable sprite operand.</summary>
    /// <param name="index">Zero-based presentation slot index.</param>
    /// <returns>Address of the selected spritemap word.</returns>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(Initial + 4 * index + 2);
    }
    /// <summary>Reports whether a projectile kind uses the shared Work Robot laser program.</summary>
    /// <param name="kind">Projectile kind to classify.</param>
    /// <returns><see langword="true"/> for one of the five directional Work Robot lasers.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.WorkRobotLaserUpLeft or
        RoomEnemyProjectileKind.WorkRobotLaserHorizontal or
        RoomEnemyProjectileKind.WorkRobotLaserDownLeft or
        RoomEnemyProjectileKind.WorkRobotLaserUpRight or
        RoomEnemyProjectileKind.WorkRobotLaserDownRight;

    /// <summary>Resolves a compiled duration or loop instruction at its native address.</summary>
    /// <param name="address">Bank-relative instruction-stream address.</param>
    /// <returns>The fixed duration or loop operand stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address does not identify a compiled mechanics word.</exception>
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
}

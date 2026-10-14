using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.WorkRobots;

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
    /// <summary>Three startup and four loop frames whose spritemaps are installed separately from laser timing.</summary>
    public static int PresentationWordCount => 7;

    /// <summary>Finds the visual operand following a four-tick frame word in the bank-$86 laser program.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(Initial + 4 * index + 2);
    }
    /// <summary>Admits the five directional robot-laser definitions that share this animation program.</summary>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.WorkRobotLaserUpLeft or
        RoomEnemyProjectileKind.WorkRobotLaserHorizontal or
        RoomEnemyProjectileKind.WorkRobotLaserDownLeft or
        RoomEnemyProjectileKind.WorkRobotLaserUpRight or
        RoomEnemyProjectileKind.WorkRobotLaserDownRight;

    /// <summary>Returns four-tick frame waits and the loop goto while rejecting visual operands and unknown addresses.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address == LoopCommand)
            return (ushort)EnemyProjectileInstruction.GotoY;
        if (address == LoopCommand + 2)
            return Loop;
        int frameOffset = address - Initial;
        if (frameOffset >= 0 && frameOffset < 28 && frameOffset % 4 == 0)
            return 4;
        throw new InvalidDataException(
            $"Work Robot laser instruction mechanics pointer $86:{address:X4} is not compiled.");
    }
}

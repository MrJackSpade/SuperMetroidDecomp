using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// Verification's native view of the WorkRobots identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class WorkRobotsNativeWords
{
    /// <summary>The routine word naming each <see cref="WorkRobotLaser"/>.</summary>
    internal static readonly NativeWordIdentities<WorkRobotLaser> WorkRobotLasers = new(new Dictionary<WorkRobotLaser, ushort>
    {
        [WorkRobotLaser.UpLeft] = 0xd2a6,
        [WorkRobotLaser.Horizontal] = 0xd2b4,
        [WorkRobotLaser.DownLeft] = 0xd2c2,
        [WorkRobotLaser.UpRight] = 0xd2d0,
        [WorkRobotLaser.DownRight] = 0xd2de,
    });
}

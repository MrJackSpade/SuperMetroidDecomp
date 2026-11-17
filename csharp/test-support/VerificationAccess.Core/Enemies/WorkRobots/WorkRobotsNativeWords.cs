using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// Verification's native view of the WorkRobots identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class WorkRobotsNativeWords
{
    /// <summary>
    /// The <see cref="WorkRobotLaser"/> each laser definition word names: a laser flies the way its robot faces, so the
    /// left and right definitions of a laser name one member (#2278).
    /// </summary>
    internal static readonly NativeWordIdentities<WorkRobotLaser> WorkRobotLasers = new(new Dictionary<ushort, WorkRobotLaser>
    {
        [0xd2a6] = WorkRobotLaser.Upward,
        [0xd2b4] = WorkRobotLaser.Horizontal,
        [0xd2c2] = WorkRobotLaser.Downward,
        [0xd2d0] = WorkRobotLaser.Upward,
        [0xd2de] = WorkRobotLaser.Downward,
    });
}

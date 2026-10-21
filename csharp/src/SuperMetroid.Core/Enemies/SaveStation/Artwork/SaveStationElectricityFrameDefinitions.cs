using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.SaveStation.Artwork;

/// <summary>
/// The presentation boundary of the save-station electricity's programs (#1296): the projectile frame each
/// <see cref="SaveStationElectricityFrame"/> pose shows.
/// </summary>
internal static class SaveStationElectricityFrameDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<SaveStationElectricityFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<SaveStationElectricityFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.SaveStationElectricityFrame00,
            [new(1)] = EnemyProjectileFrameId.SaveStationElectricityFrame01,
            [new(2)] = EnemyProjectileFrameId.SaveStationElectricityFrame02,
            [new(3)] = EnemyProjectileFrameId.SaveStationElectricityFrame03,
            [new(4)] = EnemyProjectileFrameId.SaveStationElectricityFrame04,
            [new(5)] = EnemyProjectileFrameId.SaveStationElectricityFrame05,
            [new(6)] = EnemyProjectileFrameId.SaveStationElectricityFrame06,
            [new(7)] = EnemyProjectileFrameId.SaveStationElectricityFrame07,
        };
}

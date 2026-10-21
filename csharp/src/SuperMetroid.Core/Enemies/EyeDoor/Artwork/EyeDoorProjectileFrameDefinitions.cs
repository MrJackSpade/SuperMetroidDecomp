using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor.Artwork;

/// <summary>
/// The presentation boundary of the eye door's projectile programs (#641): the projectile frame each
/// <see cref="EyeDoorProjectileFrame"/> pose shows.
/// </summary>
internal static class EyeDoorProjectileFrameDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<EyeDoorProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<EyeDoorProjectileFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.EyeDoorFrame00,
            [new(1)] = EnemyProjectileFrameId.EyeDoorFrame01,
            [new(2)] = EnemyProjectileFrameId.EyeDoorFrame02,
            [new(3)] = EnemyProjectileFrameId.EyeDoorFrame03,
            [new(4)] = EnemyProjectileFrameId.EyeDoorFrame04,
            [new(5)] = EnemyProjectileFrameId.EyeDoorFrame05,
            [new(6)] = EnemyProjectileFrameId.EyeDoorFrame06,
            [new(7)] = EnemyProjectileFrameId.EyeDoorFrame07,
            [new(8)] = EnemyProjectileFrameId.EyeDoorFrame08,
            [new(9)] = EnemyProjectileFrameId.EyeDoorFrame09,
            [new(10)] = EnemyProjectileFrameId.EyeDoorFrame10,
        };
}

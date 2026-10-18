using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Rooms.Doors.Artwork;

/// <summary>
/// The presentation boundary of the eye-door sweat drop's programs (#1296): the projectile frame each
/// <see cref="EyeDoorSweatFrame"/> pose shows.
/// </summary>
internal static class EyeDoorSweatFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 4;

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<EyeDoorSweatFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<EyeDoorSweatFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.EyeDoorSweatFrame00,
            [new(1)] = EnemyProjectileFrameId.EyeDoorSweatFrame01,
            [new(2)] = EnemyProjectileFrameId.EyeDoorSweatFrame02,
            [new(3)] = EnemyProjectileFrameId.EyeDoorSweatFrame03,
        };
}

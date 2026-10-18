using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.YappingMaws.Artwork;

/// <summary>
/// The presentation boundary of the Yapping Maw's body links programs (#641): the projectile frame each
/// <see cref="YappingMawBodyProjectileFrame"/> pose shows.
/// </summary>
internal static class YappingMawBodyProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 2;

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<YappingMawBodyProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<YappingMawBodyProjectileFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.YappingMawFrame00,
            [new(1)] = EnemyProjectileFrameId.YappingMawFrame01,
        };
}

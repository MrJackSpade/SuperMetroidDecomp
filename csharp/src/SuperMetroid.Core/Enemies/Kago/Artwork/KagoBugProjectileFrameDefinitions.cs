using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Kago.Artwork;

/// <summary>
/// The presentation boundary of the Kago's bugs programs (#641): the projectile frame each
/// <see cref="KagoBugProjectileFrame"/> pose shows.
/// </summary>
internal static class KagoBugProjectileFrameDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<KagoBugProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<KagoBugProjectileFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.KagoBugFrame00,
            [new(1)] = EnemyProjectileFrameId.KagoBugFrame01,
            [new(2)] = EnemyProjectileFrameId.KagoBugFrame02,
            [new(3)] = EnemyProjectileFrameId.KagoBugFrame03,
            [new(4)] = EnemyProjectileFrameId.KagoBugFrame04,
            [new(5)] = EnemyProjectileFrameId.KagoBugFrame05,
            [new(6)] = EnemyProjectileFrameId.KagoBugFrame06,
            [new(7)] = EnemyProjectileFrameId.KagoBugFrame07,
            [new(8)] = EnemyProjectileFrameId.KagoBugFrame08,
            [new(9)] = EnemyProjectileFrameId.KagoBugFrame09,
            [new(10)] = EnemyProjectileFrameId.KagoBugFrame10,
        };
}

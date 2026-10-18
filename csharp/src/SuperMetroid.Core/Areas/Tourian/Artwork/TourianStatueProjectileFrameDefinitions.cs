using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>
/// The presentation boundary of the statue projectile programs (#641): the projectile frame each
/// <see cref="TourianStatueProjectileFrame"/> pose shows.
/// </summary>
internal static class TourianStatueProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 28;

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<TourianStatueProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<TourianStatueProjectileFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.TourianStatueFrame00,
            [new(1)] = EnemyProjectileFrameId.TourianStatueFrame01,
            [new(2)] = EnemyProjectileFrameId.TourianStatueFrame02,
            [new(3)] = EnemyProjectileFrameId.TourianStatueFrame03,
            [new(4)] = EnemyProjectileFrameId.TourianStatueFrame04,
            [new(5)] = EnemyProjectileFrameId.TourianStatueFrame05,
            [new(6)] = EnemyProjectileFrameId.TourianStatueFrame06,
            [new(7)] = EnemyProjectileFrameId.TourianStatueFrame07,
            [new(8)] = EnemyProjectileFrameId.TourianStatueFrame08,
            [new(9)] = EnemyProjectileFrameId.TourianStatueFrame09,
            [new(10)] = EnemyProjectileFrameId.TourianStatueFrame10,
            [new(11)] = EnemyProjectileFrameId.TourianStatueFrame11,
            [new(12)] = EnemyProjectileFrameId.TourianStatueFrame12,
            [new(13)] = EnemyProjectileFrameId.TourianStatueFrame13,
            [new(14)] = EnemyProjectileFrameId.TourianStatueFrame14,
            [new(15)] = EnemyProjectileFrameId.TourianStatueFrame15,
            [new(16)] = EnemyProjectileFrameId.TourianStatueFrame16,
            [new(17)] = EnemyProjectileFrameId.TourianStatueFrame17,
            [new(18)] = EnemyProjectileFrameId.TourianStatueFrame18,
            [new(19)] = EnemyProjectileFrameId.TourianStatueFrame19,
            [new(20)] = EnemyProjectileFrameId.TourianStatueFrame20,
            [new(21)] = EnemyProjectileFrameId.TourianStatueFrame21,
            [new(22)] = EnemyProjectileFrameId.TourianStatueFrame22,
            [new(23)] = EnemyProjectileFrameId.TourianStatueFrame23,
            [new(24)] = EnemyProjectileFrameId.TourianStatueFrame24,
            [new(25)] = EnemyProjectileFrameId.TourianStatueFrame25,
            [new(26)] = EnemyProjectileFrameId.TourianStatueFrame26,
            [new(27)] = EnemyProjectileFrameId.TourianStatueFrame27,
        };
}

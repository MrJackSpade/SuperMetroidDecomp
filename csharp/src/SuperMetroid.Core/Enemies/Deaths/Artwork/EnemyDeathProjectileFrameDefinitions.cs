using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths.Artwork;

/// <summary>
/// The presentation boundary of an enemy's death explosion and drop programs (#641): the projectile frame each
/// <see cref="EnemyDeathProjectileFrame"/> pose shows.
/// </summary>
internal static class EnemyDeathProjectileFrameDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<EnemyDeathProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<EnemyDeathProjectileFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.EnemyDeathFrame00,
            [new(1)] = EnemyProjectileFrameId.EnemyDeathFrame01,
            [new(2)] = EnemyProjectileFrameId.EnemyDeathFrame02,
            [new(3)] = EnemyProjectileFrameId.EnemyDeathFrame03,
            [new(4)] = EnemyProjectileFrameId.EnemyDeathFrame04,
            [new(5)] = EnemyProjectileFrameId.EnemyDeathFrame05,
            [new(6)] = EnemyProjectileFrameId.EnemyDeathFrame06,
            [new(7)] = EnemyProjectileFrameId.EnemyDeathFrame07,
            [new(8)] = EnemyProjectileFrameId.EnemyDeathFrame08,
            [new(9)] = EnemyProjectileFrameId.EnemyDeathFrame09,
            [new(10)] = EnemyProjectileFrameId.EnemyDeathFrame10,
            [new(11)] = EnemyProjectileFrameId.EnemyDeathFrame11,
            [new(12)] = EnemyProjectileFrameId.EnemyDeathFrame12,
            [new(13)] = EnemyProjectileFrameId.EnemyDeathFrame13,
            [new(14)] = EnemyProjectileFrameId.EnemyDeathFrame14,
            [new(15)] = EnemyProjectileFrameId.EnemyDeathFrame15,
            [new(16)] = EnemyProjectileFrameId.EnemyDeathFrame16,
            [new(17)] = EnemyProjectileFrameId.EnemyDeathFrame17,
            [new(18)] = EnemyProjectileFrameId.EnemyDeathFrame18,
            [new(19)] = EnemyProjectileFrameId.EnemyDeathFrame19,
            [new(20)] = EnemyProjectileFrameId.EnemyDeathFrame20,
            [new(21)] = EnemyProjectileFrameId.EnemyDeathFrame21,
            [new(22)] = EnemyProjectileFrameId.EnemyDeathFrame22,
            [new(23)] = EnemyProjectileFrameId.EnemyDeathFrame23,
            [new(24)] = EnemyProjectileFrameId.EnemyDeathFrame24,
            [new(25)] = EnemyProjectileFrameId.EnemyDeathFrame25,
            [new(26)] = EnemyProjectileFrameId.EnemyDeathFrame26,
            [new(27)] = EnemyProjectileFrameId.EnemyDeathFrame27,
            [new(28)] = EnemyProjectileFrameId.EnemyDeathFrame28,
            [new(29)] = EnemyProjectileFrameId.EnemyDeathFrame29,
            [new(30)] = EnemyProjectileFrameId.EnemyDeathFrame30,
            [new(31)] = EnemyProjectileFrameId.EnemyPickupFrame00,
            [new(32)] = EnemyProjectileFrameId.EnemyPickupFrame01,
            [new(33)] = EnemyProjectileFrameId.EnemyPickupFrame02,
            [new(34)] = EnemyProjectileFrameId.EnemyPickupFrame03,
            [new(35)] = EnemyProjectileFrameId.EnemyPickupFrame04,
            [new(36)] = EnemyProjectileFrameId.EnemyPickupFrame05,
            [new(37)] = EnemyProjectileFrameId.EnemyPickupFrame06,
            [new(38)] = EnemyProjectileFrameId.EnemyPickupFrame07,
            [new(39)] = EnemyProjectileFrameId.EnemyPickupFrame08,
            [new(40)] = EnemyProjectileFrameId.EnemyPickupFrame09,
            [new(41)] = EnemyProjectileFrameId.EnemyPickupFrame10,
            [new(42)] = EnemyProjectileFrameId.EnemyPickupFrame11,
            [new(43)] = EnemyProjectileFrameId.EnemyPickupFrame12,
            [new(44)] = EnemyProjectileFrameId.EnemyPickupFrame13,
            [new(45)] = EnemyProjectileFrameId.EnemyPickupFrame14,
            [new(46)] = EnemyProjectileFrameId.EnemyPickupFrame15,
        };
}

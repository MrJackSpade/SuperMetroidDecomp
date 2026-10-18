using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates.Artwork;

/// <summary>
/// The presentation boundary of the laser and claw programs (#641): the projectile frame each
/// <see cref="SpacePirateProjectileFrame"/> pose shows.
/// </summary>
internal static class SpacePirateProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 42;

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<SpacePirateProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<SpacePirateProjectileFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.SpacePirateFrame00,
            [new(1)] = EnemyProjectileFrameId.SpacePirateFrame01,
            [new(2)] = EnemyProjectileFrameId.SpacePirateFrame02,
            [new(3)] = EnemyProjectileFrameId.SpacePirateFrame03,
            [new(4)] = EnemyProjectileFrameId.SpacePirateFrame04,
            [new(5)] = EnemyProjectileFrameId.SpacePirateFrame05,
            [new(6)] = EnemyProjectileFrameId.SpacePirateFrame06,
            [new(7)] = EnemyProjectileFrameId.SpacePirateFrame07,
            [new(8)] = EnemyProjectileFrameId.SpacePirateFrame08,
            [new(9)] = EnemyProjectileFrameId.SpacePirateFrame09,
            [new(10)] = EnemyProjectileFrameId.SpacePirateFrame10,
            [new(11)] = EnemyProjectileFrameId.SpacePirateFrame11,
            [new(12)] = EnemyProjectileFrameId.SpacePirateFrame12,
            [new(13)] = EnemyProjectileFrameId.SpacePirateFrame13,
            [new(14)] = EnemyProjectileFrameId.SpacePirateFrame14,
            [new(15)] = EnemyProjectileFrameId.SpacePirateFrame15,
            [new(16)] = EnemyProjectileFrameId.SpacePirateFrame16,
            [new(17)] = EnemyProjectileFrameId.SpacePirateFrame17,
            [new(18)] = EnemyProjectileFrameId.SpacePirateFrame18,
            [new(19)] = EnemyProjectileFrameId.SpacePirateFrame19,
            [new(20)] = EnemyProjectileFrameId.SpacePirateFrame20,
            [new(21)] = EnemyProjectileFrameId.SpacePirateFrame21,
            [new(22)] = EnemyProjectileFrameId.SpacePirateFrame22,
            [new(23)] = EnemyProjectileFrameId.SpacePirateFrame23,
            [new(24)] = EnemyProjectileFrameId.SpacePirateFrame24,
            [new(25)] = EnemyProjectileFrameId.SpacePirateFrame25,
            [new(26)] = EnemyProjectileFrameId.SpacePirateFrame26,
            [new(27)] = EnemyProjectileFrameId.SpacePirateFrame27,
            [new(28)] = EnemyProjectileFrameId.SpacePirateFrame28,
            [new(29)] = EnemyProjectileFrameId.SpacePirateFrame29,
            [new(30)] = EnemyProjectileFrameId.SpacePirateFrame30,
            [new(31)] = EnemyProjectileFrameId.SpacePirateFrame31,
            [new(32)] = EnemyProjectileFrameId.SpacePirateFrame32,
            [new(33)] = EnemyProjectileFrameId.SpacePirateFrame33,
            [new(34)] = EnemyProjectileFrameId.SpacePirateFrame34,
            [new(35)] = EnemyProjectileFrameId.SpacePirateFrame35,
            [new(36)] = EnemyProjectileFrameId.SpacePirateFrame36,
            [new(37)] = EnemyProjectileFrameId.SpacePirateFrame37,
            [new(38)] = EnemyProjectileFrameId.SpacePirateFrame38,
            [new(39)] = EnemyProjectileFrameId.SpacePirateFrame39,
            [new(40)] = EnemyProjectileFrameId.SpacePirateFrame40,
            [new(41)] = EnemyProjectileFrameId.SpacePirateFrame41,
        };
}

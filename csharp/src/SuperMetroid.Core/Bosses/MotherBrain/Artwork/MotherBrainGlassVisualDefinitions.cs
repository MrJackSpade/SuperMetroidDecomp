using SuperMetroid.Core.Enemies.Projectiles;


namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of Mother Brain's glass (#641): the projectile frame every typed
/// frame shows.
/// </summary>
internal static class MotherBrainGlassVisualDefinitions
{
    /// <summary>Distinct frames: eight per shard group, then the sparkle's four.</summary>
    internal const int ProjectileFrameCount = 8 * MotherBrainGlassAnimations.ShardPhaseCount + MotherBrainGlassAnimations.SparklePhaseCount;

    /// <summary>Every frame in installed-artwork order: the shard groups in turn, then the sparkle.</summary>
    internal static IReadOnlyList<MotherBrainGlassFrame> ProjectileFrames { get; } =
    [
        .. Enum.GetValues<MotherBrainGlassProgram>().Where(program => program != MotherBrainGlassProgram.Sparkle)
            .SelectMany(program => Enumerable.Range(0, MotherBrainGlassAnimations.ShardPhaseCount)
                .Select(phase => new MotherBrainGlassFrame(program, phase))),
        .. Enumerable.Range(0, MotherBrainGlassAnimations.SparklePhaseCount)
            .Select(phase => new MotherBrainGlassFrame(MotherBrainGlassProgram.Sparkle, phase)),
    ];

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<MotherBrainGlassFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<MotherBrainGlassFrame, EnemyProjectileFrameId>
        {
            [new(MotherBrainGlassProgram.ShardGroup0, 0)] = EnemyProjectileFrameId.MotherBrainGlassFrame00,
            [new(MotherBrainGlassProgram.ShardGroup0, 1)] = EnemyProjectileFrameId.MotherBrainGlassFrame01,
            [new(MotherBrainGlassProgram.ShardGroup0, 2)] = EnemyProjectileFrameId.MotherBrainGlassFrame02,
            [new(MotherBrainGlassProgram.ShardGroup0, 3)] = EnemyProjectileFrameId.MotherBrainGlassFrame03,
            [new(MotherBrainGlassProgram.ShardGroup0, 4)] = EnemyProjectileFrameId.MotherBrainGlassFrame04,
            [new(MotherBrainGlassProgram.ShardGroup0, 5)] = EnemyProjectileFrameId.MotherBrainGlassFrame05,
            [new(MotherBrainGlassProgram.ShardGroup0, 6)] = EnemyProjectileFrameId.MotherBrainGlassFrame06,
            [new(MotherBrainGlassProgram.ShardGroup0, 7)] = EnemyProjectileFrameId.MotherBrainGlassFrame07,
            [new(MotherBrainGlassProgram.ShardGroup1, 0)] = EnemyProjectileFrameId.MotherBrainGlassFrame08,
            [new(MotherBrainGlassProgram.ShardGroup1, 1)] = EnemyProjectileFrameId.MotherBrainGlassFrame09,
            [new(MotherBrainGlassProgram.ShardGroup1, 2)] = EnemyProjectileFrameId.MotherBrainGlassFrame10,
            [new(MotherBrainGlassProgram.ShardGroup1, 3)] = EnemyProjectileFrameId.MotherBrainGlassFrame11,
            [new(MotherBrainGlassProgram.ShardGroup1, 4)] = EnemyProjectileFrameId.MotherBrainGlassFrame12,
            [new(MotherBrainGlassProgram.ShardGroup1, 5)] = EnemyProjectileFrameId.MotherBrainGlassFrame13,
            [new(MotherBrainGlassProgram.ShardGroup1, 6)] = EnemyProjectileFrameId.MotherBrainGlassFrame14,
            [new(MotherBrainGlassProgram.ShardGroup1, 7)] = EnemyProjectileFrameId.MotherBrainGlassFrame15,
            [new(MotherBrainGlassProgram.ShardGroup2, 0)] = EnemyProjectileFrameId.MotherBrainGlassFrame16,
            [new(MotherBrainGlassProgram.ShardGroup2, 1)] = EnemyProjectileFrameId.MotherBrainGlassFrame17,
            [new(MotherBrainGlassProgram.ShardGroup2, 2)] = EnemyProjectileFrameId.MotherBrainGlassFrame18,
            [new(MotherBrainGlassProgram.ShardGroup2, 3)] = EnemyProjectileFrameId.MotherBrainGlassFrame19,
            [new(MotherBrainGlassProgram.ShardGroup2, 4)] = EnemyProjectileFrameId.MotherBrainGlassFrame20,
            [new(MotherBrainGlassProgram.ShardGroup2, 5)] = EnemyProjectileFrameId.MotherBrainGlassFrame21,
            [new(MotherBrainGlassProgram.ShardGroup2, 6)] = EnemyProjectileFrameId.MotherBrainGlassFrame22,
            [new(MotherBrainGlassProgram.ShardGroup2, 7)] = EnemyProjectileFrameId.MotherBrainGlassFrame23,
            [new(MotherBrainGlassProgram.ShardGroup3, 0)] = EnemyProjectileFrameId.MotherBrainGlassFrame24,
            [new(MotherBrainGlassProgram.ShardGroup3, 1)] = EnemyProjectileFrameId.MotherBrainGlassFrame25,
            [new(MotherBrainGlassProgram.ShardGroup3, 2)] = EnemyProjectileFrameId.MotherBrainGlassFrame26,
            [new(MotherBrainGlassProgram.ShardGroup3, 3)] = EnemyProjectileFrameId.MotherBrainGlassFrame27,
            [new(MotherBrainGlassProgram.ShardGroup3, 4)] = EnemyProjectileFrameId.MotherBrainGlassFrame28,
            [new(MotherBrainGlassProgram.ShardGroup3, 5)] = EnemyProjectileFrameId.MotherBrainGlassFrame29,
            [new(MotherBrainGlassProgram.ShardGroup3, 6)] = EnemyProjectileFrameId.MotherBrainGlassFrame30,
            [new(MotherBrainGlassProgram.ShardGroup3, 7)] = EnemyProjectileFrameId.MotherBrainGlassFrame31,
            [new(MotherBrainGlassProgram.ShardGroup4, 0)] = EnemyProjectileFrameId.MotherBrainGlassFrame32,
            [new(MotherBrainGlassProgram.ShardGroup4, 1)] = EnemyProjectileFrameId.MotherBrainGlassFrame33,
            [new(MotherBrainGlassProgram.ShardGroup4, 2)] = EnemyProjectileFrameId.MotherBrainGlassFrame34,
            [new(MotherBrainGlassProgram.ShardGroup4, 3)] = EnemyProjectileFrameId.MotherBrainGlassFrame35,
            [new(MotherBrainGlassProgram.ShardGroup4, 4)] = EnemyProjectileFrameId.MotherBrainGlassFrame36,
            [new(MotherBrainGlassProgram.ShardGroup4, 5)] = EnemyProjectileFrameId.MotherBrainGlassFrame37,
            [new(MotherBrainGlassProgram.ShardGroup4, 6)] = EnemyProjectileFrameId.MotherBrainGlassFrame38,
            [new(MotherBrainGlassProgram.ShardGroup4, 7)] = EnemyProjectileFrameId.MotherBrainGlassFrame39,
            [new(MotherBrainGlassProgram.ShardGroup5, 0)] = EnemyProjectileFrameId.MotherBrainGlassFrame40,
            [new(MotherBrainGlassProgram.ShardGroup5, 1)] = EnemyProjectileFrameId.MotherBrainGlassFrame41,
            [new(MotherBrainGlassProgram.ShardGroup5, 2)] = EnemyProjectileFrameId.MotherBrainGlassFrame42,
            [new(MotherBrainGlassProgram.ShardGroup5, 3)] = EnemyProjectileFrameId.MotherBrainGlassFrame43,
            [new(MotherBrainGlassProgram.ShardGroup5, 4)] = EnemyProjectileFrameId.MotherBrainGlassFrame44,
            [new(MotherBrainGlassProgram.ShardGroup5, 5)] = EnemyProjectileFrameId.MotherBrainGlassFrame45,
            [new(MotherBrainGlassProgram.ShardGroup5, 6)] = EnemyProjectileFrameId.MotherBrainGlassFrame46,
            [new(MotherBrainGlassProgram.ShardGroup5, 7)] = EnemyProjectileFrameId.MotherBrainGlassFrame47,
            [new(MotherBrainGlassProgram.ShardGroup6, 0)] = EnemyProjectileFrameId.MotherBrainGlassFrame48,
            [new(MotherBrainGlassProgram.ShardGroup6, 1)] = EnemyProjectileFrameId.MotherBrainGlassFrame49,
            [new(MotherBrainGlassProgram.ShardGroup6, 2)] = EnemyProjectileFrameId.MotherBrainGlassFrame50,
            [new(MotherBrainGlassProgram.ShardGroup6, 3)] = EnemyProjectileFrameId.MotherBrainGlassFrame51,
            [new(MotherBrainGlassProgram.ShardGroup6, 4)] = EnemyProjectileFrameId.MotherBrainGlassFrame52,
            [new(MotherBrainGlassProgram.ShardGroup6, 5)] = EnemyProjectileFrameId.MotherBrainGlassFrame53,
            [new(MotherBrainGlassProgram.ShardGroup6, 6)] = EnemyProjectileFrameId.MotherBrainGlassFrame54,
            [new(MotherBrainGlassProgram.ShardGroup6, 7)] = EnemyProjectileFrameId.MotherBrainGlassFrame55,
            [new(MotherBrainGlassProgram.ShardGroup7, 0)] = EnemyProjectileFrameId.MotherBrainGlassFrame56,
            [new(MotherBrainGlassProgram.ShardGroup7, 1)] = EnemyProjectileFrameId.MotherBrainGlassFrame57,
            [new(MotherBrainGlassProgram.ShardGroup7, 2)] = EnemyProjectileFrameId.MotherBrainGlassFrame58,
            [new(MotherBrainGlassProgram.ShardGroup7, 3)] = EnemyProjectileFrameId.MotherBrainGlassFrame59,
            [new(MotherBrainGlassProgram.ShardGroup7, 4)] = EnemyProjectileFrameId.MotherBrainGlassFrame60,
            [new(MotherBrainGlassProgram.ShardGroup7, 5)] = EnemyProjectileFrameId.MotherBrainGlassFrame61,
            [new(MotherBrainGlassProgram.ShardGroup7, 6)] = EnemyProjectileFrameId.MotherBrainGlassFrame62,
            [new(MotherBrainGlassProgram.ShardGroup7, 7)] = EnemyProjectileFrameId.MotherBrainGlassFrame63,
            [new(MotherBrainGlassProgram.Sparkle, 0)] = EnemyProjectileFrameId.MotherBrainGlassFrame64,
            [new(MotherBrainGlassProgram.Sparkle, 1)] = EnemyProjectileFrameId.MotherBrainGlassFrame65,
            [new(MotherBrainGlassProgram.Sparkle, 2)] = EnemyProjectileFrameId.MotherBrainGlassFrame66,
            [new(MotherBrainGlassProgram.Sparkle, 3)] = EnemyProjectileFrameId.MotherBrainGlassFrame67,
        };
}

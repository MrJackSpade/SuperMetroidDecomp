using SuperMetroid.Core.Enemies.Projectiles;


namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of Mother Brain's glass (#641): the projectile frame every typed
/// frame shows.
/// </summary>
internal static class MotherBrainGlassVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<MotherBrainGlassFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<MotherBrainGlassFrame, EnemyProjectileFrameId>
        {
            [new(MotherBrainGlassProgram.ShardGroup0, 0)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupFirst,
            [new(MotherBrainGlassProgram.ShardGroup0, 1)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupSecond,
            [new(MotherBrainGlassProgram.ShardGroup0, 2)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupThird,
            [new(MotherBrainGlassProgram.ShardGroup0, 3)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupFourth,
            [new(MotherBrainGlassProgram.ShardGroup0, 4)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupFifth,
            [new(MotherBrainGlassProgram.ShardGroup0, 5)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupSixth,
            [new(MotherBrainGlassProgram.ShardGroup0, 6)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupSeventh,
            [new(MotherBrainGlassProgram.ShardGroup0, 7)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupEighth,
            [new(MotherBrainGlassProgram.ShardGroup1, 0)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupFirst,
            [new(MotherBrainGlassProgram.ShardGroup1, 1)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupSecond,
            [new(MotherBrainGlassProgram.ShardGroup1, 2)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupThird,
            [new(MotherBrainGlassProgram.ShardGroup1, 3)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupFourth,
            [new(MotherBrainGlassProgram.ShardGroup1, 4)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupFifth,
            [new(MotherBrainGlassProgram.ShardGroup1, 5)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupSixth,
            [new(MotherBrainGlassProgram.ShardGroup1, 6)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupSeventh,
            [new(MotherBrainGlassProgram.ShardGroup1, 7)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupEighth,
            [new(MotherBrainGlassProgram.ShardGroup2, 0)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupFirst,
            [new(MotherBrainGlassProgram.ShardGroup2, 1)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupSecond,
            [new(MotherBrainGlassProgram.ShardGroup2, 2)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupThird,
            [new(MotherBrainGlassProgram.ShardGroup2, 3)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupFourth,
            [new(MotherBrainGlassProgram.ShardGroup2, 4)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupFifth,
            [new(MotherBrainGlassProgram.ShardGroup2, 5)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupSixth,
            [new(MotherBrainGlassProgram.ShardGroup2, 6)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupSeventh,
            [new(MotherBrainGlassProgram.ShardGroup2, 7)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupEighth,
            [new(MotherBrainGlassProgram.ShardGroup3, 0)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupFirst,
            [new(MotherBrainGlassProgram.ShardGroup3, 1)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupSecond,
            [new(MotherBrainGlassProgram.ShardGroup3, 2)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupThird,
            [new(MotherBrainGlassProgram.ShardGroup3, 3)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupFourth,
            [new(MotherBrainGlassProgram.ShardGroup3, 4)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupFifth,
            [new(MotherBrainGlassProgram.ShardGroup3, 5)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupSixth,
            [new(MotherBrainGlassProgram.ShardGroup3, 6)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupSeventh,
            [new(MotherBrainGlassProgram.ShardGroup3, 7)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupEighth,
            [new(MotherBrainGlassProgram.ShardGroup4, 0)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupFirst,
            [new(MotherBrainGlassProgram.ShardGroup4, 1)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupSecond,
            [new(MotherBrainGlassProgram.ShardGroup4, 2)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupThird,
            [new(MotherBrainGlassProgram.ShardGroup4, 3)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupFourth,
            [new(MotherBrainGlassProgram.ShardGroup4, 4)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupFifth,
            [new(MotherBrainGlassProgram.ShardGroup4, 5)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupSixth,
            [new(MotherBrainGlassProgram.ShardGroup4, 6)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupSeventh,
            [new(MotherBrainGlassProgram.ShardGroup4, 7)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupEighth,
            [new(MotherBrainGlassProgram.ShardGroup5, 0)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupFirst,
            [new(MotherBrainGlassProgram.ShardGroup5, 1)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupSecond,
            [new(MotherBrainGlassProgram.ShardGroup5, 2)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupThird,
            [new(MotherBrainGlassProgram.ShardGroup5, 3)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupFourth,
            [new(MotherBrainGlassProgram.ShardGroup5, 4)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupFifth,
            [new(MotherBrainGlassProgram.ShardGroup5, 5)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupSixth,
            [new(MotherBrainGlassProgram.ShardGroup5, 6)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupSeventh,
            [new(MotherBrainGlassProgram.ShardGroup5, 7)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupEighth,
            [new(MotherBrainGlassProgram.ShardGroup6, 0)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupFirst,
            [new(MotherBrainGlassProgram.ShardGroup6, 1)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupSecond,
            [new(MotherBrainGlassProgram.ShardGroup6, 2)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupThird,
            [new(MotherBrainGlassProgram.ShardGroup6, 3)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupFourth,
            [new(MotherBrainGlassProgram.ShardGroup6, 4)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupFifth,
            [new(MotherBrainGlassProgram.ShardGroup6, 5)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupSixth,
            [new(MotherBrainGlassProgram.ShardGroup6, 6)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupSeventh,
            [new(MotherBrainGlassProgram.ShardGroup6, 7)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupEighth,
            [new(MotherBrainGlassProgram.ShardGroup7, 0)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupFirst,
            [new(MotherBrainGlassProgram.ShardGroup7, 1)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupSecond,
            [new(MotherBrainGlassProgram.ShardGroup7, 2)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupThird,
            [new(MotherBrainGlassProgram.ShardGroup7, 3)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupFourth,
            [new(MotherBrainGlassProgram.ShardGroup7, 4)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupFifth,
            [new(MotherBrainGlassProgram.ShardGroup7, 5)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupSixth,
            [new(MotherBrainGlassProgram.ShardGroup7, 6)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupSeventh,
            [new(MotherBrainGlassProgram.ShardGroup7, 7)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupEighth,
            [new(MotherBrainGlassProgram.Sparkle, 0)] = EnemyProjectileFrameId.MotherBrainGlassSparkleFirst,
            [new(MotherBrainGlassProgram.Sparkle, 1)] = EnemyProjectileFrameId.MotherBrainGlassSparkleSecond,
            [new(MotherBrainGlassProgram.Sparkle, 2)] = EnemyProjectileFrameId.MotherBrainGlassSparkleThird,
            [new(MotherBrainGlassProgram.Sparkle, 3)] = EnemyProjectileFrameId.MotherBrainGlassSparkleFourth,
        };
}

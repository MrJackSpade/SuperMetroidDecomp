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
            [new(MotherBrainGlassSegment.ShardGroup0, 0)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupFirst,
            [new(MotherBrainGlassSegment.ShardGroup0, 1)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupSecond,
            [new(MotherBrainGlassSegment.ShardGroup0, 2)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupThird,
            [new(MotherBrainGlassSegment.ShardGroup0, 3)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupFourth,
            [new(MotherBrainGlassSegment.ShardGroup0, 4)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupFifth,
            [new(MotherBrainGlassSegment.ShardGroup0, 5)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupSixth,
            [new(MotherBrainGlassSegment.ShardGroup0, 6)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupSeventh,
            [new(MotherBrainGlassSegment.ShardGroup0, 7)] = EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupEighth,
            [new(MotherBrainGlassSegment.ShardGroup1, 0)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupFirst,
            [new(MotherBrainGlassSegment.ShardGroup1, 1)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupSecond,
            [new(MotherBrainGlassSegment.ShardGroup1, 2)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupThird,
            [new(MotherBrainGlassSegment.ShardGroup1, 3)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupFourth,
            [new(MotherBrainGlassSegment.ShardGroup1, 4)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupFifth,
            [new(MotherBrainGlassSegment.ShardGroup1, 5)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupSixth,
            [new(MotherBrainGlassSegment.ShardGroup1, 6)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupSeventh,
            [new(MotherBrainGlassSegment.ShardGroup1, 7)] = EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupEighth,
            [new(MotherBrainGlassSegment.ShardGroup2, 0)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupFirst,
            [new(MotherBrainGlassSegment.ShardGroup2, 1)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupSecond,
            [new(MotherBrainGlassSegment.ShardGroup2, 2)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupThird,
            [new(MotherBrainGlassSegment.ShardGroup2, 3)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupFourth,
            [new(MotherBrainGlassSegment.ShardGroup2, 4)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupFifth,
            [new(MotherBrainGlassSegment.ShardGroup2, 5)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupSixth,
            [new(MotherBrainGlassSegment.ShardGroup2, 6)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupSeventh,
            [new(MotherBrainGlassSegment.ShardGroup2, 7)] = EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupEighth,
            [new(MotherBrainGlassSegment.ShardGroup3, 0)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupFirst,
            [new(MotherBrainGlassSegment.ShardGroup3, 1)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupSecond,
            [new(MotherBrainGlassSegment.ShardGroup3, 2)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupThird,
            [new(MotherBrainGlassSegment.ShardGroup3, 3)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupFourth,
            [new(MotherBrainGlassSegment.ShardGroup3, 4)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupFifth,
            [new(MotherBrainGlassSegment.ShardGroup3, 5)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupSixth,
            [new(MotherBrainGlassSegment.ShardGroup3, 6)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupSeventh,
            [new(MotherBrainGlassSegment.ShardGroup3, 7)] = EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupEighth,
            [new(MotherBrainGlassSegment.ShardGroup4, 0)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupFirst,
            [new(MotherBrainGlassSegment.ShardGroup4, 1)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupSecond,
            [new(MotherBrainGlassSegment.ShardGroup4, 2)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupThird,
            [new(MotherBrainGlassSegment.ShardGroup4, 3)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupFourth,
            [new(MotherBrainGlassSegment.ShardGroup4, 4)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupFifth,
            [new(MotherBrainGlassSegment.ShardGroup4, 5)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupSixth,
            [new(MotherBrainGlassSegment.ShardGroup4, 6)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupSeventh,
            [new(MotherBrainGlassSegment.ShardGroup4, 7)] = EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupEighth,
            [new(MotherBrainGlassSegment.ShardGroup5, 0)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupFirst,
            [new(MotherBrainGlassSegment.ShardGroup5, 1)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupSecond,
            [new(MotherBrainGlassSegment.ShardGroup5, 2)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupThird,
            [new(MotherBrainGlassSegment.ShardGroup5, 3)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupFourth,
            [new(MotherBrainGlassSegment.ShardGroup5, 4)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupFifth,
            [new(MotherBrainGlassSegment.ShardGroup5, 5)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupSixth,
            [new(MotherBrainGlassSegment.ShardGroup5, 6)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupSeventh,
            [new(MotherBrainGlassSegment.ShardGroup5, 7)] = EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupEighth,
            [new(MotherBrainGlassSegment.ShardGroup6, 0)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupFirst,
            [new(MotherBrainGlassSegment.ShardGroup6, 1)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupSecond,
            [new(MotherBrainGlassSegment.ShardGroup6, 2)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupThird,
            [new(MotherBrainGlassSegment.ShardGroup6, 3)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupFourth,
            [new(MotherBrainGlassSegment.ShardGroup6, 4)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupFifth,
            [new(MotherBrainGlassSegment.ShardGroup6, 5)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupSixth,
            [new(MotherBrainGlassSegment.ShardGroup6, 6)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupSeventh,
            [new(MotherBrainGlassSegment.ShardGroup6, 7)] = EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupEighth,
            [new(MotherBrainGlassSegment.ShardGroup7, 0)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupFirst,
            [new(MotherBrainGlassSegment.ShardGroup7, 1)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupSecond,
            [new(MotherBrainGlassSegment.ShardGroup7, 2)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupThird,
            [new(MotherBrainGlassSegment.ShardGroup7, 3)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupFourth,
            [new(MotherBrainGlassSegment.ShardGroup7, 4)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupFifth,
            [new(MotherBrainGlassSegment.ShardGroup7, 5)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupSixth,
            [new(MotherBrainGlassSegment.ShardGroup7, 6)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupSeventh,
            [new(MotherBrainGlassSegment.ShardGroup7, 7)] = EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupEighth,
            [new(MotherBrainGlassSegment.Sparkle, 0)] = EnemyProjectileFrameId.MotherBrainGlassSparkleFirst,
            [new(MotherBrainGlassSegment.Sparkle, 1)] = EnemyProjectileFrameId.MotherBrainGlassSparkleSecond,
            [new(MotherBrainGlassSegment.Sparkle, 2)] = EnemyProjectileFrameId.MotherBrainGlassSparkleThird,
            [new(MotherBrainGlassSegment.Sparkle, 3)] = EnemyProjectileFrameId.MotherBrainGlassSparkleFourth,
        };
}

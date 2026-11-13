using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Gunship.Artwork;

/// <summary>The gunship liftoff dust clouds' installed frames and the projectile definition's list identity.</summary>
internal static class GunshipDustVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<GunshipDustFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<GunshipDustFrame, EnemyProjectileFrameId>
        {
            [new(GunshipDustProgram.RightInnerLoop, 0)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopFirst,
            [new(GunshipDustProgram.RightInnerLoop, 1)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopSecond,
            [new(GunshipDustProgram.RightInnerLoop, 2)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopThird,
            [new(GunshipDustProgram.RightInnerLoop, 3)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopFourth,
            [new(GunshipDustProgram.RightInnerLoop, 4)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopFifth,
            [new(GunshipDustProgram.RightInnerLoop, 5)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopSixth,
            [new(GunshipDustProgram.RightInnerLoop, 6)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopSeventh,
            [new(GunshipDustProgram.RightInnerLoop, 7)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopEighth,
            [new(GunshipDustProgram.RightMiddleLoop, 0)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopFirst,
            [new(GunshipDustProgram.RightMiddleLoop, 1)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopSecond,
            [new(GunshipDustProgram.RightMiddleLoop, 2)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopThird,
            [new(GunshipDustProgram.RightMiddleLoop, 3)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopFourth,
            [new(GunshipDustProgram.RightMiddleLoop, 4)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopFifth,
            [new(GunshipDustProgram.RightMiddleLoop, 5)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopSixth,
            [new(GunshipDustProgram.RightMiddleLoop, 6)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopSeventh,
            [new(GunshipDustProgram.RightMiddleLoop, 7)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopEighth,
            [new(GunshipDustProgram.RightOuterLoop, 0)] = EnemyProjectileFrameId.GunshipDustRightOuterLoopFirst,
            [new(GunshipDustProgram.RightOuterLoop, 1)] = EnemyProjectileFrameId.GunshipDustRightOuterLoopSecond,
            [new(GunshipDustProgram.RightOuterLoop, 2)] = EnemyProjectileFrameId.GunshipDustRightOuterLoopThird,
            [new(GunshipDustProgram.RightOuterLoop, 3)] = EnemyProjectileFrameId.GunshipDustRightOuterLoopFourth,
            [new(GunshipDustProgram.RightOuterLoop, 4)] = EnemyProjectileFrameId.GunshipDustRightOuterLoopFifth,
            [new(GunshipDustProgram.RightOuterLoop, 5)] = EnemyProjectileFrameId.GunshipDustRightOuterLoopSixth,
            [new(GunshipDustProgram.RightOuterLoop, 6)] = EnemyProjectileFrameId.GunshipDustRightOuterLoopSeventh,
            [new(GunshipDustProgram.LeftInnerLoop, 0)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopFirst,
            [new(GunshipDustProgram.LeftInnerLoop, 1)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopSecond,
            [new(GunshipDustProgram.LeftInnerLoop, 2)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopThird,
            [new(GunshipDustProgram.LeftInnerLoop, 3)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopFourth,
            [new(GunshipDustProgram.LeftInnerLoop, 4)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopFifth,
            [new(GunshipDustProgram.LeftInnerLoop, 5)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopSixth,
            [new(GunshipDustProgram.LeftInnerLoop, 6)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopSeventh,
            [new(GunshipDustProgram.LeftInnerLoop, 7)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopEighth,
            [new(GunshipDustProgram.LeftMiddleLoop, 0)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopFirst,
            [new(GunshipDustProgram.LeftMiddleLoop, 1)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopSecond,
            [new(GunshipDustProgram.LeftMiddleLoop, 2)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopThird,
            [new(GunshipDustProgram.LeftMiddleLoop, 3)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopFourth,
            [new(GunshipDustProgram.LeftMiddleLoop, 4)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopFifth,
            [new(GunshipDustProgram.LeftMiddleLoop, 5)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopSixth,
            [new(GunshipDustProgram.LeftMiddleLoop, 6)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopSeventh,
            [new(GunshipDustProgram.LeftMiddleLoop, 7)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopEighth,
            [new(GunshipDustProgram.LeftOuterLoop, 0)] = EnemyProjectileFrameId.GunshipDustLeftOuterLoopFirst,
            [new(GunshipDustProgram.LeftOuterLoop, 1)] = EnemyProjectileFrameId.GunshipDustLeftOuterLoopSecond,
            [new(GunshipDustProgram.LeftOuterLoop, 2)] = EnemyProjectileFrameId.GunshipDustLeftOuterLoopThird,
            [new(GunshipDustProgram.LeftOuterLoop, 3)] = EnemyProjectileFrameId.GunshipDustLeftOuterLoopFourth,
            [new(GunshipDustProgram.LeftOuterLoop, 4)] = EnemyProjectileFrameId.GunshipDustLeftOuterLoopFifth,
            [new(GunshipDustProgram.LeftOuterLoop, 5)] = EnemyProjectileFrameId.GunshipDustLeftOuterLoopSixth,
            [new(GunshipDustProgram.LeftOuterLoop, 6)] = EnemyProjectileFrameId.GunshipDustLeftOuterLoopSeventh,
        };
}

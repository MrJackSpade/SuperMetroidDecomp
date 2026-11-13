using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Gunship.Artwork;

/// <summary>The presentation boundary of the gunship's liftoff dust clouds: the projectile frame each of their frames shows.</summary>
internal static class GunshipDustVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<GunshipDustFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<GunshipDustFrame, EnemyProjectileFrameId>
        {
            [new(GunshipDustSegment.RightInnerLoop, 0)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopFirst,
            [new(GunshipDustSegment.RightInnerLoop, 1)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopSecond,
            [new(GunshipDustSegment.RightInnerLoop, 2)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopThird,
            [new(GunshipDustSegment.RightInnerLoop, 3)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopFourth,
            [new(GunshipDustSegment.RightInnerLoop, 4)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopFifth,
            [new(GunshipDustSegment.RightInnerLoop, 5)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopSixth,
            [new(GunshipDustSegment.RightInnerLoop, 6)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopSeventh,
            [new(GunshipDustSegment.RightInnerLoop, 7)] = EnemyProjectileFrameId.GunshipDustRightInnerLoopEighth,
            [new(GunshipDustSegment.RightMiddleLoop, 0)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopFirst,
            [new(GunshipDustSegment.RightMiddleLoop, 1)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopSecond,
            [new(GunshipDustSegment.RightMiddleLoop, 2)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopThird,
            [new(GunshipDustSegment.RightMiddleLoop, 3)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopFourth,
            [new(GunshipDustSegment.RightMiddleLoop, 4)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopFifth,
            [new(GunshipDustSegment.RightMiddleLoop, 5)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopSixth,
            [new(GunshipDustSegment.RightMiddleLoop, 6)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopSeventh,
            [new(GunshipDustSegment.RightMiddleLoop, 7)] = EnemyProjectileFrameId.GunshipDustRightMiddleLoopEighth,
            [new(GunshipDustSegment.RightOuterLoop, 0)] = EnemyProjectileFrameId.GunshipDustRightOuterLoopFirst,
            [new(GunshipDustSegment.RightOuterLoop, 1)] = EnemyProjectileFrameId.GunshipDustRightOuterLoopSecond,
            [new(GunshipDustSegment.RightOuterLoop, 2)] = EnemyProjectileFrameId.GunshipDustRightOuterLoopThird,
            [new(GunshipDustSegment.RightOuterLoop, 3)] = EnemyProjectileFrameId.GunshipDustRightOuterLoopFourth,
            [new(GunshipDustSegment.RightOuterLoop, 4)] = EnemyProjectileFrameId.GunshipDustRightOuterLoopFifth,
            [new(GunshipDustSegment.RightOuterLoop, 5)] = EnemyProjectileFrameId.GunshipDustRightOuterLoopSixth,
            [new(GunshipDustSegment.RightOuterLoop, 6)] = EnemyProjectileFrameId.GunshipDustRightOuterLoopSeventh,
            [new(GunshipDustSegment.LeftInnerLoop, 0)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopFirst,
            [new(GunshipDustSegment.LeftInnerLoop, 1)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopSecond,
            [new(GunshipDustSegment.LeftInnerLoop, 2)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopThird,
            [new(GunshipDustSegment.LeftInnerLoop, 3)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopFourth,
            [new(GunshipDustSegment.LeftInnerLoop, 4)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopFifth,
            [new(GunshipDustSegment.LeftInnerLoop, 5)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopSixth,
            [new(GunshipDustSegment.LeftInnerLoop, 6)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopSeventh,
            [new(GunshipDustSegment.LeftInnerLoop, 7)] = EnemyProjectileFrameId.GunshipDustLeftInnerLoopEighth,
            [new(GunshipDustSegment.LeftMiddleLoop, 0)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopFirst,
            [new(GunshipDustSegment.LeftMiddleLoop, 1)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopSecond,
            [new(GunshipDustSegment.LeftMiddleLoop, 2)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopThird,
            [new(GunshipDustSegment.LeftMiddleLoop, 3)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopFourth,
            [new(GunshipDustSegment.LeftMiddleLoop, 4)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopFifth,
            [new(GunshipDustSegment.LeftMiddleLoop, 5)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopSixth,
            [new(GunshipDustSegment.LeftMiddleLoop, 6)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopSeventh,
            [new(GunshipDustSegment.LeftMiddleLoop, 7)] = EnemyProjectileFrameId.GunshipDustLeftMiddleLoopEighth,
            [new(GunshipDustSegment.LeftOuterLoop, 0)] = EnemyProjectileFrameId.GunshipDustLeftOuterLoopFirst,
            [new(GunshipDustSegment.LeftOuterLoop, 1)] = EnemyProjectileFrameId.GunshipDustLeftOuterLoopSecond,
            [new(GunshipDustSegment.LeftOuterLoop, 2)] = EnemyProjectileFrameId.GunshipDustLeftOuterLoopThird,
            [new(GunshipDustSegment.LeftOuterLoop, 3)] = EnemyProjectileFrameId.GunshipDustLeftOuterLoopFourth,
            [new(GunshipDustSegment.LeftOuterLoop, 4)] = EnemyProjectileFrameId.GunshipDustLeftOuterLoopFifth,
            [new(GunshipDustSegment.LeftOuterLoop, 5)] = EnemyProjectileFrameId.GunshipDustLeftOuterLoopSixth,
            [new(GunshipDustSegment.LeftOuterLoop, 6)] = EnemyProjectileFrameId.GunshipDustLeftOuterLoopSeventh,
        };
}

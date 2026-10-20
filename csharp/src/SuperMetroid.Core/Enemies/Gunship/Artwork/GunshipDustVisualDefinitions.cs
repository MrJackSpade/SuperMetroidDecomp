using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Gunship.Artwork;

/// <summary>The gunship liftoff dust clouds' installed frames and the projectile definition's native list identity.</summary>
internal static class GunshipDustVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<GunshipDustFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<GunshipDustFrame, EnemyProjectileFrameId>
        {
            [new(GunshipDustProgram.RightInnerLoop, 0)] = EnemyProjectileFrameId.GunshipDustFrame00,
            [new(GunshipDustProgram.RightInnerLoop, 1)] = EnemyProjectileFrameId.GunshipDustFrame01,
            [new(GunshipDustProgram.RightInnerLoop, 2)] = EnemyProjectileFrameId.GunshipDustFrame02,
            [new(GunshipDustProgram.RightInnerLoop, 3)] = EnemyProjectileFrameId.GunshipDustFrame03,
            [new(GunshipDustProgram.RightInnerLoop, 4)] = EnemyProjectileFrameId.GunshipDustFrame04,
            [new(GunshipDustProgram.RightInnerLoop, 5)] = EnemyProjectileFrameId.GunshipDustFrame05,
            [new(GunshipDustProgram.RightInnerLoop, 6)] = EnemyProjectileFrameId.GunshipDustFrame06,
            [new(GunshipDustProgram.RightInnerLoop, 7)] = EnemyProjectileFrameId.GunshipDustFrame07,
            [new(GunshipDustProgram.RightMiddleLoop, 0)] = EnemyProjectileFrameId.GunshipDustFrame08,
            [new(GunshipDustProgram.RightMiddleLoop, 1)] = EnemyProjectileFrameId.GunshipDustFrame09,
            [new(GunshipDustProgram.RightMiddleLoop, 2)] = EnemyProjectileFrameId.GunshipDustFrame10,
            [new(GunshipDustProgram.RightMiddleLoop, 3)] = EnemyProjectileFrameId.GunshipDustFrame11,
            [new(GunshipDustProgram.RightMiddleLoop, 4)] = EnemyProjectileFrameId.GunshipDustFrame12,
            [new(GunshipDustProgram.RightMiddleLoop, 5)] = EnemyProjectileFrameId.GunshipDustFrame13,
            [new(GunshipDustProgram.RightMiddleLoop, 6)] = EnemyProjectileFrameId.GunshipDustFrame14,
            [new(GunshipDustProgram.RightMiddleLoop, 7)] = EnemyProjectileFrameId.GunshipDustFrame15,
            [new(GunshipDustProgram.RightOuterLoop, 0)] = EnemyProjectileFrameId.GunshipDustFrame16,
            [new(GunshipDustProgram.RightOuterLoop, 1)] = EnemyProjectileFrameId.GunshipDustFrame17,
            [new(GunshipDustProgram.RightOuterLoop, 2)] = EnemyProjectileFrameId.GunshipDustFrame18,
            [new(GunshipDustProgram.RightOuterLoop, 3)] = EnemyProjectileFrameId.GunshipDustFrame19,
            [new(GunshipDustProgram.RightOuterLoop, 4)] = EnemyProjectileFrameId.GunshipDustFrame20,
            [new(GunshipDustProgram.RightOuterLoop, 5)] = EnemyProjectileFrameId.GunshipDustFrame21,
            [new(GunshipDustProgram.RightOuterLoop, 6)] = EnemyProjectileFrameId.GunshipDustFrame22,
            [new(GunshipDustProgram.LeftInnerLoop, 0)] = EnemyProjectileFrameId.GunshipDustFrame23,
            [new(GunshipDustProgram.LeftInnerLoop, 1)] = EnemyProjectileFrameId.GunshipDustFrame24,
            [new(GunshipDustProgram.LeftInnerLoop, 2)] = EnemyProjectileFrameId.GunshipDustFrame25,
            [new(GunshipDustProgram.LeftInnerLoop, 3)] = EnemyProjectileFrameId.GunshipDustFrame26,
            [new(GunshipDustProgram.LeftInnerLoop, 4)] = EnemyProjectileFrameId.GunshipDustFrame27,
            [new(GunshipDustProgram.LeftInnerLoop, 5)] = EnemyProjectileFrameId.GunshipDustFrame28,
            [new(GunshipDustProgram.LeftInnerLoop, 6)] = EnemyProjectileFrameId.GunshipDustFrame29,
            [new(GunshipDustProgram.LeftInnerLoop, 7)] = EnemyProjectileFrameId.GunshipDustFrame30,
            [new(GunshipDustProgram.LeftMiddleLoop, 0)] = EnemyProjectileFrameId.GunshipDustFrame31,
            [new(GunshipDustProgram.LeftMiddleLoop, 1)] = EnemyProjectileFrameId.GunshipDustFrame32,
            [new(GunshipDustProgram.LeftMiddleLoop, 2)] = EnemyProjectileFrameId.GunshipDustFrame33,
            [new(GunshipDustProgram.LeftMiddleLoop, 3)] = EnemyProjectileFrameId.GunshipDustFrame34,
            [new(GunshipDustProgram.LeftMiddleLoop, 4)] = EnemyProjectileFrameId.GunshipDustFrame35,
            [new(GunshipDustProgram.LeftMiddleLoop, 5)] = EnemyProjectileFrameId.GunshipDustFrame36,
            [new(GunshipDustProgram.LeftMiddleLoop, 6)] = EnemyProjectileFrameId.GunshipDustFrame37,
            [new(GunshipDustProgram.LeftMiddleLoop, 7)] = EnemyProjectileFrameId.GunshipDustFrame38,
            [new(GunshipDustProgram.LeftOuterLoop, 0)] = EnemyProjectileFrameId.GunshipDustFrame39,
            [new(GunshipDustProgram.LeftOuterLoop, 1)] = EnemyProjectileFrameId.GunshipDustFrame40,
            [new(GunshipDustProgram.LeftOuterLoop, 2)] = EnemyProjectileFrameId.GunshipDustFrame41,
            [new(GunshipDustProgram.LeftOuterLoop, 3)] = EnemyProjectileFrameId.GunshipDustFrame42,
            [new(GunshipDustProgram.LeftOuterLoop, 4)] = EnemyProjectileFrameId.GunshipDustFrame43,
            [new(GunshipDustProgram.LeftOuterLoop, 5)] = EnemyProjectileFrameId.GunshipDustFrame44,
            [new(GunshipDustProgram.LeftOuterLoop, 6)] = EnemyProjectileFrameId.GunshipDustFrame45,
        };
}

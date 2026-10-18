using SuperMetroid.Core.Enemies.Projectiles;

using P = SuperMetroid.Core.Enemies.Gunship.GunshipDustProgram;

namespace SuperMetroid.Core.Enemies.Gunship.Artwork;

/// <summary>The gunship liftoff dust clouds' installed frames and the projectile definition's native list identity.</summary>
internal static class GunshipDustVisualDefinitions
{
    /// <summary>
    /// <c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index0_0</c> at $86:A197: the list the dust's projectile definition
    /// names. Spawning starts the parameter's typed program instead.
    /// </summary>
    internal const ushort DefinitionList = 0xa197;

    /// <summary>The frames in installed order.</summary>
    internal static IReadOnlyList<GunshipDustFrame> Frames { get; } = [new(P.RightInnerLoop, 0), new(P.RightInnerLoop, 1), new(P.RightInnerLoop, 2), new(P.RightInnerLoop, 3), new(P.RightInnerLoop, 4), new(P.RightInnerLoop, 5), new(P.RightInnerLoop, 6), new(P.RightInnerLoop, 7), new(P.RightMiddleLoop, 0), new(P.RightMiddleLoop, 1), new(P.RightMiddleLoop, 2), new(P.RightMiddleLoop, 3), new(P.RightMiddleLoop, 4), new(P.RightMiddleLoop, 5), new(P.RightMiddleLoop, 6), new(P.RightMiddleLoop, 7), new(P.RightOuterLoop, 0), new(P.RightOuterLoop, 1), new(P.RightOuterLoop, 2), new(P.RightOuterLoop, 3), new(P.RightOuterLoop, 4), new(P.RightOuterLoop, 5), new(P.RightOuterLoop, 6), new(P.LeftInnerLoop, 0), new(P.LeftInnerLoop, 1), new(P.LeftInnerLoop, 2), new(P.LeftInnerLoop, 3), new(P.LeftInnerLoop, 4), new(P.LeftInnerLoop, 5), new(P.LeftInnerLoop, 6), new(P.LeftInnerLoop, 7), new(P.LeftMiddleLoop, 0), new(P.LeftMiddleLoop, 1), new(P.LeftMiddleLoop, 2), new(P.LeftMiddleLoop, 3), new(P.LeftMiddleLoop, 4), new(P.LeftMiddleLoop, 5), new(P.LeftMiddleLoop, 6), new(P.LeftMiddleLoop, 7), new(P.LeftOuterLoop, 0), new(P.LeftOuterLoop, 1), new(P.LeftOuterLoop, 2), new(P.LeftOuterLoop, 3), new(P.LeftOuterLoop, 4), new(P.LeftOuterLoop, 5), new(P.LeftOuterLoop, 6)];

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

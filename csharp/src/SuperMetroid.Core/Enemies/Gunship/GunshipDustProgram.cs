namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The gunship liftoff dust clouds' typed programs (#641): three cloud shapes on each side of the ship.</summary>
internal enum GunshipDustProgram
{
    /// <summary><c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index0_0</c> at $86:A197: loads the cloud's single pass.</summary>
    RightInner,
    /// <summary>Repeats the RightInner cloud's poses while its general timer counts, then deletes it ($86:A19B).</summary>
    RightInnerLoop,
    /// <summary><c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index2_0</c> at $86:A1C1: loads the cloud's single pass.</summary>
    RightMiddle,
    /// <summary>Repeats the RightMiddle cloud's poses while its general timer counts, then deletes it ($86:A1C5).</summary>
    RightMiddleLoop,
    /// <summary><c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index4_0</c> at $86:A1EB: loads the cloud's single pass.</summary>
    RightOuter,
    /// <summary>Repeats the RightOuter cloud's poses while its general timer counts, then deletes it ($86:A1EF).</summary>
    RightOuterLoop,
    /// <summary><c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index6_0</c> at $86:A211: loads the cloud's single pass.</summary>
    LeftInner,
    /// <summary>Repeats the LeftInner cloud's poses while its general timer counts, then deletes it ($86:A215).</summary>
    LeftInnerLoop,
    /// <summary><c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index8_0</c> at $86:A23B: loads the cloud's single pass.</summary>
    LeftMiddle,
    /// <summary>Repeats the LeftMiddle cloud's poses while its general timer counts, then deletes it ($86:A23F).</summary>
    LeftMiddleLoop,
    /// <summary><c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_IndexA_0</c> at $86:A265: loads the cloud's single pass.</summary>
    LeftOuter,
    /// <summary>Repeats the LeftOuter cloud's poses while its general timer counts, then deletes it ($86:A269).</summary>
    LeftOuterLoop,
}

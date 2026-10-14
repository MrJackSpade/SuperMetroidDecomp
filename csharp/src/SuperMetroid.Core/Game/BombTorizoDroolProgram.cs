namespace SuperMetroid.Core.Game;

/// <summary>The six Bomb Torizo drool instruction lists in bank <c>$86</c>.</summary>
internal enum BombTorizoDroolProgram : ushort
{
    /// <summary><c>InstList_EnemyProj_BombTorizoLowHealthDrool_4FrameDelay</c> at $86:A46A.</summary>
    FourFrameDelay = 0xa46a,
    /// <summary><c>InstList_EnemyProj_BombTorizoLowHealthDrool_2FrameDelay</c> at $86:A46E.</summary>
    TwoFrameDelay = 0xa46e,
    /// <summary><c>InstList_EnemyProj_BombTorizoLowHealthDrool_NoDelay_0</c> at $86:A472.</summary>
    NoDelay = 0xa472,
    /// <summary><c>InstList_EnemyProj_BombTorizoLowHealthDrool_NoDelay_1</c> at $86:A482.</summary>
    FallingLoop = 0xa482,
    /// <summary><c>InstList_EnemyProj_BombTorizoLowHealthDrool_HitWall</c> at $86:A48A.</summary>
    WallImpact = 0xa48a,
    /// <summary><c>InstList_EnemyProj_BombTorizoLowHealthDrool_HitFloor</c> at $86:A48E.</summary>
    FloorImpact = 0xa48e,
}

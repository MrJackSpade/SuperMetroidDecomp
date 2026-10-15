namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Dragon's rising and falling fireball programs (#641), one per bank-$86 list a spawn, branch or family routine enters, in authored order.
/// Generated from the verified native lists; see <see cref="DragonFireballAnimations"/>.
/// </summary>
internal enum DragonFireballProgram
{
    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Rising_Left</c> at $86:B4BF.</summary>
    RisingLeft,
    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Rising_Right</c> at $86:B4CB.</summary>
    RisingRight,
    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Falling_Left</c> at $86:B4D7.</summary>
    FallingLeft,
    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Falling_Right</c> at $86:B4E3.</summary>
    FallingRight,
}

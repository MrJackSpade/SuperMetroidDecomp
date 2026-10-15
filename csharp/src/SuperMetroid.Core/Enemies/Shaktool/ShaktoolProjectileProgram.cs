namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Shaktool's attack-circle programs (#641), one per bank-$86 list a spawn, branch or family routine enters, in authored order.
/// Generated from the verified native lists; see <see cref="ShaktoolProjectileAnimations"/>.
/// </summary>
internal enum ShaktoolProjectileProgram
{
    /// <summary><c>InstList_EnemyProjectile_ShaktoolsAttack_FrontCircle_0</c> at $86:BD68.</summary>
    Front,
    /// <summary>The loop at $86:BD70.</summary>
    FrontLoop,
    /// <summary><c>InstList_EnemyProjectile_ShaktoolsAttack_MiddleCircle_0</c> at $86:BD78.</summary>
    Middle,
    /// <summary>The loop at $86:BD84.</summary>
    MiddleLoop,
    /// <summary><c>InstList_EnemyProjectile_ShaktoolsAttack_BackCircle_0</c> at $86:BD8C.</summary>
    Back,
    /// <summary>The loop at $86:BD94.</summary>
    BackLoop,
}

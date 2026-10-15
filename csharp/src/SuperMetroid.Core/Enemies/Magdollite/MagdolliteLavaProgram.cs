namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite's thrown-lava programs (#641), one per bank-$86 list a spawn, branch or family routine enters, in authored order.
/// Generated from the verified native lists; see <see cref="MagdolliteLavaAnimations"/>.
/// </summary>
internal enum MagdolliteLavaProgram
{
    /// <summary><c>InstList_EnemyProjectile_Delete</c> at $86:84FC.</summary>
    SharedDelete,
    /// <summary><c>InstList_EnemyProjectile_MagdolliteFlame</c> at $86:DFD8.</summary>
    Left,
    /// <summary><c>InstList_EnemyProjectile_MagdolliteFlame_</c> at $86:DFDE.</summary>
    Right,
    /// <summary><c>InstList_EnemyProjectile_Shot_MagdolliteFlame</c> at $86:DFE4.</summary>
    Shot,
}

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// The Kago bugs' landing, falling, jumping and shot programs (#641), one per bank-$86 list a spawn, branch or family routine enters, in authored order.
/// Generated from the verified native lists; see <see cref="KagoBugProjectileAnimations"/>.
/// </summary>
internal enum KagoBugProjectileProgram
{
    /// <summary><c>InstList_EnemyProjectile_Delete</c> at $86:84FC.</summary>
    SharedDelete,
    /// <summary><c>InstList_EnemyProjectile_KagoBug_HitFloor</c> at $86:D03C.</summary>
    Landed,
    /// <summary><c>InstList_EnemyProjectile_KagoBug_Falling</c> at $86:D04A.</summary>
    Falling,
    /// <summary><c>InstList_EnemyProjectile_KagoBug_Jump_0</c> at $86:D052.</summary>
    JumpStart,
    /// <summary><c>InstList_EnemyProjectile_KagoBug_Jump_1</c> at $86:D05C.</summary>
    JumpLoop,
    /// <summary><c>InstList_EnemyProjectile_Shot_KagoBug</c> at $86:D064.</summary>
    Shot,
}

using SuperMetroid.Core.Enemies.EyeDoor;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The eye door projectile's flight, impact and shot programs (#641), one per bank-$86 list a spawn, branch or family routine enters, in authored order.
/// Generated from the verified native lists; see <see cref="EyeDoorProjectileAnimations"/>.
/// </summary>
internal enum EyeDoorProjectileProgram
{
    /// <summary><c>InstList_EnemyProjectile_EyeDoorProjectile_Normal_0</c> at $86:B5D9.</summary>
    Initial,
    /// <summary><c>InstList_EnemyProjectile_EyeDoorProjectile_Normal_1</c> at $86:B5EB.</summary>
    FlyingLoop,
    /// <summary><c>InstList_EnemyProjectile_EyeDoorProjectile_Explode</c> at $86:B5F3.</summary>
    Impact,
    /// <summary><c>InstList_EnemyProjectile_Shot_EyeDoorProjectile</c> at $86:B603.</summary>
    Shot,
}

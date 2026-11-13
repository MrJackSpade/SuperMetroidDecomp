using SuperMetroid.Core.Enemies.EyeDoor;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The eye door projectile's flight, impact and shot programs (#641), one per animation script a spawn, branch or family step enters, in authored order.
/// Generated from the verified authored scripts; see <see cref="EyeDoorProjectileAnimations"/>.
/// </summary>
internal enum EyeDoorProjectileProgram
{
    /// <summary>Enemy projectile animation script: eye door projectile, normal, part 0.</summary>
    Initial,
    /// <summary>Enemy projectile animation script: eye door projectile, normal, part 1.</summary>
    FlyingLoop,
    /// <summary>Enemy projectile animation script: eye door projectile, explode.</summary>
    Impact,
    /// <summary>Enemy projectile animation script: shot, eye door projectile.</summary>
    Shot,
}

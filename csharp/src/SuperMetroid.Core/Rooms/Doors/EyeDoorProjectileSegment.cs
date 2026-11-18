using SuperMetroid.Core.Enemies.EyeDoor;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The eye door projectile's flight, impact and shot animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Generated from the authored scripts; see <see cref="EyeDoorProjectileAnimations"/>.
/// </summary>
internal enum EyeDoorProjectileSegment
{
    /// <summary>The eye door's shot as it is fired.</summary>
    Fired,
    /// <summary>The eye door's shot exploding on impact.</summary>
    Impact,
    /// <summary>The eye door's shot when Samus shoots it.</summary>
    Shot,
}

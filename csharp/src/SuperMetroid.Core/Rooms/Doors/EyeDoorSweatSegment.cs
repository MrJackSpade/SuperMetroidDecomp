using SuperMetroid.Core.Enemies.EyeDoor;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The eye-door sweat drop's falling and floor-splash animations (#1296): one segment per animation a spawn or family step enters, in authored order. See
/// <see cref="EyeDoorSweatAnimations"/>.
/// </summary>
internal enum EyeDoorSweatSegment
{
    /// <summary>The drop falls in one pose.</summary>
    Falling,
    /// <summary>The drop splashes and deletes.</summary>
    HitFloor,
}

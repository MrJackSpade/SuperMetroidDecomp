using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The eye door's wakes.</summary>
internal static class EyeDoorFrameBehaviors
{
    /// <summary>The eye wakes once its door's opened bit is set.</summary>
    internal static RoomObjectFrameBehavior WakeWhenDoorBitSet { get; } = Of(EyeDoorFrameBehavior.WakeWhenDoorBitSet);

    /// <summary>The missile-hit frame behaviour on an eye component: a missile hit wakes it, a Super Missile seeds its counter.</summary>
    internal static RoomObjectFrameBehavior MissileHit { get; } = Of(EyeDoorFrameBehavior.MissileHit);

    /// <summary>The behavior object for <paramref name="routine"/>.</summary>
    private static RoomObjectFrameBehavior Of(EyeDoorFrameBehavior routine) =>
        new($"eye door {routine}", (slot, context) => context.RoomObjects.EyeDoors.RunEyeDoorFrameBehavior(slot, routine));
}

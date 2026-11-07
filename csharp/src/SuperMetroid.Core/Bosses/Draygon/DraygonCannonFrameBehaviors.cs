using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's cannons' missile hit.</summary>
internal static class DraygonCannonFrameBehaviors
{
    /// <summary>A missile hit wakes the cannon; a Super Missile seeds its counter.</summary>
    internal static RoomObjectFrameBehavior MissileHit { get; } = new(
        "Draygon cannon missile hit",
        static (slot, _) => DraygonCannonObjects.RunDraygonCannonFrameBehavior(slot));
}

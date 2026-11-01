using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's cannons' missile hit.</summary>
internal static class DraygonCannonFrameBehaviors
{
    /// <summary>A missile hit wakes the cannon; a Super Missile seeds its counter.</summary>
    internal static PlmFrameBehavior MissileHit { get; } = new(
        "Draygon cannon missile hit",
        static (slot, _) => DraygonCannonPlms.RunDraygonCannonFrameBehavior(slot));
}

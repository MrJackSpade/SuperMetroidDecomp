using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's cannons' missile hit.</summary>
internal static class DraygonCannonPreInstructions
{
    /// <summary><c>$84:DB64</c>: a missile hit wakes the cannon; a Super Missile seeds its counter.</summary>
    internal static PlmPreInstruction MissileHit { get; } = new(
        "Draygon cannon missile hit",
        static (slot, _) => DraygonCannonPlms.RunDraygonCannonPreInstruction(slot));
}

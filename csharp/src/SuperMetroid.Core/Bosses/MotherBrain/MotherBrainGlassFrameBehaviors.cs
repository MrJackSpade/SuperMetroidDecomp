using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's glass hit counter.</summary>
internal static class MotherBrainGlassFrameBehaviors
{
    /// <summary>Missile hits advance the glass's hit count.</summary>
    internal static RoomObjectFrameBehavior Hit { get; } = new(
        "Mother Brain glass hit",
        static (slot, _) => MotherBrainGlassObjects.RunMotherBrainGlassFrameBehavior(slot));
}

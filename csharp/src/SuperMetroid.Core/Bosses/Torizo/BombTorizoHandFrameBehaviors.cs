using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb Torizo's hand's wake.</summary>
internal static class BombTorizoHandFrameBehaviors
{
    /// <summary>The sleeping hand wakes once Samus has bombs.</summary>
    internal static RoomObjectFrameBehavior WakeIfSamusHasBombs { get; } = new(
        "Bomb Torizo hand wake",
        static (slot, context) => context.RoomObjects.Bosses().BombTorizoHand.RunBombTorizoHandFrameBehavior(slot));
}

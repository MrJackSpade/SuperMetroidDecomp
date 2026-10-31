using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge values and helpers of <see cref="KraidBehavior"/> that only verification reads.</summary>
internal static class KraidBehaviorConstants
{
    /// <summary>Converts a recorded native mouth-shape address into wrapped world-space collision bounds.</summary>
    internal static KraidPassBounds PlaceKraidPassBounds(RoomEnemySlot body, ushort pointer) =>
        KraidCombat.PlaceKraidPassBounds(body, KraidNativeWords.MouthHitboxes.Decode(pointer, "Kraid mouth hitbox"));
}

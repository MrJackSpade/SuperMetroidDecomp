using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge values and helpers of <see cref="KraidBehavior"/> that only verification reads.</summary>
internal static class KraidBehaviorConstants
{
    /// <summary>Converts a recorded native mouth-shape address into wrapped world-space collision bounds.</summary>
    internal static KraidBehavior.KraidCollisionScratch LoadKraidCollisionScratch(RoomEnemySlot body, ushort pointer) =>
        KraidBehavior.LoadKraidCollisionScratch(body, KraidMouthHitboxDefinitionsConstants.ResolveCollision(pointer));
}

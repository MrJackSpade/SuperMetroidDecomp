namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One fixed collision component of a Spore Spawn frame.</summary>
/// <param name="Offset">The component's offset from the body's position.</param>
/// <param name="Hitboxes">The component's rectangles, tested in order.</param>
internal readonly record struct SporeSpawnCollisionComponent(
    Offset16<RoomPixels> Offset, IReadOnlyList<SporeSpawnCollisionHitbox> Hitboxes);

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Fixed offset and bank-$A5 hitbox-list identity for Spore Spawn.</summary>
internal readonly record struct SporeSpawnCollisionComponent(
    Offset16<RoomPixels> Offset, ushort HitboxPointer);

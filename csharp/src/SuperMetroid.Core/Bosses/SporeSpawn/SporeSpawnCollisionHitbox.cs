namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Cartridge rectangle and touch/shot callbacks, independent of OAM artwork.</summary>
internal readonly record struct SporeSpawnCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

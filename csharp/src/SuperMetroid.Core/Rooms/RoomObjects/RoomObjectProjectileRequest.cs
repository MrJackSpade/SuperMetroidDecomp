namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// An enemy projectile a room object throws as it steps (#2514): a shard of Mother Brain's glass or a falling fragment of
/// the Bomb Torizo statue's hand. The step's outcome carries each in the order it was thrown, and the update that ran the
/// step spawns them after it.
/// </summary>
public abstract record RoomObjectProjectileRequest
{
    /// <summary>Only the glass's shards and the statue hand's fragments derive from this.</summary>
    /// <param name="roomObjectBlock">The block of the room object that threw the projectile.</param>
    private protected RoomObjectProjectileRequest(Position16<RoomBlocks> roomObjectBlock) => RoomObjectBlock = roomObjectBlock;

    /// <summary>The block of the room object that threw the projectile, which the projectile's place is relative to.</summary>
    public Position16<RoomBlocks> RoomObjectBlock { get; }

    /// <summary>Spawns the projectile among the room's enemy projectiles, through the spawner its thrower gave it.</summary>
    internal abstract void Spawn();
}

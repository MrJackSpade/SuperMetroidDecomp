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

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this room object projectile request, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(RoomObjectProjectileRequestReactions reactions);


    /// <summary>The block of the room object that threw the projectile, which the projectile's place is relative to.</summary>
    public Position16<RoomBlocks> RoomObjectBlock { get; }
}

/// <summary>What the owner of a <see cref="RoomObjectProjectileRequest"/> does on each of its cases (#2553).</summary>
/// <param name="MotherBrainGlassProjectileRequest">Reacts to a <see cref="Bosses.MotherBrain.MotherBrainGlassProjectileRequest"/>.</param>
/// <param name="BombTorizoStatueProjectileRequest">Reacts to a <see cref="Bosses.Torizo.BombTorizoStatueProjectileRequest"/>.</param>
internal sealed record RoomObjectProjectileRequestReactions(Action<Bosses.MotherBrain.MotherBrainGlassProjectileRequest> MotherBrainGlassProjectileRequest, Action<Bosses.Torizo.BombTorizoStatueProjectileRequest> BombTorizoStatueProjectileRequest);

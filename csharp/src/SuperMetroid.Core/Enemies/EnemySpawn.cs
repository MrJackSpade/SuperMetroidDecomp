namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A population entry bound to the species creation that builds its enemy (#2554): the entry, and the settings it gives
/// its species as the type that creation takes, checked once where the entry is bound. An occupant holds the spawn it was
/// set up from, so its entity is built, and a respawn rebuilt, with no further check.
/// </summary>
internal abstract class EnemySpawn
{
    /// <summary>Binds <paramref name="placement"/>.</summary>
    /// <param name="placement">The population entry.</param>
    private protected EnemySpawn(RoomEnemyPlacement placement) => Placement = Ensure.NotNull(placement);

    /// <summary>The population entry the enemy is spawned from.</summary>
    internal RoomEnemyPlacement Placement { get; }

    /// <summary>
    /// How a death explosion's respawn cue rebuilds the entry's enemy; null for an entry whose species no respawn builds
    /// (one only its room's population load places, or one spawned joining an encounter), none of which a population or
    /// spawn lets respawn (checked where it is bound).
    /// </summary>
    internal abstract IEnemyBuild? Respawn { get; }

    /// <summary>Rejects an entry that would let an enemy no respawn builds respawn.</summary>
    /// <param name="placement">The entry being bound.</param>
    /// <param name="species">How its species builds the enemy, for the message.</param>
    /// <returns><paramref name="placement"/>.</returns>
    /// <exception cref="InvalidDataException">The entry lets its enemy respawn.</exception>
    private protected static RoomEnemyPlacement NeverRespawning(RoomEnemyPlacement placement, string species) =>
        Ensure.NotNull(placement).Presence.HasAny(EnemyPresence.RespawnsIfKilled)
            ? throw new InvalidDataException($"Enemy {placement.DefinitionId} is {species}, so no respawn builds it, yet its entry lets it respawn.")
            : placement;
}

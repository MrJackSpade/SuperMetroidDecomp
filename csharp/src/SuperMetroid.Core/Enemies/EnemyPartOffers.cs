using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The creations a part already placed offers the parts its room lists after it (#2554): a body that begins an encounter
/// offers its later parts the creation that builds each of them joining it, so a part placed after its partner receives
/// that partner by construction. The population load binds an entry whose enemy is offered through the offer, and a
/// respawn rebuilds it the same way; the latest offer for a definition stands for the rest of the room.
/// </summary>
[RoomLifetime]
internal sealed class EnemyPartOffers
{
    /// <summary>How each offered definition's entries bind, by the creation the offer carries.</summary>
    private readonly Dictionary<EnemyDefinitionId, Func<RoomEnemyPlacement, PopulationEnemySpawn>> _offered = [];

    /// <summary>Offers <paramref name="create"/> as how the entries of <paramref name="part"/> listed from here on are built.</summary>
    /// <typeparam name="TSettings">The settings the creation takes.</typeparam>
    /// <param name="part">The later part's definition.</param>
    /// <param name="create">The creation, which carries what the part joins.</param>
    internal void Offer<TSettings>(EnemyDefinitionId part, EnemySpawnCreation<TSettings> create) where TSettings : EnemySpawnSettings
    {
        Ensure.NotNull(create);
        _offered[part] = placement => placement.SpawnSettings is TSettings settings
            ? new OrdinaryEnemySpawn<TSettings>(placement, settings, create)
            : throw new InvalidDataException($"Enemy {placement.DefinitionId}'s population entry carries {placement.SpawnSettings}, not {typeof(TSettings).Name}.");
    }

    /// <summary>
    /// Offers <paramref name="create"/> as how the entries of <paramref name="part"/> listed from here on are built, for a
    /// part only the room's population load places, in the load's surroundings.
    /// </summary>
    /// <typeparam name="TSettings">The settings the creation takes.</typeparam>
    /// <param name="part">The later part's definition.</param>
    /// <param name="create">The creation, which carries what the part joins.</param>
    internal void OfferLoaded<TSettings>(EnemyDefinitionId part, EnemyLoadedCreation<TSettings> create) where TSettings : EnemySpawnSettings
    {
        Ensure.NotNull(create);
        _offered[part] = placement => placement.SpawnSettings is TSettings settings
            ? new LoadedEnemySpawn<TSettings>(placement, settings, create)
            : throw new InvalidDataException($"Enemy {placement.DefinitionId}'s population entry carries {placement.SpawnSettings}, not {typeof(TSettings).Name}.");
    }

    /// <summary>Binds <paramref name="placement"/> through the offer its enemy has, when an earlier part made one.</summary>
    /// <param name="placement">The population entry.</param>
    /// <param name="spawn">The bound entry, when an offer stands for its enemy.</param>
    /// <returns>Whether an offer stands for the entry's enemy.</returns>
    /// <exception cref="InvalidDataException">The entry carries other settings than the offered creation takes.</exception>
    internal bool TryBind(RoomEnemyPlacement placement, [NotNullWhen(true)] out PopulationEnemySpawn? spawn)
    {
        if (_offered.TryGetValue(Ensure.NotNull(placement).DefinitionId, out Func<RoomEnemyPlacement, PopulationEnemySpawn>? bind))
        {
            spawn = bind(placement);
            return true;
        }
        spawn = null;
        return false;
    }
}

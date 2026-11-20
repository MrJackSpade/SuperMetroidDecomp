namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Authors a fixture place's population entry, which the occupant bound there carries (#2558) and its species'
/// initialization and authored reads take.
/// </summary>
internal static class RoomEnemyPlaceSpawnParameters
{
    /// <summary>
    /// Binds <paramref name="species"/> in the place as a check places it: from the entry the place's occupant carries,
    /// or its last occupant left, renamed for <paramref name="species"/>; a place no enemy has been spawned in gets an
    /// entry placing <paramref name="species"/> at the room origin with no contact, processing, presence or settings of
    /// its own.
    /// </summary>
    internal static void Bind(this RoomEnemyPlace place, EnemyDefinitionId species, RoomEnemyDefinition definition) =>
        place.Bind(place.History is PopulatedEnemyPlace populated
            ? populated.LastSpawn with { DefinitionId = species }
            : PlacementFor(species, EnemySpawnSettings.None), definition);

    /// <summary>
    /// Sets the spawn settings the place's population entry gives its species (#1471): the place's enemy is bound again
    /// from its entry with <paramref name="settings"/>, as a population load binds it, before the check initializes it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The check has bound no enemy in the place.</exception>
    internal static void SetSpawnSettings(this RoomEnemyPlace place, EnemySpawnSettings settings)
    {
        RoomEnemyPlacement entry = place.History is PopulatedEnemyPlace populated
            ? populated.LastSpawn
            : throw new InvalidOperationException($"A check sets the spawn settings of enemy place {place.Ordinal}, where it has bound no enemy.");
        place.Bind(Settled(entry, settings), place.KeptDefinition);
    }

    /// <summary>
    /// Binds <paramref name="species"/> placed with <paramref name="settings"/>, where the place's entry places it, or at
    /// the room origin in a place no enemy has been spawned in.
    /// </summary>
    internal static void BindPlaced(this RoomEnemyPlace place, EnemyDefinitionId species, EnemySpawnSettings settings) =>
        place.Bind(Settled((place.History is PopulatedEnemyPlace populated ? populated.LastSpawn : PlacementFor(species, settings))
            with { DefinitionId = species }, settings), place.KeptDefinition);

    /// <summary>
    /// Binds <paramref name="species"/> in the place as an entry placing it at the room origin would, with no contact,
    /// processing or presence of its own and <paramref name="settings"/>.
    /// </summary>
    internal static void PopulateFor(this RoomEnemyPlace place, EnemyDefinitionId species, EnemySpawnSettings settings) =>
        place.Bind(PlacementFor(species, settings), place.KeptDefinition);

    /// <summary><paramref name="entry"/> giving its species <paramref name="settings"/> instead of its own.</summary>
    private static RoomEnemyPlacement Settled(RoomEnemyPlacement entry, EnemySpawnSettings settings) =>
        new(entry.DefinitionId, entry.Placing, entry.Contact, entry.Processing, entry.Presence, entry.CollidesByFrameHitboxes, settings);

    /// <summary>An entry placing <paramref name="species"/> at the room origin with no contact, processing or presence of its own.</summary>
    internal static RoomEnemyPlacement PlacementFor(EnemyDefinitionId species, EnemySpawnSettings settings) =>
        new(species, EnemyPlacing.At(Position16<RoomPixels>.Zero), EnemyContact.None, EnemyProcessing.None, EnemyPresence.None,
            CollidesByFrameHitboxes: false, settings);

    /// <summary>
    /// The settings a fixture slot's population entry gives its species, as the <typeparamref name="T"/> the check placed it
    /// with: the species itself receives them typed from its entry (#2554).
    /// </summary>
    internal static T SpawnSettings<T>(this RoomEnemyPlace place) where T : EnemySpawnSettings =>
        (T)place.Spawn.Population.SpawnSettings;

    /// <summary>The settings a population entry gives its species, as the <typeparamref name="T"/> the check knows it carries.</summary>
    internal static T SpawnSettingsOf<T>(this RoomEnemyPlacement placement) where T : EnemySpawnSettings =>
        (T)placement.SpawnSettings;
}

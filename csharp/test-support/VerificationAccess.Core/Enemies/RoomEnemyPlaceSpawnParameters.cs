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
        place.Bind((place.Occupant?.Spawn ?? place.LastSpawn) is { } entry
            ? entry with { DefinitionId = species }
            : PlacementFor(species, EnemySpawnSettings.None), definition);

    /// <summary>
    /// Sets the spawn settings the place's population entry gives its species (#1471): the place's enemy is bound again
    /// from its entry with <paramref name="settings"/>, as a population load binds it, before the check initializes it.
    /// </summary>
    /// <exception cref="InvalidOperationException">No enemy has been bound in the place.</exception>
    internal static void SetSpawnSettings(this RoomEnemyPlace place, EnemySpawnSettings settings)
    {
        RoomEnemyPlacement entry = (place.Occupant?.Spawn ?? place.LastSpawn) ?? throw new InvalidOperationException(
            $"A check sets enemy place {place.Ordinal}'s spawn settings before binding an enemy there.");
        place.Bind(entry with { SpawnSettings = settings }, place.Definition);
    }

    /// <summary>
    /// Binds <paramref name="species"/> placed with <paramref name="settings"/>, where the place's entry places it, or at
    /// the room origin in a place no enemy has been spawned in.
    /// </summary>
    internal static void BindPlaced(this RoomEnemyPlace place, EnemyDefinitionId species, EnemySpawnSettings settings) =>
        place.Bind(((place.Occupant?.Spawn ?? place.LastSpawn) ?? PlacementFor(species, settings))
            with { DefinitionId = species, SpawnSettings = settings }, place.Definition);

    /// <summary>
    /// Binds <paramref name="species"/> in the place as an entry placing it at the room origin would, with no contact,
    /// processing or presence of its own and <paramref name="settings"/>.
    /// </summary>
    internal static void PopulateFor(this RoomEnemyPlace place, EnemyDefinitionId species, EnemySpawnSettings settings) =>
        place.Bind(PlacementFor(species, settings), place.Definition);

    /// <summary>An entry placing <paramref name="species"/> at the room origin with no contact, processing or presence of its own.</summary>
    internal static RoomEnemyPlacement PlacementFor(EnemyDefinitionId species, EnemySpawnSettings settings) =>
        new(species, Position16<RoomPixels>.Zero, EnemyContact.None, EnemyProcessing.None, EnemyPresence.None,
            CollidesByFrameHitboxes: false, settings);
}

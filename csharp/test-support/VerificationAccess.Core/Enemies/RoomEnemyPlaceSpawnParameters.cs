namespace SuperMetroid.Core.Enemies;

/// <summary>Authors a fixture slot's spawn settings, which the enemy's initialization and authored reads take.</summary>
internal static class RoomEnemyPlaceSpawnParameters
{
    /// <summary>
    /// Sets the spawn settings the slot's population entry gives its species (#1471). A slot no population entry has
    /// populated is populated for the enemy bound in it (<see cref="PopulateFor"/>).
    /// </summary>
    /// <exception cref="InvalidOperationException">The slot is neither populated nor holding an enemy.</exception>
    internal static void SetSpawnSettings(this RoomEnemyPlace place, EnemySpawnSettings settings)
    {
        if (place.Spawn.Population is { } population)
        {
            place.Spawn = place.Spawn with { Population = population with { SpawnSettings = settings } };
            return;
        }
        RoomEnemyOccupant occupant = place.Occupant ?? throw new InvalidOperationException(
            $"A check sets enemy place {place.Ordinal}'s spawn settings before populating it or binding an enemy there.");
        place.PopulateFor(occupant.DefinitionId, settings);
    }

    /// <summary>
    /// Populates the slot as an entry placing <paramref name="species"/> at the room origin would, with no contact,
    /// processing or presence of its own and <paramref name="settings"/>, for a check that binds the enemy itself.
    /// </summary>
    internal static void PopulateFor(this RoomEnemyPlace place, EnemyDefinitionId species, EnemySpawnSettings settings) =>
        place.Spawn = new RoomEnemySpawnSnapshot(new RoomEnemyPlacement(species, Position16<RoomPixels>.Zero, EnemyContact.None,
            EnemyProcessing.None, EnemyPresence.None, CollidesByFrameHitboxes: false, settings));
}

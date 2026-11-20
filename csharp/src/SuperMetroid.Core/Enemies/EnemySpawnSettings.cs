namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a population entry tells the species it spawns how to start (#1471): each species that is told anything reads
/// its own derived settings, such as a crawler's surface or a shutter's speed and direction, decoded once by the import.
/// A species that is told nothing gets <see cref="None"/>. Each kind of settings hands itself to the factory that takes it
/// (<see cref="Place"/>), so the factory receives it typed (#2554).
/// </summary>
public record EnemySpawnSettings
{
    /// <summary>Creates the settings of a species that is told something; <see cref="None"/> is the only plain instance.</summary>
    protected EnemySpawnSettings()
    {
    }

    /// <summary>The settings of a species that its population entry tells nothing.</summary>
    public static EnemySpawnSettings None { get; } = new();

    /// <summary>
    /// Builds the enemy of the entry carrying these settings through the factory that takes them; settings that tell
    /// nothing reach the factory of the entry's definition.
    /// </summary>
    /// <param name="species">The room's species registry.</param>
    /// <param name="placing">The occupant being placed.</param>
    internal virtual EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).PlaceUntold(placing);
}

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a population entry tells the species it spawns how to start (#1471): each species that is told anything reads
/// its own derived settings, such as a crawler's surface or a shutter's speed and direction, decoded once by the import.
/// A species that is told nothing gets <see cref="None"/>.
/// </summary>
public record EnemySpawnSettings
{
    /// <summary>Creates the settings of a species that is told something; <see cref="None"/> is the only plain instance.</summary>
    protected EnemySpawnSettings()
    {
    }

    /// <summary>The settings of a species that its population entry tells nothing.</summary>
    public static EnemySpawnSettings None { get; } = new();
}

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// One of Spore Spawn's four health bands (#2242): the colours his body shows from his first damaging hit of an open
/// phase on. He is in the highest band whose <see cref="MinimumHealth"/> his health reaches, and in
/// the last of <see cref="SporeSpawnHealthBandDefinitions.HighestFirst"/> below them all.
/// </summary>
public sealed class SporeSpawnHealthBand
{
    /// <summary>Defines a band.</summary>
    /// <param name="minimumHealth">The least health in the band, or null for the lowest band.</param>
    /// <param name="key">The band's name in its shade's name.</param>
    internal SporeSpawnHealthBand(int? minimumHealth, [InstalledKey] string key)
    {
        MinimumHealth = minimumHealth;
        Key = key;
    }

    /// <summary>The least health in the band, or null for the lowest band, which holds all the health below the others.</summary>
    public int? MinimumHealth { get; }

    /// <summary>The band's name in its shade's name.</summary>
    public string Key { get; }

    /// <summary>The band Spore Spawn is in at <paramref name="health"/>.</summary>
    internal static SporeSpawnHealthBand Of(int health) =>
        SporeSpawnHealthBandDefinitions.HighestFirst.First(band => band.MinimumHealth is not { } least || health >= least);

    /// <inheritdoc/>
    public override string ToString() => Key;
}

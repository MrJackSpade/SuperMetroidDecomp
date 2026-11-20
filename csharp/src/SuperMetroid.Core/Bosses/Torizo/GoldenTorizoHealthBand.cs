namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One of Golden Torizo's health bands (#2242): the colours his body and belly show on the hurt flash's unflashed updates.
/// He is in the highest band whose <see cref="MinimumHealth"/> his health reaches, and in
/// <see cref="GoldenTorizoHealthBandDefinitions.NearlyDead"/> below them all.
/// </summary>
public sealed class GoldenTorizoHealthBand
{
    /// <summary>Defines a band.</summary>
    /// <param name="minimumHealth">The least health in the band: zero for the lowest band, which holds all the health below the others.</param>
    /// <param name="key">The band's name in its shades' names.</param>
    internal GoldenTorizoHealthBand(int minimumHealth, [InstalledKey] string key)
    {
        MinimumHealth = minimumHealth;
        Key = key;
    }

    /// <summary>The least health in the band: zero for the lowest band, which holds all the health below the others.</summary>
    public int MinimumHealth { get; }

    /// <summary>The band's name in its shades' names.</summary>
    public string Key { get; }

    /// <summary>The band Golden Torizo is in at <paramref name="health"/>.</summary>
    internal static GoldenTorizoHealthBand Of(int health) =>
        GoldenTorizoHealthBandDefinitions.HighestFirst.First(band => health >= band.MinimumHealth);

    /// <inheritdoc/>
    public override string ToString() => Key;
}

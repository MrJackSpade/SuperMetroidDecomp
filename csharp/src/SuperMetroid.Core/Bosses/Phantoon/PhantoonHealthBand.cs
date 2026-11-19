namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One of Phantoon's eight health bands (#2259): the colours his body shows, from his own at full health to his reddest
/// below an eighth of it. He is in the highest band whose <see cref="MinimumHealth"/> his health reaches, and in
/// <see cref="PhantoonHealthBandDefinitions.BelowEighth"/> below them all.
/// </summary>
public sealed class PhantoonHealthBand
{
    /// <summary>Defines a band.</summary>
    /// <param name="minimumHealth">The least health in the band, or null for the lowest band.</param>
    /// <param name="key">The band's name in its shades' names.</param>
    internal PhantoonHealthBand(int? minimumHealth, [InstalledKey] string key)
    {
        MinimumHealth = minimumHealth;
        Key = key;
    }

    /// <summary>The least health in the band, or null for the lowest band, which holds all the health below the others.</summary>
    public int? MinimumHealth { get; }

    /// <summary>The band's name in its shades' names.</summary>
    public string Key { get; }

    /// <summary>The band Phantoon is in at <paramref name="health"/>.</summary>
    internal static PhantoonHealthBand Of(int health) =>
        PhantoonHealthBandDefinitions.HighestFirst.First(band => band.MinimumHealth is not { } least || health >= least);

    /// <inheritdoc/>
    public override string ToString() => Key;
}

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's eight health bands (#2297): the colours his body and sprite parts show as he takes damage. He is in the
/// highest band whose <see cref="MinimumEighths"/> of the health he initialized with his health reaches, and in
/// <see cref="KraidHealthBandDefinitions.BelowQuarter"/> below them all; health exactly on a band's minimum is in that band.
/// </summary>
public sealed partial class KraidHealthBand
{
    /// <summary>Defines a band.</summary>
    /// <param name="minimumEighths">The least health in the band, as eighths of his initial health, or null for the lowest band.</param>
    /// <param name="key">The band's name in its shade's name.</param>
    internal KraidHealthBand(int? minimumEighths, [InstalledKey] string key)
    {
        MinimumEighths = minimumEighths;
        Key = key;
    }

    /// <summary>
    /// The least health in the band, as eighths of the health he initialized with (<see cref="KraidHealthThresholds.Eighths"/>),
    /// or null for the lowest band, which holds all the health below the others.
    /// </summary>
    public int? MinimumEighths { get; }

    /// <summary>The band's name in its shade's name.</summary>
    public string Key { get; }

    /// <summary>The band Kraid is in at <paramref name="health"/>, having initialized with <paramref name="initialHealth"/>.</summary>
    internal static KraidHealthBand Of(int health, int initialHealth) =>
        KraidHealthBandDefinitions.HighestFirst.First(band =>
            band.MinimumEighths is not { } least || health >= KraidHealthThresholds.Eighths(initialHealth, least));

    /// <inheritdoc/>
    public override string ToString() => Key;
}

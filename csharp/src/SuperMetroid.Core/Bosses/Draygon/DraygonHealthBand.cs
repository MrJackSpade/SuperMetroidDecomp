namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// One of Draygon's eight health bands (#2266): the colours his body layer shows as he takes damage. He is in the highest
/// band whose <see cref="MinimumHealth"/> his health reaches, and in <see cref="DraygonHealthBandDefinitions.BelowEighth"/>
/// below them all; health exactly on a band's minimum is in that band.
/// </summary>
public sealed partial class DraygonHealthBand
{
    /// <summary>Defines a band.</summary>
    /// <param name="minimumHealth">The least health in the band: zero for the lowest band, which holds all the health below the others.</param>
    /// <param name="key">The band's name in its shade's name.</param>
    internal DraygonHealthBand(int minimumHealth, [InstalledKey] string key)
    {
        MinimumHealth = minimumHealth;
        Key = key;
    }

    /// <summary>The least health in the band: zero for the lowest band, which holds all the health below the others.</summary>
    internal int MinimumHealth { get; }

    /// <summary>The band's name in its shade's name.</summary>
    public string Key { get; }

    /// <summary>
    /// The band Draygon is in at <paramref name="health"/>: the first, highest first, whose least health his health reaches,
    /// as the game's threshold search finds it, so any health at or above the highest band's least is in
    /// that band.
    /// </summary>
    internal static DraygonHealthBand Of(int health) =>
        DraygonHealthBandDefinitions.HighestFirst.First(band => health >= band.MinimumHealth);

    /// <inheritdoc/>
    public override string ToString() => Key;
}

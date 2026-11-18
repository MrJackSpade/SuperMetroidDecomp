namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// One of Botwoon's eight health bands (#2266): the colours his head and body show once his health has fallen below the
/// band's <see cref="HealthBelow"/>. He shows them in <see cref="BotwoonHealthBandDefinitions.InOrderShown"/>'s order, one
/// band an update, and keeps his own colours until his health first falls below full.
/// </summary>
public sealed class BotwoonHealthBand
{
    /// <summary>Defines a band.</summary>
    /// <param name="healthBelow">The health his health must fall below for the band to show.</param>
    /// <param name="key">The band's name in its shade's name.</param>
    internal BotwoonHealthBand(int healthBelow, [InstalledKey] string key)
    {
        HealthBelow = healthBelow;
        Key = key;
    }

    /// <summary>The health his health must fall below for the band to show.</summary>
    public int HealthBelow { get; }

    /// <summary>The band's name in its shade's name.</summary>
    public string Key { get; }

    /// <summary>
    /// The band Botwoon shows after <paramref name="shown"/>, or his first band when he still shows his own colours (null);
    /// null once he shows his last.
    /// </summary>
    internal static BotwoonHealthBand? After(BotwoonHealthBand? shown)
    {
        IReadOnlyList<BotwoonHealthBand> bands = BotwoonHealthBandDefinitions.InOrderShown;
        if (shown is null)
            return bands[0];
        for (int band = 0; band < bands.Count - 1; band++)
        {
            if (bands[band] == shown)
                return bands[band + 1];
        }
        return bands[^1] == shown ? null : throw new InvalidDataException($"Botwoon shows health band {shown}, which is not one of his.");
    }

    /// <inheritdoc/>
    public override string ToString() => Key;
}

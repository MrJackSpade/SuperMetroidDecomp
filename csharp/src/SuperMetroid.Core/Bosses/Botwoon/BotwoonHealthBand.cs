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
    /// <param name="next">The band he shows after this one; null for his last.</param>
    internal BotwoonHealthBand(int healthBelow, [InstalledKey] string key, BotwoonHealthBand? next)
    {
        HealthBelow = healthBelow;
        Key = key;
        Next = next;
    }

    /// <summary>The band he shows after this one; null for his last.</summary>
    private BotwoonHealthBand? Next { get; }

    /// <summary>The health his health must fall below for the band to show.</summary>
    public int HealthBelow { get; }

    /// <summary>The band's name in its shade's name.</summary>
    public string Key { get; }

    /// <summary>
    /// The band Botwoon shows after <paramref name="shown"/>, or his first band when he still shows his own colours (null);
    /// null once he shows his last.
    /// </summary>
    internal static BotwoonHealthBand? After(BotwoonHealthBand? shown) =>
        shown is null ? BotwoonHealthBandDefinitions.InOrderShown[0] : shown.Next;

    /// <inheritdoc/>
    public override string ToString() => Key;
}

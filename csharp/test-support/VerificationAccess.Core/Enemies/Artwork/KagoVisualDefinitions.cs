namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Kago's three distinct bank-$A8 OAM compositions. Its slow and hit-accelerated
/// instruction loops share these frames but retain separate compiled timings.
/// </summary>
internal static class KagoVisualDefinitions
{
    /// <summary>Native bank for the Kago enemy definition and spritemaps.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Spritemap_Kago_0 at $A8:ABDA begins three consecutive four-part OAM records.</summary>
    private const ushort FirstFrame = 0xabda;

    /// <summary>Each composition occupies its two-byte count plus four five-byte parts.</summary>
    internal static IEnumerable<NativeEnemySpritemapDefinition> Frames()
    {
        for (int phase = 0; phase < 3; phase++)
            yield return new(Bank, (ushort)(FirstFrame + phase * (2 + 4 * 5)), $"kago_cycle_{phase}");
    }
}

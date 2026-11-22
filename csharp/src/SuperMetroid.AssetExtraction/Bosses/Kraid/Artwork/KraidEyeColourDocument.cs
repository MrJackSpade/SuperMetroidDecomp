namespace SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;

/// <summary>
/// The installed colours of Kraid's three eyes (#1637), as a player edits them in place: the colours gameplay's eye glow
/// shows, starts from and fades back to under the hurt flash and under each of his eight health bands. The rest of his
/// colours are import data, which only pre-colours his pictures. Each colour is written as <c>#rrggbb</c> (#2564).
/// </summary>
public sealed partial record KraidEyeColourDocument
{
    /// <summary>Schema revision; loading requires <see cref="KraidEyeColourFile.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>The three eyes' colours under the hurt flash, first eye first.</summary>
    public required string[] HurtFlash { get; init; }

    /// <summary>The three eyes' colours under each of the eight health bands, keyed by the band's installed key, first eye first.</summary>
    public required Dictionary<string, string[]> HealthBands { get; init; }
}

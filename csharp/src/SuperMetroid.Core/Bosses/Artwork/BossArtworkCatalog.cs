using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>
/// The bosses' installed artwork beside the shared enemy sheets (#1395) that gameplay reads: Kraid's eye colours, which
/// his eye glow reads back (#1512). The bosses' colours, characters and frames their pre-coloured pictures and body
/// sheets are composed from belong to the import pipeline's subclass (#1463, #1466, #1512).
/// </summary>
public abstract record BossArtworkCatalog
{
    /// <summary>Kraid's installed eye colours; null only for constructed fixtures without Kraid.</summary>
    public KraidEyeColours? KraidEyes { get; init; }
}

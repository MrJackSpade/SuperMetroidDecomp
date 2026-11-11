using System.Collections.Immutable;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Where in the cartridge's 256-colour image the colours of a picture the import composed were coloured from (#1512, #1544,
/// #1633): the import's knowledge, recorded beside a picture composed against the cartridge so its composition can be
/// confirmed against the cartridge. Each colour came from a composed palette index of the half of the colour table the
/// sheet was composed against. A sprite sheet's index is its part's own row times sixteen plus its colour, coloured from
/// the sprite half's row of that row with the sheet's palette bits ORed in; a gameplay background sheet's colour group was
/// installed from the background set the composed manifest names for it; a frontend screen's background sheet names none,
/// and its groups lie in the background half in their own order; a full-palette sheet's index is its colour's place in the
/// image. Nothing of it is installed: an installed picture is only its colours, and Core never addresses a colour by these
/// indices.
/// </summary>
public static class InstalledPictureColours
{
    /// <summary>
    /// The sets a gameplay background sheet installed from the cartridge's background colours names, group after group:
    /// such a sheet keeps their order, so its group of each place was installed from the set of that place.
    /// </summary>
    public static ImmutableArray<InstalledBackgroundSet> CartridgeOrderSets { get; } = [.. InstalledBackgroundSetPositionDefinitions.InCartridgeOrder];
}

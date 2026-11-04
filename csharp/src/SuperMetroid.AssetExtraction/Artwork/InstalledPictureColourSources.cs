using System.Collections.Immutable;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Where a loaded picture's manifest says its colours were installed from (#1512): the background sets of its colour
/// groups, group after group, or none, and the palette bits a sprite sheet's draws OR into each ink's own row.
/// </summary>
/// <param name="Sets">The sets, one per colour group, or empty for a sheet that names none.</param>
/// <param name="PaletteBits">The palette bits ORed into each sprite ink's own row; zero for every other sheet.</param>
internal sealed record InstalledPictureColourSources(ImmutableArray<InstalledBackgroundSet> Sets, int PaletteBits);

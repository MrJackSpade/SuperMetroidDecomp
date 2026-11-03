using System.Collections.Immutable;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>The background sets a loaded picture's manifest names for its colour groups, group after group, or none (#1512).</summary>
/// <param name="Sets">The sets, one per colour group, or empty for a sheet that names none.</param>
internal sealed record InstalledPictureBackgroundSets(ImmutableArray<InstalledBackgroundSet> Sets);

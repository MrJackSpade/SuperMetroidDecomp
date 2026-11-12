namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>One installed sheet manifest, by its family and name.</summary>
/// <param name="Family">The family's directory name.</param>
/// <param name="Sheet">The sheet's installed name.</param>
/// <param name="Document">The manifest.</param>
internal sealed record InstalledSheetManifest(string Family, string Sheet, InstalledSheetDocument Document);

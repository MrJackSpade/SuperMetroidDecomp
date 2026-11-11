namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>A converted sheet and the name it is installed under (#1635).</summary>
/// <param name="Name">The sheet's installed name: its manifest's file name without extension.</param>
/// <param name="Sheet">What the installation holds of it.</param>
internal sealed record NamedInstalledSheet(string Name, InstalledSheet Sheet);

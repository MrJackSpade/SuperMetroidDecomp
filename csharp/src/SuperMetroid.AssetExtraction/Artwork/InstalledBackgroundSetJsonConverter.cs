using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Reads and writes a <see cref="InstalledBackgroundSet"/> in an installed document by its installed key (#2520).</summary>
internal sealed partial class InstalledBackgroundSetJsonConverter() : InstalledKeyJsonConverter<InstalledBackgroundSet>(InstalledBackgroundSetKeys.Installed);

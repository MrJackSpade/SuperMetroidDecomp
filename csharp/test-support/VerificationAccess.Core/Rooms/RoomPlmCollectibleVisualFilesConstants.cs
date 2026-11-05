namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmCollectibleVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmCollectibleVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled collectible visuals.</summary>
    /// <param name="directory">Stock collectible directory.</param>
    public static void ValidateStock(string directory) => _ = RoomPlmCollectibleVisualFiles.Load(directory);
}

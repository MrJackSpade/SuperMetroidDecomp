
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmChozoStatueVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmChozoStatueVisualFilesConstants
{
    /// <summary>Checks stock Chozo provenance, digest, schema, complete layout coverage, and equality with compiled appearances.</summary>
    /// <param name="directory">Installed stock Chozo directory.</param>
    public static void ValidateStock(string directory) => _ = RoomPlmChozoStatueVisualFiles.Load(directory);
}

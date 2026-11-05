
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmBotwoonWallVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmBotwoonWallVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Botwoon wall visuals.</summary>
    /// <param name="directory">Stock Botwoon wall directory.</param>
    public static void ValidateStock(string directory) => _ = RoomPlmBotwoonWallVisualFiles.Load(directory);
}

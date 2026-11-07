
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectBotwoonWallVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectBotwoonWallVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Botwoon wall visuals.</summary>
    /// <param name="directory">Stock Botwoon wall directory.</param>
    public static void ValidateStock(string directory) => _ = RoomObjectBotwoonWallVisualFiles.Load(directory);
}

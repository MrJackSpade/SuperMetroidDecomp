
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectBlueDoorVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectBlueDoorVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled blue-door visuals.</summary>
    /// <param name="directory">Stock blue-door directory.</param>
    public static void ValidateStock(string directory) => _ = RoomObjectBlueDoorVisualFiles.Load(directory);
}

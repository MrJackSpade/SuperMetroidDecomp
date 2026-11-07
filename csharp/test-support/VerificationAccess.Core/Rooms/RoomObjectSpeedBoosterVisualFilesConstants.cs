
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectSpeedBoosterVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectSpeedBoosterVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with the compiled Speed Booster visual.</summary>
    /// <param name="directory">Stock Speed Booster directory.</param>
    public static void ValidateStock(string directory) => _ = RoomObjectSpeedBoosterVisualFiles.Load(directory);
}

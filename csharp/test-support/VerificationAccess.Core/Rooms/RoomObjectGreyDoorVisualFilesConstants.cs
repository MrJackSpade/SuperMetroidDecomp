
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectGreyDoorVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectGreyDoorVisualFilesConstants
{
    /// <summary>Runs stock manifest, hash, schema, and compiled-appearance checks without reading a cartridge, or writing files.</summary>
    /// <param name="directory">Grey-door family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">The stock installation fails the checks performed by <see cref="RoomObjectGreyDoorVisualFiles.Load"/>.</exception>
    public static void ValidateStock(string directory) => _ = RoomObjectGreyDoorVisualFiles.Load(directory);
}

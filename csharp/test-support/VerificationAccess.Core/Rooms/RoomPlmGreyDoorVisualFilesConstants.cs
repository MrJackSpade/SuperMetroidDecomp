
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmGreyDoorVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmGreyDoorVisualFilesConstants
{
    /// <summary>Runs stock manifest, hash, schema, and compiled-appearance checks without reading a cartridge, or writing files.</summary>
    /// <param name="directory">Grey-door family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">The stock installation fails the checks performed by <see cref="RoomPlmGreyDoorVisualFiles.Load"/>.</exception>
    public static void ValidateStock(string directory) => _ = RoomPlmGreyDoorVisualFiles.Load(directory);
}


namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectDownwardGateVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectDownwardGateVisualFilesConstants
{
    /// <summary>Runs stock manifest, schema, integrity, and compiled-appearance checks through <see cref="RoomObjectDownwardGateVisualFiles.Load"/> without cartridge reads, or writes.</summary>
    /// <param name="directory">Downward-gate family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">Stock validation fails.</exception>
    public static void ValidateStock(string directory) => _ = RoomObjectDownwardGateVisualFiles.Load(directory);
}


namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmDownwardGateVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmDownwardGateVisualFilesConstants
{
    /// <summary>Runs stock manifest, schema, integrity, and compiled-appearance checks through <see cref="RoomPlmDownwardGateVisualFiles.Load"/> without cartridge reads, or writes.</summary>
    /// <param name="directory">Downward-gate family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">Stock validation fails.</exception>
    public static void ValidateStock(string directory) => _ = RoomPlmDownwardGateVisualFiles.Load(directory);
}


namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmMotherBrainGlassVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmMotherBrainGlassVisualFilesConstants
{
    /// <summary>Runs stock glass manifest, integrity, schema, and compiled-appearance checks without cartridge reads, or file writes.</summary>
    /// <param name="stockDirectory">Glass family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">The stock installation fails the checks performed by <see cref="RoomPlmMotherBrainGlassVisualFiles.Load"/>.</exception>
    public static void ValidateStock(string stockDirectory) => RoomPlmMotherBrainGlassVisualFiles.Load(stockDirectory);
}

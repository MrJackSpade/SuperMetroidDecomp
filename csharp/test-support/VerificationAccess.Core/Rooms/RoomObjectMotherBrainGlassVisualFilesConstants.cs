
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectMotherBrainGlassVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectMotherBrainGlassVisualFilesConstants
{
    /// <summary>Runs stock glass manifest, integrity, schema, and compiled-appearance checks without cartridge reads, or file writes.</summary>
    /// <param name="stockDirectory">Glass family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">The stock installation fails the checks performed by <see cref="RoomObjectMotherBrainGlassVisualFiles.Load"/>.</exception>
    public static void ValidateStock(string stockDirectory) => RoomObjectMotherBrainGlassVisualFiles.Load(stockDirectory);
}

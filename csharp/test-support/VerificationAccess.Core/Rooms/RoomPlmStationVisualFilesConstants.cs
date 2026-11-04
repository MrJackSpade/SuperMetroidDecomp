
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmStationVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmStationVisualFilesConstants
{
    /// <summary>Performs the stock checks from <see cref="RoomPlmStationVisualFiles.Load"/> without overrides, cartridge reads, or file writes.</summary>
    /// <param name="directory">Station family directory containing stock JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = RoomPlmStationVisualFiles.Load(directory, null);
}

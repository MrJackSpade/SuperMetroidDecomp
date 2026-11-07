
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectStationVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectStationVisualFilesConstants
{
    /// <summary>Performs the stock checks from <see cref="RoomObjectStationVisualFiles.Load"/> without cartridge reads, or file writes.</summary>
    /// <param name="directory">Station family directory containing stock JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = RoomObjectStationVisualFiles.Load(directory);
}

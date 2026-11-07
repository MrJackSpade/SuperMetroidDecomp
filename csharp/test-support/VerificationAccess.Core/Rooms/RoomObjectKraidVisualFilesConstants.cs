
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectKraidVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectKraidVisualFilesConstants
{
    /// <summary>Validates the stock Kraid installation through <see cref="RoomObjectKraidVisualFiles.Load"/> with no cartridge reads, or file writes.</summary>
    /// <param name="directory">Family directory containing the Kraid visual JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = RoomObjectKraidVisualFiles.Load(directory);
}

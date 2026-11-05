
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmKraidVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmKraidVisualFilesConstants
{
    /// <summary>Validates the stock Kraid installation through <see cref="RoomPlmKraidVisualFiles.Load"/> with no cartridge reads, or file writes.</summary>
    /// <param name="directory">Family directory containing the Kraid visual JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = RoomPlmKraidVisualFiles.Load(directory);
}

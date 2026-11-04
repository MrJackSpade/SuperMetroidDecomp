
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmMotherBrainFakeDeathVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmMotherBrainFakeDeathVisualFilesConstants
{
    /// <summary>Checks stock fake-death room assets through <see cref="RoomPlmMotherBrainFakeDeathVisualFiles.Load"/> without selecting overrides, reading a cartridge, or writing files.</summary>
    /// <param name="directory">Family directory containing fake-death room JSON and its manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled visual checks fail.</exception>
    public static void ValidateStock(string directory) => _ = RoomPlmMotherBrainFakeDeathVisualFiles.Load(directory, null);
}

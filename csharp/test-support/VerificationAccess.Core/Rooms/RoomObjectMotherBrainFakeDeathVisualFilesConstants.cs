
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectMotherBrainFakeDeathVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectMotherBrainFakeDeathVisualFilesConstants
{
    /// <summary>Checks stock fake-death room assets through <see cref="RoomObjectMotherBrainFakeDeathVisualFiles.Load"/> without reading a cartridge, or writing files.</summary>
    /// <param name="directory">Family directory containing fake-death room JSON and its manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled visual checks fail.</exception>
    public static void ValidateStock(string directory) => _ = RoomObjectMotherBrainFakeDeathVisualFiles.Load(directory);
}

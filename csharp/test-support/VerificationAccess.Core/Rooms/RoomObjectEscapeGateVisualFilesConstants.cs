
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectEscapeGateVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectEscapeGateVisualFilesConstants
{
    /// <summary>Checks stock escape-gate provenance, digest, schema, three-frame coverage, and equality with compiled appearances.</summary>
    /// <param name="directory">Installed stock escape-gate directory.</param>
    public static void ValidateStock(string directory) => _ = RoomObjectEscapeGateVisualFiles.Load(directory);
}

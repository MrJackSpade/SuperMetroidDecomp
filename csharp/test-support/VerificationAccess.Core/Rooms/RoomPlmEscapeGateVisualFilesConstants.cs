
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmEscapeGateVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmEscapeGateVisualFilesConstants
{
    /// <summary>Checks stock escape-gate provenance, digest, schema, three-frame coverage, and equality with compiled appearances; ignores overrides.</summary>
    /// <param name="directory">Installed stock escape-gate directory.</param>
    public static void ValidateStock(string directory) => _ = RoomPlmEscapeGateVisualFiles.Load(directory, null);
}

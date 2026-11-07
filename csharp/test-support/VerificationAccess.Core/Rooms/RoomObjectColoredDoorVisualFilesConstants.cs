
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectColoredDoorVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectColoredDoorVisualFilesConstants
{
    /// <summary>Checks stock colored-door provenance, digest, schema, forty-eight-frame coverage, and equality with compiled appearance.</summary>
    /// <param name="directory">Installed stock colored-door directory.</param>
    public static void ValidateStock(string directory) => _ = RoomObjectColoredDoorVisualFiles.Load(directory);
}

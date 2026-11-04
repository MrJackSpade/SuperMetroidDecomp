
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmColoredDoorVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmColoredDoorVisualFilesConstants
{
    /// <summary>Checks stock colored-door provenance, digest, schema, forty-eight-frame coverage, and equality with compiled appearance; ignores overrides.</summary>
    /// <param name="directory">Installed stock colored-door directory.</param>
    public static void ValidateStock(string directory) => _ = RoomPlmColoredDoorVisualFiles.Load(directory, null);
}


namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectTourianAccessVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectTourianAccessVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Tourian access visuals.</summary>
    /// <param name="directory">Stock Tourian access directory.</param>
    public static void ValidateStock(string directory) => _ = RoomObjectTourianAccessVisualFiles.Load(directory);
}

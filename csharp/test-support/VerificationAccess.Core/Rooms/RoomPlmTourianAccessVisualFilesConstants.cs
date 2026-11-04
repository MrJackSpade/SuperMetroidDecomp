
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmTourianAccessVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmTourianAccessVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Tourian access visuals.</summary>
    /// <param name="directory">Stock Tourian access directory; overrides are ignored.</param>
    public static void ValidateStock(string directory) => _ = RoomPlmTourianAccessVisualFiles.Load(directory, null);
}

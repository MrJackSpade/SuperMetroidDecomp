
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmBlueDoorVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmBlueDoorVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled blue-door visuals.</summary>
    /// <param name="directory">Stock blue-door directory; overrides are ignored.</param>
    public static void ValidateStock(string directory) => _ = RoomPlmBlueDoorVisualFiles.Load(directory, null);
}

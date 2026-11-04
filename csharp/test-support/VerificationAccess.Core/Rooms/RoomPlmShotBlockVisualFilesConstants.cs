
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmShotBlockVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmShotBlockVisualFilesConstants
{
    /// <summary>Runs stock provenance, hash, schema, and compiled-appearance checks through <see cref="RoomPlmShotBlockVisualFiles.Load"/> without overrides, cartridge access, or writes.</summary>
    /// <param name="directory">Shot-block family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">Stock validation fails.</exception>
    public static void ValidateStock(string directory) => _ = RoomPlmShotBlockVisualFiles.Load(directory, null);
}

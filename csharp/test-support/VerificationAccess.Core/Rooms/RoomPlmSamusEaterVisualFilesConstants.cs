
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmSamusEaterVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmSamusEaterVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Samus Eater visuals.</summary>
    /// <param name="directory">Stock Samus Eater directory.</param>
    public static void ValidateStock(string directory) => _ = RoomPlmSamusEaterVisualFiles.Load(directory);
}

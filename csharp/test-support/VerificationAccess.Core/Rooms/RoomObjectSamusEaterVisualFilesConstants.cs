
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectSamusEaterVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectSamusEaterVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Samus Eater visuals.</summary>
    /// <param name="directory">Stock Samus Eater directory.</param>
    public static void ValidateStock(string directory) => _ = RoomObjectSamusEaterVisualFiles.Load(directory);
}

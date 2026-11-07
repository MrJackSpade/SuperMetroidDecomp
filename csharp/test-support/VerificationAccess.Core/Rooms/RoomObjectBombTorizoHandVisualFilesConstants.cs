
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectBombTorizoHandVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectBombTorizoHandVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Bomb Torizo hand visuals.</summary>
    /// <param name="directory">Stock Bomb Torizo hand directory.</param>
    public static void ValidateStock(string directory) => _ = RoomObjectBombTorizoHandVisualFiles.Load(directory);
}

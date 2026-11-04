
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmBombTorizoHandVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmBombTorizoHandVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Bomb Torizo hand visuals.</summary>
    /// <param name="directory">Stock Bomb Torizo hand directory; overrides are ignored.</param>
    public static void ValidateStock(string directory) => _ = RoomPlmBombTorizoHandVisualFiles.Load(directory, null);
}

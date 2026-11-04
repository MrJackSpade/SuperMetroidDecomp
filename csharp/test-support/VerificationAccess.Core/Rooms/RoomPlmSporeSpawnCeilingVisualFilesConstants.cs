
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmSporeSpawnCeilingVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmSporeSpawnCeilingVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Spore Spawn ceiling visuals.</summary>
    /// <param name="directory">Stock Spore Spawn ceiling directory; overrides are ignored.</param>
    public static void ValidateStock(string directory) => _ = RoomPlmSporeSpawnCeilingVisualFiles.Load(directory, null);
}

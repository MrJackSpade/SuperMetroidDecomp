
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectSporeSpawnCeilingVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectSporeSpawnCeilingVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Spore Spawn ceiling visuals.</summary>
    /// <param name="directory">Stock Spore Spawn ceiling directory.</param>
    public static void ValidateStock(string directory) => _ = RoomObjectSporeSpawnCeilingVisualFiles.Load(directory);
}

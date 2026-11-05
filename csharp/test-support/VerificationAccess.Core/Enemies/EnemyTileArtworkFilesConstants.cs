
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyTileArtworkFiles"/> that only verification reads.</summary>
internal static class EnemyTileArtworkFilesConstants
{
    /// <summary>Validates and compiles the complete installed enemy-art catalog without selecting any player overrides.</summary>
    /// <param name="stockDirectory">Directory containing the enemy manifest and all ordinary and supplemental stock resources.</param>
    /// <remarks>Checks the supported cartridge identity, retail definition coverage, manifest hashes, and resource schemas without cartridge access or file writes.</remarks>
    /// <exception cref="InvalidDataException">Stock provenance, coverage, dimensions, or an artwork document is invalid.</exception>
    public static void ValidateStock(string stockDirectory) => _ = EnemyTileArtworkFiles.Load(stockDirectory);
}

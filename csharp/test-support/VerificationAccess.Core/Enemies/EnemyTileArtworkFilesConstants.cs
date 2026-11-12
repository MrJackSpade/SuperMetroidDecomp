
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyTileArtworkFiles"/> that only verification reads.</summary>
internal static class EnemyTileArtworkFilesConstants
{
    /// <summary>Validates and compiles what the game reads of the installed enemy documents (#1636).</summary>
    /// <param name="stockDirectory">Directory containing the enemy manifest and the installed enemy documents.</param>
    /// <remarks>Checks the supported cartridge identity, retail definition coverage and the frame and colour documents' schemas without cartridge access or file writes.</remarks>
    /// <exception cref="InvalidDataException">Stock provenance, coverage, or an artwork document is invalid.</exception>
    public static void ValidateStock(string stockDirectory) => _ = EnemyTileArtworkFiles.Load(stockDirectory);
}

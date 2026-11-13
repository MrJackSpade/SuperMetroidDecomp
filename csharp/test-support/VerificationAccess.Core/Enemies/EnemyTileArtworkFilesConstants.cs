
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyTileArtworkFiles"/> that only verification reads.</summary>
internal static class EnemyTileArtworkFilesConstants
{
    /// <summary>Validates and compiles what the game reads of the installed enemy documents (#1636).</summary>
    /// <param name="stockDirectory">Directory containing the installed enemy documents.</param>
    /// <remarks>Checks the frame and colour documents' schemas without cartridge access or file writes; it opens no manifest (#1693).</remarks>
    /// <exception cref="InvalidDataException">An installed document is invalid.</exception>
    public static void ValidateStock(string stockDirectory) => _ = EnemyTileArtworkFiles.Load(stockDirectory);
}

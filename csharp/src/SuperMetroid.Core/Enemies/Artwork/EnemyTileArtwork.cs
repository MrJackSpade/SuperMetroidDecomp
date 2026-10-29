using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The installed ordinary enemy artwork's colouring: an enemy's palette loaded where its graphics set shows it.</summary>
internal static class EnemyTileArtwork
{
    /// <summary>Loads the sixteen indexed colors selected by a room graphics-set record.</summary>
    /// <exception cref="InvalidDataException">The enemy has no installed palette.</exception>
    internal static void LoadPaletteTo(EnemyTileArtworkCatalog artwork, EnemyDefinitionId definitionId, ColourRun destination)
    {
        if (!artwork.TryGetPalette(definitionId, out EnemyPaletteSheet? palette))
            throw new InvalidDataException($"Enemy {definitionId} has no installed palette.");
        palette.LoadTo(destination);
    }
}

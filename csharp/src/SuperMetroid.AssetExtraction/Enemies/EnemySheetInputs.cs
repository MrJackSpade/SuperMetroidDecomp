using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The installed stock artwork the enemy sheets compose from (#1392), loaded once for all of them.</summary>
/// <param name="Enemies">The enemies' tiles, palettes, compositions and extended frames.</param>
/// <param name="StandardObjects">The standard OBJ characters a room load uploads first.</param>
/// <param name="Palettes">The starting and common sprite colours a room load copies.</param>
internal sealed record EnemySheetInputs(EnemyTileArtworkCatalog Enemies, ReadOnlyMemory<byte> StandardObjects, GameplayBasePaletteCatalog Palettes)
{
    /// <summary>Loads the stock artwork from installed content.</summary>
    internal static EnemySheetInputs Load(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        return new(
            EnemyTileArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.EnemyTileDirectoryName), null).Enemies,
            StandardObjectArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.StandardObjectDirectoryName), null).Transfer,
            GameplayBasePaletteFiles.Load(Path.Combine(content, GameInstallationLayout.GameplayBasePaletteDirectoryName), null));
    }
}

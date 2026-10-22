using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed stock artwork the enemy sheets compose from (#1392), loaded once for all of them. It resolves the
/// artwork an encounter uploads over the characters after its room loads, as the game's installed artwork does.
/// </summary>
/// <param name="Enemies">The enemies' tiles, palettes, compositions and extended frames.</param>
/// <param name="Bosses">The bosses' artwork, which their encounters upload.</param>
/// <param name="Areas">The areas' artwork, which their encounters upload.</param>
/// <param name="StandardObjects">The standard OBJ characters a room load uploads first.</param>
/// <param name="Palettes">The starting and common sprite colours a room load copies.</param>
internal sealed record EnemySheetInputs(EnemyTileArtworkCatalog Enemies, BossArtworkCatalog Bosses, AreaArtworkCatalog Areas,
    ReadOnlyMemory<byte> StandardObjects, GameplayBasePaletteCatalog Palettes)
    : IObjCharacterSourceResolver, IInstalledObjCharacterArtwork, IInstalledBossObjCharacterArtwork, IInstalledAreaObjCharacterArtwork
{
    /// <summary>Loads the stock artwork from installed content.</summary>
    internal static EnemySheetInputs Load(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        InstalledEnemyArtwork installed = EnemyTileArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.EnemyTileDirectoryName), null);
        return new(
            installed.Enemies,
            installed.Bosses,
            installed.Areas,
            StandardObjectArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.StandardObjectDirectoryName), null).Transfer,
            GameplayBasePaletteFiles.Load(Path.Combine(content, GameInstallationLayout.GameplayBasePaletteDirectoryName), null));
    }

    /// <inheritdoc/>
    EnemyTileArtworkCatalog IInstalledObjCharacterArtwork.EnemyTiles => Enemies;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">No encounter uploads the room-FX tiles over the characters.</exception>
    RoomFxAnimatedTileAtlas IInstalledObjCharacterArtwork.AnimatedTiles => throw new InvalidOperationException(
        "No encounter uploads room-FX animated tiles over the OBJ characters an enemy sheet composes over.");

    /// <inheritdoc/>
    BossArtworkCatalog IInstalledBossObjCharacterArtwork.BossTiles => Bosses;

    /// <inheritdoc/>
    AreaArtworkCatalog IInstalledAreaObjCharacterArtwork.AreaTiles => Areas;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">No catalogued upload names a VRAM asset.</exception>
    ReadOnlyMemory<byte> IObjCharacterSourceResolver.ResolveAsset(VramAssetId asset) => throw new InvalidOperationException(
        $"No catalogued encounter upload names VRAM asset {asset}.");

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">No catalogued upload names a native transfer.</exception>
    ReadOnlyMemory<byte> IObjCharacterSourceResolver.ResolveTransfer(int sourceAddress, int byteCount) => throw new InvalidOperationException(
        $"No catalogued encounter upload names a native transfer from ${sourceAddress:X6}.");
}

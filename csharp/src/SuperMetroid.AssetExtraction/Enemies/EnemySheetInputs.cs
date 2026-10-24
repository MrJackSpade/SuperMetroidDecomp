using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Areas.Escape;

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
/// <param name="EscapeTimer">The escape timer's characters, which escapes upload.</param>
internal sealed record EnemySheetInputs(EnemyTileArtworkCatalog Enemies, BossArtworkCatalog Bosses, AreaArtworkCatalog Areas,
    ReadOnlyMemory<byte> StandardObjects, GameplayBasePaletteCatalog Palettes, EscapeTimerTileAtlas EscapeTimer)
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
            StandardObjectArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.StandardObjectDirectoryName), null).Characters,
            GameplayBasePaletteFiles.Load(Path.Combine(content, GameInstallationLayout.GameplayBasePaletteDirectoryName), null),
            LoadEscapeTimer(Path.Combine(content, GameInstallationLayout.MapDirectoryName, EscapeTimerTileAtlasFormat.FileName)));
    }

    /// <summary>The installed escape timer characters at <paramref name="path"/>.</summary>
    private static EscapeTimerCharacters LoadEscapeTimer(string path)
    {
        using FileStream png = File.OpenRead(path);
        return EscapeTimerCharacters.Read(png);
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
    /// <exception cref="InvalidDataException">The enemy artwork holds no gunship takeoff pictures.</exception>
    /// <exception cref="ArgumentOutOfRangeException">No catalogued upload names the asset.</exception>
    ReadOnlyMemory<byte> IObjCharacterSourceResolver.ResolveAsset(VramAssetId asset) =>
        asset is VramAssetId.EscapeTimerFirstTiles or VramAssetId.EscapeTimerSecondTiles ? EscapeTimer.Resolve(asset)
            : (Enemies.GunshipLiftoff ?? throw new InvalidDataException("Enemy sheets require the installed gunship takeoff pictures.")).Resolve(asset);

}

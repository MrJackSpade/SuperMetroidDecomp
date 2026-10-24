using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Resolves enemy tile transfers and named enemy, boss and area characters from the enemy, boss and area artwork
/// catalogs, for the VRAM queue and the OBJ character page, as the runtime's artwork does for its bound artwork. It
/// serves typed assets only from a supplied escape-timer atlas, and no animated tiles.
/// </summary>
/// <param name="installed">The enemy, boss and area artwork whose transfers and characters are served.</param>
/// <param name="timer">The escape-timer atlas whose pages are served as typed assets, or null for none.</param>
internal sealed class EnemyTileTransfers(InstalledEnemyArtwork installed,
    SuperMetroid.Core.Escape.Artwork.EscapeTimerTileAtlas? timer = null)
    : IVramAssetProvider, IInstalledArtworkTransferSource, IObjCharacterSourceResolver, IInstalledObjCharacterArtwork,
        IInstalledBossObjCharacterArtwork, IInstalledAreaObjCharacterArtwork
{
    /// <inheritdoc/>
    public EnemyTileArtworkCatalog EnemyTiles => installed.Enemies;

    /// <inheritdoc/>
    public BossArtworkCatalog BossTiles => installed.Bosses;

    /// <inheritdoc/>
    public AreaArtworkCatalog AreaTiles => installed.Areas;

    /// <summary>Rejects animated tiles, which enemy tile fixtures never bind.</summary>
    public SuperMetroid.Core.Fx.Artwork.RoomFxAnimatedTileAtlas AnimatedTiles =>
        throw new InvalidOperationException("Enemy tile fixtures bind no animated tiles.");
    /// <summary>Resolves an escape-timer page from the supplied atlas; enemy tile fixtures queue no other asset.</summary>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
        timer?.Resolve(asset) ?? throw new InvalidOperationException($"Enemy tile fixtures queue no asset {asset}.");

    /// <summary>Resolves a native enemy, Ceres or Torizo transfer, in the runtime artwork's order.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        if (installed.Enemies.TryResolve(sourceAddress, byteCount, out data))
            return true;
        if (installed.Areas.CeresEscapeTiles?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (installed.Areas.CeresEscapeOverlayTilemaps?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (installed.Bosses.TorizoSwaps?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        data = default;
        return false;
    }

    /// <summary>Resolves an escape-timer page from the supplied atlas; enemy tile fixtures bind no other asset.</summary>
    public ReadOnlyMemory<byte> ResolveAsset(VramAssetId asset) => Resolve(asset);

    /// <summary>Resolves a recorded native enemy tile transfer.</summary>
    public ReadOnlyMemory<byte> ResolveCartridgeSource(int sourceAddress, int byteCount) =>
        TryResolve(sourceAddress, byteCount, out ReadOnlyMemory<byte> data) ? data
            : throw new InvalidOperationException($"No enemy tiles at ${sourceAddress:X6}.");

}

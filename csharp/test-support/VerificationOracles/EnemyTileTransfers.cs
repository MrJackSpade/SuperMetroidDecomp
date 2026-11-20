using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Areas.Escape;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Resolves enemy tile transfers and named enemy, boss and area characters from the enemy, boss and area artwork
/// catalogs, for the VRAM queue and the OBJ character page, as the runtime's artwork does for its bound artwork. It
/// serves typed assets only from a supplied escape-timer atlas, and no animated tiles.
/// </summary>
/// <param name="installed">The enemy, boss and area artwork whose transfers and characters are served.</param>
/// <param name="timer">The escape-timer atlas whose pages are served as typed assets, or null for none.</param>
internal sealed class EnemyTileTransfers(ImportEnemyArtwork installed,
    SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerCharacters? timer = null)
    : IInstalledArtworkContent, IInstalledArtworkTransferSource, IInstalledSpriteCharacterArtwork,
        IInstalledBossSpriteCharacterArtwork, IInstalledAreaSpriteCharacterArtwork
{
    /// <inheritdoc/>
    public EnemyTileSourceArtwork EnemyTiles => installed.Enemies;

    /// <inheritdoc/>
    public BossSourceArtwork BossTiles => installed.Bosses;

    /// <inheritdoc/>
    public AreaSourceArtwork AreaTiles => installed.Areas;

    /// <summary>Resolves an escape-timer page from the supplied atlas; enemy tile fixtures queue no other asset.</summary>
    public ReadOnlyMemory<byte> Resolve(InstalledArtworkId asset) =>
        timer is null ? throw new InvalidOperationException($"Enemy tile fixtures queue no asset {asset}.") : EscapeTimerTilePageTransfers.Resolve(timer, asset);

    /// <summary>Resolves a native Ceres or Torizo transfer, in the runtime artwork's order; enemy tiles are named by identity (#1412).</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        if (installed.Areas.CeresEscapeTiles?.TryResolveNative(sourceAddress, byteCount, out data) == true)
            return true;
        if (installed.Areas.CeresEscapeOverlayTilemaps?.TryResolveNative(sourceAddress, byteCount, out data) == true)
            return true;
        if (installed.Bosses.TorizoSwaps?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        data = default;
        return false;
    }



}

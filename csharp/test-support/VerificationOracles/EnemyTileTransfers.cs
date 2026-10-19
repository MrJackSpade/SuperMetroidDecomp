using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Resolves enemy tile transfers and named enemy and boss characters from one enemy artwork catalog, for the VRAM queue
/// and the OBJ character page, as the runtime's artwork does for its bound enemy tiles. It serves typed assets only from
/// a supplied escape-timer atlas, and no animated tiles.
/// </summary>
/// <param name="artwork">The enemy artwork whose tile transfers are served.</param>
/// <param name="timer">The escape-timer atlas whose pages are served as typed assets, or null for none.</param>
internal sealed class EnemyTileTransfers(EnemyTileArtworkCatalog artwork,
    SuperMetroid.Core.Areas.Escape.Artwork.EscapeTimerTileAtlas? timer = null)
    : IVramAssetProvider, IInstalledArtworkTransferSource, IObjCharacterSourceResolver, IInstalledObjCharacterArtwork
{
    /// <inheritdoc/>
    public EnemyTileArtworkCatalog EnemyTiles => artwork;

    /// <summary>Rejects animated tiles, which enemy tile fixtures never bind.</summary>
    public SuperMetroid.Core.Fx.Artwork.RoomFxAnimatedTileAtlas AnimatedTiles =>
        throw new InvalidOperationException("Enemy tile fixtures bind no animated tiles.");
    /// <summary>Resolves an escape-timer page from the supplied atlas; enemy tile fixtures queue no other asset.</summary>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
        timer?.Resolve(asset) ?? throw new InvalidOperationException($"Enemy tile fixtures queue no asset {asset}.");

    /// <summary>Resolves a native enemy tile transfer.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data) => artwork.TryResolve(sourceAddress, byteCount, out data);

    /// <summary>Resolves an escape-timer page from the supplied atlas; enemy tile fixtures bind no other asset.</summary>
    public ReadOnlyMemory<byte> ResolveAsset(VramAssetId asset) => Resolve(asset);

    /// <summary>Resolves a recorded native enemy tile transfer.</summary>
    public ReadOnlyMemory<byte> ResolveTransfer(int sourceAddress, int byteCount) =>
        artwork.TryResolve(sourceAddress, byteCount, out ReadOnlyMemory<byte> data) ? data
            : throw new InvalidOperationException($"No enemy tiles at ${sourceAddress:X6}.");

}

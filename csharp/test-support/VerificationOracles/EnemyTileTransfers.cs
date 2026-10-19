using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Resolves enemy tile transfers from one enemy artwork catalog, for the VRAM queue and the OBJ character page, as the
/// runtime's artwork does for its bound enemy tiles. It serves no typed assets or skeleton chunks.
/// </summary>
/// <param name="artwork">The enemy artwork whose tile transfers are served.</param>
internal sealed class EnemyTileTransfers(EnemyTileArtworkCatalog artwork)
    : IVramAssetProvider, IInstalledArtworkTransferSource, IObjCharacterSourceResolver
{
    /// <summary>Rejects typed assets, which enemy tile fixtures never queue.</summary>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset) => throw new InvalidOperationException($"Enemy tile fixtures queue no asset {asset}.");

    /// <summary>Resolves a native enemy tile transfer.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data) => artwork.TryResolve(sourceAddress, byteCount, out data);

    /// <summary>Rejects typed assets, which enemy tile fixtures never bind.</summary>
    public ReadOnlyMemory<byte> ResolveAsset(VramAssetId asset) => Resolve(asset);

    /// <summary>Resolves a recorded native enemy tile transfer.</summary>
    public ReadOnlyMemory<byte> ResolveTransfer(int sourceAddress, int byteCount) =>
        artwork.TryResolve(sourceAddress, byteCount, out ReadOnlyMemory<byte> data) ? data
            : throw new InvalidOperationException($"No enemy tiles at ${sourceAddress:X6}.");

    /// <summary>Rejects skeleton chunks, which enemy tile fixtures never bind.</summary>
    public ReadOnlyMemory<byte> ResolveCrocomireSkeletonChunk(int entry) =>
        throw new InvalidOperationException($"Enemy tile fixtures bind no skeleton chunk {entry}.");
}

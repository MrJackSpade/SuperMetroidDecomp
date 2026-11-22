using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Serves a beam catalog's characters to the verification queue by the native transfer that selects them: the game queues
/// a beam's characters by that transfer and never by an installed asset.
/// </summary>
/// <param name="catalog">The beam catalog.</param>
internal sealed class BeamTileTransfers(BeamTileCatalog catalog) : IInstalledArtworkContent, IInstalledArtworkTransferSource
{
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Always: no beam upload is queued by an installed asset.</exception>
    public ReadOnlyMemory<byte> Resolve(InstalledArtworkId asset) =>
        throw new InvalidOperationException($"A beam upload is queued by its native transfer, never by the installed asset {asset}.");

    /// <inheritdoc/>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data) => catalog.TryResolve(sourceAddress, byteCount, out data);
}

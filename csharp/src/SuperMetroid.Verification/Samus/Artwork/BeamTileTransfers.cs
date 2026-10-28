using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Serves a beam catalog's characters to the verification queue, by asset and by the native transfer that selects them.</summary>
/// <param name="catalog">The beam catalog.</param>
internal sealed class BeamTileTransfers(BeamTileCatalog catalog) : IInstalledArtworkContent, IInstalledArtworkTransferSource
{
    /// <inheritdoc/>
    public ReadOnlyMemory<byte> Resolve(InstalledArtworkId asset) => catalog.Resolve(asset);

    /// <inheritdoc/>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data) => catalog.TryResolve(sourceAddress, byteCount, out data);
}

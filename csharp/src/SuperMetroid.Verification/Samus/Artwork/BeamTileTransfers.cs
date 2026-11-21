using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Serves a beam catalog's characters to the verification queue, by asset and by the native transfer that selects them.</summary>
/// <param name="catalog">The beam catalog.</param>
internal sealed class BeamTileTransfers(BeamTileCatalog catalog) : IInstalledArtworkContent, IInstalledArtworkTransferSource
{
    /// <summary>The beam combination each queued beam asset uploads, every combination with artwork by its asset.</summary>
    private static readonly Dictionary<InstalledArtworkId, SamusBeamCombination> Beams = Enum.GetValues<SamusBeamCombination>()
        .Where(beam => beam is not (SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaIceWave))
        .ToDictionary(BeamTileCatalog.AssetFor);

    /// <inheritdoc/>
    public ReadOnlyMemory<byte> Resolve(InstalledArtworkId asset) => catalog.Resolve(Beams[asset]);

    /// <inheritdoc/>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data) => catalog.TryResolve(sourceAddress, byteCount, out data);
}

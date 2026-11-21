using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge keeps each grapple tile transfer's characters (#1411): the $9A:8200/8A00/9200 Tiles_GrappleBeam groups.</summary>
internal static class GrappleTileSources
{
    /// <summary>
    /// The cartridge address of each transfer's first character byte, by its asset: the endpoints a point stride apart from
    /// the first point's, the segments a segment stride apart from the character after it.
    /// </summary>
    private static readonly Dictionary<InstalledArtworkId, int> Sources = new()
    {
        [InstalledArtworkId.GrapplePointFirstTiles] = GrappleTileSourceDefinitions.FirstPointSource.Value,
        [InstalledArtworkId.GrapplePointSecondTiles] = GrappleTileSourceDefinitions.FirstPointSource.Value + GrappleTileSourceDefinitions.PointSourceStride,
        [InstalledArtworkId.GrapplePointThirdTiles] = GrappleTileSourceDefinitions.FirstPointSource.Value + GrappleTileSourceDefinitions.PointSourceStride * 2,
        [InstalledArtworkId.GrapplePointFourthTiles] = GrappleTileSourceDefinitions.FirstPointSource.Value + GrappleTileSourceDefinitions.PointSourceStride * 3,
        [GrappleTileDefinitions.Horizontal] = GrappleTileSourceDefinitions.FirstPointSource.Value + 32,
        [GrappleTileDefinitions.Diagonal] = GrappleTileSourceDefinitions.FirstPointSource.Value + 32 + GrappleTileSourceDefinitions.SegmentSourceStride,
        [GrappleTileDefinitions.Vertical] = GrappleTileSourceDefinitions.FirstPointSource.Value + 32 + GrappleTileSourceDefinitions.SegmentSourceStride * 2,
    };

    /// <summary>Where the cartridge keeps a transfer's characters.</summary>
    extension(GrappleTileTransfer transfer)
    {
        /// <summary>The cartridge address of the transfer's first character byte.</summary>
        /// <exception cref="InvalidDataException">The transfer is not a grapple tile transfer.</exception>
        internal int SourceAddress => Sources.TryGetValue(transfer.Asset, out int source)
            ? source
            : throw new InvalidDataException($"{transfer} is not a grapple tile transfer.");
    }
}

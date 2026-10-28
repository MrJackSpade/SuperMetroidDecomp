using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge keeps each grapple tile transfer's characters (#1411): the $9A:8200/8A00/9200 Tiles_GrappleBeam groups.</summary>
internal static class GrappleTileSources
{
    /// <summary>The four endpoint transfers come before the three segment transfers.</summary>
    private const int PointTransferCount = 4;

    /// <summary>Where the cartridge keeps a transfer's characters.</summary>
    extension(GrappleTileTransfer transfer)
    {
        /// <summary>The cartridge address of the transfer's first character byte.</summary>
        internal int SourceAddress
        {
            get
            {
                int index = Index(transfer);
                return index < PointTransferCount
                    ? GrappleTileSourceDefinitions.FirstPointSource.Value + GrappleTileSourceDefinitions.PointSourceStride * index
                    : GrappleTileSourceDefinitions.FirstPointSource.Value + 32 + GrappleTileSourceDefinitions.SegmentSourceStride * (index - PointTransferCount);
            }
        }
    }

    /// <summary>The transfer's place among the seven, in native order.</summary>
    private static int Index(GrappleTileTransfer transfer)
    {
        for (int index = 0; index < GrappleTileDefinitions.Transfers.Count; index++)
            if (GrappleTileDefinitions.Transfers[index] == transfer)
                return index;
        throw new InvalidDataException($"{transfer} is not a grapple tile transfer.");
    }
}

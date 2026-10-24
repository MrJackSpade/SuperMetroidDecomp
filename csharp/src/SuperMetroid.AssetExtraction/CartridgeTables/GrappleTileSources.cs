using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge keeps each grapple tile transfer's characters (#1411): the $9A:8200/8A00/9200 Tiles_GrappleBeam groups.</summary>
[CpuBusBoundary("catalog: grapple tile sources")]
internal static class GrappleTileSources
{
    /// <summary>$9A:8200, Tiles_GrappleBeam_Horizontal_Beam, the first endpoint character.</summary>
    private const int FirstPointSource = 0x9a8200;
    /// <summary>Endpoint animation characters occupy $200-byte source strides.</summary>
    private const int PointSourceStride = 0x200;
    /// <summary>Horizontal, diagonal and vertical beam groups occupy $800-byte source strides.</summary>
    private const int SegmentSourceStride = 0x800;
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
                    ? FirstPointSource + PointSourceStride * index
                    : FirstPointSource + 32 + SegmentSourceStride * (index - PointTransferCount);
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

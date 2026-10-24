using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge keeps the grapple's endpoint and rope characters (#1411).</summary>
[CpuBusBoundary("catalog: grapple tile sources")]
internal static class GrappleTileSourceDefinitions
{
    /// <summary>$9A:8200, Tiles_GrappleBeam_Horizontal_Beam, the first endpoint character.</summary>
    internal static readonly SnesAddress FirstPointSource = new(0x9a, 0x8200);
    /// <summary>Endpoint animation characters occupy $200-byte source strides.</summary>
    internal const int PointSourceStride = 0x200;
    /// <summary>Horizontal, diagonal and vertical beam groups occupy $800-byte source strides.</summary>
    internal const int SegmentSourceStride = 0x800;
}

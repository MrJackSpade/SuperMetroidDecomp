using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Grapple tile ownership and the pinned $9B:C342/C346 endpoint/angle selection tables.</summary>
public static class GrappleTileDefinitions
{
    public const string FileName = "grapple-tiles.png";
    /// <summary>Four endpoint characters followed by three groups of four segment characters.</summary>
    public const int Width = 128;
    public const int Height = 8;
    /// <summary>$9B:BFFB selects VRAM word $6200 for the endpoint character.</summary>
    public const ushort PointDestination = 0x6200;
    /// <summary>$9B:C02B selects VRAM word $6210 for the four segment characters.</summary>
    public const ushort SegmentDestination = 0x6210;
    private const VramAssetId Horizontal = VramAssetId.GrappleHorizontalSegmentTiles;
    private const VramAssetId Diagonal = VramAssetId.GrappleDiagonalSegmentTiles;
    private const VramAssetId Vertical = VramAssetId.GrappleVerticalSegmentTiles;
    private static readonly GrappleTileTransfer[] transfers =
    [
        new(VramAssetId.GrapplePointFirstTiles, 0x9a8200, 0, 32),
        new(VramAssetId.GrapplePointSecondTiles, 0x9a8400, 32, 32),
        new(VramAssetId.GrapplePointThirdTiles, 0x9a8600, 64, 32),
        new(VramAssetId.GrapplePointFourthTiles, 0x9a8800, 96, 32),
        new(Horizontal, 0x9a8220, 128, 128),
        new(Diagonal, 0x9a8a20, 256, 128),
        new(Vertical, 0x9a9220, 384, 128),
    ];
    /// <summary>$9A:8200/8A00/9200 Tiles_GrappleBeam groups; only endpoint and segment-owned characters are extracted.</summary>
    public static ReadOnlySpan<GrappleTileTransfer> Transfers => transfers;
    // Exact 64 entries at $9B:C346..C3C4; repeated sectors retain cartridge boundaries.
    private static readonly VramAssetId[] segments =
    [
        Vertical, Vertical, Vertical,
        Diagonal, Diagonal, Diagonal, Diagonal, Diagonal, Diagonal, Diagonal, Diagonal,
        Horizontal, Horizontal, Horizontal, Horizontal, Horizontal, Horizontal, Horizontal, Horizontal, Horizontal, Horizontal,
        Diagonal, Diagonal, Diagonal, Diagonal, Diagonal, Diagonal, Diagonal, Diagonal,
        Vertical, Vertical, Vertical, Vertical, Vertical, Vertical,
        Diagonal, Diagonal, Diagonal, Diagonal, Diagonal, Diagonal, Diagonal, Diagonal,
        Horizontal, Horizontal, Horizontal, Horizontal, Horizontal, Horizontal, Horizontal, Horizontal, Horizontal, Horizontal,
        Diagonal, Diagonal, Diagonal, Diagonal, Diagonal, Diagonal, Diagonal, Diagonal,
        Vertical, Vertical, Vertical,
    ];

    /// <summary>$9B:C005 folds the unsigned angle into an even table byte offset, equivalent to angle / 1024.</summary>
    public static VramAssetId SegmentAssetFor(ushort angle) => segments[angle >> 10];
    /// <summary>$9B:BFBD cycles endpoint characters at $8200/$8400/$8600/$8800, independently of rope angle.</summary>
    public static VramAssetId PointAssetFor(ushort frame) => frame switch
    {
        0 => VramAssetId.GrapplePointFirstTiles,
        1 => VramAssetId.GrapplePointSecondTiles,
        2 => VramAssetId.GrapplePointThirdTiles,
        3 => VramAssetId.GrapplePointFourthTiles,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };
    public static GrappleTileTransfer TransferFor(VramAssetId asset)
    {
        foreach (var transfer in transfers) if (transfer.Asset == asset) return transfer;
        throw new InvalidDataException($"Not a Grapple tile asset: {asset}.");
    }
}

/// <summary>One native transfer and its planar-byte position in the sixteen-character replacement sheet.</summary>
public readonly record struct GrappleTileTransfer(VramAssetId Asset, int SourceAddress, int AtlasOffset, ushort ByteCount);

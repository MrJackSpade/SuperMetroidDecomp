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
    /// <summary>$9A:8200, Tiles_GrappleBeam_Horizontal_Beam, the first endpoint character.</summary>
    private const int FirstPointSource = 0x9a8200;
    /// <summary>Endpoint animation characters occupy $200-byte source strides.</summary>
    private const int PointSourceStride = 0x200;
    /// <summary>Horizontal, diagonal and vertical beam groups occupy $800-byte source strides.</summary>
    private const int SegmentSourceStride = 0x800;
    private static readonly TransferList transfers = new();
    /// <summary>$9A:8200/8A00/9200 Tiles_GrappleBeam groups; only endpoint and segment-owned characters are extracted.</summary>
    public static IReadOnlyList<GrappleTileTransfer> Transfers => transfers;

    /// <summary>$9B:C005 folds the unsigned angle into an even table byte offset, equivalent to angle / 1024.</summary>
    public static VramAssetId SegmentAssetFor(ushort angle)
    {
        // The 64 sectors repeat after half a turn. Reflect each half-turn about
        // its midpoint, retaining the native three/eight/five sector widths.
        int halfTurnSector = (angle >> 10) & 31;
        int distance = Math.Min(halfTurnSector, 31 - halfTurnSector);
        return distance < 3 ? Vertical : distance < 11 ? Diagonal : Horizontal;
    }
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
        int index = asset switch
        {
            VramAssetId.GrapplePointFirstTiles => 0,
            VramAssetId.GrapplePointSecondTiles => 1,
            VramAssetId.GrapplePointThirdTiles => 2,
            VramAssetId.GrapplePointFourthTiles => 3,
            Horizontal => 4,
            Diagonal => 5,
            Vertical => 6,
            _ => throw new InvalidDataException($"Not a Grapple tile asset: {asset}."),
        };
        return transfers[index];
    }

    private sealed class TransferList : IReadOnlyList<GrappleTileTransfer>
    {
        public int Count => 7;
        public GrappleTileTransfer this[int index]
        {
            get
            {
                if ((uint)index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                if (index < 4)
                    return new(PointAssetFor((ushort)index),
                        FirstPointSource + PointSourceStride * index, 32 * index, 32);
                int orientation = index - 4;
                VramAssetId asset = orientation switch
                {
                    0 => Horizontal,
                    1 => Diagonal,
                    _ => Vertical,
                };
                return new(asset, FirstPointSource + 32 + SegmentSourceStride * orientation,
                    128 * (orientation + 1), 128);
            }
        }
        public IEnumerator<GrappleTileTransfer> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>One native transfer and its planar-byte position in the sixteen-character replacement sheet.</summary>
public readonly record struct GrappleTileTransfer(VramAssetId Asset, int SourceAddress, int AtlasOffset, ushort ByteCount);

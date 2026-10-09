using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Grapple tile ownership and the pinned $9B:C342/C346 endpoint/angle selection tables.</summary>
public static class GrappleTileDefinitions
{
    /// <summary>Indexed PNG resource for the four endpoint-animation characters and three four-character rope-orientation groups; importing artwork leaves native angle selection and endpoint cadence unchanged.</summary>
    public const string FileName = "grapple-tiles.png";
    /// <summary>Four endpoint characters followed by three groups of four segment characters.</summary>
    public const int Width = 128;
    /// <summary>Sheet height in pixels: one row of sixteen 8-by-8 four-bit planar characters, totaling 512 encoded bytes.</summary>
    public const int Height = 8;
    /// <summary>$9B:BFFB selects VRAM word $6200 for the endpoint character.</summary>
    public const ushort PointDestination = 0x6200;
    /// <summary>$9B:C02B selects VRAM word $6210 for the four segment characters.</summary>
    public const ushort SegmentDestination = 0x6210;
    private const VramAssetId Horizontal = VramAssetId.GrappleHorizontalSegmentTiles;
    private const VramAssetId Diagonal = VramAssetId.GrappleDiagonalSegmentTiles;
    private const VramAssetId Vertical = VramAssetId.GrappleVerticalSegmentTiles;
    /// <summary>
    /// $9A:8A00, the $9B:C344 exclusive end pointer. Builds before a988015bc read the begin/end pair
    /// as two alternating endpoint frames and queued this source as the second one.
    /// </summary>
    public const int LegacyAlternateEndpointSource = 0x9a8a00;
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
    /// <summary>Resolves one of seven Grapple-owned upload identities to its native source and planar-sheet slice; this lookup neither advances endpoint animation nor enqueues a VRAM transfer.</summary>
    /// <param name="asset">One of the four endpoint-frame identities or the horizontal, diagonal, or vertical segment identity.</param>
    /// <returns>A 32-byte endpoint or 128-byte segment descriptor; destination VRAM words are separately defined by <see cref="PointDestination"/> and <see cref="SegmentDestination"/>.</returns>
    /// <exception cref="InvalidDataException">The asset identity is not owned by the Grapple tile catalog.</exception>
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
/// <param name="Asset">Stable installed-artwork identity retained by deferred VRAM writes instead of embedding image bytes in pending simulation state.</param>
/// <param name="SourceAddress">Native 24-bit bank-$9A source identity used to recognize restored cartridge-style transfers; replacement upload bytes come from the installed atlas.</param>
/// <param name="AtlasOffset">Byte offset into the sheet's encoded 4-bpp characters, not a PNG pixel offset: 0/32/64/96 for endpoints and 128/256/384 for orientation groups.</param>
/// <param name="ByteCount">Transfer length in bytes: 32 for one endpoint character or 128 for four rope characters; not a VRAM word count.</param>
public readonly record struct GrappleTileTransfer(VramAssetId Asset, int SourceAddress, int AtlasOffset, ushort ByteCount);

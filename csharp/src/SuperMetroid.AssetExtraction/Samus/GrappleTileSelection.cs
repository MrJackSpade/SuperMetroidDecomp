using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using static SuperMetroid.AssetExtraction.Samus.GrappleTileDefinitions;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The installed rope and endpoint characters, and their transfer, each rope orientation and endpoint frame selects.</summary>
public static partial class GrappleTileSelection
{
    /// <summary>The transfer of each orientation's installed rope characters, one of <see cref="GrappleTileDefinitions.Transfers"/>.</summary>
    private static readonly IdentityTable<GrappleRopeOrientation, GrappleTileTransfer> SegmentTransfers = new(new Dictionary<GrappleRopeOrientation, GrappleTileTransfer>
    {
        [GrappleRopeOrientation.Horizontal] = Transfers[4],
        [GrappleRopeOrientation.Diagonal] = Transfers[5],
        [GrappleRopeOrientation.Vertical] = Transfers[6],
    });

    /// <summary>The transfer of the installed rope characters of an orientation.</summary>
    public static GrappleTileTransfer SegmentTransferFor(GrappleRopeOrientation orientation) => SegmentTransfers[orientation];

    /// <summary>$9B:BFBD cycles endpoint characters at $8200/$8400/$8600/$8800, independently of rope angle.</summary>
    public static GrappleTileTransfer PointTransferFor(GrappleEndpointFrame frame) => PointTransfers[frame];

    /// <summary>The transfer of each endpoint frame's installed character, one of <see cref="GrappleTileDefinitions.Transfers"/>.</summary>
    private static readonly IdentityTable<GrappleEndpointFrame, GrappleTileTransfer> PointTransfers = new(new Dictionary<GrappleEndpointFrame, GrappleTileTransfer>
    {
        [GrappleEndpointFrame.Point] = Transfers[0],
        [GrappleEndpointFrame.Orb] = Transfers[1],
        [GrappleEndpointFrame.Ring] = Transfers[2],
        [GrappleEndpointFrame.SparklingRing] = Transfers[3],
    });
}

using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using static SuperMetroid.AssetExtraction.Samus.GrappleTileDefinitions;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The installed rope and endpoint characters, and their transfer, each rope orientation and endpoint frame selects.</summary>
public static class GrappleTileSelection
{
    /// <summary>The installed rope characters of an orientation.</summary>
    public static InstalledArtworkId SegmentAssetFor(GrappleRopeOrientation orientation) => orientation switch
    {
        GrappleRopeOrientation.Horizontal => Horizontal,
        GrappleRopeOrientation.Diagonal => Diagonal,
        GrappleRopeOrientation.Vertical => Vertical,
        _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined rope orientation."),
    };

    /// <summary>$9B:BFBD cycles endpoint characters at $8200/$8400/$8600/$8800, independently of rope angle.</summary>
    public static InstalledArtworkId PointAssetFor(int frame) => frame switch
    {
        0 => InstalledArtworkId.GrapplePointFirstTiles,
        1 => InstalledArtworkId.GrapplePointSecondTiles,
        2 => InstalledArtworkId.GrapplePointThirdTiles,
        3 => InstalledArtworkId.GrapplePointFourthTiles,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    /// <summary>Resolves one of seven Grapple-owned upload identities to its native source and planar-sheet slice; this lookup neither advances endpoint animation nor enqueues a VRAM transfer.</summary>
    /// <param name="asset">One of the four endpoint-frame identities or the horizontal, diagonal, or vertical segment identity.</param>
    /// <returns>A 32-byte endpoint or 128-byte segment descriptor; destination VRAM words are separately defined by <c>PointDestination</c> and <c>SegmentDestination</c>.</returns>
    /// <exception cref="InvalidDataException">The asset identity is not owned by the Grapple tile catalog.</exception>
    public static GrappleTileTransfer TransferFor(InstalledArtworkId asset)
    {
        foreach (GrappleTileTransfer transfer in Transfers)
        {
            if (transfer.Asset == asset)
                return transfer;
        }
        throw new InvalidDataException($"Not a Grapple tile asset: {asset}.");
    }
}

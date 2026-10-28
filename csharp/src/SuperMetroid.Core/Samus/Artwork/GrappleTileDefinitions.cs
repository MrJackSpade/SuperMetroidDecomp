using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Grapple tile ownership and the pinned $9B:C342/C346 endpoint/angle selection tables.</summary>
public static class GrappleTileDefinitions
{
    /// <summary>Indexed PNG resource for the four endpoint-animation characters and three four-character rope-orientation groups; importing artwork leaves native angle selection and endpoint cadence unchanged.</summary>
    public const string FileName = "grapple-tiles.png";
    /// <summary>Four endpoint characters followed by three groups of four segment characters.</summary>
    public const int Width = 128;
    /// <summary>Sheet height in pixels: one row of sixteen 8-by-8 four-bit planar characters, totaling 512 encoded bytes.</summary>
    public const int Height = 8;
    /// <summary>OBJ character $20, where `$9B:BFA5` uploads the endpoint character; the four rope characters follow it.</summary>
    public const int FirstCharacter = 0x20;
    /// <summary>Installed tile identity for horizontal rope segments.</summary>
    internal const InstalledArtworkId Horizontal = InstalledArtworkId.GrappleHorizontalSegmentTiles;
    /// <summary>Installed tile identity for diagonal rope segments.</summary>
    internal const InstalledArtworkId Diagonal = InstalledArtworkId.GrappleDiagonalSegmentTiles;
    /// <summary>Installed tile identity for vertical rope segments.</summary>
    internal const InstalledArtworkId Vertical = InstalledArtworkId.GrappleVerticalSegmentTiles;
    /// <summary>
    /// $9A:8200/8A00/9200 Tiles_GrappleBeam groups; only endpoint and segment-owned characters are extracted: the four
    /// endpoint transfers, one character each, then the horizontal, diagonal and vertical segments, four characters each,
    /// as atlas offsets and byte counts in native upload order.
    /// </summary>
    public static IReadOnlyList<GrappleTileTransfer> Transfers { get; } =
    [
        new(InstalledArtworkId.GrapplePointFirstTiles, 0, 32),
        new(InstalledArtworkId.GrapplePointSecondTiles, 32, 32),
        new(InstalledArtworkId.GrapplePointThirdTiles, 64, 32),
        new(InstalledArtworkId.GrapplePointFourthTiles, 96, 32),
        new(Horizontal, 128, 128),
        new(Diagonal, 256, 128),
        new(Vertical, 384, 128),
    ];
}

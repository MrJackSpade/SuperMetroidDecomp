using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Presentation identities for the endpoint and four timed rope records.</summary>
public static class GrappleSpriteDefinitions
{
    /// <summary>Editable JSON filename for Grapple's endpoint and four rope-segment OBJ attribute selections, excluding segment timing and beam mechanics.</summary>
    public const string FileName = "grapple-sprites.json";
    /// <summary>Supported revision of the endpoint-plus-four-segment visual attribute schema.</summary>
    public const int Version = 1;
    /// <summary>The rope's four timed frames, each a duration and one appearance.</summary>
    public const int SegmentFrameCount = 4;
    /// <summary>$94:B18D first OBJ tile: four consecutive rope-animation tiles $21-$24.</summary>
    private const int FirstSegmentTile = 33;
    /// <summary>$94:B18D-$B199 common OBJ palette five for rope segments.</summary>
    private const int SegmentPalette = 5;
    /// <summary>$94:B18D-$B199 common OBJ priority three for rope segments.</summary>
    private const int SegmentPriority = 3;

    /// <summary>$94:B13C/B17C, DrawGrappleBeamEnd: the endpoint is character $20 in palette five, priority three.</summary>
    internal static SpritePart StockEndpoint { get; } = SpritePart.Single(32, SegmentPalette, SegmentPriority);

    /// <summary>The native rope appearance of a timed frame: characters $21-$24 with palette five and priority three.</summary>
    internal static SpritePart StockSegment(int frame) =>
        SpritePart.Single(FirstSegmentTile + frame, SegmentPalette, SegmentPriority);
}

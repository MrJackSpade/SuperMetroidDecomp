namespace SuperMetroid.Core.Assets;

/// <summary>Presentation identities for the endpoint and four timed rope records.</summary>
public static class GrappleSpriteDefinitions
{
    public const string FileName = "grapple-sprites.json";
    public const int Version = 1;
    /// <summary>$94:B13D: immediate endpoint attributes in DrawGrappleBeamEnd_NotConnected.</summary>
    public const int EndpointAttributeAddress = 0x94b13d;
    /// <summary>$94:B18D/B191/B195/B199: visual attribute words in the four timed segment records.</summary>
    public static ReadOnlySpan<int> SegmentAttributeAddresses => [0x94b18d, 0x94b191, 0x94b195, 0x94b199];
}

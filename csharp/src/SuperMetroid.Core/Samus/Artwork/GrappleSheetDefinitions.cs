using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured grapple sheet (#1392): the beam's endpoint in each frame of its animation and its rope segments in
/// each orientation, picture of the rope's travelling pattern and rope-direction mirroring. The grapple's flare is drawn
/// from the projectile sheet.
/// </summary>
public static class GrappleSheetDefinitions
{
    /// <summary>
    /// The endpoint's animation, its frames named for what they show (#2285): a point swelling to an orb, opening into a
    /// ring and sparkling, then the point again.
    /// </summary>
    public static IReadOnlyList<string> Endpoint { get; } = ["Endpoint.Point", "Endpoint.Orb", "Endpoint.Ring", "Endpoint.SparklingRing"];

    /// <summary>
    /// The rope's pictures, named for how far along its cycle the rope's travelling pattern stands (#2285): at its start,
    /// a quarter, half and three quarters of the way on.
    /// </summary>
    public static IReadOnlyList<string> RopePattern { get; } = ["Start", "Quarter", "Half", "ThreeQuarters"];

    /// <summary>The frame-name word each rope mirroring is installed under; <see cref="GrappleSheets.RopeFrame"/> reads it.</summary>
    internal static readonly IReadOnlyDictionary<SpriteMirror, string> MirrorNames = new Dictionary<SpriteMirror, string>
    {
        [SpriteMirror.None] = "Unmirrored",
        [SpriteMirror.Horizontal] = "MirroredHorizontally",
        [SpriteMirror.Vertical] = "MirroredVertically",
        [SpriteMirror.Horizontal | SpriteMirror.Vertical] = "MirroredBothWays",
    };
}

using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured grapple sheet (#1392): the beam's endpoint in each animation frame and its rope segments in each
/// orientation, segment frame and rope-direction mirroring. The grapple's flare is drawn from the projectile sheet.
/// </summary>
public static class GrappleSheetDefinitions
{
    /// <summary>Endpoint animation frames.</summary>
    public const int EndpointFrameCount = 4;

    /// <summary>Every rope-direction mirroring a segment is drawn with.</summary>
    public static IReadOnlyList<SpriteMirror> RopeMirrors { get; } =
        [SpriteMirror.None, SpriteMirror.Horizontal, SpriteMirror.Vertical, SpriteMirror.Horizontal | SpriteMirror.Vertical];

    /// <summary>The endpoint in animation frame <paramref name="pointFrame"/>.</summary>
    public static string EndpointFrame(int pointFrame) => $"Endpoint.{pointFrame}";

    /// <summary>A rope segment of an orientation and segment frame, as the rope's direction mirrors it.</summary>
    public static string RopeFrame(GrappleRopeOrientation orientation, int segmentFrame, SpriteMirror mirror) =>
        $"Rope.{orientation}.{segmentFrame}.{(int)mirror}";

    /// <summary>Every frame the grapple sheet holds.</summary>
    public static IEnumerable<string> Frames =>
        Enumerable.Range(0, EndpointFrameCount).Select(EndpointFrame)
            .Concat(Enum.GetValues<GrappleRopeOrientation>().SelectMany(orientation =>
                Enumerable.Range(0, GrappleSpriteDefinitions.SegmentFrameCount).SelectMany(segment =>
                    RopeMirrors.Select(mirror => RopeFrame(orientation, segment, mirror)))));
}

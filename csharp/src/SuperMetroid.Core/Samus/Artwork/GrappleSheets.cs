using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Samus.Artwork.GrappleSheetDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The grapple sheet's endpoint and rope frames.</summary>
public static class GrappleSheets
{
    /// <summary>The endpoint in animation frame <paramref name="pointFrame"/>.</summary>
    public static string EndpointFrame(int pointFrame) => $"Endpoint.{pointFrame}";

    /// <summary>A rope segment of an orientation and segment frame, as the rope's direction mirrors it.</summary>
    public static string RopeFrame(GrappleRopeOrientation orientation, int segmentFrame, SpriteMirror mirror) =>
        $"Rope.{orientation}.{segmentFrame}.{MirrorName(mirror)}";

    /// <summary>The frame-name segment for a rope mirroring, in <see cref="GrappleSheetDefinitions.RopeMirrors"/> order.</summary>
    private static string MirrorName(SpriteMirror mirror) =>
        MirrorNames.TryGetValue(mirror, out string? value) ? value : throw new ArgumentOutOfRangeException(nameof(mirror), mirror, "Undefined rope mirroring.");

    /// <summary>Every frame the grapple sheet holds.</summary>
    public static IEnumerable<string> Frames =>
        Enumerable.Range(0, EndpointFrameCount).Select(EndpointFrame)
            .Concat(Enum.GetValues<GrappleRopeOrientation>().SelectMany(orientation =>
                Enumerable.Range(0, SuperMetroid.Core.Samus.Grapple.GrappleBeamDefinitions.Rendering.SegmentAnimationFrameCount).SelectMany(segment =>
                    RopeMirrors.Select(mirror => RopeFrame(orientation, segment, mirror)))));
}

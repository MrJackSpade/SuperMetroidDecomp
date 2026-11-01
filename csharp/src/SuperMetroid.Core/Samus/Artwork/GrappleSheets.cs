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

    /// <summary>The frame-name segment for a rope mirroring.</summary>
    private static string MirrorName(SpriteMirror mirror) =>
        MirrorNames.TryGetValue(mirror, out string? value) ? value : throw new ArgumentOutOfRangeException(nameof(mirror), mirror, "Undefined rope mirroring.");
}

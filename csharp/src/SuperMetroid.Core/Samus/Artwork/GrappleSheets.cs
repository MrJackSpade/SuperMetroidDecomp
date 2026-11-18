using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Samus.Artwork.GrappleSheetDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The grapple sheet's endpoint and rope frames.</summary>
public static class GrappleSheets
{
    /// <summary>The endpoint showing <paramref name="frame"/>.</summary>
    public static string EndpointFrame(GrappleEndpointFrame frame) => $"Endpoint.{frame}";

    /// <summary>A rope segment of an orientation showing rope picture <paramref name="picture"/>, as the rope's direction mirrors it.</summary>
    public static string RopeFrame(GrappleRopeOrientation orientation, GrappleRopePicture picture, SpriteMirror mirror) =>
        $"Rope.{orientation}.{picture}.{MirrorName(mirror)}";

    /// <summary>The frame-name segment for a rope mirroring.</summary>
    private static string MirrorName(SpriteMirror mirror) =>
        MirrorNames.TryGetValue(mirror, out string? value) ? value : throw new ArgumentOutOfRangeException(nameof(mirror), mirror, "Undefined rope mirroring.");
}

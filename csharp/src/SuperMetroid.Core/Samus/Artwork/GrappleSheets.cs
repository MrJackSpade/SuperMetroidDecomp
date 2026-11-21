using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Samus.Artwork.GrappleSheetDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The grapple sheet's endpoint and rope frames.</summary>
public static class GrappleSheets
{
    /// <summary>The endpoint showing <paramref name="frame"/>.</summary>
    [InstalledKey]
    public static string EndpointFrame(GrappleEndpointFrame frame) => $"Endpoint.{GrappleEndpointFrameKeys.Installed.Of(frame)}";

    /// <summary>A rope segment of an orientation showing rope picture <paramref name="picture"/>, as the rope's direction mirrors it.</summary>
    [InstalledKey]
    public static string RopeFrame(GrappleRopeOrientation orientation, GrappleRopePicture picture, SpriteMirror mirror) =>
        $"Rope.{GrappleRopeOrientationKeys.Installed.Of(orientation)}.{GrappleRopePictureKeys.Installed.Of(picture)}.{MirrorName(mirror)}";

    /// <summary>The frame-name segment for a rope mirroring.</summary>
    private static string MirrorName(SpriteMirror mirror) =>
        MirrorNames[mirror];
}

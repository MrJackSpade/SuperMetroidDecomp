using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Samus.Artwork.GrappleSheetDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The grapple sheet's endpoint and rope frames.</summary>
public static class GrappleSheets
{
    /// <summary>The endpoint at step <paramref name="pointFrame"/> of its animation.</summary>
    public static string EndpointFrame(int pointFrame) => Endpoint[pointFrame];

    /// <summary>A rope segment of an orientation showing rope picture <paramref name="segmentFrame"/>, as the rope's direction mirrors it.</summary>
    public static string RopeFrame(GrappleRopeOrientation orientation, int segmentFrame, SpriteMirror mirror) =>
        $"Rope.{orientation}.{RopePattern[segmentFrame]}.{MirrorName(mirror)}";

    /// <summary>The frame-name segment for a rope mirroring.</summary>
    private static string MirrorName(SpriteMirror mirror) =>
        MirrorNames.TryGetValue(mirror, out string? value) ? value : throw new ArgumentOutOfRangeException(nameof(mirror), mirror, "Undefined rope mirroring.");
}

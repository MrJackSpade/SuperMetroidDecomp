using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured grapple sheet (#1392): the beam's endpoint in each animation frame and its rope segments in each
/// orientation, segment frame and rope-direction mirroring. The grapple's flare is drawn from the projectile sheet.
/// </summary>
public static class GrappleSheetDefinitions
{
    /// <summary>The frame-name word each rope mirroring is installed under; <see cref="GrappleSheets.RopeFrame"/> reads it.</summary>
    internal static readonly IReadOnlyDictionary<SpriteMirror, string> MirrorNames = new Dictionary<SpriteMirror, string>
    {
        [SpriteMirror.None] = "Unmirrored",
        [SpriteMirror.Horizontal] = "MirroredHorizontally",
        [SpriteMirror.Vertical] = "MirroredVertically",
        [SpriteMirror.Horizontal | SpriteMirror.Vertical] = "MirroredBothWays",
    };
}

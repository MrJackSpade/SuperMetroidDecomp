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

    /// <summary>The value each key selects; MirrorName looks it up.</summary>
    internal static readonly IReadOnlyDictionary<SpriteMirror, string> MirrorNames = new Dictionary<SpriteMirror, string>
    {
        [SpriteMirror.None] = "0",
        [SpriteMirror.Horizontal] = "1",
        [SpriteMirror.Vertical] = "2",
        [SpriteMirror.Horizontal | SpriteMirror.Vertical] = "3",
    };
}

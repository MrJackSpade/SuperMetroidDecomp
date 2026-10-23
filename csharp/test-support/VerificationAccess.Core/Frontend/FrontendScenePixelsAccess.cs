using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Verification access: a scene's or gameplay frame's pixels through the one published path, its render snapshot
/// rasterized by the software backend, for checks that assert on pixels. Production has no other render path.
/// </summary>
internal static class FrontendScenePixelsAccess
{
    /// <summary>The identity the rendered snapshots carry; rendering does not read it.</summary>
    private static RenderFrameIdentity Identity => new(1, 1, 0);

    /// <summary>Rasterizes a layered snapshot.</summary>
    internal static Rgba32[] Pixels(LayeredScene snapshot) =>
        SoftwareFrameSnapshotRenderer.Render(new DisplayPacket(Identity, snapshot));

    /// <summary>Rasterizes a published gameplay frame.</summary>
    /// <param name="frame">The published frame.</param>
    internal static Rgba32[] Pixels(GameplayDisplayFrame frame) => Pixels(GameplayDisplayCapture.Capture(frame));
}

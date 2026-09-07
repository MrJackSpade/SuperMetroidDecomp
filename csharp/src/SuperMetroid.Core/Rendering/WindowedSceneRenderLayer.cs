using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Replaces a rectangular pixel region with another owned scene, including its backdrop.</summary>
/// <remarks>Edges are half-open. Child scenes cannot contain scene insertions, bounding evaluation depth.</remarks>
public sealed record WindowedSceneRenderLayer : RenderLayer
{
    public LayeredRenderSnapshot Scene { get; }
    public int Left { get; }
    public int Top { get; }
    public int Right { get; }
    public int Bottom { get; }
    public WindowedSceneRenderLayer(LayeredRenderSnapshot scene, int left, int top, int right, int bottom)
    {
        ArgumentNullException.ThrowIfNull(scene);
        if (left < 0 || top < 0 || right < left || bottom < top ||
            right > SnesPpuLayout.ScreenWidthPixels || bottom > SnesPpuLayout.ScreenHeightPixels)
            throw new ArgumentOutOfRangeException(nameof(left));
        foreach (RenderLayer layer in scene.Layers)
            if (layer is WindowedSceneRenderLayer or XrayWindowRenderLayer) throw new ArgumentException("Nested scene insertions are unsupported.", nameof(scene));
        Scene = scene; Left = left; Top = top; Right = right; Bottom = bottom;
    }
}

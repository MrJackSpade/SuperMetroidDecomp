using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Replaces a rectangular pixel region with another owned scene, including its backdrop.</summary>
/// <remarks>Edges are half-open. Child scenes cannot contain scene insertions, bounding evaluation depth.</remarks>
public sealed record WindowedSceneRenderLayer : RenderLayer
{
    /// <summary>Gets the retained immutable child snapshot, with its own PPU memory and layer ladder; matching screen pixels replace the parent, including backdrop pixels.</summary>
    public LayeredRenderSnapshot Scene { get; }
    /// <summary>Gets the inclusive left screen-pixel edge, from 0 through 256; the child scene is not translated to this origin.</summary>
    public int Left { get; }
    /// <summary>Gets the inclusive top physical scanline, from 0 through 224; child sampling retains full-screen coordinates.</summary>
    public int Top { get; }
    /// <summary>Gets the exclusive right screen-pixel edge, at least Left and at most 256.</summary>
    public int Right { get; }
    /// <summary>Gets the exclusive bottom physical scanline, at least Top and at most 224.</summary>
    public int Bottom { get; }
    /// <summary>Creates a half-open screen rectangle that replaces parent pixels with a separately rendered child scene, without translating or scaling it.</summary>
    /// <param name="scene">Immutable child snapshot retained by reference; it may not itself contain a windowed-scene insertion.</param>
    /// <param name="left">Inclusive horizontal edge in native screen pixels.</param>
    /// <param name="top">Inclusive vertical edge in physical scanlines.</param>
    /// <param name="right">Exclusive horizontal edge; equality with left is allowed and produces an empty-width rectangle.</param>
    /// <param name="bottom">Exclusive vertical edge; equality with top is allowed and produces an empty-height rectangle.</param>
    /// <exception cref="ArgumentNullException"><paramref name="scene"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Edges are crossed or outside the 256-by-224 visible screen.</exception>
    /// <exception cref="ArgumentException"><paramref name="scene"/> contains another windowed-scene insertion.</exception>
    public WindowedSceneRenderLayer(LayeredRenderSnapshot scene, int left, int top, int right, int bottom)
    {
        ArgumentNullException.ThrowIfNull(scene);
        if (left < 0 || top < 0 || right < left || bottom < top ||
            right > SnesPpuLayout.ScreenWidthPixels || bottom > SnesPpuLayout.ScreenHeightPixels)
            throw new ArgumentOutOfRangeException(nameof(left));
        foreach (RenderLayer layer in scene.Layers)
            if (layer is WindowedSceneRenderLayer) throw new ArgumentException("Nested scene insertions are unsupported.", nameof(scene));
        Scene = scene; Left = left; Top = top; Right = right; Bottom = bottom;
    }
}

namespace SuperMetroid.Core.Rendering;

/// <summary>Replaces a rectangular pixel region with another owned scene, including its backdrop.</summary>
/// <remarks>Edges are half-open. Child scenes cannot contain scene insertions, bounding evaluation depth.</remarks>
public sealed partial record InsetSceneLayer : DisplayLayer
{
    /// <summary>The inset scene, with its own paint and layers; within the rectangle its pixels, backdrop included, replace the parent's.</summary>
    public LayeredScene Scene { get; }
    /// <summary>Gets the inclusive left screen-pixel edge, within the screen; the child scene is not translated to this origin.</summary>
    public int Left { get; }
    /// <summary>Gets the inclusive top screen line, within the screen; child sampling retains full-screen coordinates.</summary>
    public int Top { get; }
    /// <summary>Gets the exclusive right screen-pixel edge, at least Left and at most the screen's width.</summary>
    public int Right { get; }
    /// <summary>Gets the exclusive bottom screen line, at least Top and at most the screen's height.</summary>
    public int Bottom { get; }
    /// <summary>Creates a half-open screen rectangle that replaces parent pixels with a separately rendered child scene, without translating or scaling it.</summary>
    /// <param name="scene">Immutable child snapshot retained by reference; it may not itself contain a windowed-scene insertion.</param>
    /// <param name="left">Inclusive horizontal edge in screen pixels.</param>
    /// <param name="top">Inclusive vertical edge in screen lines.</param>
    /// <param name="right">Exclusive horizontal edge; equality with left is allowed and produces an empty-width rectangle.</param>
    /// <param name="bottom">Exclusive vertical edge; equality with top is allowed and produces an empty-height rectangle.</param>
    /// <exception cref="ArgumentNullException"><paramref name="scene"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Edges are crossed or outside the visible screen.</exception>
    /// <exception cref="ArgumentException"><paramref name="scene"/> contains another windowed-scene insertion.</exception>
    public InsetSceneLayer(LayeredScene scene, int left, int top, int right, int bottom)
    {
        Ensure.NotNull(scene);
        if (left < 0 || top < 0 || right < left || bottom < top ||
            right > ScreenGeometry.ScreenWidthPixels || bottom > ScreenGeometry.ScreenHeightPixels)
            throw new ArgumentOutOfRangeException(nameof(left));
        foreach (DisplayLayer layer in scene.Layers)
            if (layer is InsetSceneLayer) throw new ArgumentException("Nested scene insertions are unsupported.", nameof(scene));
        Scene = scene; Left = left; Top = top; Right = right; Bottom = bottom;
    }
}

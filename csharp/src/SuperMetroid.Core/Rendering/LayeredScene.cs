using SuperMetroid.Core.Assets;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frame's scene: what it paints with, and its layers drawn from back to front over the screen (#1514).
/// Each layer is named for what it shows (a plane of pictures, the HUD, a layer of sprites, a glow, a tint, an inset
/// scene); what a layer shows, and so which of the screen's pictures and sprites lie in front of which, is the producer's
/// decision, made by the order of its layers. The frame's brightness applies last.
/// </summary>
public sealed partial class LayeredScene : DisplayScene
{
    /// <summary>The scene's own copy of the layer order, unchanged by later edits to the order it was given.</summary>
    internal readonly DisplayLayer[] layers;
    /// <summary>The frame's backdrop, shading and the sprite draws its gameplay layer composes, retained by reference, not copied again.</summary>
    public FramePaint Paint { get; }
    /// <summary>The layers, drawn in this order from back to front.</summary>
    public ReadOnlySpan<DisplayLayer> Layers => layers;

    /// <summary>
    /// Every sprite draw the frame shows, frontmost first: the paint's, which a whole-gameplay layer or a turning floor
    /// composes itself, then each sprite layer's draws, from the frontmost sprite layer back. A pass that reads all of the
    /// frame's sprites together, such as a turning floor's blend, reads these.
    /// </summary>
    public ImmutableArray<ImageSpriteDraw> SpriteDraws { get; }

    /// <summary>Keeps the frame's paint and copies its layer order.</summary>
    /// <param name="paint">The frame's backdrop, shading and the sprite draws its gameplay layer composes, independent of later simulation writes.</param>
    /// <param name="layers">The layers from back to front; with none the backdrop shows alone, and a whole-gameplay layer must come first.</param>
    /// <param name="brightness">The frame's brightness, from black (zero) to <see cref="DisplayBrightness.Full"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="paint"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is out of range.</exception>
    /// <exception cref="ArgumentException">
    /// A layer is null, a whole-gameplay layer comes after another, or a glow names sprites that are not one of
    /// the scene's sprite layers.
    /// </exception>
    public LayeredScene(FramePaint paint, ReadOnlySpan<DisplayLayer> layers, DisplayBrightness brightness)
        : base(brightness)
    {
        Ensure.NotNull(paint);
        // Each layer is one of the display model's sealed records and owns any array data. Copy the sequence so a scene
        // cannot rearrange a queued frame's layer order after publication.
        foreach (DisplayLayer layer in layers)
        {
            if (layer is null)
                throw new ArgumentException("A display layer is null.", nameof(layers));
            if (layer is AddedPicturedPlaneLayer { GlowsOverSprites: { } sprites } && !Holds(layers, sprites))
                throw new ArgumentException("A glow covers the sprites of one of its scene's sprite layers.", nameof(layers));
        }
        for (int i = 1; i < layers.Length; i++)
            if (layers[i] is GameplaySceneLayer or TurningRoomLayer or GameplayBlendLayer)
                throw new ArgumentException("The whole-gameplay layer must be the first layer.", nameof(layers));
        Paint = paint;
        this.layers = layers.ToArray();
        SpriteDraws = AllSpriteDraws(paint, this.layers);
    }

    /// <inheritdoc/>
    public override void DrawWith<TDrawer>(TDrawer drawer) => drawer.DrawLayered(this);

    /// <inheritdoc/>
    internal override DisplayScene AtBrightness(DisplayBrightness brightness) =>
        brightness == Brightness ? this : new LayeredScene(Paint, layers, brightness);

    /// <summary>Whether <paramref name="layer"/> is one of <paramref name="layers"/>, as the same instance.</summary>
    private static bool Holds(ReadOnlySpan<DisplayLayer> layers, SpritesLayer layer)
    {
        foreach (DisplayLayer shown in layers)
        {
            if (ReferenceEquals(shown, layer))
                return true;
        }
        return false;
    }

    /// <summary>The paint's draws, then each sprite layer's, from the frontmost sprite layer back.</summary>
    private static ImmutableArray<ImageSpriteDraw> AllSpriteDraws(FramePaint paint, DisplayLayer[] layers)
    {
        var draws = ImmutableArray.CreateBuilder<ImageSpriteDraw>();
        draws.AddRange(paint.SpriteDraws);
        for (int index = layers.Length - 1; index >= 0; index--)
        {
            if (layers[index] is SpritesLayer sprites)
                draws.AddRange(sprites.Draws);
        }
        return draws.ToImmutable();
    }
}

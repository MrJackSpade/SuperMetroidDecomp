using SuperMetroid.Core.Assets;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frame's scene: what it paints with, and its layers drawn from back to front over the 256-by-224 screen (#1514).
/// Each layer is named for what it shows (a plane of pictures, the HUD, a layer of sprites, a glow, a tint, an inset
/// scene); what a layer shows, and so which of the screen's pictures and sprites lie in front of which, is the producer's
/// decision, made by the order of its layers. The frame's brightness applies last.
/// </summary>
public sealed class LayeredScene
{
    /// <summary>The scene's own copy of the layer order, unchanged by later edits to the caller's.</summary>
    internal readonly DisplayLayer[] layers;
    /// <summary>The frame's backdrop, shading and the sprite draws its gameplay layer composes, retained by reference, not copied again.</summary>
    public FramePaint Paint { get; }
    /// <summary>The layers, drawn in this order from back to front.</summary>
    public ReadOnlySpan<DisplayLayer> Layers => layers;
    /// <summary>The frame's one brightness, from black (zero) to <see cref="DisplayBrightness.Full"/>, applied after every layer.</summary>
    public int Brightness { get; }

    /// <summary>
    /// Every sprite draw the frame shows, frontmost first: the paint's, which a whole-gameplay layer or a turning floor
    /// composes itself, then each sprite layer's draws, from the frontmost sprite layer back. A pass that reads all of the
    /// frame's sprites together, such as a turning floor's blend, reads these.
    /// </summary>
    public ImmutableArray<SpriteDraw> SpriteDraws { get; }

    /// <summary>Keeps the frame's paint and copies its layer order.</summary>
    /// <param name="paint">The frame's backdrop, shading and the sprite draws its gameplay layer composes, independent of later simulation writes.</param>
    /// <param name="layers">The layers from back to front; with none the backdrop shows alone, and a whole-gameplay layer must come first.</param>
    /// <param name="brightness">The frame's brightness, from black (zero) to <see cref="DisplayBrightness.Full"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="paint"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is out of range.</exception>
    /// <exception cref="ArgumentException">
    /// A layer is null or unsupported, a turning floor selects two blends at once, a whole-gameplay layer comes after
    /// another, or a glow names sprites that are not one of the scene's sprite layers.
    /// </exception>
    public LayeredScene(FramePaint paint, ReadOnlySpan<DisplayLayer> layers, int brightness)
    {
        Ensure.NotNull(paint);
        Brightness = DisplayBrightness.Require(brightness);
        // Each accepted record is sealed and owns any array data. Copy the sequence so a scene
        // cannot rearrange a queued frame's layer order after publication.
        foreach (DisplayLayer layer in layers)
        {
            if (layer is not (SpritesLayer or HudLayer or TurningFloorLayer or ScreenTintLayer or GameplaySceneLayer or ColourAddWindowLayer or MessageBoxLayer or OverlayBlendLayer or TurningRoomLayer or InsetSceneLayer or GameplayBlendLayer or PicturedPlaneLayer or AddedPicturedPlaneLayer or PictureGridLayer or ScreenGradientLayer))
                throw new ArgumentException("Unrecognized or null display layer.", nameof(layers));
            if (layer is TurningFloorLayer { SubtractsSprites: true, AddsForeground: true })
                throw new ArgumentException("A turning floor cannot both subtract the sprites and add itself.", nameof(layers));
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
    private static ImmutableArray<SpriteDraw> AllSpriteDraws(FramePaint paint, DisplayLayer[] layers)
    {
        var draws = ImmutableArray.CreateBuilder<SpriteDraw>();
        draws.AddRange(paint.SpriteDraws);
        for (int index = layers.Length - 1; index >= 0; index--)
        {
            if (layers[index] is SpritesLayer sprites)
                draws.AddRange(sprites.Draws);
        }
        return draws.ToImmutable();
    }
}

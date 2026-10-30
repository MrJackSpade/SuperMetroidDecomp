namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frame's scene: what it paints with, and its layers drawn from back to front over the 256-by-224 screen.
/// Layers draw tiles and sprites, blend, tint and inset other scenes; the frame's brightness applies last.
/// </summary>
public sealed class LayeredScene
{
    /// <summary>The scene's own copy of the layer order, unchanged by later edits to the caller's.</summary>
    internal readonly DisplayLayer[] layers;
    /// <summary>The frame's colours and sprite draws, retained by reference, not copied again.</summary>
    public FramePaint Paint { get; }
    /// <summary>The layers, drawn in this order from back to front.</summary>
    public ReadOnlySpan<DisplayLayer> Layers => layers;
    /// <summary>The frame's one brightness, from black (zero) to <see cref="DisplayBrightness.Full"/>, applied after every layer.</summary>
    public int Brightness { get; }

    /// <summary>Keeps the frame's paint and copies its layer order.</summary>
    /// <param name="paint">The frame's colours and sprite draws, independent of later simulation writes.</param>
    /// <param name="layers">The layers from back to front; with none the backdrop shows alone, and a whole-gameplay layer must come first.</param>
    /// <param name="brightness">The frame's brightness, from black (zero) to <see cref="DisplayBrightness.Full"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="paint"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is out of range.</exception>
    /// <exception cref="ArgumentException">A layer is null or unsupported, has invalid geometry, depths or colours, selects two blends at once, or puts a whole-gameplay layer after another.</exception>
    public LayeredScene(FramePaint paint, ReadOnlySpan<DisplayLayer> layers, int brightness)
    {
        Ensure.NotNull(paint);
        Brightness = DisplayBrightness.Require(brightness);
        // Each accepted record is sealed and owns any array data. Copy the sequence so a scene
        // cannot rearrange a queued frame's layer order after publication.
        foreach (DisplayLayer layer in layers)
        {
            if (layer is not (SpriteDepthLayer or SpritesLayer or HudLayer or TurningFloorLayer or ScreenTintLayer or GameplaySceneLayer or ColourAddBandsLayer or MessageBoxLayer or OverlayBlendLayer or TurningRoomLayer or InsetSceneLayer or GameplayBlendLayer or PicturedPlaneLayer or AddedPicturedPlaneLayer or PictureGridLayer))
                throw new ArgumentException("Unrecognized or null display layer.", nameof(layers));
            if (layer is TurningFloorLayer { SubtractsSprites: true, AddsForeground: true })
                throw new ArgumentException("A turning floor cannot both subtract the sprites and add itself.", nameof(layers));
            if (layer is SpritesLayer { FixedColor: not null, AddToScreen: true })
                throw new ArgumentException("A sprite tint requires drawing the sprites, not adding them.", nameof(layers));
        }
        for (int i = 1; i < layers.Length; i++)
            if (layers[i] is GameplaySceneLayer or TurningRoomLayer or GameplayBlendLayer)
                throw new ArgumentException("The whole-gameplay layer must be the first layer.", nameof(layers));
        Paint = paint;
        this.layers = layers.ToArray();
    }

}

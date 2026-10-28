using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// One displayed frame, closed and immutable: a layered scene, a turning-floor scene or a solid colour, shown at one
/// brightness. It holds neither game objects nor renderer resources.
/// </summary>
public sealed class DisplayPacket
{
    /// <summary>The brightness of a solid-colour frame; unused by the scene frames, whose scenes carry theirs.</summary>
    private readonly int solidBrightness;

    /// <summary>The host's publication identity and the game frame it shows; sequence and generation are positive, while the mailbox enforces their ordering.</summary>
    public RenderFrameIdentity Identity { get; }
    /// <summary>The layered scene this frame shows, or <see langword="null"/> when it shows a turning floor or a solid colour.</summary>
    public LayeredScene? Layers { get; }
    /// <summary>The turning floor, backdrop and sprites this frame shows, or <see langword="null"/> when it shows layers or a solid colour.</summary>
    public FloorScene? Floor { get; }
    /// <summary>The colour filling the whole frame when it shows a solid colour, or <see langword="null"/> when it shows a scene.</summary>
    public RgbaPixel? SolidColor { get; }
    /// <summary>The frame's visible size of 256 by 224 pixels, the HUD included where there is one, before the host scales it to the window.</summary>
    public Extent16<ScreenPixels> Size { get; } = ScreenGeometry.ScreenSize;

    /// <summary>
    /// The frame's one brightness, from black (zero) to <see cref="DisplayBrightness.Full"/>, applied once after the
    /// scene is composed: the scene's own for a layered or turning-floor frame, the frame's for a solid colour.
    /// </summary>
    public int Brightness => Layers?.Brightness ?? Floor?.Brightness ?? solidBrightness;

    /// <summary>Creates a frame showing a layered scene at the scene's brightness.</summary>
    /// <param name="identity">Publication identity with positive host sequence and generation.</param>
    /// <param name="layers">The complete layered scene, including its brightness.</param>
    public DisplayPacket(RenderFrameIdentity identity, LayeredScene layers)
        : this(identity)
    {
        Ensure.NotNull(layers);
        Layers = layers;
    }

    /// <summary>Creates a frame showing a turning floor, backdrop and sprites at the scene's brightness.</summary>
    /// <param name="identity">Publication identity with positive host sequence and generation.</param>
    /// <param name="floor">The complete turning-floor scene, including its brightness.</param>
    public DisplayPacket(RenderFrameIdentity identity, FloorScene floor)
        : this(identity)
    {
        Ensure.NotNull(floor);
        Floor = floor;
    }

    /// <summary>Creates a frame filled with one colour, shown at <paramref name="brightness"/>.</summary>
    /// <param name="identity">Publication identity with positive host sequence and generation.</param>
    /// <param name="solidColor">The colour filling all 256 by 224 pixels before the brightness applies.</param>
    /// <param name="brightness">The frame's brightness, from black (zero) to <see cref="DisplayBrightness.Full"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The brightness is below black or above full.</exception>
    public DisplayPacket(RenderFrameIdentity identity, RgbaPixel solidColor, int brightness = DisplayBrightness.Full)
        : this(identity)
    {
        SolidColor = solidColor;
        solidBrightness = DisplayBrightness.Require(brightness);
    }

    /// <summary>Validates the host identity.</summary>
    private DisplayPacket(RenderFrameIdentity identity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(identity.Sequence);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(identity.Generation);
        Identity = identity;
    }
}

using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frame showing a turning floor over the backdrop with the sprites in front, then the frame's brightness.
/// It is the frame for a screen that shows only a floor and sprites; gameplay with several layers,
/// bands or blends is a layered scene.
/// </summary>
public sealed record FloorScene
{
    /// <summary>The frame's colours and sprite draws, retained by reference without another copy.</summary>
    public FramePaint Paint { get; }
    /// <summary>The floor's transform; null shows no floor, leaving the backdrop.</summary>
    public AffineTransform? Background { get; }

    /// <summary>The floor's map and tiles (#1403), present exactly when <see cref="Background"/> is.</summary>
    public AffineFloor? Floor { get; }
    /// <summary>The frame's one brightness, 0 (black) through 15, applied after the floor, the sprites and the gradient.</summary>
    public int Brightness { get; }
    /// <summary>The scene's own copy of each line's gradient colour.</summary>
    private readonly SuperMetroid.Core.Rendering.TitleGradientLine[] gradient;
    /// <summary>
    /// The gradient colour on each screen line 0..223, or empty for no gradient. Each line's colour is subtracted from
    /// the floor and backdrop, or added to them and to the frontmost sprites that blend.
    /// </summary>
    public ReadOnlySpan<SuperMetroid.Core.Rendering.TitleGradientLine> Gradient => gradient;

    /// <summary>Captures a 256-by-224 frame of floor and backdrop with the sprites in front, copying only the optional gradient.</summary>
    /// <param name="paint">The frame's colours and sprite draws; retained rather than recaptured.</param>
    /// <param name="background">The floor's transform, or null to show the backdrop behind the sprites. The floor is measured from screen column zero, with the first visible row at line one.</param>
    /// <param name="brightness">The frame's brightness, 0 (black) through 15.</param>
    /// <param name="gradient">Empty, or exactly 224 lines of gradient colour, each subtracted from the floor and backdrop or added to them and the sprites; copied.</param>
    /// <exception cref="ArgumentNullException"><paramref name="paint"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is out of range.</exception>
    /// <param name="floor">The floor's map and tiles, given exactly when <paramref name="background"/> is.</param>
    /// <exception cref="ArgumentException"><paramref name="gradient"/> has the wrong nonzero length or an unsupported control, or a floor is given without a transform or a transform without a floor.</exception>
    public FloorScene(FramePaint paint, AffineTransform? background,
        int brightness, ReadOnlySpan<SuperMetroid.Core.Rendering.TitleGradientLine> gradient = default,
        AffineFloor? floor = null)
    {
        if (background.HasValue != floor is not null)
            throw new ArgumentException("A floor's transform and its map come together.", nameof(floor));
        Floor = floor;
        Ensure.NotNull(paint);
        Brightness = DisplayBrightness.Require(brightness);
        Paint = paint;
        Background = background;
        if (!gradient.IsEmpty && gradient.Length != ScreenGeometry.ScreenHeightPixels)
            throw new ArgumentException("A title gradient covers all 224 screen lines.", nameof(gradient));
        foreach (var line in gradient)
            if (!IsTitleGradientControl(line.Control))
                throw new ArgumentException("A title gradient line blends in an unsupported way.", nameof(gradient));
        this.gradient = gradient.ToArray();
    }

    /// <summary>The two blends the title gradient uses: subtracting from the floor and backdrop, or adding to them and the sprites.</summary>
    private static bool IsTitleGradientControl(ColorBlend control) => control is
        (ColorBlend.Subtract | ColorBlend.Backdrop | ColorBlend.Bg1) or
        (ColorBlend.Backdrop | ColorBlend.Sprites | ColorBlend.Bg1);
}

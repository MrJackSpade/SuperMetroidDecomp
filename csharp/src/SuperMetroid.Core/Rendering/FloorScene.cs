namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frame showing a turning floor over the backdrop with the sprites in front, then the frame's brightness.
/// It is the frame for a screen that shows only a floor and sprites; gameplay with several layers,
/// bands or blends is a layered scene.
/// </summary>
public sealed class FloorScene : DisplayScene
{
    /// <summary>The frame's backdrop, shading and sprite draws, retained by reference without another copy.</summary>
    public FramePaint Paint { get; }

    /// <summary>The floor and how it appears on the screen (#1403, #2505), or null to show no floor, leaving the backdrop.</summary>
    public PlacedFloor? Floor { get; }

    /// <summary>
    /// The gradient drawn over the floor, the backdrop and the sprites (#1468), or null for none. Each band's colour is
    /// subtracted from the floor and backdrop, or added to them and to the frontmost sprites that blend.
    /// </summary>
    public ScreenGradientLayer? Gradient { get; }

    /// <summary>Captures a 256-by-224 frame of floor and backdrop with the sprites in front and an optional gradient over them.</summary>
    /// <param name="paint">The frame's backdrop, shading and sprite draws; retained rather than recaptured.</param>
    /// <param name="floor">The floor and how it appears on the screen, or null to show the backdrop behind the sprites.</param>
    /// <param name="brightness">The frame's brightness, 0 (black) through 15, applied after the floor, the sprites and the gradient.</param>
    /// <param name="gradient">The gradient drawn over the floor, the backdrop and the sprites, or null for none.</param>
    /// <exception cref="ArgumentNullException"><paramref name="paint"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is out of range.</exception>
    public FloorScene(FramePaint paint, PlacedFloor? floor, int brightness, ScreenGradientLayer? gradient = null)
        : base(brightness)
    {
        Paint = Ensure.NotNull(paint);
        Floor = floor;
        Gradient = gradient;
    }

    /// <inheritdoc/>
    public override void DrawWith<TDrawer>(TDrawer drawer) => drawer.DrawTurningFloor(this);

    /// <inheritdoc/>
    internal override DisplayScene AtBrightness(int brightness) =>
        brightness == Brightness ? this : new FloorScene(Paint, Floor, brightness, Gradient);
}

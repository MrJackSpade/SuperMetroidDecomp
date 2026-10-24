using SuperMetroid.Core.Rooms.Backgrounds;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Immutable full-screen Mode 7/backdrop followed by OBJ and master brightness.
/// This composition is one supported packet shape, not a substitute for mixed-mode
/// gameplay, windows or main/subscreen color math.
/// </summary>
public sealed record FloorScene
{
    /// <summary>The frame's colours and sprite draws, retained by reference without another copy.</summary>
    public FramePaint Paint { get; }
    /// <summary>The floor's transform; null shows no floor, leaving the backdrop.</summary>
    public AffineTransform? Background { get; }

    /// <summary>The floor's map and tiles (#1403), present exactly when <see cref="Background"/> is.</summary>
    public AffineFloor? Floor { get; }
    /// <summary>Master brightness 0..15 applied after BG1, OBJ, and optional gradient color math.</summary>
    public int Brightness { get; }
    /// <summary>Owned per-scanline fixed-color controls copied from the caller at snapshot construction.</summary>
    private readonly SuperMetroid.Core.Rendering.TitleGradientLine[] gradient;
    /// <summary>
    /// Owned fixed-color controls indexed by visible scanline 0..223, or empty to disable title color math.
    /// $A1 subtracts from BG1/backdrop; $31 adds to BG1/backdrop and winning OBJ palettes 4..7.
    /// </summary>
    public ReadOnlySpan<SuperMetroid.Core.Rendering.TitleGradientLine> Gradient => gradient;

    /// <summary>Captures a 256-by-224-pixel BG1/backdrop-then-OBJ composition, copying only the optional gradient.</summary>
    /// <param name="paint">The frame's colours and sprite draws; retained rather than recaptured.</param>
    /// <param name="background">The floor's transform, or null to leave the backdrop below OBJ. Sampling uses screen X starting at zero and physical scanline one for the first visible row.</param>
    /// <param name="brightness">Final master brightness from 0 through 15 inclusive; zero produces black.</param>
    /// <param name="gradient">Empty, or exactly 224 scanline controls whose CGADSUB control subtracts from BG1 and the backdrop or adds to them and the OBJs; copied into owned storage.</param>
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
        Brightness = Ensure.BetweenInclusive(0, FadeLevel.Full, brightness);
        Paint = paint;
        Background = background;
        if (!gradient.IsEmpty && gradient.Length != Hardware.SnesPpuLayout.ScreenHeightPixels)
            throw new ArgumentException("Title gradient requires 224 scanlines.", nameof(gradient));
        foreach (var line in gradient)
            if (!IsTitleGradientControl(line.Control))
                throw new ArgumentException("Title gradient has an unsupported CGADSUB control.", nameof(gradient));
        this.gradient = gradient.ToArray();
    }

    /// <summary>The two CGADSUB settings the title gradient uses: subtracting from BG1 and the backdrop, or adding to them and the OBJs.</summary>
    private static bool IsTitleGradientControl(ColorBlend control) => control is
        (ColorBlend.Subtract | ColorBlend.Backdrop | ColorBlend.Bg1) or
        (ColorBlend.Backdrop | ColorBlend.Sprites | ColorBlend.Bg1);
}

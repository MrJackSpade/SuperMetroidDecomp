namespace SuperMetroid.Core.Spatial;

/// <summary>
/// How a turning floor appears on the screen (#1514): the floor image is laid with floor point <see cref="Scroll"/> at the
/// screen's top-left corner, then turned clockwise by <see cref="Angle"/> and scaled by <see cref="Scale"/> about floor
/// point <see cref="Center"/>, which stays where that first placement put it. The backend turns the description into its
/// own matrix.
/// </summary>
/// <remarks>
/// A screen pixel <c>p</c> shows the floor point <c>Center + Scale * R * (p + Scroll - Center)</c>, where <c>R</c> turns a
/// screen direction anticlockwise by <see cref="Angle"/>; on a screen whose rows run downward that shows the floor turned
/// clockwise.
/// </remarks>
public readonly record struct FloorView
{
    /// <summary>The scale that shows the floor at its own size: one floor pixel to each screen pixel.</summary>
    public const double OwnSize = 1;

    /// <summary>Describes a floor turned by <paramref name="angle"/> and scaled by <paramref name="scale"/> about <paramref name="centre"/>.</summary>
    /// <param name="angle">How far the floor is turned clockwise on the screen, in radians.</param>
    /// <param name="scale">How many floor pixels one screen pixel spans, zero or more.</param>
    /// <param name="centre">The floor point the floor turns and scales about.</param>
    /// <param name="scroll">The floor point placed at the screen's top-left corner before the floor is turned and scaled.</param>
    /// <param name="outside">What shows beyond the floor's edges.</param>
    /// <exception cref="ArgumentOutOfRangeException">The angle is not finite, or the scale is negative or not finite.</exception>
    public FloorView(double angle, double scale, Offset16<FloorPixels> centre, Offset16<FloorPixels> scroll,
        FloorOverflowMode outside = FloorOverflowMode.Transparent)
    {
        if (!double.IsFinite(angle))
            throw new ArgumentOutOfRangeException(nameof(angle), angle, "A floor's angle is a finite number of radians.");
        if (!double.IsFinite(scale) || scale < 0)
            throw new ArgumentOutOfRangeException(nameof(scale), scale, "A floor's scale is a finite number of floor pixels per screen pixel, zero or more.");
        Angle = angle;
        Scale = scale;
        Center = centre;
        Scroll = scroll;
        Outside = outside;
    }

    /// <summary>How far the floor is turned clockwise on the screen, in radians.</summary>
    public double Angle { get; }

    /// <summary>
    /// How many floor pixels one screen pixel spans: <see cref="OwnSize"/> shows the floor at its own size, larger scales
    /// show it smaller, and zero spreads its centre point across the whole screen.
    /// </summary>
    public double Scale { get; }

    /// <summary>The floor point the floor turns and scales about.</summary>
    public Offset16<FloorPixels> Center { get; }

    /// <summary>The floor point placed at the screen's top-left corner before the floor is turned and scaled.</summary>
    public Offset16<FloorPixels> Scroll { get; init; }

    /// <summary>What shows beyond the floor's edges.</summary>
    public FloorOverflowMode Outside { get; init; }

    /// <summary>The view of a scene's floor turned by <paramref name="angle"/> and shown at <paramref name="zoom"/>.</summary>
    /// <param name="angle">How far the scene has turned the floor clockwise.</param>
    /// <param name="zoom">How far the scene has zoomed the floor.</param>
    /// <param name="centre">The floor point the floor turns and scales about.</param>
    /// <param name="scroll">The floor point placed at the screen's top-left corner before the floor is turned and scaled.</param>
    /// <param name="outside">What shows beyond the floor's edges.</param>
    public static FloorView Turned(RotationAngle angle, FloorZoom zoom, Offset16<FloorPixels> centre, Offset16<FloorPixels> scroll,
        FloorOverflowMode outside = FloorOverflowMode.Transparent) =>
        new(angle.Radians, zoom.FloorPixelsPerScreenPixel, centre, scroll, outside);
}

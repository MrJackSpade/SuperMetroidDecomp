namespace SuperMetroid.Core.Rendering;

/// <summary>
/// An oval of screen pixels (#1468): every pixel within an ellipse of the given radii of some point on its core, a
/// horizontal run of pixels from <see cref="CoreStart"/> to <see cref="CoreWidth"/> pixels right of it. A core of width
/// zero makes a plain ellipse, such as the Power Bomb's blast; a wider core makes a column with rounded sides, such as the
/// suit pickup's light beam. The oval may extend off the screen.
/// </summary>
public sealed record WindowOval : WindowShape
{
    /// <summary>The left end of the oval's core, on the oval's middle line.</summary>
    public Offset16<ScreenPixels> CoreStart { get; }

    /// <summary>How many pixels the core runs right of <see cref="CoreStart"/>; zero or more.</summary>
    public int CoreWidth { get; }

    /// <summary>How far the oval reaches left and right of its core (X) and above and below it (Y); one or more each.</summary>
    public Extent16<ScreenPixels> Radii { get; }

    /// <summary>Creates an oval around a horizontal core.</summary>
    /// <param name="coreStart">The left end of the core, on the oval's middle line.</param>
    /// <param name="coreWidth">How many pixels the core runs right of <paramref name="coreStart"/>; zero or more.</param>
    /// <param name="radii">How far the oval reaches beside its core (X) and above and below it (Y); one or more each.</param>
    /// <exception cref="ArgumentOutOfRangeException">The core's width is negative or a radius is below one.</exception>
    public WindowOval(Offset16<ScreenPixels> coreStart, int coreWidth, Extent16<ScreenPixels> radii)
    {
        CoreStart = coreStart;
        CoreWidth = Ensure.AtLeastZero(coreWidth);
        if (radii.X == 0 || radii.Y == 0)
            throw new ArgumentOutOfRangeException(nameof(radii), radii, "A window oval reaches at least one pixel each way.");
        Radii = radii;
    }

    /// <summary>An ellipse centred on <paramref name="centre"/>.</summary>
    public static WindowOval Ellipse(Offset16<ScreenPixels> centre, Extent16<ScreenPixels> radii) => new(centre, 0, radii);
}

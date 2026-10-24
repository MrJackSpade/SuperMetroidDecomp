using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The colours a frame's background art shows (#1408): the backdrop, and the colour each ink of the frame's images shows
/// now. An image is coloured with its standard colours; a colour effect that changes what its inks show (a glow, a fade,
/// a liquid's tint, the message box's text colours) recolours it. Backends colour a background picture from
/// <see cref="ShowsOwnColours"/> and <see cref="Colour(SpriteImage, int)"/> alone, never by looking a colour up by position.
/// </summary>
public sealed class BackgroundColours
{
    /// <summary>The inks the frame's images name: a perspective floor's tiles name every one.</summary>
    public const int Inks = ScreenPalette.ColorCount;

    /// <summary>The colour each ink shows.</summary>
    private readonly Bgr555[] colours;

    /// <summary>Copies the colour each ink shows.</summary>
    /// <param name="colours">The colour of each of the <see cref="Inks"/> inks; ink zero is the backdrop.</param>
    /// <exception cref="ArgumentException">There is not one colour per ink.</exception>
    public BackgroundColours(ReadOnlySpan<Bgr555> colours)
    {
        Ensure.LengthEqual(colours, Inks);
        this.colours = colours.ToArray();
    }

    /// <summary>Every ink's colour, for the resolution of sprite draws no owner coloured and the software rasterizer.</summary>
    internal ReadOnlySpan<Bgr555> AllColours => colours;

    /// <summary>The colour shown where nothing is drawn.</summary>
    public Bgr555 Backdrop => colours[0];

    /// <summary>The colour ink <paramref name="ink"/> shows.</summary>
    public Bgr555 Colour(int ink) => colours[Ensure.BetweenInclusive(0, Inks - 1, ink)];

    /// <summary>These colours with the backdrop shown in <paramref name="backdrop"/>.</summary>
    public BackgroundColours WithBackdrop(Bgr555 backdrop) => With(0, backdrop);

    /// <summary>These colours with ink <paramref name="ink"/> shown in <paramref name="colour"/>.</summary>
    public BackgroundColours With(int ink, Bgr555 colour)
    {
        var shown = new BackgroundColours(colours);
        shown.colours[Ensure.BetweenInclusive(0, Inks - 1, ink)] = colour;
        return shown;
    }

    /// <summary>The inks in one of an image's colour groups, which show its own colours or another's together.</summary>
    public const int GroupColours = 16;

    /// <summary>The colour groups <paramref name="sheet"/>'s inks fall in.</summary>
    public static int Groups(SpriteImage sheet) => Ensure.NotNull(sheet).StandardColours.Length / GroupColours;

    /// <summary>
    /// Whether the inks of <paramref name="sheet"/>'s group <paramref name="group"/> show the sheet's own colours, so its
    /// pixels show the sheet's pre-coloured pixels; otherwise they show <see cref="Colour(SpriteImage, int)"/>. Colour zero
    /// never decides that, as it is transparent wherever a pixel would show the sheet's own.
    /// </summary>
    public bool ShowsOwnColours(SpriteImage sheet, int group)
    {
        int first = Ensure.BetweenInclusive(0, Groups(sheet) - 1, group) * GroupColours;
        return colours.AsSpan(sheet.FirstColour + first + 1, GroupColours - 1)
            .SequenceEqual(sheet.StandardColours.Slice(first + 1, GroupColours - 1));
    }

    /// <summary>The colour <paramref name="sheet"/>'s ink <paramref name="ink"/> shows.</summary>
    public Bgr555 Colour(SpriteImage sheet, int ink) =>
        colours[Ensure.NotNull(sheet).FirstColour + Ensure.BetweenInclusive(0, sheet.StandardColours.Length - 1, ink)];
}

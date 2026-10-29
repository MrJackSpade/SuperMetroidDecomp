using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The colours a frame's images show (#1408, #1464): the backdrop, and for each colour group of the frame's images the
/// colours its owner holds now. A background image's groups are coloured by the background sets, an actor's by the
/// sprite sets and a perspective floor's by both, each group by the set of the same identity. An image is coloured with
/// its standard colours; a colour effect that changes what an owner holds (a glow, a fade, a liquid's tint, the message
/// box's text colours) recolours it. Backends colour a picture from <see cref="ShowsOwnColours"/> and
/// <see cref="Colour(SpriteImage, int)"/> alone.
/// </summary>
public sealed class BackgroundColours
{
    /// <summary>The inks in one of an image's colour groups, which show its own colours or another's together.</summary>
    public const int GroupColours = ColourSets.SetColours;

    /// <summary>Keeps the background and sprite sets as the frame shows them.</summary>
    /// <param name="backgrounds">The background sets, whose first set's first colour is the backdrop.</param>
    /// <param name="sprites">The sprite sets, with the colours their owners show over them laid on.</param>
    public BackgroundColours(ShownColourSets backgrounds, ShownColourSets sprites)
    {
        Backgrounds = Ensure.NotNull(backgrounds);
        Sprites = Ensure.NotNull(sprites);
    }

    /// <summary>Gameplay's colours as a frame shows them now.</summary>
    public static BackgroundColours Of(GameplayColours colours)
    {
        Ensure.NotNull(colours);
        return new BackgroundColours(colours.ShownBackgrounds(), colours.ShownSprites());
    }

    /// <summary>The background sets as the frame shows them.</summary>
    internal ShownColourSets Backgrounds { get; }

    /// <summary>The sprite sets as the frame shows them, for colouring the sprite draws no owner coloured.</summary>
    internal ShownColourSets Sprites { get; }

    /// <summary>The colour shown where nothing is drawn: the first background set's first colour.</summary>
    public RgbColor Backdrop => Backgrounds.Set(0)[0];

    /// <summary>These colours with the backdrop shown in <paramref name="backdrop"/>.</summary>
    public BackgroundColours WithBackdrop(RgbColor backdrop) => WithBackgroundColour(0, 0, backdrop);

    /// <summary>These colours with colour <paramref name="colour"/> of background set <paramref name="set"/> shown in <paramref name="value"/>.</summary>
    public BackgroundColours WithBackgroundColour(int set, int colour, RgbColor value) =>
        new(Backgrounds.With(set, colour, value), Sprites);

    /// <summary>The colour groups <paramref name="sheet"/>'s inks fall in.</summary>
    public static int Groups(SpriteImage sheet) => Ensure.NotNull(sheet).StandardColours.Length / GroupColours;

    /// <summary>
    /// Whether the inks of <paramref name="sheet"/>'s group <paramref name="group"/> show the sheet's own colours, so its
    /// pixels show the sheet's pre-coloured pixels; otherwise they show <see cref="Colour(SpriteImage, int)"/>. Colour zero
    /// never decides that, as it is transparent wherever a pixel would show the sheet's own.
    /// </summary>
    public bool ShowsOwnColours(SpriteImage sheet, int group)
    {
        ReadOnlySpan<RgbColor> shown = Group(sheet, group);
        return shown[1..].SequenceEqual(sheet.StandardColours.Slice(group * GroupColours, GroupColours)[1..]);
    }

    /// <summary>The colour <paramref name="sheet"/>'s ink <paramref name="ink"/> shows.</summary>
    public RgbColor Colour(SpriteImage sheet, int ink)
    {
        Ensure.BetweenInclusive(0, Ensure.NotNull(sheet).StandardColours.Length - 1, ink);
        return Group(sheet, ink / GroupColours)[ink % GroupColours];
    }

    /// <summary>
    /// The colours <paramref name="sheet"/>'s group <paramref name="group"/> shows: the set of the same identity of the
    /// owner that colours the sheet, a perspective floor's groups taking the background sets and then the sprite sets.
    /// </summary>
    private ReadOnlySpan<RgbColor> Group(SpriteImage sheet, int group)
    {
        Ensure.BetweenInclusive(0, Groups(sheet) - 1, group);
        return sheet.Colours switch
        {
            SpriteImageColours.Sprite => Sprites.Set(group),
            SpriteImageColours.Background or SpriteImageColours.TwoBppBackground => Backgrounds.Set(group),
            SpriteImageColours.FullPalette => group < ColourSets.SetCount ? Backgrounds.Set(group) : Sprites.Set(group - ColourSets.SetCount),
            _ => throw new InvalidDataException($"Image {sheet.Name} names an unknown colouring {sheet.Colours}."),
        };
    }
}

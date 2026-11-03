using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The colours a frame's images show (#1408, #1464): the backdrop, and for each colour group of the frame's images the
/// colours its owner holds now. A frontend screen's background image's group shows the screen's set of the same identity, an actor's group the
/// sprite set of the same identity, and a perspective floor's groups its background groups and then the sprite sets. A
/// gameplay background picture holds no group here: it is coloured from its frame's <see cref="GameplayShading"/>
/// (#1512), and a gameplay frame keeps only its backdrop. Backends colour a picture from
/// <see cref="ShowsOwnColours"/> and <see cref="Colour(SpriteImage, int)"/> alone.
/// </summary>
public sealed class BackgroundColours
{
    /// <summary>The inks in one of an image's colour groups, which show its own colours or another's together.</summary>
    public const int GroupColours = ColourSets.SetColours;

    /// <summary>The background groups of a perspective floor's image, which its sprite groups follow.</summary>
    private const int FloorBackgroundGroups = SpriteImage.SpriteColourCount / GroupColours;

    /// <summary>
    /// Keeps a gameplay frame's backdrop and sprite sets (#1512): its background pictures are coloured from the frame's
    /// <see cref="GameplayShading"/>, so it holds no background set.
    /// </summary>
    /// <param name="backdrop">The colour shown where nothing is drawn.</param>
    /// <param name="sprites">The sprite sets, with the colours their owners show over them laid on.</param>
    public BackgroundColours(RgbColor backdrop, ShownColourSets sprites)
        : this(new GameplayBackdrop(backdrop), sprites)
    {
    }

    /// <summary>Keeps a frontend screen's background and sprite sets as the frame shows them.</summary>
    /// <param name="backgrounds">The screen's background sets, whose first set's first colour is the backdrop.</param>
    /// <param name="sprites">The screen's sprite sets.</param>
    public BackgroundColours(ShownColourSets backgrounds, ShownColourSets sprites)
        : this(new ScreenShownBackgrounds(backgrounds), sprites)
    {
    }

    /// <summary>Keeps the background colours and sprite sets as the frame shows them.</summary>
    private BackgroundColours(ShownBackgrounds backgrounds, ShownColourSets sprites)
    {
        Backgrounds = Ensure.NotNull(backgrounds);
        Sprites = Ensure.NotNull(sprites);
    }

    /// <summary>
    /// A frontend screen's colours whose pictures and sprites all carry their own shading (#1512): only the backdrop,
    /// <paramref name="backdrop"/>, is shown from them.
    /// </summary>
    public static BackgroundColours ScreenBackdrop(RgbColor backdrop)
    {
        var none = ShownColourSets.Of(new ScreenColourSets());
        return new BackgroundColours(none.With(0, 0, backdrop), none);
    }

    /// <summary>The background colours as the frame shows them.</summary>
    internal ShownBackgrounds Backgrounds { get; }

    /// <summary>The sprite sets as the frame shows them, for colouring the sprite draws no owner coloured.</summary>
    internal ShownColourSets Sprites { get; }

    /// <summary>The colour shown where nothing is drawn.</summary>
    public RgbColor Backdrop => Backgrounds.Backdrop;

    /// <summary>These colours with the backdrop shown in <paramref name="backdrop"/>.</summary>
    public BackgroundColours WithBackdrop(RgbColor backdrop) => new(Backgrounds.WithBackdrop(backdrop), Sprites);

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
    /// The colours <paramref name="sheet"/>'s group <paramref name="group"/> shows: the background or sprite colours of
    /// the owner that colours the sheet, a perspective floor's groups taking its background groups' and then the sprite
    /// sets.
    /// </summary>
    private ReadOnlySpan<RgbColor> Group(SpriteImage sheet, int group)
    {
        Ensure.BetweenInclusive(0, Groups(sheet) - 1, group);
        return sheet.Colours switch
        {
            SpriteImageColours.Sprite => Sprites.Set(group),
            SpriteImageColours.Background or SpriteImageColours.TwoBppBackground => Backgrounds.Group(sheet, group),
            SpriteImageColours.FullPalette => group < FloorBackgroundGroups
                ? Backgrounds.Group(sheet, group)
                : Sprites.Set(group - FloorBackgroundGroups),
            _ => throw new InvalidDataException($"Image {sheet.Name} names an unknown colouring {sheet.Colours}."),
        };
    }
}

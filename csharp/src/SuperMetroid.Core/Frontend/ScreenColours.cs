using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// A whole frontend screen's installed colours by owner (#1464): its background sets' and its sprite sets'. The import
/// pipeline decodes them from the installed palette, whose colour order is its own knowledge; a screen loads them, or
/// some owner's or group's of them, into its <see cref="ScreenPalette"/>, and fades read their targets from them.
/// </summary>
public sealed class ScreenColours
{
    /// <summary>The colours of a whole screen: every set of both owners.</summary>
    public const int ColourCount = 2 * ColourSets.SetCount * ColourSets.SetColours;

    /// <summary>The colours by owner, which nothing else holds.</summary>
    private readonly ScreenColourValues<RgbColor> colours = new();

    /// <summary>Copies the decoded owners' colours.</summary>
    /// <param name="backgroundSets">Every background set's colours, set after set.</param>
    /// <param name="spriteSets">Every sprite set's colours, set after set.</param>
    /// <exception cref="ArgumentException">An owner's part does not hold its eight sets' colours.</exception>
    internal ScreenColours(ReadOnlySpan<RgbColor> backgroundSets, ReadOnlySpan<RgbColor> spriteSets)
    {
        Ensure.LengthEqual(backgroundSets, colours.Backgrounds.Length);
        Ensure.LengthEqual(spriteSets, colours.Sprites.Length);
        backgroundSets.CopyTo(colours.Backgrounds);
        spriteSets.CopyTo(colours.Sprites);
    }

    /// <summary>A copy of <paramref name="screen"/>'s colours as they are now.</summary>
    internal static ScreenColours Of(ScreenPalette screen) =>
        new(Ensure.NotNull(screen).BackgroundSets.All.Colors, screen.SpriteSets.All.Colors);

    /// <summary>The colours <paramref name="colour"/> gives each cell of a whole screen.</summary>
    internal static ScreenColours From(Func<ScreenColourCell, RgbColor> colour)
    {
        Ensure.NotNull(colour);
        var screen = new ScreenPalette();
        for (int set = 0; set < ColourSets.SetCount; set++)
        for (int cell = 0; cell < ColourSets.SetColours; cell++)
        {
            screen.BackgroundSets.Set(set).Write(cell, colour(new ScreenColourCell(ScreenColourOwner.Backgrounds, set, cell)));
            screen.SpriteSets.Set(set).Write(cell, colour(new ScreenColourCell(ScreenColourOwner.Sprites, set, cell)));
        }
        return Of(screen);
    }

    /// <summary>The colour of <paramref name="cell"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cell names no owner, set or colour of a screen.</exception>
    public RgbColor this[ScreenColourCell cell] => cell.Owner switch
    {
        ScreenColourOwner.Backgrounds => BackgroundSet(cell.Set)[cell.Colour],
        ScreenColourOwner.Sprites => SpriteSet(cell.Set)[cell.Colour],
        _ => throw new ArgumentOutOfRangeException(nameof(cell), cell, "Undefined screen colour owner."),
    };

    /// <summary>Every background set's colours, set after set.</summary>
    public ReadOnlySpan<RgbColor> Backgrounds => colours.Backgrounds;

    /// <summary>Every sprite set's colours, set after set.</summary>
    public ReadOnlySpan<RgbColor> Sprites => colours.Sprites;

    /// <summary>Background set <paramref name="set"/>'s colours, zero through seven.</summary>
    public ReadOnlySpan<RgbColor> BackgroundSet(int set) => colours.BackgroundColours(set, 0, ColourSets.SetColours);

    /// <summary>Sprite set <paramref name="set"/>'s colours, zero through seven.</summary>
    public ReadOnlySpan<RgbColor> SpriteSet(int set) => colours.SpriteColours(set, 0, ColourSets.SetColours);

    /// <summary>The colours of <paramref name="group"/> these colours hold.</summary>
    public ReadOnlySpan<RgbColor> Colours(ScreenColourGroup group) => ScreenColourGroups.In(colours, group);

    /// <summary>The colour of <paramref name="group"/>, a single-colour group.</summary>
    /// <exception cref="ArgumentException"><paramref name="group"/> holds more than one colour.</exception>
    public RgbColor Colour(ScreenColourGroup group)
    {
        ReadOnlySpan<RgbColor> groupColours = Colours(group);
        Ensure.LengthEqual(groupColours, 1);
        return groupColours[0];
    }

    /// <summary>Replaces every colour of <paramref name="screen"/> with these.</summary>
    public void LoadTo(ScreenPalette screen)
    {
        LoadBackgroundsTo(screen);
        LoadSpritesTo(screen);
    }

    /// <summary>Replaces every colour of <paramref name="screen"/> with these, except the colours of <paramref name="kept"/>.</summary>
    /// <param name="screen">The screen whose colours change.</param>
    /// <param name="kept">Runs of the screen's colours that keep what they hold, such as colours an animation owns.</param>
    public void LoadToExcept(ScreenPalette screen, params ColourRun[] kept)
    {
        var keptColours = new ColourImage(kept);
        LoadTo(screen);
        keptColours.Restore();
    }

    /// <summary>Replaces <paramref name="screen"/>'s background sets' colours with these.</summary>
    public void LoadBackgroundsTo(ScreenPalette screen) => Ensure.NotNull(screen).BackgroundSets.All.CopyFrom(Backgrounds);

    /// <summary>Replaces <paramref name="screen"/>'s sprite sets' colours with these.</summary>
    public void LoadSpritesTo(ScreenPalette screen) => Ensure.NotNull(screen).SpriteSets.All.CopyFrom(Sprites);

    /// <summary>Replaces the colours of <paramref name="group"/> on <paramref name="screen"/> with these.</summary>
    public void LoadTo(ScreenPalette screen, ScreenColourGroup group) =>
        Ensure.NotNull(screen).Colours(group).CopyFrom(Colours(group));
}

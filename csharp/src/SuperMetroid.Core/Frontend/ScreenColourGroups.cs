using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The frontend screens' colour groups resolved to the owner that holds them (#1464): a group's colours are a run of the
/// screen's background sets or sprite sets, and a sprite drawn in a whole-set group shows that sprite set. Effects and
/// actors name a <see cref="ScreenColourGroup"/>, and this owner resolves it through
/// <see cref="ScreenColourGroupDefinitions"/>.
/// </summary>
internal static class ScreenColourGroups
{
    /// <summary>The colours of <paramref name="group"/> on <paramref name="screen"/>, read and written in place.</summary>
    public static ColourRun Colours(this ScreenPalette screen, ScreenColourGroup group) => Placement(group).In(screen);

    /// <summary>The values of <paramref name="group"/>'s colours among <paramref name="values"/>, to change in place.</summary>
    public static Span<T> In<T>(PerScreenColour<T> values, ScreenColourGroup group) => Placement(group).In(values);

    /// <summary>
    /// The sprite set a sprite drawn in <paramref name="colouring"/> shows; the colouring must be one whole sprite set.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="colouring"/> is not a whole sprite set.</exception>
    public static int SpriteSet(ScreenColourGroup colouring) => Placement(colouring).DrawnSpriteSet();

    /// <summary>The screen colour cell that holds the first colour of <paramref name="group"/>.</summary>
    public static ScreenColourCell FirstCell(ScreenColourGroup group)
    {
        ScreenColourPlacement placement = Placement(group);
        return new ScreenColourCell(placement.Owner, placement.Set, placement.Colour);
    }

    /// <summary>Where the screen holds <paramref name="group"/>.</summary>
    private static ScreenColourPlacement Placement(ScreenColourGroup group) =>
        ScreenColourGroupDefinitions.Groups.TryGetValue(group, out ScreenColourPlacement? placement)
            ? placement
            : throw new ArgumentOutOfRangeException(nameof(group), group, "Undefined screen colour group.");
}

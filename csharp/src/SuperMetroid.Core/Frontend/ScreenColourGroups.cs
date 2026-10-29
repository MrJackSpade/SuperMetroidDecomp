using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The frontend screens' colour groups resolved to the colours the screen holds them in (#1464). This is the one place a
/// screen colour group meets a position among the screen's colours; effects and actors name a <see cref="ScreenColourGroup"/>
/// and this owner resolves it.
/// </summary>
internal static class ScreenColourGroups
{
    /// <summary>The colours of <paramref name="group"/> among <paramref name="colours"/>, read and written in place.</summary>
    public static ColourRun Colours(this ScreenPalette colours, ScreenColourGroup group)
    {
        ScreenColourGroupDefinition definition = Definition(group);
        return Ensure.NotNull(colours).All.Slice(First(definition), definition.ColourCount);
    }

    /// <summary>
    /// The entries for <paramref name="group"/> in <paramref name="screenOrdered"/>, one entry per colour of a whole screen,
    /// laid out as a screen palette's colours.
    /// </summary>
    public static ReadOnlySpan<T> In<T>(ReadOnlySpan<T> screenOrdered, ScreenColourGroup group)
    {
        Ensure.LengthEqual(screenOrdered, ScreenPalette.ColorCount);
        ScreenColourGroupDefinition definition = Definition(group);
        return screenOrdered.Slice(First(definition), definition.ColourCount);
    }

    /// <summary>
    /// The entries for <paramref name="group"/> in <paramref name="screenOrdered"/>, one entry per colour of a whole screen,
    /// laid out as a screen palette's colours, to change in place.
    /// </summary>
    public static Span<T> InPlace<T>(Span<T> screenOrdered, ScreenColourGroup group)
    {
        Ensure.LengthEqual<T>(screenOrdered, ScreenPalette.ColorCount);
        ScreenColourGroupDefinition definition = Definition(group);
        return screenOrdered.Slice(First(definition), definition.ColourCount);
    }

    /// <summary>The colour <paramref name="group"/>, a single-colour group, in <paramref name="screenColours"/>.</summary>
    public static RgbColor ColourIn(ReadOnlySpan<RgbColor> screenColours, ScreenColourGroup group)
    {
        ReadOnlySpan<RgbColor> colours = In(screenColours, group);
        Ensure.LengthEqual(colours, 1);
        return colours[0];
    }

    /// <summary>
    /// The sprite row a sprite drawn in <paramref name="colouring"/> selects; the colouring must be one whole sprite row.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="colouring"/> is not a whole sprite row.</exception>
    public static int SpriteRow(ScreenColourGroup colouring)
    {
        ScreenColourGroupDefinition definition = Definition(colouring);
        return definition.IsWholeSpriteRow
            ? definition.Row
            : throw new ArgumentOutOfRangeException(nameof(colouring), colouring, "A sprite draws in a whole sprite row's colours.");
    }

    /// <summary>The definition of <paramref name="group"/>.</summary>
    private static ScreenColourGroupDefinition Definition(ScreenColourGroup group) =>
        ScreenColourGroupDefinitions.Groups.TryGetValue(group, out ScreenColourGroupDefinition definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(group), group, "Undefined screen colour group.");

    /// <summary>The position of the group's first colour among a screen's colours.</summary>
    private static int First(ScreenColourGroupDefinition definition)
    {
        int kindStart = definition.Kind == ScreenColourKind.Sprite ? ScreenPalette.RowsPerKind * ScreenPalette.ColoursPerRow : 0;
        return kindStart + definition.Row * ScreenPalette.ColoursPerRow + definition.FirstColour;
    }
}

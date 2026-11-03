using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.World;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>
/// The escape timer's shades for a door's fades (#1512), which the import pre-colours the timer's sheet in: each step of
/// the fade out to black and of the fade in from it, of every colour of its row the door does not keep lit. A door keeps
/// the timer's colours one, two, four and thirteen, as <c>GameState_A_LoadingNextRoom</c>'s third copy into its fade's
/// target keeps them.
/// </summary>
internal static class EscapeTimerDoorFadeShades
{
    /// <summary>Colours in the timer's sprite row.</summary>
    private const int RowColours = 16;

    /// <summary>The timer's colours a door keeps lit.</summary>
    internal static readonly ImmutableArray<int> Kept = [1, 2, 4, 13];

    /// <summary>Every door fade shade of the timer drawn in sprite row <paramref name="row"/> of <paramref name="standard"/>.</summary>
    /// <param name="standard">The sprite colours the timer's sheet is coloured from, row after row.</param>
    /// <param name="row">The sprite row the timer is drawn in.</param>
    internal static IReadOnlyList<SheetShade> Of(ReadOnlySpan<RgbColor> standard, int row)
    {
        int[] faded = [.. Enumerable.Range(0, RowColours).Except(Kept).Select(colour => row * RowColours + colour)];
        var shades = new List<SheetShade>();
        for (int value = 0; value < DoorTransitionColourDefinitions.HudFadeShades; value++)
        {
            shades.Add(Shade(DoorFadeShades.EscapeTimerFadingOut(value), faded, standard, value, GradualFadeDirection.Toward));
            shades.Add(Shade(DoorFadeShades.EscapeTimerFadingIn(value), faded, standard, value, GradualFadeDirection.From));
        }
        return shades;
    }

    /// <summary>
    /// <paramref name="inks"/> after the steps of a door's fade to or from black from the shared counter's zero through
    /// <paramref name="value"/>.
    /// </summary>
    private static SheetShade Shade(PictureShade shade, int[] inks, ReadOnlySpan<RgbColor> standard, int value, GradualFadeDirection direction)
    {
        var fade = new GradualColourFade(RgbColor.Black, direction, DoorTransitionColourDefinitions.FadeDenominator,
            [.. Enumerable.Range(0, value + 1)]);
        var colours = new RgbColor[inks.Length];
        for (int index = 0; index < inks.Length; index++)
            colours[index] = fade.Apply(standard[inks[index]]);
        return new SheetShade(shade, inks, colours);
    }
}

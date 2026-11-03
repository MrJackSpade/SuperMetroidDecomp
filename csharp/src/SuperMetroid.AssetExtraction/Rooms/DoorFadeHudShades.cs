using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.World;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The HUD's shades for a door's fades (#1512), which the import pre-colours a tileset's layer-3 pictures in: each step of
/// the fade out to black and of the fade in from it, of every HUD colour the door does not keep lit. A door keeps the
/// minimap's explored and unexplored colours, the highlighted item's and the minimap's room highlight, and between two
/// common rooms the HUD's item and minimap backgrounds too, as <c>GameState_A_LoadingNextRoom</c> copies them into its
/// fade's target.
/// </summary>
internal static class DoorFadeHudShades
{
    /// <summary>The HUD's inks: its two colour sets of sixteen, as a layer-3 picture inks them.</summary>
    private const int HudInks = 32;

    /// <summary>The inks every door keeps lit: the minimap's explored and unexplored colours, the highlighted item's and the room highlight.</summary>
    private static readonly ImmutableArray<int> Kept = [9, 10, 13, 14, 17, 18, 19, 29];

    /// <summary>The inks a door between two common rooms also keeps lit: the HUD's item backgrounds and the minimap's.</summary>
    private static readonly ImmutableArray<int> KeptBetweenCommonRooms = [20, 21, 22, 23, 28];

    /// <summary>Every door fade shade of the HUD whose standard colours are <paramref name="standard"/>.</summary>
    /// <param name="standard">The standard colours of a tileset's layer-3 pictures.</param>
    internal static IReadOnlyList<SheetShade> Of(ReadOnlySpan<RgbColor> standard)
    {
        int[] ordinary = Faded(Kept);
        int[] common = Faded([.. Kept, .. KeptBetweenCommonRooms]);
        var shades = new List<SheetShade>();
        for (int value = 0; value < DoorTransitionColourDefinitions.HudFadeShades; value++)
        {
            shades.Add(Shade(DoorFadeShades.HudFadingOut(value), ordinary, standard, value, GradualFadeDirection.Toward));
            shades.Add(Shade(DoorFadeShades.HudFadingOutBetweenCommonRooms(value), common, standard, value, GradualFadeDirection.Toward));
            shades.Add(Shade(DoorFadeShades.HudFadingIn(value), ordinary, standard, value, GradualFadeDirection.From));
            shades.Add(Shade(DoorFadeShades.HudFadingInBetweenCommonRooms(value), common, standard, value, GradualFadeDirection.From));
        }
        return shades;
    }

    /// <summary>The HUD inks a door fades: all but <paramref name="kept"/>, in rising order.</summary>
    private static int[] Faded(ImmutableArray<int> kept) => [.. Enumerable.Range(0, HudInks).Except(kept)];

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

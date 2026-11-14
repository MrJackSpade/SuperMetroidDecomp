using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The shades of the palette-FX colour groups that recolour an enemy's sprite row (#1512): the Ridley and Phantoon sprite
/// statues' grey-out, the escape's shutter flash and the landed gunship's glow (#1733). Each record an object's program shows is pre-coloured over the row
/// from the group's first colour, as the handler wrote it there; the enemy that shows the group lays the record's shade.
/// </summary>
internal static class PaletteFxSpriteShades
{
    /// <summary>Colours in one sprite row.</summary>
    private const int RowColours = 16;

    /// <summary>The colour of its sprite row each group's first written colour is.</summary>
    private static readonly ImmutableDictionary<ColourEffectColours, int> FirstColours = new Dictionary<ColourEffectColours, int>
    {
        [ColourEffectColours.RidleyStatue] = 0,
        [ColourEffectColours.PhantoonStatue] = 0,
        [ColourEffectColours.TourianShutters] = 9,
        [ColourEffectColours.GunshipGlow] = 15,
    }.ToImmutableDictionary();

    /// <summary>
    /// The shade of every record <paramref name="effect"/>'s program shows in <paramref name="group"/>, over the sprite
    /// row whose first ink is <paramref name="rowFirstInk"/>, coloured from <paramref name="colours"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The group recolours no sprite row, the program shows another group, a colour is not installed, or a record writes
    /// past the row.
    /// </exception>
    internal static IEnumerable<SheetShade> Of(ColourEffectId effect, ColourEffectColours group, int rowFirstInk, IPaletteFxColorSource colours)
    {
        Ensure.NotNull(colours);
        if (!FirstColours.TryGetValue(group, out int firstColour))
            throw new InvalidDataException($"Palette-FX colours {group} recolour no sprite row.");
        var shades = new List<SheetShade>();
        foreach ((ColourEffectColours shownGroup, ColourEffectFrame record) in PaletteFxRecordShades.ShownBy([effect]))
        {
            if (shownGroup != group)
                throw new InvalidDataException($"Palette-FX object {effect} shows {shownGroup}, not {group}.");
            shades.Add(PaletteFxRecordShades.Shade(group, record, new InkRange(rowFirstInk + firstColour, rowFirstInk + RowColours), colours));
        }
        return shades;
    }
}

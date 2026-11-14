using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The shades of a frontend screen's palette-FX colour groups (#1512), which the import pre-colours the screen's sheets
/// in: each record a group the screen's objects run can show, its colours over the inks of the sheet the group recolours.
/// The screen's composer places each group's first ink, as <c>PaletteFXObject_Handler</c> writes a record's colours from
/// the object's colour pointer, skipping those it skips.
/// </summary>
internal static class PaletteFxScreenShades
{
    /// <summary>
    /// The shades of every record of <paramref name="group"/> that <paramref name="objects"/>' programs show, over
    /// <paramref name="inks"/>, coloured from <paramref name="colours"/>.
    /// </summary>
    /// <param name="objects">The palette-FX objects the screen spawns.</param>
    /// <param name="group">The colour group the sheet shows.</param>
    /// <param name="inks">
    /// From the sheet's ink the group's first written colour is to the ink after the last the group may write: the end of
    /// its colour group.
    /// </param>
    /// <param name="colours">The records' installed colours.</param>
    /// <exception cref="InvalidDataException">A record has no installed colour, or writes at or past the end of <paramref name="inks"/>.</exception>
    internal static IReadOnlyList<SheetShade> Of(IEnumerable<ColourEffectId> objects, ColourEffectColours group, InkRange inks,
        IPaletteFxColorSource colours) =>
        [.. PaletteFxRecordShades.ShownBy(objects).Where(entry => entry.Colours == group)
            .Select(entry => PaletteFxRecordShades.Shade(group, entry.Frame, inks, colours))];
}

using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// What every palette-FX shade builder shares (#1512, #1598): the frames a set of programs can show, each under the colour
/// group its last cue named, and the shade of one frame over the inks its group recolours, as
/// <c>PaletteFXObject_Handler</c> writes the frame's record's colours from the object's colour pointer, skipping those it
/// skips. The room's, the screens', Samus's, the sprites' and the bosses' shades differ only in where a group's inks lie.
/// </summary>
internal static partial class PaletteFxRecordShades
{
    /// <summary>
    /// Every colour group and frame <paramref name="objects"/> can show, each once, ordered by group, then by effect and
    /// frame, the order of the cartridge's records (<see cref="ColourEffectShownFrames"/>).
    /// </summary>
    internal static ShownColourEffectFrame[] ShownBy(IEnumerable<ColourEffectId> objects) => ColourEffectShownFrames.ShownBy(objects);

    /// <summary>
    /// The shade of <paramref name="group"/> showing <paramref name="frame"/> over <paramref name="inks"/>, coloured from
    /// <paramref name="colours"/> with the colours of the record the frame shows, skipping the colours its layout skips.
    /// </summary>
    /// <exception cref="InvalidDataException">A colour is missing from the import's palette-effect colours, or the record writes at or past the end of <paramref name="inks"/>.</exception>
    internal static SheetShade Shade(ColourEffectColours group, ColourEffectFrame frame, InkRange inks, IPaletteFxColorSource colours)
    {
        Ensure.NotNull(colours);
        PaletteFxRecord record = PaletteFxRecord.Of(frame);
        int ink = inks.First;
        var written = new Dictionary<int, RgbColor>();
        int colour = 0;
        foreach (PaletteFxRecordPart part in SuperMetroid.AssetExtraction.Fx.PaletteFx.PaletteFxRecords.Layout(record))
        {
            if (part.Skip)
            {
                ink += part.Count;
                continue;
            }
            for (int index = 0; index < part.Count; index++, ink++, colour++)
            {
                if (ink >= inks.End)
                    throw new InvalidDataException($"Palette-FX frame {frame} writes {group} past its colour group.");
                if (!colours.TryReadColor(new PaletteFxColorKey(record, colour), out RgbColor shown))
                    throw new InvalidDataException($"Palette-FX colour {colour} of frame {frame} is missing from the import's palette-effect colours.");
                written.Add(ink, shown);
            }
        }
        return SheetShade.Of(ColourEffectShades.Of(group, frame), written);
    }
}

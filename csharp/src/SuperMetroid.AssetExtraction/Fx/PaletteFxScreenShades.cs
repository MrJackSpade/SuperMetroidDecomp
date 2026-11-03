using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Programs;

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
    /// The shades of every record of <paramref name="group"/> that <paramref name="objects"/>' programs show, over the inks
    /// from <paramref name="first"/> to before <paramref name="end"/>, coloured from <paramref name="colours"/>.
    /// </summary>
    /// <param name="objects">The palette-FX objects the screen spawns.</param>
    /// <param name="group">The colour group the sheet shows.</param>
    /// <param name="first">The sheet's ink the group's first written colour is.</param>
    /// <param name="end">The ink after the last the group may write: the end of its colour group.</param>
    /// <param name="colours">The records' installed colours.</param>
    /// <exception cref="InvalidDataException">A record has no installed colour, or writes at or past <paramref name="end"/>.</exception>
    internal static IReadOnlyList<SheetShade> Of(IEnumerable<PaletteFxObject> objects, PaletteFxColours group, int first, int end,
        IPaletteFxColorSource colours)
    {
        Ensure.NotNull(objects);
        var shown = new SortedSet<(PaletteFxColours Colours, int Record)>();
        foreach (PaletteFxObject shownObject in objects)
            Collect(RoomPaletteFxSelection.Get(shownObject).InitialProgram, shown);
        return [.. shown.Where(entry => entry.Colours == group)
            .Select(entry => Shade(group, new PaletteFxFrame(entry.Record), first, end, colours))];
    }

    /// <summary>
    /// The shade of <paramref name="group"/> showing <paramref name="record"/> over the inks from <paramref name="first"/>
    /// to before <paramref name="end"/>, coloured from <paramref name="colours"/>, skipping the colours its layout skips.
    /// </summary>
    /// <exception cref="InvalidDataException">A colour is not installed, or the record writes at or past <paramref name="end"/>.</exception>
    internal static SheetShade Shade(PaletteFxColours group, PaletteFxFrame record, int first, int end, IPaletteFxColorSource colours)
    {
        Ensure.NotNull(colours);
        int ink = first;
        var inks = new List<int>();
        var shades = new List<RgbColor>();
        int written = 0;
        foreach (PaletteFxRecordPart part in PaletteFxRecordDefinitions.Layout(record))
        {
            if (part.Skip)
            {
                ink += part.Count;
                continue;
            }
            for (int index = 0; index < part.Count; index++, ink++, written++)
            {
                if (ink >= end)
                    throw new InvalidDataException($"Palette-FX record {record.Record} writes {group} past its colour group.");
                if (!colours.TryReadColor(new PaletteFxColorKey(record, written), out RgbColor colour))
                    throw new InvalidDataException($"Palette-FX colour {written} of record {record.Record} has no installed definition.");
                inks.Add(ink);
                shades.Add(colour);
            }
        }
        return new SheetShade(PaletteFxShades.Of(group, record), [.. inks], [.. shades]);
    }

    /// <summary>
    /// Adds every colour group and record <paramref name="start"/> can show to <paramref name="shown"/>: its steps and those
    /// of each program it jumps, falls or repeats into, each under the colours its last cue named. No screen object's
    /// frame behaviour switches its program.
    /// </summary>
    internal static void Collect(PaletteFxProgram start, ISet<(PaletteFxColours Colours, int Record)> shown)
    {
        Ensure.NotNull(shown);
        var visited = new HashSet<(PaletteFxProgram Program, PaletteFxColours? Colours)>();
        var pending = new Stack<(PaletteFxProgram Program, PaletteFxColours? Colours)>();
        pending.Push((start, null));
        while (pending.Count > 0)
        {
            (PaletteFxProgram program, PaletteFxColours? colours) = pending.Pop();
            if (!visited.Add((program, colours)))
                continue;
            foreach (AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> step in PaletteFxAnimations.Programs.Steps(program))
            {
                switch (step)
                {
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.CueStep { Cue: PaletteFxCue.SetColours setColours }:
                        colours = setColours.Target;
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.ShowStep show:
                        if (colours is { } group)
                            shown.Add((group, show.Frame.Record));
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.GoToStep goTo:
                        pending.Push((goTo.Target, colours));
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.FallIntoStep fallInto:
                        pending.Push((fallInto.Target, colours));
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.RepeatWhileCountingStep repeat:
                        pending.Push((repeat.Target, colours));
                        break;
                }
            }
        }
    }
}

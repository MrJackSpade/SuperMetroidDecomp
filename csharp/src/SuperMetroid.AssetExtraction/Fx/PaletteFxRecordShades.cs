using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// What every palette-FX shade builder shares (#1512): the records a set of programs can show, each under the colour
/// group its last cue named, and the shade of one record over the inks its group recolours, as
/// <c>PaletteFXObject_Handler</c> writes a record's colours from the object's colour pointer, skipping those it skips.
/// The room's, the screens', Samus's, the sprites' and the bosses' shades differ only in where a group's inks lie.
/// </summary>
internal static class PaletteFxRecordShades
{
    /// <summary>Orders shown records by colour group, then by record.</summary>
    private static readonly Comparer<PaletteFxShownRecord> ShownOrder = Comparer<PaletteFxShownRecord>.Create((left, right) =>
    {
        int byColours = Comparer<PaletteFxColours>.Default.Compare(left.Colours, right.Colours);
        return byColours != 0 ? byColours : left.Record.Record.CompareTo(right.Record.Record);
    });

    /// <summary>
    /// Every colour group and record <paramref name="starts"/> can show, each once, ordered by group then record: each
    /// program's steps and those of each program it jumps, falls or repeats into, or switches to above the Crateria
    /// lightning's height, each under the colours its last cue named.
    /// </summary>
    internal static PaletteFxShownRecord[] Shown(IEnumerable<PaletteFxProgram> starts)
    {
        Ensure.NotNull(starts);
        var shown = new SortedSet<PaletteFxShownRecord>(ShownOrder);
        foreach (PaletteFxProgram start in starts)
            Collect(start, shown);
        return [.. shown];
    }

    /// <summary>The records the initial programs of <paramref name="objects"/> can show, as <see cref="Shown(IEnumerable{PaletteFxProgram})"/> gives them.</summary>
    internal static PaletteFxShownRecord[] ShownBy(IEnumerable<PaletteFxObject> objects) =>
        Shown(Ensure.NotNull(objects).Select(shownObject => RoomPaletteFxSelection.Get(shownObject).InitialProgram));

    /// <summary>
    /// The shade of <paramref name="group"/> showing <paramref name="record"/> over <paramref name="inks"/>, coloured from
    /// <paramref name="colours"/>, skipping the colours its layout skips.
    /// </summary>
    /// <exception cref="InvalidDataException">A colour is not installed, or the record writes at or past the end of <paramref name="inks"/>.</exception>
    internal static SheetShade Shade(PaletteFxColours group, PaletteFxFrame record, InkRange inks, IPaletteFxColorSource colours)
    {
        Ensure.NotNull(colours);
        int ink = inks.First;
        var written = new List<int>();
        var shades = new List<RgbColor>();
        int colour = 0;
        foreach (PaletteFxRecordPart part in PaletteFxRecordDefinitions.Layout(record))
        {
            if (part.Skip)
            {
                ink += part.Count;
                continue;
            }
            for (int index = 0; index < part.Count; index++, ink++, colour++)
            {
                if (ink >= inks.End)
                    throw new InvalidDataException($"Palette-FX record {record.Record} writes {group} past its colour group.");
                if (!colours.TryReadColor(new PaletteFxColorKey(record, colour), out RgbColor shown))
                    throw new InvalidDataException($"Palette-FX colour {colour} of record {record.Record} has no installed definition.");
                written.Add(ink);
                shades.Add(shown);
            }
        }
        return new SheetShade(PaletteFxShades.Of(group, record), [.. written], [.. shades]);
    }

    /// <summary>Adds every colour group and record <paramref name="start"/> can show to <paramref name="shown"/>.</summary>
    /// <exception cref="InvalidOperationException">A program holds a step of no defined kind.</exception>
    private static void Collect(PaletteFxProgram start, SortedSet<PaletteFxShownRecord> shown)
    {
        var visited = new HashSet<PaletteFxReachedProgram>();
        var pending = new Stack<PaletteFxReachedProgram>();
        pending.Push(new PaletteFxReachedProgram(start, null));
        while (pending.Count > 0)
        {
            PaletteFxReachedProgram reached = pending.Pop();
            if (!visited.Add(reached))
                continue;
            PaletteFxColours? colours = reached.Colours;
            foreach (AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> step in PaletteFxAnimations.Programs.Steps(reached.Program))
            {
                switch (step)
                {
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.CueStep { Cue: PaletteFxCue.SetColours setColours }:
                        colours = setColours.Target;
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.CueStep { Cue: PaletteFxCue.SetFrameBehavior behavior }:
                        if (SwitchedTo(behavior.FrameBehavior) is { } switched)
                            pending.Push(new PaletteFxReachedProgram(switched, colours));
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.ShowStep show:
                        if (colours is { } group)
                            shown.Add(new PaletteFxShownRecord(group, show.Frame));
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.GoToStep goTo:
                        pending.Push(new PaletteFxReachedProgram(goTo.Target, colours));
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.FallIntoStep fallInto:
                        pending.Push(new PaletteFxReachedProgram(fallInto.Target, colours));
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.RepeatWhileCountingStep repeat:
                        pending.Push(new PaletteFxReachedProgram(repeat.Target, colours));
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.CueStep:
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.HoldStep:
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.WaitStep:
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.SetCounterStep:
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.StopStep:
                        break;
                    default:
                        throw new InvalidOperationException($"Palette-FX program {reached.Program} has an undefined step {step}.");
                }
            }
        }
    }

    /// <summary>The program a frame behaviour switches its object to, or null for one that switches to none.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="behavior"/> is not a defined frame behaviour.</exception>
    private static PaletteFxProgram? SwitchedTo(PaletteFxFrameBehavior behavior) => behavior switch
    {
        PaletteFxFrameBehavior.SwitchAboveY380 => PaletteFxProgram.Crateria11,
        PaletteFxFrameBehavior.SwitchAboveY380Second => PaletteFxProgram.CrateriaAboveY380Second,
        PaletteFxFrameBehavior.Null => null,
        PaletteFxFrameBehavior.Cleared => null,
        PaletteFxFrameBehavior.Intro => null,
        PaletteFxFrameBehavior.DeleteWhenEnemyZeroDies => null,
        PaletteFxFrameBehavior.Heat => null,
        PaletteFxFrameBehavior.DeleteWhenAreaMiniBossDies => null,
        PaletteFxFrameBehavior.InspectAdjacentSlot => null,
        _ => throw new ArgumentOutOfRangeException(nameof(behavior), behavior, "Undefined palette-FX frame behaviour."),
    };
}

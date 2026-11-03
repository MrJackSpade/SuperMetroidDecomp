using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The shades of the room's palette-FX colour groups (#1512), which the import pre-colours a tileset's room pictures in:
/// each record a group its rooms run can show, its colours over the inks the group recolours. Which inks those are is the
/// import's: each group starts at one ink of a room picture and runs to the end of that ink's colour group, as
/// <c>PaletteFXObject_Handler</c> writes a record's colours from the object's colour pointer, skipping those it skips.
/// </summary>
internal static class PaletteFxRoomShades
{
    /// <summary>The colours of one of a room picture's colour groups.</summary>
    private const int GroupInks = 16;

    /// <summary>The first ink of a room picture each of the room's colour groups recolours.</summary>
    private static readonly ImmutableDictionary<PaletteFxColours, int> FirstInks = new Dictionary<PaletteFxColours, int>
    {
        [PaletteFxColours.DraygonStatue] = 96,
        [PaletteFxColours.KraidStatue] = 112,
        [PaletteFxColours.WreckedShipGreenLights] = 76,
        [PaletteFxColours.LandingSiteLightning] = 84,
        [PaletteFxColours.UnusedDarkLightning] = 65,
        [PaletteFxColours.BrinstarBlueSpores] = 113,
        [PaletteFxColours.BrinstarRedGlow] = 100,
        [PaletteFxColours.Beacon] = 113,
        [PaletteFxColours.NorfairHeatForeground] = 53,
        [PaletteFxColours.NorfairForegroundPalette4] = 65,
        [PaletteFxColours.NorfairForegroundPalette5] = 81,
        [PaletteFxColours.NorfairForegroundPalette6] = 97,
        [PaletteFxColours.MaridiaSandPits] = 36,
        [PaletteFxColours.MaridiaSandFalls] = 40,
        [PaletteFxColours.MaridiaBackgroundWaterfalls] = 52,
        [PaletteFxColours.TourianArkanoidBlocksAndOrbs] = 116,
        [PaletteFxColours.TourianBackground] = 56,
        [PaletteFxColours.TourianLevel] = 84,
        [PaletteFxColours.OldTourianLevel] = 81,
        [PaletteFxColours.OldTourianRailings] = 105,
        [PaletteFxColours.OldTourianPanels] = 85,
        [PaletteFxColours.UpperCrateriaLevel] = 65,
        [PaletteFxColours.CrateriaEscapeLightning] = 81,
        [PaletteFxColours.CreBlockPixel] = 87,
    }.ToImmutableDictionary();

    /// <summary>The objects Tourian's statue room spawns as each boss's statue greys out, which no FX record names.</summary>
    private static readonly ImmutableArray<PaletteFxObject> StatueGreyOuts =
    [
        PaletteFxObject.GreyOutTourianStatueDraygon, PaletteFxObject.GreyOutTourianStatueKraid,
        PaletteFxObject.GreyOutTourianStatueRidley, PaletteFxObject.GreyOutTourianStatuePhantoon,
    ];

    /// <summary>
    /// The shades of every record of the room's colour groups that <paramref name="rooms"/>' palette effects show, coloured
    /// from <paramref name="colours"/>.
    /// </summary>
    /// <param name="rooms">The rooms whose pictures are shaded, with the state each loads.</param>
    /// <param name="colours">The records' installed colours.</param>
    /// <exception cref="InvalidDataException">A record has no installed colour, or writes past its group's colours.</exception>
    internal static IReadOnlyList<SheetShade> Of(IEnumerable<(RoomHeaderDefinition Room, RoomStateSettings State)> rooms,
        IPaletteFxColorSource colours)
    {
        Ensure.NotNull(rooms);
        Ensure.NotNull(colours);
        var objects = new HashSet<PaletteFxObject>();
        foreach ((RoomHeaderDefinition room, RoomStateSettings state) in rooms)
        {
            int area = AreaIds.ToIndex(room.AreaIndex);
            foreach (RoomFxRecordDefinition record in state.Fx?.Records ?? [])
            {
                for (int bit = 0; bit < RoomFxEffectSlotSet.Count; bit++)
                {
                    if (record.PaletteFxSlots.Includes(bit))
                        objects.Add(RoomPaletteFxSelection.GetAreaDefinition(area, bit));
                }
            }
            if (room.AreaIndex == AreaId.Tourian)
                objects.UnionWith(StatueGreyOuts);
        }
        var shown = new SortedSet<(PaletteFxColours Colours, int Record)>();
        foreach (PaletteFxObject shownObject in objects)
            Collect(RoomPaletteFxSelection.Get(shownObject).InitialProgram, shown);
        return [.. shown.Where(entry => FirstInks.ContainsKey(entry.Colours))
            .Select(entry => Shade(entry.Colours, new PaletteFxFrame(entry.Record), colours))];
    }

    /// <summary>The shade of <paramref name="group"/> showing <paramref name="record"/>, coloured from <paramref name="colours"/>.</summary>
    /// <exception cref="InvalidDataException">A colour is not installed, or the record writes past its group's colours.</exception>
    internal static SheetShade Shade(PaletteFxColours group, PaletteFxFrame record, IPaletteFxColorSource colours)
    {
        int ink = FirstInks.TryGetValue(group, out int first) ? first
            : throw new InvalidDataException($"Palette-FX colours {group} recolour no room picture.");
        int end = (first / GroupInks + 1) * GroupInks;
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
    /// of each program it jumps, falls or repeats into, or switches to above the Crateria lightning's height, each under
    /// the colours its last cue named.
    /// </summary>
    private static void Collect(PaletteFxProgram start, ISet<(PaletteFxColours Colours, int Record)> shown)
    {
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
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.CueStep { Cue: PaletteFxCue.SetFrameBehavior behavior }:
                        if (SwitchedTo(behavior.FrameBehavior) is { } switched)
                            pending.Push((switched, colours));
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

    /// <summary>The program a frame behaviour switches its object to, or null for one that switches to none.</summary>
    private static PaletteFxProgram? SwitchedTo(PaletteFxFrameBehavior behavior) => behavior switch
    {
        PaletteFxFrameBehavior.SwitchAboveY380 => PaletteFxProgram.Crateria11,
        PaletteFxFrameBehavior.SwitchAboveY380Second => PaletteFxProgram.CrateriaAboveY380Second,
        _ => null,
    };
}

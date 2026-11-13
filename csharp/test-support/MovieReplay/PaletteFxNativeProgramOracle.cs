using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The ROM oracle of the palette-FX objects' typed animations (#1298, #1600): each segment's beats matched to its native
/// bank-$8D list, each animation matched to the lists' control flow, and the animation place a recording's native
/// instruction pointer stands for. The one native instruction the animations deliberately do not reproduce is the
/// set-pre-instruction of the deleted adjacent-slot inspection (#1526), a presentation quirk: the comparison steps over
/// exactly that instruction, wherever a list runs or branches into it, and fails on any other native instruction the typed
/// beats do not match.
/// </summary>
internal static class PaletteFxNativeProgramOracle
{
    /// <summary>
    /// Reads every segment's native list, comparing its beats with the segment's: each beat's native address, and each
    /// control instruction with the beat it follows.
    /// </summary>
    internal static Dictionary<PaletteFxSegment, PaletteFxNativeList> Lists(IImportCartridgeSource cartridge)
    {
        Dictionary<ushort, PaletteFxSegment> byEntry = PaletteFxNativeListAddresses.Entries.ToDictionary(entry => entry.Entry, entry => entry.Program);
        return PaletteFxNativeListAddresses.Entries.ToDictionary(entry => entry.Program, entry => Read(cartridge, byEntry, entry.Program, entry.Entry));
    }

    /// <summary>
    /// Checks that every typed animation plays the phases the native lists' control flow plays from its first segment:
    /// counters set before a list, counted repeats, runs on into the next list, gotos forward and back, and deletes.
    /// </summary>
    internal static void VerifyAnimations(IReadOnlyDictionary<PaletteFxSegment, PaletteFxNativeList> lists)
    {
        Ensure.NotNull(lists);
        HashSet<PaletteFxSegment> bodies = [.. lists.Values.SelectMany(list => list.Controls)
            .Where(control => control.Kind == PaletteFxNativeControlKind.Repeat).Select(control => control.Target!.Value)];
        foreach (PaletteFxSegment start in Enum.GetValues<PaletteFxSegment>())
        {
            if (bodies.Contains(start))
            {
                VerificationAssertions.AssertTrue(!PaletteFxAnimations.StartingAt.ContainsKey(start), $"no animation starts in counted body {start}");
                continue;
            }
            (List<AnimationPhase<PaletteFxSegment>> phases, AnimationEnd end) = Linearize(lists, start);
            Animation<PaletteFxSegment> typed = PaletteFxAnimations.StartingWith(start);
            VerificationAssertions.AssertTrue(typed.Phases.SequenceEqual(phases), $"{start}'s phases follow its native lists");
            VerificationAssertions.AssertEqual(end, typed.End, $"{start}'s end follows its native lists");
        }
    }

    /// <summary>
    /// The place a live object resumes at in one of <paramref name="candidates"/> (the animations it can be playing, its
    /// own first): the phase beat at <paramref name="nativePointer"/>, or the end of a phase's play where the pointer rests on
    /// the control instruction that follows it.
    /// </summary>
    /// <exception cref="InvalidDataException">No candidate animation plays the beat or control at the pointer.</exception>
    internal static PaletteFxImportedPlace PlaceAt(
        IReadOnlyDictionary<PaletteFxSegment, PaletteFxNativeList> lists, IEnumerable<PaletteFxSegment> candidates, ushort nativePointer)
    {
        Ensure.NotNull(lists);
        foreach (PaletteFxSegment candidate in Ensure.NotNull(candidates))
        {
            Animation<PaletteFxSegment> animation = PaletteFxAnimations.StartingWith(candidate);
            for (int index = 0; index < animation.Phases.Count; index++)
            {
                AnimationPhase<PaletteFxSegment> phase = animation.Phases[index];
                PaletteFxNativeList list = lists[phase.Segment];
                // A pointer at a list's entry stands at its first beat, before any deleted inspection the list opens with.
                if (phase.FirstBeat == 0 && PaletteFxNativeListAddresses.Entry(phase.Segment) == nativePointer)
                    return new(candidate, new AnimationPlace(index, 0));
                for (int beat = 0; beat < phase.BeatCount; beat++)
                {
                    if (list.BeatAddresses[phase.FirstBeat + beat] == nativePointer)
                        return new(candidate, new AnimationPlace(index, beat));
                }
                if (list.Controls.FirstOrDefault(control => control.AfterBeat == phase.FirstBeat + phase.BeatCount) is { } after &&
                    after.Address == nativePointer && after.Kind != PaletteFxNativeControlKind.RunOnIntoNext)
                    return new(candidate, new AnimationPlace(index, phase.BeatCount));
            }
        }
        throw new InvalidDataException($"Native palette-FX pointer $8D:{nativePointer:X4} stands at no beat of the animations {string.Join(", ", candidates)} play.");
    }

    /// <summary>The phases and end the native lists play from <paramref name="start"/>, by the typed animations' rules.</summary>
    private static (List<AnimationPhase<PaletteFxSegment>> Phases, AnimationEnd End) Linearize(
        IReadOnlyDictionary<PaletteFxSegment, PaletteFxNativeList> lists, PaletteFxSegment start)
    {
        var phases = new List<AnimationPhase<PaletteFxSegment>>();
        bool counted = false;
        var entered = new Dictionary<PaletteFxSegment, int> { [start] = 0 };
        PaletteFxSegment segment = start;
        int runStart = 0, plays = 1;
        int? pending = null;
        for (int guard = 0; guard < 100000; guard++)
        {
            foreach (PaletteFxNativeControl control in lists[segment].Controls)
            {
                if (control.AfterBeat > runStart)
                    phases.Add(new(segment, runStart, control.AfterBeat - runStart, plays,
                        counted && plays == 1 ? AnimationPhaseCounting.EveryPlay : AnimationPhaseCounting.RepeatsOnly));
                runStart = control.AfterBeat;
                plays = 1;
                counted = false;
                switch (control.Kind)
                {
                    case PaletteFxNativeControlKind.SetCounter:
                        pending = control.Count;
                        continue;
                    case PaletteFxNativeControlKind.Repeat:
                        continue;
                    case PaletteFxNativeControlKind.Delete:
                        return (phases, AnimationEnd.Stops);
                    case PaletteFxNativeControlKind.Goto:
                    case PaletteFxNativeControlKind.RunOnIntoNext:
                        PaletteFxSegment target = control.Target!.Value;
                        if (entered.TryGetValue(target, out int loop))
                            return (phases, AnimationEnd.LoopsFrom(loop));
                        entered[target] = phases.Count;
                        segment = target;
                        runStart = 0;
                        plays = pending ?? 1;
                        counted = pending is not null;
                        pending = null;
                        break;
                    default:
                        throw new InvalidDataException($"Undefined native control {control.Kind}.");
                }
                break;
            }
        }
        throw new InvalidDataException($"The native lists from {start} never end or loop.");
    }

    /// <summary>Reads one segment's native list, comparing its beats with the segment's.</summary>
    private static PaletteFxNativeList Read(IImportCartridgeSource cartridge, Dictionary<ushort, PaletteFxSegment> byEntry, PaletteFxSegment program, ushort entry)
    {
        IReadOnlyList<AnimationBeat<int, PaletteFxCue>> beats = PaletteFxSegments.All.Beats(program);
        var addresses = new List<ushort>();
        var controls = new List<PaletteFxNativeControl>();
        ushort cursor = entry;
        while (true)
        {
            cursor = PastDeletedInspection(cursor);
            int beat = addresses.Count;
            string at = $"palette-FX {program} beat {beat} at $8D:{cursor:X4}";
            if (cursor != entry && byEntry.TryGetValue(cursor, out PaletteFxSegment next))
            {
                controls.Add(new(PaletteFxNativeControlKind.RunOnIntoNext, beat, cursor, next, null));
                return Finish();
            }
            ushort word = Word(cursor);
            if (word < PaletteFxListWords.CommandWord)
            {
                var frame = VerificationAssertions.AssertIs<AnimationBeat<int, PaletteFxCue>.Frame>(Beat(beat, at), $"{at}: timed record");
                VerificationAssertions.AssertEqual(word, frame.Duration, $"{at}: duration");
                // The frame's record, numbered by the segments' order of frames, lies where the native list shows it (#1598).
                PaletteFxRecord record = PaletteFxRecord.Of(new PaletteFxFrame(program, frame.Shown));
                VerificationAssertions.AssertEqual(cursor, PaletteFxColorLayoutAccess.RecordAddress(record), $"{at}: record address");
                addresses.Add(cursor);
                cursor = CompareLayout(cursor, PaletteFxRecordDefinitions.Layout(record), at);
                continue;
            }
            PaletteFxInstruction instruction = (PaletteFxInstruction)word;
            ushort operandWord = Word(cursor + 2);
            byte operandByte = LittleEndianBytes.LowByte(operandWord);
            switch (instruction)
            {
                case PaletteFxInstruction.Goto:
                    controls.Add(new(PaletteFxNativeControlKind.Goto, beat, cursor, byEntry[PastDeletedInspection(operandWord)], null));
                    return Finish();
                case PaletteFxInstruction.Delete:
                    controls.Add(new(PaletteFxNativeControlKind.Delete, beat, cursor, null, null));
                    return Finish();
                case PaletteFxInstruction.DecrementTimerAndGoto:
                    PaletteFxSegment repeated = byEntry[PastDeletedInspection(operandWord)];
                    VerificationAssertions.AssertEqual(program, repeated, $"{at}: a counted repeat replays its own list");
                    controls.Add(new(PaletteFxNativeControlKind.Repeat, beat, cursor, repeated, null));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case PaletteFxInstruction.SetTimer:
                    // The native command writes only the timer's low byte; its high byte is zero from spawn and every
                    // counted loop runs it down to zero, so the typed play count is the same.
                    controls.Add(new(PaletteFxNativeControlKind.SetCounter, beat, cursor, null, operandByte));
                    cursor = unchecked((ushort)(cursor + 3));
                    continue;
                case PaletteFxInstruction.SetColorIndex:
                    // The native operand is a CGRAM byte index; the typed event names the colour group, which must be
                    // placed where the operand starts.
                    VerificationAssertions.AssertTrue(operandWord % sizeof(ushort) == 0, $"{at}: colour byte index is word-aligned");
                    var setColours = VerificationAssertions.AssertIs<PaletteFxCue.SetColours>(Event(beat, at), $"{at}: colours event");
                    VerificationAssertions.AssertEqual(operandWord / sizeof(ushort), setColours.Target.CartridgeColour,
                        $"{at}: {setColours.Target} starts at the native colour");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case PaletteFxInstruction.SetFrameBehavior:
                    AssertEvent(beat, new PaletteFxCue.Watch(FxNativeWords.ConditionOf(FxNativeWords.PaletteFxPreInstructions.Decode(operandWord, "palette-FX pre-instruction"))
                        ?? throw new InvalidDataException($"{at}: the native list installs a pre-instruction that does nothing, which no typed animation watches.")), at);
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case PaletteFxInstruction.ClearFrameBehavior:
                    throw new InvalidDataException($"{at}: the native list clears its pre-instruction, which no colour effect's animation does.");
                case PaletteFxInstruction.SetPaletteFxIndex:
                    AssertEvent(beat, new PaletteFxCue.PublishHeatPhase(new HeatPhase(operandByte)), at);
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 3));
                    continue;
                case PaletteFxInstruction.QueueSfx2:
                    AssertEvent(beat, new PaletteFxCue.QueueSound(Sound: SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, operandByte)), at);
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 3));
                    continue;
                case PaletteFxInstruction.Wait:
                case PaletteFxInstruction.ColorPlus2:
                case PaletteFxInstruction.ColorPlus3:
                case PaletteFxInstruction.ColorPlus4:
                case PaletteFxInstruction.ColorPlus8:
                case PaletteFxInstruction.ColorPlus9:
                case PaletteFxInstruction.ColorPlus15:
                case PaletteFxInstruction.QueueMusic:
                case PaletteFxInstruction.QueueSfx1:
                case PaletteFxInstruction.QueueSfx3:
                default:
                    throw new InvalidDataException($"{at}: native command {instruction} has no typed form.");
            }
        }

        PaletteFxNativeList Finish()
        {
            VerificationAssertions.AssertEqual(beats.Count, addresses.Count, $"palette-FX {program}: its native list holds each of its beats");
            return new PaletteFxNativeList([.. addresses], [.. controls]);
        }

        AnimationBeat<int, PaletteFxCue> Beat(int beat, string at) => beat < beats.Count
            ? beats[beat]
            : throw new InvalidDataException($"{at}: the native list holds more beats than the segment.");

        PaletteFxCue Event(int beat, string at) =>
            VerificationAssertions.AssertIs<AnimationBeat<int, PaletteFxCue>.Event>(Beat(beat, at), $"{at}: event").Raised;

        void AssertEvent(int beat, PaletteFxCue expected, string at) =>
            VerificationAssertions.AssertEqual(expected, Event(beat, at), $"{at}: event");

        ushort Word(int address) => cartridge.ReadCartridgeWord(PaletteFxListWords.ListBank | address);

        // The deleted adjacent-slot inspection (#1526) is a presentation quirk the port does not reproduce: where the native
        // list sets that one pre-instruction, the typed animation runs straight on to what follows it.
        ushort PastDeletedInspection(ushort address) =>
            Word(address) == (ushort)PaletteFxInstruction.SetFrameBehavior &&
            Word(address + 2) == FxNativeWords.DeletedSlotInspectionPreInstruction
                ? unchecked((ushort)(address + 4))
                : address;

        // A record is its timer, its colour words and skip commands, then the closing wait.
        ushort CompareLayout(ushort record, IReadOnlyList<PaletteFxRecordPart> layout, string at)
        {
            ushort word = unchecked((ushort)(record + 2));
            foreach (PaletteFxRecordPart part in layout)
            {
                if (part.Skip)
                {
                    ushort skip = Word(word);
                    int skipped = (PaletteFxInstruction)skip switch
                    {
                        PaletteFxInstruction.ColorPlus2 => 2,
                        PaletteFxInstruction.ColorPlus3 => 3,
                        PaletteFxInstruction.ColorPlus4 => 4,
                        PaletteFxInstruction.ColorPlus8 => 8,
                        PaletteFxInstruction.ColorPlus9 => 9,
                        PaletteFxInstruction.ColorPlus15 => 15,
                        PaletteFxInstruction.Wait or
                            PaletteFxInstruction.Delete or
                            PaletteFxInstruction.SetFrameBehavior or
                            PaletteFxInstruction.ClearFrameBehavior or
                            PaletteFxInstruction.Goto or
                            PaletteFxInstruction.DecrementTimerAndGoto or
                            PaletteFxInstruction.SetTimer or
                            PaletteFxInstruction.SetColorIndex or
                            PaletteFxInstruction.QueueMusic or
                            PaletteFxInstruction.QueueSfx1 or
                            PaletteFxInstruction.QueueSfx2 or
                            PaletteFxInstruction.QueueSfx3 or
                            PaletteFxInstruction.SetPaletteFxIndex => throw new InvalidDataException($"{at}: $8D:{word:X4} ${skip:X4} is not a colour skip."),
                        _ => throw new InvalidDataException($"{at}: $8D:{word:X4} ${skip:X4} is not a colour skip."),
                    };
                    VerificationAssertions.AssertEqual(skipped, part.Count, $"{at}: skip at $8D:{word:X4}");
                    word = unchecked((ushort)(word + 2));
                    continue;
                }
                for (int color = 0; color < part.Count; color++)
                {
                    VerificationAssertions.AssertTrue(Word(word) < PaletteFxListWords.CommandWord, $"{at}: colour word at $8D:{word:X4}");
                    word = unchecked((ushort)(word + 2));
                }
            }
            VerificationAssertions.AssertEqual((ushort)PaletteFxInstruction.Wait, Word(word), $"{at}: the record closes with a wait");
            return unchecked((ushort)(word + 2));
        }
    }
}

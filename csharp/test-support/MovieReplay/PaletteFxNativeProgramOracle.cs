using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The ROM oracle of the colour effects' typed animations (#1298, #1600, #1739): each native bank-$8D list matched to the
/// run of steps it is in each animation that plays it, each animation matched to the lists' control flow, and the animation
/// place a recording's native instruction pointer stands for. The one native instruction the animations deliberately do not reproduce is the
/// set-pre-instruction of the deleted adjacent-slot inspection (#1526), a presentation quirk: the comparison steps over
/// exactly that instruction, wherever a list runs or branches into it, and fails on any other native instruction the typed
/// beats do not match.
/// </summary>
internal static class PaletteFxNativeProgramOracle
{
    /// <summary>
    /// Reads every native list once, by its entry, comparing its beats with the run of steps it is in each animation that
    /// plays it: each beat's native address, and each control instruction with the beat it follows.
    /// </summary>
    internal static Dictionary<ushort, PaletteFxNativeList> Lists(IImportCartridgeSource cartridge)
    {
        HashSet<ushort> entries = [.. PaletteFxNativeListAddresses.Entries.Select(entry => entry.Entry)];
        return PaletteFxNativeListAddresses.Entries.GroupBy(entry => entry.Entry)
            .ToDictionary(list => list.Key, list => Read(cartridge, entries, [.. list]));
    }

    /// <summary>
    /// Checks that every typed animation plays the phases the native lists' control flow plays from its first list: counters
    /// set before a list, counted repeats, runs on into the next list, gotos forward and back, and deletes.
    /// </summary>
    internal static void VerifyAnimations(IReadOnlyDictionary<ushort, PaletteFxNativeList> lists)
    {
        Ensure.NotNull(lists);
        foreach (ColourEffectAnimation animation in Enum.GetValues<ColourEffectAnimation>())
        {
            (List<AnimationPhase<ColourEffectAnimation>> phases, AnimationEnd end) = Linearize(lists, animation);
            Animation<ColourEffectAnimation> typed = ColourEffectAnimations.Of(animation);
            VerificationAssertions.AssertTrue(typed.Phases.SequenceEqual(phases), $"{animation}'s phases follow its native lists");
            VerificationAssertions.AssertEqual(end, typed.End, $"{animation}'s end follows its native lists");
        }
    }

    /// <summary>The native list <paramref name="animation"/>'s step <paramref name="step"/> lies in, and where the list starts in it.</summary>
    /// <exception cref="InvalidOperationException">No native list holds the step.</exception>
    private static PaletteFxNativeListAddressesEntry ListHolding(ColourEffectAnimation animation, int step)
    {
        PaletteFxNativeListAddressesEntry[] starting = [.. PaletteFxNativeListAddresses.Entries
            .Where(entry => entry.Animation == animation && entry.FirstStep <= step)];
        return starting.Length > 0
            ? starting.MaxBy(entry => entry.FirstStep)
            : throw new InvalidOperationException($"No native list holds step {step} of {animation}.");
    }

    /// <summary>Where the native list at <paramref name="entry"/> starts in <paramref name="animation"/>.</summary>
    /// <exception cref="InvalidOperationException">The animation does not play the list.</exception>
    private static int FirstStep(ColourEffectAnimation animation, ushort entry)
    {
        PaletteFxNativeListAddressesEntry[] places = [.. PaletteFxNativeListAddresses.Entries
            .Where(place => place.Animation == animation && place.Entry == entry)];
        return places.Length == 1
            ? places[0].FirstStep
            : throw new InvalidOperationException($"{animation} plays {places.Length} native lists at $8D:{entry:X4}, not one.");
    }

    /// <summary>The steps of <paramref name="place"/>'s animation its native list holds: to the next list's start, or to its last step.</summary>
    private static int StepCount(PaletteFxNativeListAddressesEntry place)
    {
        int next = PaletteFxNativeListAddresses.Entries.Where(entry => entry.Animation == place.Animation && entry.FirstStep > place.FirstStep)
            .Select(entry => entry.FirstStep).DefaultIfEmpty(ColourEffectAnimations.Steps.Beats(place.Animation).Count).Min();
        return next - place.FirstStep;
    }

    /// <summary>
    /// The place a live object resumes at in one of <paramref name="candidates"/> (the animations it can be playing, its
    /// own first): the phase beat at <paramref name="nativePointer"/>, or the end of a phase's play where the pointer rests on
    /// the control instruction that follows it.
    /// </summary>
    /// <exception cref="InvalidDataException">No candidate animation plays the beat or control at the pointer.</exception>
    internal static PaletteFxImportedPlace PlaceAt(
        IReadOnlyDictionary<ushort, PaletteFxNativeList> lists, IEnumerable<ColourEffectAnimation> candidates, ushort nativePointer)
    {
        Ensure.NotNull(lists);
        foreach (ColourEffectAnimation candidate in Ensure.NotNull(candidates))
        {
            Animation<ColourEffectAnimation> animation = ColourEffectAnimations.Of(candidate);
            for (int index = 0; index < animation.Phases.Count; index++)
            {
                AnimationPhase<ColourEffectAnimation> phase = animation.Phases[index];
                PaletteFxNativeListAddressesEntry holding = ListHolding(candidate, phase.FirstBeat);
                PaletteFxNativeList list = lists[holding.Entry];
                int first = phase.FirstBeat - holding.FirstStep;
                // A pointer at a list's entry stands at its first beat, before any deleted inspection the list opens with.
                if (first == 0 && holding.Entry == nativePointer)
                    return new(candidate, new AnimationPlace(index, 0));
                for (int beat = 0; beat < phase.BeatCount; beat++)
                {
                    if (list.BeatAddresses[first + beat] == nativePointer)
                        return new(candidate, new AnimationPlace(index, beat));
                }
                if (list.Controls.FirstOrDefault(control => control.AfterBeat == first + phase.BeatCount) is { } after &&
                    after.Address == nativePointer && after.Kind != PaletteFxNativeControlKind.RunOnIntoNext)
                    return new(candidate, new AnimationPlace(index, phase.BeatCount));
            }
        }
        throw new InvalidDataException($"Native palette-FX pointer $8D:{nativePointer:X4} stands at no beat of the animations {string.Join(", ", candidates)} play.");
    }

    /// <summary>
    /// The phases and end the native lists play from <paramref name="animation"/>'s first list, by the typed animations' rules,
    /// each phase a run of the steps the list it plays is in the animation.
    /// </summary>
    private static (List<AnimationPhase<ColourEffectAnimation>> Phases, AnimationEnd End) Linearize(
        IReadOnlyDictionary<ushort, PaletteFxNativeList> lists, ColourEffectAnimation animation)
    {
        var phases = new List<AnimationPhase<ColourEffectAnimation>>();
        bool counted = false;
        ushort start = PaletteFxNativeListAddresses.Entry(animation);
        var entered = new Dictionary<ushort, int> { [start] = 0 };
        ushort segment = start;
        int runStart = 0, plays = 1;
        int? pending = null;
        for (int guard = 0; guard < 100000; guard++)
        {
            foreach (PaletteFxNativeControl control in lists[segment].Controls)
            {
                if (control.AfterBeat > runStart)
                    phases.Add(new(animation, FirstStep(animation, segment) + runStart, control.AfterBeat - runStart, plays,
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
                        ushort target = control.Target!.Value;
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
        throw new InvalidDataException($"The native lists from {animation} never end or loop.");
    }

    /// <summary>
    /// Reads one native list, comparing its beats with the run of steps it is in each of <paramref name="places"/>, the
    /// animations that play it, whose runs must be alike.
    /// </summary>
    private static PaletteFxNativeList Read(IImportCartridgeSource cartridge, HashSet<ushort> entries, PaletteFxNativeListAddressesEntry[] places)
    {
        PaletteFxNativeListAddressesEntry program = places[0];
        ushort entry = program.Entry;
        AnimationBeat<int, ColourEffectCue>[] beats = [.. ColourEffectAnimations.Steps.Beats(program.Animation).Skip(program.FirstStep).Take(StepCount(program))];
        foreach (PaletteFxNativeListAddressesEntry place in places)
        {
            VerificationAssertions.AssertTrue(beats.SequenceEqual(ColourEffectAnimations.Steps.Beats(place.Animation).Skip(place.FirstStep).Take(StepCount(place))),
                $"the native list at $8D:{entry:X4} is the same steps in {place.Animation} as in {program.Animation}");
        }
        var addresses = new List<ushort>();
        var controls = new List<PaletteFxNativeControl>();
        ushort cursor = entry;
        while (true)
        {
            cursor = PastDeletedInspection(cursor);
            int beat = addresses.Count;
            string at = $"colour effect {program.Animation} step {program.FirstStep + beat} at $8D:{cursor:X4}";
            if (cursor != entry && entries.Contains(cursor))
            {
                controls.Add(new(PaletteFxNativeControlKind.RunOnIntoNext, beat, cursor, cursor, null));
                return Finish();
            }
            ushort word = Word(cursor);
            if (word < PaletteFxListWords.CommandWord)
            {
                var frame = VerificationAssertions.AssertIs<AnimationBeat<int, ColourEffectCue>.Frame>(Beat(beat, at), $"{at}: timed record");
                VerificationAssertions.AssertEqual(word, frame.Duration, $"{at}: duration");
                // The frame's record, numbered by the cartridge's runs of records, lies where the native list shows it, in every
                // animation that plays the list (#1598, #1739).
                PaletteFxRecord record = PaletteFxRecord.Of(new ColourEffectFrame(program.Animation, frame.Shown));
                foreach (PaletteFxNativeListAddressesEntry place in places)
                {
                    VerificationAssertions.AssertEqual(cursor, PaletteFxColorLayoutAccess.RecordAddress(PaletteFxRecord.Of(new ColourEffectFrame(place.Animation, frame.Shown))),
                        $"{at}: record address in {place.Animation}");
                }
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
                    controls.Add(new(PaletteFxNativeControlKind.Goto, beat, cursor, ListAt(PastDeletedInspection(operandWord), at), null));
                    return Finish();
                case PaletteFxInstruction.Delete:
                    controls.Add(new(PaletteFxNativeControlKind.Delete, beat, cursor, null, null));
                    return Finish();
                case PaletteFxInstruction.DecrementTimerAndGoto:
                    ushort repeated = ListAt(PastDeletedInspection(operandWord), at);
                    VerificationAssertions.AssertEqual(entry, repeated, $"{at}: a counted repeat replays its own list");
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
                    var setColours = VerificationAssertions.AssertIs<ColourEffectCue.SetColours>(Event(beat, at), $"{at}: colours event");
                    VerificationAssertions.AssertEqual(operandWord / sizeof(ushort), setColours.Target.CartridgeColour,
                        $"{at}: {setColours.Target} starts at the native colour");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case PaletteFxInstruction.SetFrameBehavior:
                    AssertEvent(beat, new ColourEffectCue.Watch(FxNativeWords.ConditionOf(FxNativeWords.PaletteFxPreInstructions.Decode(operandWord, "palette-FX pre-instruction"))
                        ?? throw new InvalidDataException($"{at}: the native list installs a pre-instruction that does nothing, which no typed animation watches.")), at);
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case PaletteFxInstruction.ClearFrameBehavior:
                    throw new InvalidDataException($"{at}: the native list clears its pre-instruction, which no colour effect's animation does.");
                case PaletteFxInstruction.SetPaletteFxIndex:
                    AssertEvent(beat, new ColourEffectCue.PublishHeatPhase(new HeatPhase(operandByte)), at);
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 3));
                    continue;
                case PaletteFxInstruction.QueueSfx2:
                    AssertEvent(beat, new ColourEffectCue.QueueSound(Sound: SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, operandByte)), at);
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
            VerificationAssertions.AssertEqual(beats.Length, addresses.Count, $"the native list at $8D:{entry:X4}: it holds each of its steps in {program.Animation}");
            return new PaletteFxNativeList([.. addresses], [.. controls]);
        }

        AnimationBeat<int, ColourEffectCue> Beat(int beat, string at) => beat < beats.Length
            ? beats[beat]
            : throw new InvalidDataException($"{at}: the native list holds more beats than its run of steps.");

        ushort ListAt(ushort target, string at) => entries.Contains(target)
            ? target
            : throw new InvalidDataException($"{at}: the native list continues at $8D:{target:X4}, which starts no list.");

        ColourEffectCue Event(int beat, string at) =>
            VerificationAssertions.AssertIs<AnimationBeat<int, ColourEffectCue>.Event>(Beat(beat, at), $"{at}: event").Raised;

        void AssertEvent(int beat, ColourEffectCue expected, string at) =>
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

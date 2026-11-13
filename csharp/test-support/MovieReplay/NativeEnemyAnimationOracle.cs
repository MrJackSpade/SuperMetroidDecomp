using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The ROM oracle of an enemy family's typed animations (#1600): each segment's beats matched to its native list, and
/// each animation matched to the lists' control flow (sleeps, stops, gotos, counter loads, counted repeats and runs on
/// into the next authored list), under the family's rules for its own instructions.
/// </summary>
internal static class NativeEnemyAnimationOracle
{
    /// <summary>The longest duration a native frame instruction gives, which a list follows with its sleep to hold a frame.</summary>
    private const ushort NativeHeldFrameDuration = 0x7fff;

    /// <summary>The native entry of <paramref name="segment"/>.</summary>
    internal static ushort EntryOf<TSegment>(NativeProgramEntry<TSegment>[] entries, TSegment segment)
        where TSegment : struct, Enum =>
        entries.Single(item => EqualityComparer<TSegment>.Default.Equals(item.Program, segment)).Entry;

    /// <summary>
    /// Compares every segment with its native list and every animation with the lists' control flow, and returns the
    /// total number of beats.
    /// </summary>
    /// <param name="cartridge">The pinned cartridge.</param>
    /// <param name="bank">The lists' bank.</param>
    /// <param name="entries">Each segment's native entry, in authored order.</param>
    /// <param name="segments">The typed segments.</param>
    /// <param name="startingAt">The typed animations, by the segment each starts at.</param>
    /// <param name="spritemap">The spritemap a frame shows.</param>
    /// <param name="rules">How the family's instructions are read and compared.</param>
    /// <param name="countDownContinuations">
    /// Where an owner's branch into each count-down tail continues the play (<see cref="CompareCountDownContinuations"/>);
    /// required when the family's lists have one.
    /// </param>
    internal static int CompareAll<TSegment, TFrame, TEvent>(
        IImportCartridgeSource cartridge,
        byte bank,
        NativeProgramEntry<TSegment>[] entries,
        AnimationSegments<TSegment, TFrame, TEvent> segments,
        IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
        Func<TFrame, ushort> spritemap,
        NativeEnemyCueRules<TEvent> rules,
        IReadOnlyDictionary<TSegment, AnimationContinuation<TSegment>>? countDownContinuations = null)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        Dictionary<TSegment, NativeEnemyList<TSegment>> lists = Lists(cartridge, bank, entries, segments, spritemap, rules);
        CompareAnimations(lists, startingAt);
        CompareCountDownContinuations(lists, startingAt, countDownContinuations ?? new Dictionary<TSegment, AnimationContinuation<TSegment>>());
        return lists.Values.Sum(list => list.BeatAddresses.Length);
    }

    /// <summary>
    /// Reads every segment's native list, comparing its beats with the segment's as <see cref="CompareAll"/> does: each
    /// beat's native address, and each control instruction with its address and the beat it follows.
    /// </summary>
    internal static Dictionary<TSegment, NativeEnemyList<TSegment>> Lists<TSegment, TFrame, TEvent>(
        IImportCartridgeSource cartridge,
        byte bank,
        NativeProgramEntry<TSegment>[] entries,
        AnimationSegments<TSegment, TFrame, TEvent> segments,
        Func<TFrame, ushort> spritemap,
        NativeEnemyCueRules<TEvent> rules)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        Ensure.NotNull(entries);
        Ensure.NotNull(segments);
        Ensure.NotNull(rules);
        RequireAuthoredOrder(entries);
        return entries.ToDictionary(entry => entry.Program,
            entry => Read(cartridge, bank, entries, segments, spritemap, rules, entry.Program, entry.Entry));
    }

    /// <summary>Requires one native entry per segment, in the segments' declared (authored) order.</summary>
    internal static void RequireAuthoredOrder<TSegment>(NativeProgramEntry<TSegment>[] entries)
        where TSegment : struct, Enum
    {
        Ensure.NotNull(entries);
        VerificationAssertions.AssertEqual(Enum.GetValues<TSegment>().Length, entries.Length, $"every typed {typeof(TSegment).Name} has a native entry");
        for (int index = 0; index < entries.Length; index++)
            VerificationAssertions.AssertEqual(Enum.GetValues<TSegment>()[index], entries[index].Program, $"the {typeof(TSegment).Name} values are declared in authored order");
    }

    /// <summary>
    /// Checks that every typed animation plays the phases the native lists' control flow plays from its first segment,
    /// and that no animation starts in a counted body other than those of <paramref name="startsWithClearedCounter"/>:
    /// counted lists an owner starts directly, with the counter its spawn cleared. The first play of such a list counts
    /// that zero down, wrapping it, so the list plays once and then the longest count more times.
    /// </summary>
    internal static void CompareAnimations<TSegment>(
        IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists, IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
        IReadOnlySet<TSegment>? startsWithClearedCounter = null)
        where TSegment : struct, Enum
    {
        Ensure.NotNull(lists);
        Ensure.NotNull(startingAt);
        HashSet<TSegment> bodies = [.. lists.Values.SelectMany(list => list.Controls)
            .Where(control => control.Kind == NativeEnemyControlKind.Repeat).Select(control => control.Target!.Value)];
        HashSet<TSegment> tails = CountDownTails(lists);
        foreach (TSegment start in Enum.GetValues<TSegment>())
        {
            if (tails.Contains(start))
            {
                VerificationAssertions.AssertTrue(!startingAt.ContainsKey(start), $"no animation starts in count-down {start}");
                continue;
            }
            if (startsWithClearedCounter?.Contains(start) == true)
            {
                VerificationAssertions.AssertTrue(bodies.Contains(start), $"{start}, started with a cleared counter, is a counted body");
                (List<AnimationPhase<TSegment>> counted, AnimationEnd countedEnd) = Linearize(lists, start, WrappingCountdown.LongestStart);
                List<AnimationPhase<TSegment>> wrapped = [counted[0] with { Plays = 1 }, .. counted];
                AnimationEnd wrappedEnd = countedEnd is AnimationEnd.Loop loop ? AnimationEnd.LoopsFrom(loop.Phase + 1) : countedEnd;
                VerificationAssertions.AssertTrue(startingAt.TryGetValue(start, out Animation<TSegment>? started), $"an animation starts at {start}");
                VerificationAssertions.AssertTrue(started!.Phases.SequenceEqual(wrapped), $"{start}'s phases follow its native lists from a cleared counter");
                VerificationAssertions.AssertEqual(wrappedEnd, started.End, $"{start}'s end follows its native lists");
                continue;
            }
            if (bodies.Contains(start))
            {
                VerificationAssertions.AssertTrue(!startingAt.ContainsKey(start), $"no animation starts in counted body {start}");
                continue;
            }
            (List<AnimationPhase<TSegment>> phases, AnimationEnd end) = Linearize(lists, start);
            VerificationAssertions.AssertTrue(startingAt.TryGetValue(start, out Animation<TSegment>? typed), $"an animation starts at {start}");
            VerificationAssertions.AssertTrue(typed!.Phases.SequenceEqual(phases), $"{start}'s phases follow its native lists");
            VerificationAssertions.AssertEqual(end, typed.End, $"{start}'s end follows its native lists");
        }
    }

    /// <summary>
    /// The segments whose native list runs on into the list authored next directly after its last beat, with no control
    /// instruction between: a play resting after that beat already stands at the next list's entry (#1600).
    /// </summary>
    internal static HashSet<TSegment> RunsOnIntoNextAfterLastBeat<TSegment>(IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists)
        where TSegment : struct, Enum =>
        [.. Ensure.NotNull(lists).Where(pair => pair.Value.BeatAddresses.Length > 0
                && pair.Value.Controls.FirstOrDefault(control => control.AfterBeat == pair.Value.BeatAddresses.Length) is { Kind: NativeEnemyControlKind.RunOnIntoNext })
            .Select(pair => pair.Key)];

    /// <summary>
    /// The native instruction pointer <paramref name="playback"/> stands for: the start segment's entry before the
    /// animation's first play, the native address of the phase beat it plays next, or, at the end of a phase's play, the
    /// control instruction the pointer rests on after the phase's last beat (the next list's entry where the list runs on
    /// into it).
    /// </summary>
    /// <exception cref="InvalidOperationException">The playback stands before a later phase, or at no native place.</exception>
    internal static ushort PointerOf<TSegment>(
        IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists,
        NativeProgramEntry<TSegment>[] entries,
        IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
        AnimationPlayback<TSegment> playback)
        where TSegment : struct, Enum
    {
        Ensure.NotNull(lists);
        Ensure.NotNull(entries);
        Ensure.NotNull(startingAt);
        Animation<TSegment> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        if (place.Beat == AnimationPlace.NotEntered)
        {
            if (place.Phase != 0)
                throw new InvalidOperationException($"A {typeof(TSegment).Name} playback before phase {place.Phase} stands at no native place.");
            TSegment start = startingAt.Single(pair => ReferenceEquals(pair.Value, animation)).Key;
            return entries.Single(entry => EqualityComparer<TSegment>.Default.Equals(entry.Program, start)).Entry;
        }
        AnimationPhase<TSegment> phase = animation.Phases[place.Phase];
        NativeEnemyList<TSegment> list = lists[phase.Segment];
        if (place.Beat < phase.BeatCount)
            return list.BeatAddresses[phase.FirstBeat + place.Beat];
        return list.Controls.FirstOrDefault(control => control.AfterBeat == phase.FirstBeat + phase.BeatCount)?.Address
            ?? throw new InvalidOperationException($"{phase.Segment}'s native list has no control after beat {phase.FirstBeat + phase.BeatCount}.");
    }

    /// <summary>
    /// The playback a recording's native instruction pointer <paramref name="nativePointer"/> stands for: before the
    /// first play of the animation starting at a segment whose entry it is; otherwise the phase beat at the pointer, or
    /// the end of a phase's play where the pointer rests on the control instruction after it, in the animation starting
    /// at that phase's segment when one plays it there, else the first in authored order that does.
    /// </summary>
    /// <exception cref="InvalidDataException">No animation, or more than one native place, matches the pointer.</exception>
    internal static AnimationPlayback<TSegment> PlaybackAt<TSegment>(
        IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists,
        NativeProgramEntry<TSegment>[] entries,
        IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
        ushort nativePointer)
        where TSegment : struct, Enum
    {
        Ensure.NotNull(lists);
        Ensure.NotNull(entries);
        Ensure.NotNull(startingAt);
        if (entries.Where(entry => entry.Entry == nativePointer && startingAt.ContainsKey(entry.Program)).Select(entry => (TSegment?)entry.Program).SingleOrDefault() is { } started)
            return new AnimationPlayback<TSegment>(startingAt[started], AnimationPlace.StartOfAnimation);
        var beats = new List<NativeEnemySegmentPlace<TSegment>>();
        var ends = new List<NativeEnemySegmentPlace<TSegment>>();
        foreach (KeyValuePair<TSegment, NativeEnemyList<TSegment>> pair in lists)
        {
            TSegment segment = pair.Key;
            NativeEnemyList<TSegment> list = pair.Value;
            for (int beat = 0; beat < list.BeatAddresses.Length; beat++)
            {
                if (list.BeatAddresses[beat] == nativePointer)
                    beats.Add(new(segment, beat));
            }
            // The pointer rests on a list's first control after a beat; a later control at the same beat never holds it.
            foreach (IGrouping<int, NativeEnemyControl<TSegment>> after in list.Controls.GroupBy(control => control.AfterBeat))
            {
                if (after.First().Address == nativePointer && after.First().Kind != NativeEnemyControlKind.RunOnIntoNext)
                    ends.Add(new(segment, after.Key));
            }
        }
        if (beats.Count + ends.Count != 1)
            throw new InvalidDataException($"Native {typeof(TSegment).Name} instruction pointer ${nativePointer:X4} matches {beats.Count + ends.Count} typed places.");
        bool atEnd = ends.Count == 1;
        NativeEnemySegmentPlace<TSegment> found = atEnd ? ends[0] : beats[0];
        IEnumerable<TSegment> candidates = startingAt.ContainsKey(found.Segment)
            ? [found.Segment, .. Enum.GetValues<TSegment>().Where(startingAt.ContainsKey)]
            : Enum.GetValues<TSegment>().Where(startingAt.ContainsKey);
        foreach (TSegment candidate in candidates)
        {
            Animation<TSegment> animation = startingAt[candidate];
            for (int index = 0; index < animation.Phases.Count; index++)
            {
                AnimationPhase<TSegment> phase = animation.Phases[index];
                if (!EqualityComparer<TSegment>.Default.Equals(phase.Segment, found.Segment))
                    continue;
                if (!atEnd && found.Beat >= phase.FirstBeat && found.Beat < phase.FirstBeat + phase.BeatCount)
                    return new AnimationPlayback<TSegment>(animation, new AnimationPlace(index, found.Beat - phase.FirstBeat));
                // A pointer resting on a sleep holds the phase's last frame; one resting on a goto, repeat or stop ends its play.
                if (atEnd && found.Beat == phase.FirstBeat + phase.BeatCount)
                    return new AnimationPlayback<TSegment>(animation,
                        lists[found.Segment].Controls.First(control => control.AfterBeat == found.Beat).Kind == NativeEnemyControlKind.Sleep
                            ? AnimationPlace.HoldingIn(index, phase.BeatCount)
                            : new AnimationPlace(index, phase.BeatCount));
            }
        }
        throw new InvalidDataException($"Native {typeof(TSegment).Name} instruction pointer ${nativePointer:X4} stands at {found}, which no typed animation plays.");
    }

    /// <summary>
    /// The phases and end the native lists play from <paramref name="start"/>, by the typed animations' rules, the first
    /// list's run playing <paramref name="firstPlays"/> times.
    /// </summary>
    private static (List<AnimationPhase<TSegment>> Phases, AnimationEnd End) Linearize<TSegment>(
        IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists, TSegment start, int firstPlays = 1)
        where TSegment : struct, Enum
    {
        var phases = new List<AnimationPhase<TSegment>>();
        var entered = new Dictionary<TSegment, int> { [start] = 0 };
        TSegment segment = start;
        int runStart = 0, plays = firstPlays;
        int? pending = null;
        // A run entered after a counter load counts the owner's counter even when the load is one and it plays once.
        bool counted = false;
        for (int guard = 0; guard < 100000; guard++)
        {
            foreach (NativeEnemyControl<TSegment> control in lists[segment].Controls)
            {
                if (control.AfterBeat > runStart)
                    phases.Add(new(segment, runStart, control.AfterBeat - runStart, plays,
                        counted && plays == 1 ? AnimationPhaseCounting.EveryPlay : AnimationPhaseCounting.RepeatsOnly));
                runStart = control.AfterBeat;
                plays = 1;
                counted = false;
                switch (control.Kind)
                {
                    case NativeEnemyControlKind.SetCounter:
                        pending = control.Count;
                        continue;
                    case NativeEnemyControlKind.Repeat:
                        // A count-down opening a list repeats the whole run of the list that ran on into it, the phase
                        // just played.
                        if (!EqualityComparer<TSegment>.Default.Equals(control.Target!.Value, segment)
                            && (phases.Count == 0 || !IsWholeRunOf(lists, phases[^1], control.Target.Value)))
                        {
                            throw new InvalidDataException($"{segment}'s count-down of {control.Target.Value} follows no whole run of that list.");
                        }
                        continue;
                    case NativeEnemyControlKind.Stop:
                        return (phases, AnimationEnd.Stops);
                    case NativeEnemyControlKind.Sleep:
                        return (phases, AnimationEnd.Holds);
                    case NativeEnemyControlKind.Goto:
                    case NativeEnemyControlKind.RunOnIntoNext:
                        TSegment target = control.Target!.Value;
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

    /// <summary>Whether <paramref name="phase"/> plays every beat of <paramref name="segment"/>'s native list, from its first.</summary>
    private static bool IsWholeRunOf<TSegment>(IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists, AnimationPhase<TSegment> phase, TSegment segment)
        where TSegment : struct, Enum =>
        EqualityComparer<TSegment>.Default.Equals(phase.Segment, segment) && phase.FirstBeat == 0 && phase.BeatCount == lists[segment].BeatAddresses.Length;

    /// <summary>
    /// The segments whose native list opens with the count-down of another list (#1600): the list that runs on into it
    /// repeats there while the counter lasts. No animation starts at one; a branch into one continues the play at the end
    /// of a play of the counted phase it counts down.
    /// </summary>
    internal static HashSet<TSegment> CountDownTails<TSegment>(IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists)
        where TSegment : struct, Enum =>
        [.. Ensure.NotNull(lists).Where(pair => pair.Value.Controls.FirstOrDefault() is { Kind: NativeEnemyControlKind.Repeat, AfterBeat: 0 } control
                && !EqualityComparer<TSegment>.Default.Equals(control.Target!.Value, pair.Key))
            .Select(pair => pair.Key)];

    /// <summary>
    /// Checks the continuation an owner's branch into each count-down tail takes (#1600): the end of a play of a counted
    /// phase, of one of the family's animations, that plays the whole list the tail counts down, a list that continues into
    /// the tail directly after its last beat. Counting down there replays that phase while the counter lasts and then
    /// moves on, as the tail's count-down does; what follows is the animation's own, which <see cref="CompareAnimations"/>
    /// matched to the lists.
    /// </summary>
    internal static void CompareCountDownContinuations<TSegment>(
        IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists, IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
        IReadOnlyDictionary<TSegment, AnimationContinuation<TSegment>> continuations)
        where TSegment : struct, Enum
    {
        Ensure.NotNull(lists);
        Ensure.NotNull(startingAt);
        Ensure.NotNull(continuations);
        foreach (TSegment tail in CountDownTails(lists))
        {
            TSegment counted = lists[tail].Controls[0].Target!.Value;
            VerificationAssertions.AssertTrue(continuations.TryGetValue(tail, out AnimationContinuation<TSegment> continuation),
                $"a branch into count-down {tail} has a continuation");
            VerificationAssertions.AssertTrue(startingAt.Values.Any(animation => ReferenceEquals(animation, continuation.Animation)),
                $"{tail}'s continuation plays one of the family's animations");
            AnimationPhase<TSegment> phase = continuation.Animation.Phases[continuation.Place.Phase];
            VerificationAssertions.AssertTrue(IsWholeRunOf(lists, phase, counted) && phase.CountsRepeats,
                $"{tail}'s continuation stands in a counted phase playing all of {counted}");
            VerificationAssertions.AssertEqual(phase.BeatCount, continuation.Place.Beat, $"{tail}'s continuation stands at the end of a play");
            VerificationAssertions.AssertTrue(lists[counted].Controls.FirstOrDefault(control => control.AfterBeat == phase.BeatCount) is
                { Kind: NativeEnemyControlKind.RunOnIntoNext or NativeEnemyControlKind.Goto } next && EqualityComparer<TSegment>.Default.Equals(next.Target!.Value, tail),
                $"{counted} continues into {tail} after its last beat");
        }
    }

    /// <summary>Reads one segment's native list, comparing its beats with the segment's.</summary>
    private static NativeEnemyList<TSegment> Read<TSegment, TFrame, TEvent>(
        IImportCartridgeSource cartridge, byte bank, NativeProgramEntry<TSegment>[] entries, AnimationSegments<TSegment, TFrame, TEvent> segments,
        Func<TFrame, ushort> spritemap, NativeEnemyCueRules<TEvent> rules, TSegment segment, ushort entry)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        (Func<ushort, int?> operandWords, Action<ushort, ushort[], TEvent, string> compareCue, Func<TEvent, bool>? endsList, Func<ushort, bool>? omitted) = rules;
        IReadOnlyList<AnimationBeat<TFrame, TEvent>> beats = segments.Beats(segment);
        var addresses = new List<ushort>();
        var controls = new List<NativeEnemyControl<TSegment>>();
        ushort cursor = entry;
        while (true)
        {
            string at = $"{segment} at ${bank:X2}:{cursor:X4}";
            int beat = addresses.Count;
            if (cursor != entry && entries.Where(item => item.Entry == cursor).Select(item => (TSegment?)item.Program).SingleOrDefault() is { } following)
            {
                controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, beat, following, null, cursor));
                return Finish();
            }
            int address = (int)new SnesAddress(bank, cursor);
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            NativeEnemyListDefinitions.WordKind kind = NativeEnemyListDefinitions.Classify(word);
            switch (kind)
            {
                case NativeEnemyListDefinitions.WordKind.Frame when Beat(beat, at) is AnimationBeat<TFrame, TEvent>.HeldFrame held:
                {
                    // A typed held frame stands for the list's longest frame followed at once by its sleep (#1607): the
                    // frame shows for good, which the sleep after it reads as the same rest.
                    VerificationAssertions.AssertEqual(NativeHeldFrameDuration, word, $"{at}: a held frame's native duration");
                    VerificationAssertions.AssertEqual(operand, spritemap(held.Shown), $"{at}: held frame's spritemap");
                    ushort afterHeld = cartridge.ReadCartridgeWord((int)new SnesAddress(bank, unchecked((ushort)(cursor + 4))));
                    VerificationAssertions.AssertEqual(NativeEnemyListDefinitions.WordKind.Sleep, NativeEnemyListDefinitions.Classify(afterHeld),
                        $"{at}: a held frame's native frame is followed by its sleep");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Frame:
                {
                    var frame = VerificationAssertions.AssertIs<AnimationBeat<TFrame, TEvent>.Frame>(Beat(beat, at), $"{at}: frame");
                    VerificationAssertions.AssertEqual(word, frame.Duration, $"{at}: duration");
                    VerificationAssertions.AssertEqual(operand, spritemap(frame.Shown), $"{at}: spritemap");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Wait:
                {
                    var pause = VerificationAssertions.AssertIs<AnimationBeat<TFrame, TEvent>.Pause>(Beat(beat, at), $"{at}: wait");
                    VerificationAssertions.AssertEqual(operand, pause.Duration, $"{at}: wait length");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    VerificationAssertions.AssertTrue(operandWords(word) is not null, $"{at}: native instruction ${word:X4} is known");
                    int count = operandWords(word) ?? 0;
                    if (omitted?.Invoke(word) == true)
                    {
                        cursor = unchecked((ushort)(cursor + 2 + 2 * count));
                        continue;
                    }
                    var raised = VerificationAssertions.AssertIs<AnimationBeat<TFrame, TEvent>.Event>(Beat(beat, at), $"{at}: instruction");
                    ushort[] operands = [.. Enumerable.Range(0, count).Select(item => cartridge.ReadCartridgeWord(address + 2 + 2 * item))];
                    compareCue(word, operands, raised.Raised, at);
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 2 + 2 * count));
                    if (endsList?.Invoke(raised.Raised) != true)
                        continue;
                    // A cue that always continues elsewhere ends its list: the animation holds there.
                    controls.Add(new(NativeEnemyControlKind.Sleep, beat + 1, null, null, cursor));
                    return Finish();
                }
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    controls.Add(new(NativeEnemyControlKind.Sleep, beat, null, null, cursor));
                    return Finish();
                case NativeEnemyListDefinitions.WordKind.Stop:
                    controls.Add(new(NativeEnemyControlKind.Stop, beat, null, null, cursor));
                    return Finish();
                case NativeEnemyListDefinitions.WordKind.GoTo:
                    controls.Add(new(NativeEnemyControlKind.Goto, beat, SegmentAt(operand, at), null, cursor));
                    return Finish();
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                    controls.Add(new(NativeEnemyControlKind.SetCounter, beat, null, operand, cursor));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                {
                    TSegment repeated = SegmentAt(operand, at);
                    // A list may open with the count-down of the list that ran on into it (#1600): a branch into it
                    // counts that list's plays on from where the counter stands.
                    VerificationAssertions.AssertTrue(EqualityComparer<TSegment>.Default.Equals(segment, repeated) || (beat == 0 && controls.Count == 0),
                        $"{at}: a counted repeat replays its own list, or, opening a list, another");
                    controls.Add(new(NativeEnemyControlKind.Repeat, beat, repeated, null, cursor));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)} {kind}.");
            }
        }

        NativeEnemyList<TSegment> Finish()
        {
            VerificationAssertions.AssertEqual(beats.Count, addresses.Count, $"{segment}: its native list holds each of its beats");
            return new NativeEnemyList<TSegment>([.. addresses], [.. controls]);
        }

        AnimationBeat<TFrame, TEvent> Beat(int beat, string at) => beat < beats.Count
            ? beats[beat]
            : throw new InvalidDataException($"{at}: the native list holds more beats than {segment}.");

        TSegment SegmentAt(ushort address, string at) =>
            entries.Where(item => item.Entry == address).Select(item => (TSegment?)item.Program).SingleOrDefault()
                ?? throw new InvalidDataException($"{at}: ${address:X4} is no segment's native entry.");
    }
}

/// <summary>One enemy segment's native list (#1600): each beat's native address, and its control instructions.</summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <param name="BeatAddresses">The native address of each beat, in order.</param>
/// <param name="Controls">The list's control instructions, in order.</param>
internal sealed record NativeEnemyList<TSegment>(ushort[] BeatAddresses, NativeEnemyControl<TSegment>[] Controls)
    where TSegment : struct, Enum;

/// <summary>What a native control instruction of an enemy's list does (#1600).</summary>
internal enum NativeEnemyControlKind
{
    /// <summary>Loads the enemy's timer: the plays the next list's counted run makes.</summary>
    SetCounter,

    /// <summary>Counts the timer down and, while it is not zero, replays its own list.</summary>
    Repeat,

    /// <summary>Continues at another list.</summary>
    Goto,

    /// <summary>Runs on into the list authored next.</summary>
    RunOnIntoNext,

    /// <summary>Sleeps: the animation rests on its last frame.</summary>
    Sleep,

    /// <summary>Stops: the enemy is deleted.</summary>
    Stop,
}

/// <summary>One native control instruction of an enemy's list (#1600).</summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <param name="Kind">What it does.</param>
/// <param name="AfterBeat">The number of the list's beats before it.</param>
/// <param name="Target">The list it continues at or replays, where it names one.</param>
/// <param name="Count">The timer it loads, where it loads one.</param>
/// <param name="Address">
/// The native address the instruction pointer rests at when it stands here: the instruction's own, or the next list's
/// entry for a run on into it.
/// </param>
internal sealed record NativeEnemyControl<TSegment>(NativeEnemyControlKind Kind, int AfterBeat, TSegment? Target, int? Count, ushort Address)
    where TSegment : struct, Enum;

/// <summary>A place in one segment's native list (#1600): at a beat, or at the end of a run that stops before one.</summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <param name="Segment">The segment.</param>
/// <param name="Beat">The beat the place stands at, or that the run ends before.</param>
internal readonly record struct NativeEnemySegmentPlace<TSegment>(TSegment Segment, int Beat)
    where TSegment : struct, Enum;

using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The ROM oracle of an enemy family's typed animations (#1600): each segment's beats matched to its native list, and
/// each animation matched to the lists' control flow (sleeps, stops, gotos, gotos back into a list's own beats, counter
/// loads, counted repeats and runs on into the next authored list), under the family's rules for its own instructions. A
/// marker beat (#2301) is the port's own and occupies no native beat: it stands at the native address of what follows it
/// in its list, and a play resting before it stands where the pointer rests before the beat after it.
/// </summary>
internal static class NativeEnemyAnimationOracle
{
    /// <summary>
    /// The native entry of <paramref name="segment"/>: the first of its lists, where the cartridge repeats the segment's
    /// list under more than one entry (#1842).
    /// </summary>
    internal static ushort EntryOf<TSegment>(NativeProgramEntry<TSegment>[] entries, TSegment segment)
        where TSegment : struct, Enum =>
        entries.First(item => EqualityComparer<TSegment>.Default.Equals(item.Program, segment)).Entry;

    /// <summary>The segment whose native list starts at <paramref name="entry"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="entry"/> starts no segment's list.</exception>
    internal static TSegment SegmentOf<TSegment>(NativeProgramEntry<TSegment>[] entries, ushort entry)
        where TSegment : struct, Enum =>
        Ensure.NotNull(entries).Where(item => item.Entry == entry).Select(item => (TSegment?)item.Program).SingleOrDefault()
            ?? throw new InvalidDataException($"${entry:X4} is no {typeof(TSegment).Name}'s native entry.");

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
    /// <param name="countedEntries">
    /// The animations the family's owners start at native entries that only load the counter (#2044, #2061), compared from
    /// those entries.
    /// </param>
    /// <param name="onlyRunOnInto">The segments no owner starts, which animations only run on into (#1948, #2061).</param>
    internal static int CompareAll<TSegment, TFrame, TEvent>(
        IImportCartridgeSource cartridge,
        byte bank,
        NativeProgramEntry<TSegment>[] entries,
        AnimationSegments<TSegment, TFrame, TEvent> segments,
        IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
        Func<TFrame, ushort> spritemap,
        NativeEnemyCueRules<TEvent> rules,
        IReadOnlyDictionary<TSegment, AnimationContinuation<TSegment>>? countDownContinuations = null,
        IReadOnlyList<NativeCountedEntry<TSegment>>? countedEntries = null,
        IReadOnlySet<TSegment>? onlyRunOnInto = null)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        Dictionary<TSegment, NativeEnemyList<TSegment>> lists = Lists(cartridge, bank, entries, segments, spritemap, rules);
        CompareAnimations(lists, startingAt, onlyRunOnInto: onlyRunOnInto,
            countedStarts: CountedStarts(cartridge, bank, entries, countedEntries ?? [], rules));
        CompareCountDownContinuations(lists, startingAt, countDownContinuations ?? new Dictionary<TSegment, AnimationContinuation<TSegment>>());
        return lists.Values.Sum(list => list.BeatAddresses.Length);
    }

    /// <summary>
    /// Reads each counted entry's native list (#2044, #2061): it must hold no beat, only the counter load, and continue into a
    /// segment's list, by running on into it or by a goto.
    /// </summary>
    /// <exception cref="InvalidDataException">An entry's list holds a beat or anything but one counter load and its continuation.</exception>
    internal static IReadOnlyList<NativeCountedStart<TSegment>> CountedStarts<TSegment, TEvent>(
        IImportCartridgeSource cartridge,
        byte bank,
        NativeProgramEntry<TSegment>[] entries,
        IReadOnlyList<NativeCountedEntry<TSegment>> countedEntries,
        NativeEnemyCueRules<TEvent> rules)
        where TSegment : struct, Enum
        where TEvent : notnull
    {
        Ensure.NotNull(entries);
        Ensure.NotNull(countedEntries);
        Ensure.NotNull(rules);
        return [.. countedEntries.Select(counted =>
        {
            NativeEnemyList<TSegment> read = ReadBeatless(cartridge, bank, entries, rules, counted.Name, counted.Address);
            if (read is not { Controls: [{ Kind: NativeEnemyControlKind.SetCounter, Count: { } count },
                    { Kind: NativeEnemyControlKind.RunOnIntoNext or NativeEnemyControlKind.Goto, Target: { } into }] })
                throw new InvalidDataException($"{counted.Name}'s native entry ${counted.Address:X4} does not only load the counter and continue into a segment's list.");
            return new NativeCountedStart<TSegment>(counted, count, into);
        })];
    }

    /// <summary>
    /// The segment whose list the native entry at <paramref name="address"/> leads into (#2061): an entry holding no beat
    /// and no counter load, only instructions the port omits, before it runs on into, or goes to, a segment's list. An owner
    /// that starts the entry starts that segment's animation, as a crawler's surface selector starts its crawl.
    /// </summary>
    /// <exception cref="InvalidDataException">The entry holds a beat, loads the counter, or ends without continuing into a segment's list.</exception>
    internal static TSegment LeadsInto<TSegment, TEvent>(
        IImportCartridgeSource cartridge, byte bank, NativeProgramEntry<TSegment>[] entries, NativeEnemyCueRules<TEvent> rules, ushort address)
        where TSegment : struct, Enum
        where TEvent : notnull =>
        ReadBeatless(cartridge, bank, entries, rules, $"lead-in ${address:X4}", address) is
            { Controls: [{ Kind: NativeEnemyControlKind.RunOnIntoNext or NativeEnemyControlKind.Goto, Target: { } into }] }
            ? into
            : throw new InvalidDataException($"The native entry ${address:X4} does not only lead into a segment's list.");

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
        // A segment the cartridge repeats under more than one entry (#1842) is its first list; every repeat is read
        // against the same beats. A repeat continuing as that list does is read as it; one continuing elsewhere (#2399:
        // Crocomire's claw swing, heading three lists that each go on differently) is kept as a repeat of its own, which the
        // continuations naming its entry reach.
        var lists = new Dictionary<TSegment, NativeEnemyList<TSegment>>();
        foreach (NativeProgramEntry<TSegment> entry in entries)
        {
            NativeEnemyList<TSegment> list = Read(cartridge, bank, entries, segments.Beats(entry.Program), spritemap, rules, entry.Program.ToString(), entry.Entry);
            if (!lists.TryGetValue(entry.Program, out NativeEnemyList<TSegment>? first))
            {
                lists.Add(entry.Program, list);
                continue;
            }
            if (!list.Controls.Select(Shape).SequenceEqual(first.Controls.Select(Shape)))
                lists[entry.Program] = first with { Repeats = [.. first.Repeats, list] };
        }
        return lists;

        static NativeEnemyControl<TSegment> Shape(NativeEnemyControl<TSegment> control) => control with { Address = 0, TargetEntry = null };
    }

    /// <summary>
    /// Requires every segment to have a native entry and the segments' first entries to follow their declared (authored)
    /// order. A segment whose list the cartridge repeats under more entries (#1842) lists each repeat where the cartridge
    /// places it.
    /// </summary>
    internal static void RequireAuthoredOrder<TSegment>(NativeProgramEntry<TSegment>[] entries)
        where TSegment : struct, Enum
    {
        Ensure.NotNull(entries);
        TSegment[] firsts = [.. entries.Select(entry => entry.Program).Distinct()];
        VerificationAssertions.AssertEqual(Enum.GetValues<TSegment>().Length, firsts.Length, $"every typed {typeof(TSegment).Name} has a native entry");
        for (int index = 0; index < firsts.Length; index++)
            VerificationAssertions.AssertEqual(Enum.GetValues<TSegment>()[index], firsts[index], $"the {typeof(TSegment).Name} values are declared in authored order");
    }

    /// <summary>
    /// Checks that every typed animation plays the phases the native lists' control flow plays from its first segment,
    /// and that no animation starts in a counted body other than those of <paramref name="startsWithClearedCounter"/>:
    /// counted lists an owner starts directly, with the counter its spawn cleared. The first play of such a list counts
    /// that zero down, wrapping it, so the list plays once and then the longest count more times, which no projectile
    /// started there lives through: its typed animation loops the list's run (#2096). Each animation an owner
    /// starts at a counted entry (<paramref name="countedStarts"/>, #2044) plays the phases the lists play from the segment
    /// the entry runs on into, its first run counted as many times as the entry loads.
    /// </summary>
    internal static void CompareAnimations<TSegment>(
        IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists, IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
        IReadOnlySet<TSegment>? startsWithClearedCounter = null, IReadOnlySet<TSegment>? onlyRunOnInto = null,
        IReadOnlyList<NativeCountedStart<TSegment>>? countedStarts = null)
        where TSegment : struct, Enum
    {
        Ensure.NotNull(lists);
        Ensure.NotNull(startingAt);
        foreach (NativeCountedStart<TSegment> counted in countedStarts ?? [])
        {
            (List<AnimationPhase<TSegment>> countedPhases, AnimationEnd countedEnd) = Linearize(lists, counted.Into, counted.Count, counted.IntoEntry);
            VerificationAssertions.AssertTrue(counted.Entry.Animation.Phases.SequenceEqual(countedPhases),
                $"{counted.Entry.Name}'s phases follow its native lists from its counted entry ${counted.Entry.Address:X4}");
            VerificationAssertions.AssertEqual(countedEnd, counted.Entry.Animation.End, $"{counted.Entry.Name}'s end follows its native lists");
        }
        // A counted repeat of one of a segment's repeats (#2399: Draygon's final whips count a copy of his whip's list) makes
        // only that copy a counted body; the segment's own list, where an animation starting at the segment starts, is not.
        HashSet<TSegment> bodies = [.. lists.Values.SelectMany(list => list.Repeats.Prepend(list)).SelectMany(list => list.Controls)
            .Where(control => control.Kind == NativeEnemyControlKind.Repeat
                && (control.TargetEntry is not { } counted || counted == lists[control.Target!.Value].Entry))
            .Select(control => control.Target!.Value)];
        HashSet<TSegment> tails = CountDownTails(lists);
        foreach (TSegment start in Enum.GetValues<TSegment>())
        {
            // A segment an animation only runs on into starts no animation of its own (#1948); the animations that run on
            // into it are compared through it.
            if (onlyRunOnInto?.Contains(start) == true)
            {
                VerificationAssertions.AssertTrue(!startingAt.ContainsKey(start), $"no animation starts at {start}, which animations only run on into");
                continue;
            }
            if (tails.Contains(start))
            {
                VerificationAssertions.AssertTrue(!startingAt.ContainsKey(start), $"no animation starts in count-down {start}");
                continue;
            }
            if (startsWithClearedCounter?.Contains(start) == true)
            {
                // The first play counts the cleared counter down, wrapping it, so the list replays its run the longest count
                // more times, longer than any projectile started there lives: the typed animation loops the run (#2096).
                VerificationAssertions.AssertTrue(bodies.Contains(start), $"{start}, started with a cleared counter, is a counted body");
                (List<AnimationPhase<TSegment>> counted, _) = Linearize(lists, start, WrappingCountdown.LongestStart);
                VerificationAssertions.AssertTrue(startingAt.TryGetValue(start, out Animation<TSegment>? started), $"an animation starts at {start}");
                VerificationAssertions.AssertTrue(started!.Phases.SequenceEqual([counted[0] with { Plays = 1 }]),
                    $"{start}'s phase is its native list's counted run, played once a pass");
                VerificationAssertions.AssertEqual(new AnimationEnd.Loop(0), started.End, $"{start} loops its run, which a cleared counter replays past any projectile's life");
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
    /// Checks a family whose owners start animations by names of their own rather than by segment (#2399): each named
    /// animation plays the phases, and ends as, the native lists play from the cartridge entry it starts at, which starts a
    /// segment's list or one of its repeats; and every segment is played by some named animation.
    /// </summary>
    /// <param name="lists">Each segment's native list, with its repeats.</param>
    /// <param name="entries">Each segment's native list and repeat, in authored order.</param>
    /// <param name="starts">Each named animation: its name, for failure messages, the animation, and the native entry it starts at.</param>
    /// <exception cref="InvalidDataException">A start's entry starts no segment's list.</exception>
    internal static void CompareNamedStarts<TSegment>(
        IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists, NativeProgramEntry<TSegment>[] entries,
        IReadOnlyList<(string Name, Animation<TSegment> Animation, ushort Entry)> starts)
        where TSegment : struct, Enum
    {
        Ensure.NotNull(lists);
        Ensure.NotNull(entries);
        Ensure.NotNull(starts);
        foreach ((string Name, Animation<TSegment> Animation, ushort Entry) start in starts)
        {
            TSegment segment = SegmentOf(entries, start.Entry);
            (List<AnimationPhase<TSegment>> phases, AnimationEnd end) = Linearize(lists, segment, startEntry: start.Entry);
            VerificationAssertions.AssertTrue(start.Animation.Phases.SequenceEqual(phases),
                $"{start.Name}'s phases follow its native lists from ${start.Entry:X4}");
            VerificationAssertions.AssertEqual(end, start.Animation.End, $"{start.Name}'s end follows its native lists");
        }
        foreach (TSegment segment in lists.Keys)
        {
            VerificationAssertions.AssertTrue(starts.Any(start => start.Animation.Phases.Any(phase => EqualityComparer<TSegment>.Default.Equals(phase.Segment, segment))),
                $"some named animation plays {segment}");
        }
    }

    /// <summary>
    /// The segments whose native list runs on into the list authored next directly after its last beat, with no control
    /// instruction between: a play resting after that beat already stands at the next list's entry (#1600).
    /// </summary>
    internal static HashSet<TSegment> RunsOnIntoNextAfterLastBeat<TSegment>(IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists)
        where TSegment : struct, Enum =>
        [.. Ensure.NotNull(lists).Where(pair => pair.Value.BeatAddresses.Length > 0
                && pair.Value.Controls.FirstOrDefault(control => control.AfterBeat == pair.Value.BeatAddresses.Length
                    && control.Kind != NativeEnemyControlKind.PoseShownAgain) is { Kind: NativeEnemyControlKind.RunOnIntoNext })
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
        AnimationPlayback<TSegment> playback,
        IReadOnlyList<NativeCountedStart<TSegment>>? countedStarts = null)
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
            // An animation an owner starts at a counted entry stands there before its first play (#2044).
            if ((countedStarts ?? []).SingleOrDefault(counted => ReferenceEquals(counted.Entry.Animation, animation)) is { } countedStart)
                return countedStart.Entry.Address;
            TSegment start = startingAt.Single(pair => ReferenceEquals(pair.Value, animation)).Key;
            return entries.Single(entry => EqualityComparer<TSegment>.Default.Equals(entry.Program, start)).Entry;
        }
        AnimationPhase<TSegment> phase = animation.Phases[place.Phase];
        NativeEnemyList<TSegment> list = ListPlayedBy(lists, startingAt, animation, place.Phase, countedStarts);
        if (list.Controls.Any(control => control.Kind == NativeEnemyControlKind.WrittenOutRepeat))
            throw new InvalidOperationException($"{phase.Segment}'s native list writes its run out more than once (#2337): each play stands at its own copy, which the list's beats do not record.");
        if (place.Beat < phase.BeatCount)
            return list.BeatAddresses[phase.FirstBeat + place.Beat];
        return list.Controls.FirstOrDefault(control => control.AfterBeat == phase.FirstBeat + phase.BeatCount)?.Address
            ?? throw new InvalidOperationException($"{phase.Segment}'s native list has no control after beat {phase.FirstBeat + phase.BeatCount}.");
    }

    /// <summary>
    /// The native list phase <paramref name="phase"/> of <paramref name="animation"/> plays: its segment's first list, or,
    /// where the cartridge repeats the segment under entries continuing elsewhere (#2376), the repeat the animation's
    /// native lists reach the phase through, from the segment it starts at or the counted entry it starts at.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The animation is none of <paramref name="startingAt"/> or <paramref name="countedStarts"/>, or its phases are not the
    /// ones its native lists play.
    /// </exception>
    internal static NativeEnemyList<TSegment> ListPlayedBy<TSegment>(
        IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists,
        IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
        Animation<TSegment> animation,
        int phase,
        IReadOnlyList<NativeCountedStart<TSegment>>? countedStarts = null)
        where TSegment : struct, Enum
    {
        Ensure.NotNull(lists);
        Ensure.NotNull(startingAt);
        Ensure.NotNull(animation);
        NativeEnemyList<TSegment> first = lists[animation.Phases[phase].Segment];
        if (first.Repeats.Count == 0)
            return first;
        (List<AnimationPhase<TSegment>> Phases, AnimationEnd End, List<NativeEnemyList<TSegment>> Played) walked =
            (countedStarts ?? []).SingleOrDefault(counted => ReferenceEquals(counted.Entry.Animation, animation)) is { } countedStart
                ? LinearizeLists(lists, countedStart.Into, countedStart.Count, countedStart.IntoEntry)
                : LinearizeLists(lists, startingAt.SingleOrDefault(pair => ReferenceEquals(pair.Value, animation)) is { Value: not null } started
                    ? started.Key
                    : throw new InvalidOperationException($"A {typeof(TSegment).Name} animation playing a repeated list starts at no segment or counted entry."),
                    firstPlays: 1, startEntry: null);
        (List<AnimationPhase<TSegment>> phases, List<NativeEnemyList<TSegment>> played) = (walked.Phases, walked.Played);
        if (!phases.SequenceEqual(animation.Phases))
            throw new InvalidOperationException($"A {typeof(TSegment).Name} animation playing a repeated list does not play the phases its native lists play.");
        return played[phase];
    }

    /// <summary>
    /// The playback a recording's native instruction pointer <paramref name="nativePointer"/> stands for: before the
    /// first play of the animation starting at a segment whose entry it is, or of the animation an owner starts at a
    /// counted entry it is (#2044); otherwise the phase beat at the pointer, or the end of a phase's play where the pointer
    /// rests on the control instruction after it, in the animation starting at that phase's segment when one plays it
    /// there, else the first in authored order that does, the animations started at counted entries after those starting
    /// at segments. In a list that writes its run out again (#2337, #2405), the pointer at a beat of a copy stands in that
    /// play of the repeated phase, with the plays from it on left.
    /// </summary>
    /// <param name="lists">Each segment's native list.</param>
    /// <param name="entries">Each segment's native entry.</param>
    /// <param name="startingAt">The typed animations, by the segment each starts at.</param>
    /// <param name="nativePointer">The recorded native instruction pointer.</param>
    /// <param name="countedStarts">The animations owners start at counted entries.</param>
    /// <param name="playing">
    /// The animation the recording's owner installed, where it keeps one beside the pointer (#2145): the pointer is placed in
    /// that animation alone, since a later request for it must find it playing.
    /// </param>
    /// <exception cref="InvalidDataException">No animation, or more than one native place, matches the pointer.</exception>
    internal static AnimationPlayback<TSegment> PlaybackAt<TSegment>(
        IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists,
        NativeProgramEntry<TSegment>[] entries,
        IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
        ushort nativePointer,
        IReadOnlyList<NativeCountedStart<TSegment>>? countedStarts = null,
        Animation<TSegment>? playing = null)
        where TSegment : struct, Enum
    {
        Ensure.NotNull(lists);
        Ensure.NotNull(entries);
        Ensure.NotNull(startingAt);
        IReadOnlyList<NativeCountedStart<TSegment>> counted = playing is null ? countedStarts ?? [] : [];
        if (entries.Where(entry => entry.Entry == nativePointer && startingAt.TryGetValue(entry.Program, out Animation<TSegment>? at)
                    && (playing is null || ReferenceEquals(at, playing)))
                .Select(entry => (TSegment?)entry.Program).SingleOrDefault() is { } started)
            return new AnimationPlayback<TSegment>(startingAt[started], AnimationPlace.StartOfAnimation);
        if (counted.SingleOrDefault(start => start.Entry.Address == nativePointer) is { } countedStart)
            return new AnimationPlayback<TSegment>(countedStart.Entry.Animation, AnimationPlace.StartOfAnimation);
        var beats = new List<(NativeEnemySegmentPlace<TSegment> Place, NativeEnemyList<TSegment> List, int PlaysLeft)>();
        var ends = new List<(NativeEnemySegmentPlace<TSegment> Place, NativeEnemyList<TSegment> List, int PlaysLeft)>();
        foreach (KeyValuePair<TSegment, NativeEnemyList<TSegment>> pair in lists)
        {
            TSegment segment = pair.Key;
            // A segment the cartridge repeats under entries continuing elsewhere (#2376) is searched in each of its lists.
            foreach (NativeEnemyList<TSegment> list in pair.Value.Repeats.Prepend(pair.Value))
            {
                // Each copy of a run the list writes out again (#2337, #2405) lies its distance from the entry past the
                // first, which the list's beats record: a play standing in one has the plays from it on left, and the end
                // after the last copy ends the last play. A list written out once counts no plays.
                int[] copies = [0, .. list.Controls.Where(control => control.Kind == NativeEnemyControlKind.WrittenOutRepeat)
                    .Select(control => control.Address - list.Entry)];
                // Markers (#2301) share the address of what follows them, and a play rests before a marker, never just past
                // it: the pointer stands at the first of the beats sharing its address, and an end those beats run up to is
                // theirs.
                var pastMarkers = new HashSet<int>();
                for (int copy = 0; copy < copies.Length; copy++)
                {
                    int playsLeft = copies.Length > 1 ? copies.Length - copy : 0;
                    for (int beat = 0; beat < list.BeatAddresses.Length; beat++)
                    {
                        if (CopyAddress(list, beat, copies[copy]) != nativePointer || (beat > 0 && CopyAddress(list, beat - 1, copies[copy]) == nativePointer))
                            continue;
                        beats.Add((new(segment, beat), list, playsLeft));
                        int past = beat;
                        while (past < list.BeatAddresses.Length && CopyAddress(list, past, copies[copy]) == nativePointer)
                            past++;
                        pastMarkers.Add(past);
                    }
                }
                // The pointer rests on a list's first control after a beat; a later control at the same beat never holds it.
                foreach (IGrouping<int, NativeEnemyControl<TSegment>> after in list.Controls
                    .Where(control => control.Kind != NativeEnemyControlKind.WrittenOutRepeat).GroupBy(control => control.AfterBeat))
                {
                    if (after.First().Address == nativePointer && !pastMarkers.Contains(after.Key)
                        && after.First().Kind is not (NativeEnemyControlKind.RunOnIntoNext or NativeEnemyControlKind.LoopStart or NativeEnemyControlKind.PoseShownAgain))
                        ends.Add((new(segment, after.Key), list, copies.Length > 1 ? 1 : 0));
                }
            }
        }
        if (beats.Count + ends.Count != 1)
            throw new InvalidDataException($"Native {typeof(TSegment).Name} instruction pointer ${nativePointer:X4} matches {beats.Count + ends.Count} typed places.");
        bool atEnd = ends.Count == 1;
        (NativeEnemySegmentPlace<TSegment> found, NativeEnemyList<TSegment> foundList, int foundPlaysLeft) = atEnd ? ends[0] : beats[0];
        IEnumerable<TSegment> candidates = startingAt.ContainsKey(found.Segment)
            ? [found.Segment, .. Enum.GetValues<TSegment>().Where(startingAt.ContainsKey)]
            : Enum.GetValues<TSegment>().Where(startingAt.ContainsKey);
        IEnumerable<Animation<TSegment>> animations = playing is null
            ? candidates.Select(candidate => startingAt[candidate]).Concat(counted.Select(start => start.Entry.Animation))
            : [playing];
        foreach (Animation<TSegment> animation in animations)
        {
            for (int index = 0; index < animation.Phases.Count; index++)
            {
                AnimationPhase<TSegment> phase = animation.Phases[index];
                if (!EqualityComparer<TSegment>.Default.Equals(phase.Segment, found.Segment)
                    || !ReferenceEquals(ListPlayedBy(lists, startingAt, animation, index, countedStarts), foundList))
                    continue;
                // A pointer in a written-out copy stands in the phase playing the list's every copy.
                if (foundPlaysLeft > 0 && phase.Plays != foundList.Controls.Count(control => control.Kind == NativeEnemyControlKind.WrittenOutRepeat) + 1)
                    continue;
                if (!atEnd && found.Beat >= phase.FirstBeat && found.Beat < phase.FirstBeat + phase.BeatCount)
                    return new AnimationPlayback<TSegment>(animation, new AnimationPlace(index, found.Beat - phase.FirstBeat).WithPlaysLeft(foundPlaysLeft));
                // A pointer resting on a sleep holds the phase's last frame; one resting on a goto, repeat or stop ends its play.
                if (atEnd && found.Beat == phase.FirstBeat + phase.BeatCount)
                    return new AnimationPlayback<TSegment>(animation,
                        foundList.Controls.First(control => control.AfterBeat == found.Beat && control.Kind != NativeEnemyControlKind.WrittenOutRepeat).Kind
                            == NativeEnemyControlKind.Sleep
                            ? AnimationPlace.HoldingIn(index, phase.BeatCount)
                            : new AnimationPlace(index, phase.BeatCount).WithPlaysLeft(foundPlaysLeft));
            }
        }
        throw new InvalidDataException($"Native {typeof(TSegment).Name} instruction pointer ${nativePointer:X4} stands at {found}, which no typed animation plays.");
    }

    /// <summary>
    /// <paramref name="playback"/> with the plays its repeated phase has left read from a recording's native loop counter
    /// <paramref name="counter"/> (#2006): in a counted list the counter holds the plays the list has left, the one playing
    /// included, as the place does. A playback before its phase, holding, or in a phase played once counts nothing and
    /// is returned as it is.
    /// </summary>
    /// <exception cref="InvalidDataException">The counter is outside the repeated phase's plays.</exception>
    internal static AnimationPlayback<TSegment> WithNativePlaysLeft<TSegment>(AnimationPlayback<TSegment> playback, ushort counter)
        where TSegment : struct
    {
        AnimationPlace place = playback.Place;
        if (place.Beat == AnimationPlace.NotEntered || playback.Animation.Phases.Count == 0)
            return playback;
        AnimationPhase<TSegment> phase = playback.Animation.Phases[place.Phase];
        if (!phase.IsRepeated || place.Beat > phase.BeatCount)
            return playback;
        if (counter < 1 || counter > phase.Plays)
            throw new InvalidDataException($"A native loop counter of {counter} stands outside the {phase.Plays} plays of repeated phase {place.Phase} ({phase.Segment}).");
        return playback with { Place = place.WithPlaysLeft(counter) };
    }

    /// <summary>
    /// The native address of beat <paramref name="beat"/> of <paramref name="list"/> in the copy of its run lying
    /// <paramref name="copyOffset"/> bytes past the first (#2405).
    /// </summary>
    private static ushort CopyAddress<TSegment>(NativeEnemyList<TSegment> list, int beat, int copyOffset)
        where TSegment : struct, Enum =>
        unchecked((ushort)(list.BeatAddresses[beat] + copyOffset));

    /// <summary>
    /// The plays the repeated phase <paramref name="playback"/> stands in has left, as a recording's native loop counter
    /// holds them, or null where the playback counts none: before its phase, holding, or in a phase played once (#2006).
    /// </summary>
    internal static int? NativePlaysLeft<TSegment>(AnimationPlayback<TSegment> playback)
        where TSegment : struct
    {
        AnimationPlace place = playback.Place;
        if (place.Beat == AnimationPlace.NotEntered || playback.Animation.Phases.Count == 0)
            return null;
        AnimationPhase<TSegment> phase = playback.Animation.Phases[place.Phase];
        return phase.IsRepeated && place.Beat <= phase.BeatCount ? place.PlaysLeft : null;
    }

    /// <summary>
    /// The phases and end the native lists play from <paramref name="start"/>, by the typed animations' rules, the first
    /// list's run playing <paramref name="firstPlays"/> times. A counter loaded before the play reaches the list it counts
    /// (#2275), one a counted repeat replays, is that list's plays: a list the play passes through first plays once.
    /// </summary>
    internal static (List<AnimationPhase<TSegment>> Phases, AnimationEnd End) Linearize<TSegment>(
        IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists, TSegment start, int firstPlays = 1, ushort? startEntry = null)
        where TSegment : struct, Enum
    {
        (List<AnimationPhase<TSegment>> phases, AnimationEnd end, _) = LinearizeLists(lists, start, firstPlays, startEntry);
        return (phases, end);
    }

    /// <summary>
    /// The phases and end the native lists play from <paramref name="start"/>, as <see cref="Linearize"/> reads them, with
    /// the native list each phase plays: where the cartridge repeats a segment under entries continuing elsewhere (#2376),
    /// the repeat each continuation names. The play starts at <paramref name="startEntry"/>'s list, or the start segment's
    /// first.
    /// </summary>
    private static (List<AnimationPhase<TSegment>> Phases, AnimationEnd End, List<NativeEnemyList<TSegment>> Played) LinearizeLists<TSegment>(
        IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists, TSegment start, int firstPlays, ushort? startEntry)
        where TSegment : struct, Enum
    {
        HashSet<TSegment> counted = [.. lists.Values.SelectMany(list => list.Repeats.Prepend(list)).SelectMany(list => list.Controls)
            .Where(control => control.Kind == NativeEnemyControlKind.Repeat).Select(control => control.Target!.Value)];
        var phases = new List<AnimationPhase<TSegment>>();
        var played = new List<NativeEnemyList<TSegment>>();
        NativeEnemyList<TSegment> current = ListAt(lists, start, startEntry);
        // Where the play entered each list, at its entry (beat zero) or at a beat a goto in that list loops back to (#2144).
        var entered = new Dictionary<(TSegment Segment, ushort Entry, int Beat), int> { [(start, current.Entry, 0)] = 0 };
        TSegment segment = start;
        int runStart = 0, plays = firstPlays;
        int? pending = null;
        for (int guard = 0; guard < 100000; guard++)
        {
            foreach (NativeEnemyControl<TSegment> control in current.Controls)
            {
                if (control.AfterBeat > runStart)
                {
                    phases.Add(new(segment, runStart, control.AfterBeat - runStart, plays));
                    played.Add(current);
                }
                runStart = control.AfterBeat;
                plays = 1;
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
                        return (phases, AnimationEnd.Stops, played);
                    case NativeEnemyControlKind.Sleep:
                        return (phases, AnimationEnd.Holds, played);
                    case NativeEnemyControlKind.LoopStart:
                        // The run before the beat a later goto returns to is its own phase; the loop plays on from here.
                        entered[(segment, current.Entry, control.AfterBeat)] = phases.Count;
                        continue;
                    case NativeEnemyControlKind.PoseShownAgain:
                        // A pose its owner may show again and the instruction that decides are a phase of their own.
                        continue;
                    case NativeEnemyControlKind.WrittenOutRepeat:
                        // The list's run written out again (#2337) is one more play of the phase that played it whole.
                        if (phases.Count == 0 || !IsWholeRunOf(lists, phases[^1], segment))
                            throw new InvalidDataException($"{segment}'s written-out repeat follows no whole run of its list.");
                        phases[^1] = phases[^1] with { Plays = phases[^1].Plays + 1 };
                        continue;
                    case NativeEnemyControlKind.Goto:
                    case NativeEnemyControlKind.RunOnIntoNext:
                        TSegment target = control.Target!.Value;
                        NativeEnemyList<TSegment> next = ListAt(lists, target, control.TargetEntry);
                        if (entered.TryGetValue((target, next.Entry, control.TargetBeat), out int loop))
                            return (phases, new AnimationEnd.Loop(loop), played);
                        if (control.TargetBeat != 0)
                            throw new InvalidDataException($"{segment}'s goto to beat {control.TargetBeat} of {target} enters no list the play has entered.");
                        entered[(target, next.Entry, 0)] = phases.Count;
                        segment = target;
                        current = next;
                        runStart = 0;
                        plays = 1;
                        if (counted.Contains(target))
                        {
                            plays = pending ?? 1;
                            pending = null;
                        }
                        break;
                    default:
                        throw new InvalidDataException($"Undefined native control {control.Kind}.");
                }
                break;
            }
        }
        throw new InvalidDataException($"The native lists from {start} never end or loop.");
    }

    /// <summary>
    /// The native list of <paramref name="segment"/> starting at <paramref name="entry"/>: the segment's repeat continuing
    /// elsewhere (#2376) when one starts there, otherwise its first list, which every repeat continuing as it does reads as.
    /// </summary>
    private static NativeEnemyList<TSegment> ListAt<TSegment>(IReadOnlyDictionary<TSegment, NativeEnemyList<TSegment>> lists, TSegment segment, ushort? entry)
        where TSegment : struct, Enum
    {
        NativeEnemyList<TSegment> first = lists[segment];
        return entry is { } at ? first.Repeats.FirstOrDefault(repeat => repeat.Entry == at) ?? first : first;
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
    /// Checks the continuation an owner's branch into each count-down tail takes (#1600): the end of a play of a repeated
    /// phase, of one of the family's animations, that plays the whole list the tail counts down, a list that continues into
    /// the tail directly after its last beat, counting on the plays the branching play had left (#2006). Counting down there
    /// replays that phase while those plays last and then moves on, as the tail's count-down of the shared counter does;
    /// what follows is the animation's own, which <see cref="CompareAnimations"/> matched to the lists.
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
            VerificationAssertions.AssertTrue(IsWholeRunOf(lists, phase, counted) && phase.IsRepeated,
                $"{tail}'s continuation stands in a repeated phase playing all of {counted}");
            VerificationAssertions.AssertTrue(continuation.CountsOn, $"{tail}'s continuation counts on the plays the branching play had left");
            VerificationAssertions.AssertEqual(phase.BeatCount, continuation.Place.Beat, $"{tail}'s continuation stands at the end of a play");
            VerificationAssertions.AssertTrue(lists[counted].Controls.FirstOrDefault(control => control.AfterBeat == phase.BeatCount) is
                { Kind: NativeEnemyControlKind.RunOnIntoNext or NativeEnemyControlKind.Goto } next && EqualityComparer<TSegment>.Default.Equals(next.Target!.Value, tail),
                $"{counted} continues into {tail} after its last beat");
        }
    }

    /// <summary>
    /// Reads the native list at <paramref name="address"/>, which must hold no beat (#2044, #2061): an entry that only loads
    /// the counter or runs instructions the port omits before it continues into a segment's list.
    /// </summary>
    private static NativeEnemyList<TSegment> ReadBeatless<TSegment, TEvent>(
        IImportCartridgeSource cartridge, byte bank, NativeProgramEntry<TSegment>[] entries, NativeEnemyCueRules<TEvent> rules, string name, ushort address)
        where TSegment : struct, Enum
        where TEvent : notnull =>
        Read<TSegment, ushort, TEvent>(cartridge, bank, entries, [], _ => throw new InvalidDataException($"{name} at ${address:X4} shows no frame."),
            rules, name, address);

    /// <summary>
    /// Reads one native list, comparing its beats with <paramref name="beats"/>. A goto into a list that starts no segment
    /// (one the cartridge shares or lays out between segments, #2061) continues the reading there: the typed segment holds
    /// what that list holds as its own.
    /// </summary>
    private static NativeEnemyList<TSegment> Read<TSegment, TFrame, TEvent>(
        IImportCartridgeSource cartridge, byte bank, NativeProgramEntry<TSegment>[] entries, IReadOnlyList<AnimationBeat<TFrame, TEvent>> beats,
        Func<TFrame, ushort> spritemap, NativeEnemyCueRules<TEvent> rules, string segment, ushort entry)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        (Func<ushort, int?> operandWords, Action<ushort, ushort[], TEvent, string> compareCue, Func<TEvent, bool>? endsList, Func<ushort, bool>? omitted,
            Func<ushort, ushort?>? goesTo, Func<ushort, bool>? showsPoseAgain, Func<ushort, ushort>? picturedAt, Func<ushort, bool>? ownerLeavesAt) = rules;
        var addresses = new List<ushort>();
        var controls = new List<NativeEnemyControl<TSegment>>();
        var continuedAt = new HashSet<ushort>();
        // The last goto the reading followed into a list no segment starts, and the beats read before it: reaching a
        // segment's list with no beat read since is that goto's continuation, its own list's start included.
        (ushort Address, int Beats)? followedGoto = null;
        // The length of the run a list writes out again straight after itself, once its first copy is read (#2337).
        int? writtenRun = null;
        ushort cursor = entry;
        while (true)
        {
            string at = $"{segment} at ${bank:X2}:{cursor:X4}";
            int beat = addresses.Count;
            bool viaGoto = followedGoto is { } followed && followed.Beats == beat;
            if ((cursor != entry || viaGoto)
                && entries.Where(item => item.Entry == cursor).Select(item => (TSegment?)item.Program).SingleOrDefault() is { } following)
            {
                controls.Add(viaGoto
                    ? new(NativeEnemyControlKind.Goto, beat, following, null, followedGoto!.Value.Address, TargetEntry: cursor)
                    : new(NativeEnemyControlKind.RunOnIntoNext, beat, following, null, cursor, TargetEntry: cursor));
                return Finish();
            }
            if (cursor != entry && ownerLeavesAt?.Invoke(cursor) == true)
            {
                // The owner always leaves the list before its play reaches here (#2376): the segment ends, holding, and
                // what follows never shows.
                controls.Add(new(NativeEnemyControlKind.Sleep, beat, null, null, cursor));
                return Finish();
            }
            if (beat < beats.Count && beats[beat] is AnimationBeat<TFrame, TEvent>.Marker)
            {
                // A marker (#2301) occupies no native beat: it stands at the address of what follows it in the list.
                addresses.Add(cursor);
                continue;
            }
            if (beat == beats.Count && beat > 0 && followedGoto is null
                && controls.TrueForAll(control => control.Kind == NativeEnemyControlKind.WrittenOutRepeat))
            {
                // A list that writes its whole run out again straight after itself (#2337) plays the segment once more: the
                // words from here repeat the run's word for word.
                int run = writtenRun ??= unchecked((ushort)(cursor - entry));
                if (Enumerable.Range(0, run / 2).All(word =>
                        cartridge.ReadCartridgeWord((int)new SnesAddress(bank, unchecked((ushort)(cursor + 2 * word))))
                        == cartridge.ReadCartridgeWord((int)new SnesAddress(bank, unchecked((ushort)(entry + 2 * word))))))
                {
                    controls.Add(new(NativeEnemyControlKind.WrittenOutRepeat, beat, null, null, cursor));
                    cursor = unchecked((ushort)(cursor + run));
                    continue;
                }
            }
            int address = (int)new SnesAddress(bank, cursor);
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            NativeEnemyListDefinitions.WordKind kind = NativeEnemyListDefinitions.Classify(word);
            ushort pictured = kind != NativeEnemyListDefinitions.WordKind.Frame ? operand : Pictured(operand);
            if (kind == NativeEnemyListDefinitions.WordKind.Callback && goesTo?.Invoke(word) is { } goneTo)
            {
                // An owner routine that only goes on to another list (#2167) is that list's goto.
                kind = NativeEnemyListDefinitions.WordKind.GoTo;
                operand = goneTo;
            }
            switch (kind)
            {
                case NativeEnemyListDefinitions.WordKind.Frame when Beat(beat, at) is AnimationBeat<TFrame, TEvent>.HeldFrame held:
                {
                    // A typed held frame stands for a frame followed at once by its list's sleep (#1607): the frame shows for
                    // good, which the sleep after it reads as the same rest, so its duration only times a sleep that shows
                    // nothing else (#2250). It stands too for a frame followed by a goto back to that frame (#2068, #2096):
                    // the list shows it again and again, for good, whatever duration it names, so the segment rests on it
                    // at the goto, as at a sleep, whether the frame follows beats before it or is its list's one record,
                    // which the goto returns to (#2353).
                    VerificationAssertions.AssertEqual(pictured, spritemap(held.Shown), $"{at}: held frame's spritemap");
                    ushort afterHeldCursor = unchecked((ushort)(cursor + 4));
                    int afterHeld = (int)new SnesAddress(bank, afterHeldCursor);
                    NativeEnemyListDefinitions.WordKind followedBy = NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord(afterHeld));
                    bool loopsOnItself = followedBy == NativeEnemyListDefinitions.WordKind.GoTo && cartridge.ReadCartridgeWord(afterHeld + 2) == cursor;
                    VerificationAssertions.AssertTrue(followedBy == NativeEnemyListDefinitions.WordKind.Sleep || loopsOnItself,
                        $"{at}: a held frame's native frame is followed by its sleep, or by a goto back to the frame");
                    addresses.Add(cursor);
                    cursor = afterHeldCursor;
                    if (loopsOnItself)
                    {
                        controls.Add(new(NativeEnemyControlKind.Sleep, beat + 1, null, null, cursor));
                        return Finish();
                    }
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Frame:
                {
                    var frame = VerificationAssertions.AssertIs<AnimationBeat<TFrame, TEvent>.Frame>(Beat(beat, at), $"{at}: frame");
                    VerificationAssertions.AssertEqual(pictured, spritemap(frame.Shown), $"{at}: spritemap");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    // A pose the list shows for one update and then again straight after, in its own list, is one typed
                    // frame of both durations (#2276): the picture shows on for as long, and the pointer stands at the
                    // first record while it lasts. A typed frame as long as the first record is that record alone. The
                    // record after may name another spritemap of the same picture (#2369).
                    int shown = word;
                    while (shown < frame.Duration && !entries.Any(item => item.Entry == cursor)
                        && cartridge.ReadCartridgeWord((int)new SnesAddress(bank, cursor)) is var again
                        && NativeEnemyListDefinitions.Classify(again) == NativeEnemyListDefinitions.WordKind.Frame
                        && Pictured(cartridge.ReadCartridgeWord((int)new SnesAddress(bank, cursor) + 2)) == pictured)
                    {
                        shown += again;
                        cursor = unchecked((ushort)(cursor + 4));
                    }
                    VerificationAssertions.AssertEqual(shown, frame.Duration, $"{at}: duration");
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Wait:
                {
                    var pause = VerificationAssertions.AssertIs<AnimationBeat<TFrame, TEvent>.Pause>(Beat(beat, at), $"{at}: wait");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    // A wait straight after another, in the same list, is one wait of both lengths (#2276): the typed
                    // segment pauses once for as long, and the pointer stands at the first wait while it lasts.
                    int waited = operand;
                    while (!entries.Any(item => item.Entry == cursor)
                        && NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord((int)new SnesAddress(bank, cursor))) == NativeEnemyListDefinitions.WordKind.Wait)
                    {
                        waited += cartridge.ReadCartridgeWord((int)new SnesAddress(bank, cursor) + 2);
                        cursor = unchecked((ushort)(cursor + 4));
                    }
                    VerificationAssertions.AssertEqual(waited, pause.Duration, $"{at}: wait length");
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
                    if (showsPoseAgain?.Invoke(word) == true)
                    {
                        // An instruction that may show the pose before it again (#2180) makes the pose and itself a run of
                        // their own: the list's run splits before the pose and after the instruction.
                        VerificationAssertions.AssertTrue(beat > 0 && beats[beat - 1] is AnimationBeat<TFrame, TEvent>.Frame,
                            $"{at}: an instruction showing the pose before it again follows a pose");
                        controls.Insert(controls.FindLastIndex(control => control.AfterBeat <= beat - 1) + 1,
                            new(NativeEnemyControlKind.PoseShownAgain, beat - 1, null, null, addresses[beat - 1]));
                        controls.Add(new(NativeEnemyControlKind.PoseShownAgain, beat + 1, null, null, cursor));
                    }
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
                    if (operand != entry && addresses.IndexOf(operand) is var loopBeat and > 0)
                    {
                        // A goto back to a beat its own list already showed (#2144) loops the segment from that beat: the
                        // cues or frames before it play once, as an entry cue before its run. The pointer stands at that
                        // beat's own address when the play reaches it.
                        TSegment own = SegmentAt(entry, at);
                        controls.Insert(controls.FindLastIndex(control => control.AfterBeat <= loopBeat) + 1,
                            new(NativeEnemyControlKind.LoopStart, loopBeat, own, null, operand));
                        controls.Add(new(NativeEnemyControlKind.Goto, beat, own, null, cursor, loopBeat, entry));
                        return Finish();
                    }
                    if (!entries.Any(item => item.Entry == operand))
                    {
                        // A list no segment starts continues this one: read on there, once.
                        if (!continuedAt.Add(operand))
                            throw new InvalidDataException($"{at}: the goto to ${operand:X4} returns to a list this one already continued in.");
                        followedGoto = followedGoto is { } chained && chained.Beats == beat ? chained : (cursor, beat);
                        cursor = operand;
                        continue;
                    }
                    controls.Add(new(NativeEnemyControlKind.Goto, beat, SegmentAt(operand, at), null, cursor, TargetEntry: operand));
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
                    VerificationAssertions.AssertTrue(operand == entry
                            || entries.Any(item => item.Entry == entry && EqualityComparer<TSegment>.Default.Equals(item.Program, repeated))
                            || (beat == 0 && controls.Count == 0),
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
            return new NativeEnemyList<TSegment>([.. addresses], [.. controls], entry);
        }

        AnimationBeat<TFrame, TEvent> Beat(int beat, string at) => beat < beats.Count
            ? beats[beat]
            : throw new InvalidDataException($"{at}: the native list holds more beats than {segment}.");

        TSegment SegmentAt(ushort address, string at) =>
            entries.Where(item => item.Entry == address).Select(item => (TSegment?)item.Program).SingleOrDefault()
                ?? throw new InvalidDataException($"{at}: ${address:X4} is no segment's native entry.");

        // A frame word repeating another record's picture shows the frame read from that record (#2249): the owner's own
        // rule where it has one, otherwise the record of the frame the import's repeated records name (#2272).
        ushort Pictured(ushort spritemapWord) =>
            picturedAt?.Invoke(spritemapWord) ?? EnemyFrameCartridgeLookupToolingConstants.OwnWord(bank, spritemapWord);
    }
}

/// <summary>
/// An animation an owner starts at a native entry that is no segment's list (#2044): the entry only loads the loop counter
/// and runs on into a segment's list, whose run the animation's first phase repeats that many times, as each n00b-tube
/// shard's spawn starts its flight.
/// </summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <param name="Name">The owner's identity for the animation, for failure messages.</param>
/// <param name="Animation">The animation.</param>
/// <param name="Address">The native entry.</param>
internal sealed record NativeCountedEntry<TSegment>(string Name, Animation<TSegment> Animation, ushort Address)
    where TSegment : struct, Enum;

/// <summary>A counted entry as its native list reads (#2044): the count it loads and the segment whose list it runs on into.</summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <param name="Entry">The entry and the animation started there.</param>
/// <param name="Count">The loop count the entry loads.</param>
/// <param name="Into">The segment whose list the entry continues into.</param>
/// <param name="IntoEntry">
/// The native entry of the list it continues into, where the oracle reads it: one of the segment's repeats continuing
/// elsewhere (#2376) is told apart by it; null reads the segment's first list.
/// </param>
internal sealed record NativeCountedStart<TSegment>(NativeCountedEntry<TSegment> Entry, int Count, TSegment Into, ushort? IntoEntry = null)
    where TSegment : struct, Enum;

/// <summary>One enemy segment's native list (#1600): each beat's native address, and its control instructions.</summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <param name="BeatAddresses">
/// The native address of each beat, in order; a marker beat (#2301), which occupies none, has the address of what follows it.
/// </param>
/// <param name="Controls">The list's control instructions, in order.</param>
/// <param name="Entry">The list's native entry, where the oracle records it; zero where it does not.</param>
internal sealed record NativeEnemyList<TSegment>(ushort[] BeatAddresses, NativeEnemyControl<TSegment>[] Controls, ushort Entry = 0)
    where TSegment : struct, Enum
{
    /// <summary>
    /// The segment's other native lists, which the cartridge repeats under other entries holding the same beats but
    /// continuing elsewhere (#2376: each n00b tube shard's flicker, once for its flight and once for its fall). A play
    /// reaches each through the continuation naming its entry. Repeats continuing as this list does are not kept: a play
    /// reads them as this one (#1842).
    /// </summary>
    internal IReadOnlyList<NativeEnemyList<TSegment>> Repeats { get; init; } = [];
}

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

    /// <summary>
    /// Marks the beat of its own list a later goto in that list returns to (#2144): no instruction, only the place the loop
    /// plays on from, which splits the list's run there.
    /// </summary>
    LoopStart,

    /// <summary>
    /// Marks where the run of a pose and the instruction that may show it again starts or ends (#2180): no instruction, only
    /// the place the list's run splits so that its owner can play that run again.
    /// </summary>
    PoseShownAgain,

    /// <summary>
    /// Marks a copy of the list's whole run written out again straight after it (#2337): no instruction, only the words of
    /// the run once more, which the typed animation plays as its segment's next play.
    /// </summary>
    WrittenOutRepeat,
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
/// <param name="TargetBeat">
/// The beat of the target's list it continues at: zero, the list's entry, except for a goto back into its own list (#2144).
/// </param>
/// <param name="TargetEntry">
/// The native entry of the target's list, where the oracle reads it: one of the target segment's repeats continuing
/// elsewhere (#2376) is told apart by it; null reads the segment's first list.
/// </param>
internal sealed record NativeEnemyControl<TSegment>(NativeEnemyControlKind Kind, int AfterBeat, TSegment? Target, int? Count, ushort Address, int TargetBeat = 0, ushort? TargetEntry = null)
    where TSegment : struct, Enum;

/// <summary>A place in one segment's native list (#1600): at a beat, or at the end of a run that stops before one.</summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <param name="Segment">The segment.</param>
/// <param name="Beat">The beat the place stands at, or that the run ends before.</param>
internal readonly record struct NativeEnemySegmentPlace<TSegment>(TSegment Segment, int Beat)
    where TSegment : struct, Enum;

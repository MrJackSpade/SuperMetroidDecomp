using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The ROM oracle of an enemy family's typed animations (#1600): each segment's beats matched to its native list, and
/// each animation matched to the lists' control flow (sleeps, stops, gotos, counter loads, counted repeats and runs on
/// into the next authored list), under the family's rules for its own instructions.
/// </summary>
internal static class NativeEnemyAnimationOracle
{
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
    internal static int CompareAll<TSegment, TFrame, TEvent>(
        IImportCartridgeSource cartridge,
        byte bank,
        NativeProgramEntry<TSegment>[] entries,
        AnimationSegments<TSegment, TFrame, TEvent> segments,
        IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
        Func<TFrame, ushort> spritemap,
        NativeEnemyCueRules<TEvent> rules)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        Ensure.NotNull(entries);
        Ensure.NotNull(segments);
        Ensure.NotNull(startingAt);
        Ensure.NotNull(rules);
        VerificationAssertions.AssertEqual(Enum.GetValues<TSegment>().Length, entries.Length, $"every typed {typeof(TSegment).Name} has a native entry");
        for (int index = 0; index < entries.Length; index++)
            VerificationAssertions.AssertEqual(Enum.GetValues<TSegment>()[index], entries[index].Program, $"the {typeof(TSegment).Name} values are declared in authored order");
        Dictionary<TSegment, NativeEnemyList<TSegment>> lists = entries.ToDictionary(entry => entry.Program,
            entry => Read(cartridge, bank, entries, segments, spritemap, rules, entry.Program, entry.Entry));

        HashSet<TSegment> bodies = [.. lists.Values.SelectMany(list => list.Controls)
            .Where(control => control.Kind == NativeEnemyControlKind.Repeat).Select(control => control.Target!.Value)];
        foreach (TSegment start in Enum.GetValues<TSegment>())
        {
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
        return lists.Values.Sum(list => list.BeatAddresses.Length);
    }

    /// <summary>The phases and end the native lists play from <paramref name="start"/>, by the typed animations' rules.</summary>
    private static (List<AnimationPhase<TSegment>> Phases, AnimationEnd End) Linearize<TSegment>(
        Dictionary<TSegment, NativeEnemyList<TSegment>> lists, TSegment start)
        where TSegment : struct, Enum
    {
        var phases = new List<AnimationPhase<TSegment>>();
        var entered = new Dictionary<TSegment, int> { [start] = 0 };
        TSegment segment = start;
        int runStart = 0, plays = 1;
        int? pending = null;
        for (int guard = 0; guard < 100000; guard++)
        {
            foreach (NativeEnemyControl<TSegment> control in lists[segment].Controls)
            {
                if (control.AfterBeat > runStart)
                    phases.Add(new(segment, runStart, control.AfterBeat - runStart, plays));
                runStart = control.AfterBeat;
                plays = 1;
                switch (control.Kind)
                {
                    case NativeEnemyControlKind.SetCounter:
                        pending = control.Count;
                        continue;
                    case NativeEnemyControlKind.Repeat:
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
    private static NativeEnemyList<TSegment> Read<TSegment, TFrame, TEvent>(
        IImportCartridgeSource cartridge, byte bank, NativeProgramEntry<TSegment>[] entries, AnimationSegments<TSegment, TFrame, TEvent> segments,
        Func<TFrame, ushort> spritemap, NativeEnemyCueRules<TEvent> rules, TSegment segment, ushort entry)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        (Func<ushort, int?> operandWords, Action<ushort, ushort[], TEvent, string> compareCue, Func<TEvent, bool>? endsList) = rules;
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
                controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, beat, following, null));
                return Finish();
            }
            int address = (int)new SnesAddress(bank, cursor);
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            NativeEnemyListDefinitions.WordKind kind = NativeEnemyListDefinitions.Classify(word);
            switch (kind)
            {
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
                    var raised = VerificationAssertions.AssertIs<AnimationBeat<TFrame, TEvent>.Event>(Beat(beat, at), $"{at}: instruction");
                    ushort[] operands = [.. Enumerable.Range(0, count).Select(item => cartridge.ReadCartridgeWord(address + 2 + 2 * item))];
                    compareCue(word, operands, raised.Raised, at);
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 2 + 2 * count));
                    if (endsList?.Invoke(raised.Raised) != true)
                        continue;
                    // A cue that always continues elsewhere ends its list: the animation holds there.
                    controls.Add(new(NativeEnemyControlKind.Sleep, beat + 1, null, null));
                    return Finish();
                }
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    controls.Add(new(NativeEnemyControlKind.Sleep, beat, null, null));
                    return Finish();
                case NativeEnemyListDefinitions.WordKind.Stop:
                    controls.Add(new(NativeEnemyControlKind.Stop, beat, null, null));
                    return Finish();
                case NativeEnemyListDefinitions.WordKind.GoTo:
                    controls.Add(new(NativeEnemyControlKind.Goto, beat, SegmentAt(operand, at), null));
                    return Finish();
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                    controls.Add(new(NativeEnemyControlKind.SetCounter, beat, null, operand));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                {
                    TSegment repeated = SegmentAt(operand, at);
                    VerificationAssertions.AssertEqual(segment, repeated, $"{at}: a counted repeat replays its own list");
                    controls.Add(new(NativeEnemyControlKind.Repeat, beat, repeated, null));
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
internal sealed record NativeEnemyControl<TSegment>(NativeEnemyControlKind Kind, int AfterBeat, TSegment? Target, int? Count)
    where TSegment : struct, Enum;

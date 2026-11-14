using SuperMetroid.Core;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The shared #641 ROM oracle for typed enemy-projectile programs: walks each family's native bank-$86 lists beside its
/// typed program and requires every frame's duration and presentation key (its operand's address), every control command
/// and every family command's cue to match. The family supplies its commands' operand byte counts and its cue comparison.
/// A family whose segments are its native lists, one each, is compared segment by segment
/// (<see cref="CompareAll{TSegment, TFrame, TEvent}"/>); a family whose animations each own their steps, whatever lists
/// they run through, is compared by the native lists' entries (<see cref="CompareByEntry{TAnimation, TFrame, TEvent}"/>).
/// </summary>
internal static class NativeEnemyProjectileProgramOracle
{
    /// <summary>The longest duration a native frame command gives, which a list follows with a rest to hold a frame.</summary>
    private const ushort NativeHeldFrameDuration = 0x7fff;

    /// <summary>The program whose native list starts at <paramref name="address"/>, if one does.</summary>
    private static TProgram? ProgramAt<TProgram>(NativeProgramEntry<TProgram>[] entries, ushort address)
        where TProgram : struct, Enum =>
        entries.Any(item => item.Entry == address) ? entries.Single(item => item.Entry == address).Program : null;

    /// <summary>
    /// Compares every typed segment with its native list and every typed animation with the lists' control flow under
    /// <paramref name="rules"/> (#1600), and returns the total beat count.
    /// </summary>
    /// <param name="cartridge">The pinned cartridge.</param>
    /// <param name="entries">Each segment's native entry, in authored order.</param>
    /// <param name="segments">The typed segments.</param>
    /// <param name="startingAt">The typed animations, by the segment each starts at.</param>
    /// <param name="presentationKey">The installed artwork key a frame shows: its native operand's address.</param>
    /// <param name="rules">How the family's commands are read and compared.</param>
    /// <param name="reflectedPresentationKey">
    /// The installed artwork key of a flicker frame's reflected pose, or null for a frame without one; required when
    /// <paramref name="rules"/> name flicker commands.
    /// </param>
    /// <param name="startsWithClearedCounter">The counted lists the family's owners start directly, with a cleared counter.</param>
    internal static int CompareAll<TSegment, TFrame, TEvent>(
        IImportCartridgeSource cartridge,
        NativeProgramEntry<TSegment>[] entries,
        AnimationSegments<TSegment, TFrame, TEvent> segments,
        IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
        Func<TFrame, ushort> presentationKey,
        NativeEnemyProjectileCueRules<TEvent> rules,
        Func<TFrame, ushort?>? reflectedPresentationKey = null,
        IReadOnlySet<TSegment>? startsWithClearedCounter = null)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        Dictionary<TSegment, NativeEnemyList<TSegment>> lists = Lists(cartridge, entries, segments, presentationKey, rules, reflectedPresentationKey);
        NativeEnemyAnimationOracle.CompareAnimations(lists, startingAt, startsWithClearedCounter);
        return lists.Values.Sum(list => list.BeatAddresses.Length);
    }

    /// <summary>
    /// Reads every typed segment's native bank-$86 list, comparing its beats with the segment's as
    /// <see cref="CompareAll{TSegment, TFrame, TEvent}(IImportCartridgeSource, NativeProgramEntry{TSegment}[], AnimationSegments{TSegment, TFrame, TEvent}, IReadOnlyDictionary{TSegment, Animation{TSegment}}, Func{TFrame, ushort}, NativeEnemyProjectileCueRules{TEvent}, Func{TFrame, ushort?}?, IReadOnlySet{TSegment}?)"/>
    /// does: each beat's native address, and each control instruction with its address and the beat it follows.
    /// </summary>
    internal static Dictionary<TSegment, NativeEnemyList<TSegment>> Lists<TSegment, TFrame, TEvent>(
        IImportCartridgeSource cartridge,
        NativeProgramEntry<TSegment>[] entries,
        AnimationSegments<TSegment, TFrame, TEvent> segments,
        Func<TFrame, ushort> presentationKey,
        NativeEnemyProjectileCueRules<TEvent> rules,
        Func<TFrame, ushort?>? reflectedPresentationKey = null)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        Ensure.NotNull(entries);
        Ensure.NotNull(segments);
        Ensure.NotNull(presentationKey);
        Ensure.NotNull(rules);
        NativeEnemyAnimationOracle.RequireAuthoredOrder(entries);
        VerificationAssertions.AssertTrue(entries.Zip(entries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends), $"native {typeof(TSegment).Name} entries ascend");
        return entries.ToDictionary(item => item.Program,
            item => Read(cartridge, entries, segments, presentationKey, rules, reflectedPresentationKey, item.Program, item.Entry));
    }

    /// <summary>
    /// Compares every typed animation of a family whose animations each own their steps (#1788) with the native bank-$86
    /// lists it plays: its phases, each a run of its own steps in step order, must be the runs and plays of the lists'
    /// control flow from the list it starts at (counter loads, counted repeats, gotos and runs on into the next list, a
    /// goto or run on into a list already entered looping from the phase entered there), each step must match the native
    /// beat its phase plays there, and its end must be the lists'. Every native list must be played by some animation, and
    /// no animation may start in a counted list. Returns the total step count.
    /// </summary>
    /// <param name="cartridge">The pinned cartridge.</param>
    /// <param name="lists">The entry of every native list the family's animations play, in bank order.</param>
    /// <param name="entries">The native list each animation starts at.</param>
    /// <param name="steps">Each animation's steps.</param>
    /// <param name="animations">How each animation plays its steps.</param>
    /// <param name="presentationKey">The installed artwork key a frame shows: its native operand's address.</param>
    /// <param name="rules">How the family's commands are read and compared.</param>
    /// <param name="reflectedPresentationKey">
    /// The installed artwork key of a flicker frame's reflected pose, or null for a frame without one; required when
    /// <paramref name="rules"/> name flicker commands.
    /// </param>
    internal static int CompareByEntry<TAnimation, TFrame, TEvent>(
        IImportCartridgeSource cartridge,
        IReadOnlyList<ushort> lists,
        IReadOnlyDictionary<TAnimation, ushort> entries,
        AnimationSegments<TAnimation, TFrame, TEvent> steps,
        IReadOnlyDictionary<TAnimation, Animation<TAnimation>> animations,
        Func<TFrame, ushort> presentationKey,
        NativeEnemyProjectileCueRules<TEvent> rules,
        Func<TFrame, ushort?>? reflectedPresentationKey = null)
        where TAnimation : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        Ensure.NotNull(lists);
        Ensure.NotNull(entries);
        Ensure.NotNull(steps);
        Ensure.NotNull(animations);
        Ensure.NotNull(presentationKey);
        Ensure.NotNull(rules);
        string family = typeof(TAnimation).Name;
        VerificationAssertions.AssertTrue(lists.Zip(lists.Skip(1), (first, second) => first < second).All(ascends => ascends), $"native {family} lists ascend");
        var isList = new HashSet<ushort>(lists);
        VerificationAssertions.AssertEqual(Enum.GetValues<TAnimation>().Length, entries.Count, $"every {family} starts at a native list");
        VerificationAssertions.AssertTrue(entries.Values.All(isList.Contains), $"every {family} starts at a native list's entry");
        VerificationAssertions.AssertEqual(entries.Count, entries.Values.Distinct().Count(), $"no two {family} values start at one native list");
        var played = new HashSet<ushort>();
        var counted = new HashSet<ushort>();
        int total = 0;
        foreach (TAnimation animation in Enum.GetValues<TAnimation>())
        {
            total += CompareAnimation(cartridge, isList, entries[animation], steps.Beats(animation), animation, animations, presentationKey, rules,
                reflectedPresentationKey, played, counted);
        }
        VerificationAssertions.AssertTrue(played.SetEquals(lists),
            $"every native {family} list is played by some animation; unplayed: {string.Join(", ", lists.Except(played).Select(list => $"${list:X4}"))}");
        VerificationAssertions.AssertTrue(!entries.Values.Any(counted.Contains), $"no {family} starts in a counted list");
        return total;
    }

    /// <summary>
    /// Compares <paramref name="animation"/>, starting at the native list at <paramref name="start"/>, with the lists it
    /// plays as <see cref="CompareByEntry{TAnimation, TFrame, TEvent}"/> does; records the lists it plays and the lists a
    /// counted repeat replays. Returns its step count.
    /// </summary>
    private static int CompareAnimation<TAnimation, TFrame, TEvent>(
        IImportCartridgeSource cartridge, IReadOnlySet<ushort> isList, ushort start, IReadOnlyList<AnimationBeat<TFrame, TEvent>> beats,
        TAnimation animation, IReadOnlyDictionary<TAnimation, Animation<TAnimation>> animations, Func<TFrame, ushort> presentationKey,
        NativeEnemyProjectileCueRules<TEvent> rules, Func<TFrame, ushort?>? reflectedPresentationKey, HashSet<ushort> played, HashSet<ushort> counted)
        where TAnimation : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        var phases = new List<AnimationPhase<TAnimation>>();
        var entered = new Dictionary<ushort, int> { [start] = 0 };
        ushort list = start;
        int offset = 0, plays = 1;
        int? pending = null;
        // A run entered after a counter load counts the owner's counter even when the load is one and it plays once.
        bool countsEveryPlay = false;
        AnimationEnd? end = null;
        while (end is null)
        {
            played.Add(list);
            NativeProjectileList read = ReadList(cartridge, isList.Contains, beats, offset, presentationKey, rules, reflectedPresentationKey,
                animation.ToString(), list);
            int runStart = 0;
            ushort? next = null;
            foreach (NativeProjectileControl control in read.Controls)
            {
                if (control.AfterBeat > runStart)
                {
                    phases.Add(new(animation, offset + runStart, control.AfterBeat - runStart, plays,
                        countsEveryPlay && plays == 1 ? AnimationPhaseCounting.EveryPlay : AnimationPhaseCounting.RepeatsOnly));
                }
                runStart = control.AfterBeat;
                plays = 1;
                countsEveryPlay = false;
                switch (control.Kind)
                {
                    case NativeEnemyControlKind.SetCounter:
                        pending = control.Count;
                        continue;
                    case NativeEnemyControlKind.Repeat:
                        counted.Add(list);
                        continue;
                    case NativeEnemyControlKind.Stop:
                        end = AnimationEnd.Stops;
                        break;
                    case NativeEnemyControlKind.Sleep:
                        end = AnimationEnd.Holds;
                        break;
                    case NativeEnemyControlKind.Goto:
                    case NativeEnemyControlKind.RunOnIntoNext:
                        ushort target = control.Target!.Value;
                        if (entered.TryGetValue(target, out int loop))
                        {
                            end = AnimationEnd.LoopsFrom(loop);
                            break;
                        }
                        entered[target] = phases.Count;
                        next = target;
                        plays = pending ?? 1;
                        countsEveryPlay = pending is not null;
                        pending = null;
                        break;
                    default:
                        throw new InvalidDataException($"Undefined native control {control.Kind}.");
                }
                break;
            }
            offset += read.BeatAddresses.Length;
            if (end is null)
                list = next ?? throw new InvalidDataException($"{animation}'s native list ${list:X4} neither ends nor continues.");
        }
        VerificationAssertions.AssertTrue(animations.TryGetValue(animation, out Animation<TAnimation>? typed), $"{animation} has its animation");
        VerificationAssertions.AssertTrue(typed!.Phases.SequenceEqual(phases), $"{animation}'s phases follow its native lists from ${start:X4}");
        VerificationAssertions.AssertEqual(end, typed.End, $"{animation}'s end follows its native lists");
        VerificationAssertions.AssertEqual(beats.Count, offset, $"every step of {animation} is played by one of its phases");
        return offset;
    }

    /// <summary>Reads one typed segment's native list, comparing its beats with the segment's.</summary>
    private static NativeEnemyList<TSegment> Read<TSegment, TFrame, TEvent>(
        IImportCartridgeSource cartridge, NativeProgramEntry<TSegment>[] entries, AnimationSegments<TSegment, TFrame, TEvent> segments,
        Func<TFrame, ushort> presentationKey, NativeEnemyProjectileCueRules<TEvent> rules, Func<TFrame, ushort?>? reflectedPresentationKey,
        TSegment segment, ushort entry)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        IReadOnlyList<AnimationBeat<TFrame, TEvent>> beats = segments.Beats(segment);
        NativeProjectileList read = ReadList(cartridge, address => ProgramAt(entries, address) is not null, beats, 0, presentationKey, rules,
            reflectedPresentationKey, segment.ToString(), entry);
        VerificationAssertions.AssertEqual(beats.Count, read.BeatAddresses.Length, $"{segment}: its native list holds each of its beats");
        return new NativeEnemyList<TSegment>(read.BeatAddresses,
            [.. read.Controls.Select(control => new NativeEnemyControl<TSegment>(control.Kind, control.AfterBeat,
                control.Target is { } target ? ProgramAt(entries, target) : null, control.Count, control.Address))]);
    }

    /// <summary>
    /// Reads the native list at <paramref name="entry"/>, one of the lists <paramref name="isList"/> names, comparing each of
    /// its beats with the typed beat at its place, from <paramref name="firstBeat"/> on: each beat's native address, and each
    /// control instruction with its address, the list's beat it follows and the list it names. A list ends where the next
    /// list begins (running on into it), at a sleep or goto, where routine code follows it, or at a cue that always
    /// continues elsewhere.
    /// </summary>
    private static NativeProjectileList ReadList<TFrame, TEvent>(
        IImportCartridgeSource cartridge, Func<ushort, bool> isList, IReadOnlyList<AnimationBeat<TFrame, TEvent>> beats, int firstBeat,
        Func<TFrame, ushort> presentationKey, NativeEnemyProjectileCueRules<TEvent> rules, Func<TFrame, ushort?>? reflectedPresentationKey,
        string name, ushort entry)
        where TFrame : struct
        where TEvent : notnull
    {
        var addresses = new List<ushort>();
        var controls = new List<NativeProjectileControl>();
        ushort cursor = entry;
        AnimationBeat<TFrame, TEvent> Next(string what)
        {
            VerificationAssertions.AssertTrue(firstBeat + addresses.Count < beats.Count, $"{name} has a beat for native {what} at $86:{cursor:X4}");
            addresses.Add(cursor);
            return beats[firstBeat + addresses.Count - 1];
        }
        while (true)
        {
            string at = $"{name} at $86:{cursor:X4}";
            if (cursor != entry && isList(cursor))
            {
                controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, addresses.Count, cursor, null, cursor));
                break;
            }
            if (rules.EndsAt?.Invoke(cursor) == true)
            {
                // The list ends where routine code follows: the animation holds there.
                controls.Add(new(NativeEnemyControlKind.Sleep, addresses.Count, null, null, cursor));
                break;
            }
            int address = NativeEnemyProjectileListDefinitions.ProjectileBank | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if ((word & NativeEnemyListDefinitions.InstructionBit) == 0 && firstBeat + addresses.Count < beats.Count &&
                beats[firstBeat + addresses.Count] is AnimationBeat<TFrame, TEvent>.HeldFrame held)
            {
                // A typed held frame stands for the list's longest frame followed at once by its sleep, or by a goto to its
                // own list's entry when it is the list's only frame, which shows the same frame again (#1607).
                bool firstOfList = addresses.Count == 0;
                _ = Next("held frame");
                VerificationAssertions.AssertEqual(NativeHeldFrameDuration, word, $"{at}: a held frame's native duration");
                VerificationAssertions.AssertEqual(unchecked((ushort)(cursor + 2)), presentationKey(held.Shown), $"{at}: held frame's presentation key");
                var afterHeld = (NativeEnemyProjectileInstruction)cartridge.ReadCartridgeWord(address + 4);
                ushort afterHeldOperand = cartridge.ReadCartridgeWord(address + 6);
                VerificationAssertions.AssertTrue(afterHeld == NativeEnemyProjectileInstruction.Sleep ||
                    (afterHeld == NativeEnemyProjectileInstruction.GotoY && afterHeldOperand == entry && firstOfList),
                    $"{at}: a held frame's native frame is followed by a sleep or by a goto back to the frame");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            if ((word & NativeEnemyListDefinitions.InstructionBit) == 0)
            {
                var frame = VerificationAssertions.AssertIs<AnimationBeat<TFrame, TEvent>.Frame>(Next("frame"), $"{at}: frame");
                VerificationAssertions.AssertEqual(word, frame.Duration, $"{at}: duration");
                VerificationAssertions.AssertEqual(unchecked((ushort)(cursor + 2)), presentationKey(frame.Shown), $"{at}: presentation key");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            // The control commands every projectile list shares.
            var instruction = (NativeEnemyProjectileInstruction)word;
            if (instruction == NativeEnemyProjectileInstruction.Sleep)
            {
                controls.Add(new(NativeEnemyControlKind.Sleep, addresses.Count, null, null, cursor));
                break;
            }
            if (instruction == NativeEnemyProjectileInstruction.GotoY)
            {
                controls.Add(new(NativeEnemyControlKind.Goto, addresses.Count, ListAt(isList, operand, at), null, cursor));
                break;
            }
            if (instruction == NativeEnemyProjectileInstruction.TimerInY)
            {
                controls.Add(new(NativeEnemyControlKind.SetCounter, addresses.Count, null, operand, cursor));
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            if (instruction == NativeEnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero)
            {
                ushort repeated = ListAt(isList, operand, at);
                VerificationAssertions.AssertEqual(entry, repeated, $"{at}: a counted repeat replays its own list");
                controls.Add(new(NativeEnemyControlKind.Repeat, addresses.Count, repeated, null, cursor));
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            VerificationAssertions.AssertTrue(rules.OperandBytes(word) is not null, $"{at}: native command ${word:X4} is known");
            int count = rules.OperandBytes(word) ?? 0;
            var raised = VerificationAssertions.AssertIs<AnimationBeat<TFrame, TEvent>.Event>(Next("command"), $"{at}: command");
            byte[] operands = [.. Enumerable.Range(0, count).Select(item => LittleEndianBytes.LowByte(cartridge.ReadCartridgeWord(address + 2 + item)))];
            rules.CompareCue(word, operands, raised.Raised, at);
            if (rules.Replays?.Invoke(raised.Raised) == true)
            {
                // A replay continues at the first step of the run it is in, which is its list's entry, as the native
                // conditional goto to its own list does.
                VerificationAssertions.AssertEqual(entry, operand, $"{at}: a replay's operand is its own list");
                VerificationAssertions.AssertEqual(0, controls.Count, $"{at}: a replayed run begins its list");
            }
            if (rules.ShowsFrame?.Invoke(word) == true)
            {
                // A flicker shows its first operand's pose (or, by parity, its second's) for one update.
                var flicker = VerificationAssertions.AssertIs<AnimationBeat<TFrame, TEvent>.Frame>(Next("flicker frame"), $"{at}: flicker frame");
                VerificationAssertions.AssertEqual(1, flicker.Duration, $"{at}: flicker duration");
                VerificationAssertions.AssertEqual(unchecked((ushort)(cursor + 2)), presentationKey(flicker.Shown), $"{at}: flicker presentation key");
                Func<TFrame, ushort?> reflected = reflectedPresentationKey ??
                    throw new InvalidOperationException($"{at}: a flicker family supplies its reflected presentation keys.");
                VerificationAssertions.AssertEqual(count == 4 ? unchecked((ushort)(cursor + 4)) : null, reflected(flicker.Shown), $"{at}: flicker reflected presentation key");
            }
            cursor = unchecked((ushort)(cursor + 2 + count));
            if (rules.EndsList?.Invoke(raised.Raised) != true)
                continue;
            // A cue that always continues elsewhere ends its list: the animation holds there.
            controls.Add(new(NativeEnemyControlKind.Sleep, addresses.Count, null, null, cursor));
            break;
        }
        return new NativeProjectileList([.. addresses], [.. controls]);
    }

    /// <summary>The native list starting at <paramref name="address"/>, failing when none does.</summary>
    private static ushort ListAt(Func<ushort, bool> isList, ushort address, string at) =>
        isList(address) ? address : throw new InvalidDataException($"{at}: ${address:X4} is no segment's native entry.");

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort EntryOf<TProgram>(NativeProgramEntry<TProgram>[] entries, TProgram program)
        where TProgram : struct, Enum =>
        entries.Single(item => EqualityComparer<TProgram>.Default.Equals(item.Program, program)).Entry;

    /// <summary>One native list as read: each beat's address, and each control instruction.</summary>
    /// <param name="BeatAddresses">Each beat's native address, in order.</param>
    /// <param name="Controls">Each control instruction, in order.</param>
    private sealed record NativeProjectileList(ushort[] BeatAddresses, NativeProjectileControl[] Controls);

    /// <summary>One control instruction of a native list, naming the list it continues at by its entry.</summary>
    /// <param name="Kind">What it does.</param>
    /// <param name="AfterBeat">The number of the list's beats before it.</param>
    /// <param name="Target">The entry of the list it continues at or replays, where it names one.</param>
    /// <param name="Count">The timer it loads, where it loads one.</param>
    /// <param name="Address">Its native address.</param>
    private sealed record NativeProjectileControl(NativeEnemyControlKind Kind, int AfterBeat, ushort? Target, int? Count, ushort Address);
}

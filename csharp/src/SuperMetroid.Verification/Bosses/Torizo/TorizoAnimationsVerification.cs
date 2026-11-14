using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoNativeListAddresses;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// #1289, #1600, #1788: Bomb and Golden Torizo's body animations against the retail cartridge. Each animation's phases must
/// follow the native bank-$AA lists' control flow from the list it starts at (sleeps, stops, gotos, counter loads, counted
/// repeats, runs on into the next list, and returns, which always continue elsewhere), and each of its steps must match
/// the native list beat its phase plays there: every frame's duration and spritemap, every common command, every Torizo
/// instruction's cue and operands, every tile transfer's descriptor, and the fixed sleeps two callbacks schedule.
/// </summary>
internal static class TorizoAnimationsVerification
{
    /// <summary>The updates the low-health explosion callback sleeps for.</summary>
    private const int LowHealthExplosionSleep = 40;

    /// <summary>The updates the death explosion callback sleeps for.</summary>
    private const int DeathExplosionSleep = 1;

    /// <summary>The bytes a common $814B tile transfer occupies: its opcode word and seven-byte descriptor.</summary>
    private const int TileTransferBytes = 9;

    /// <summary>What one beat of a native body list is.</summary>
    private enum BodyBeatKind
    {
        /// <summary>A frame shown for a duration.</summary>
        Frame,
        /// <summary>A wait on the shown frame.</summary>
        Wait,
        /// <summary>A common tile transfer: an artwork look.</summary>
        TileTransfer,
        /// <summary>A Torizo instruction: a cue.</summary>
        Instruction,
        /// <summary>The fixed sleep a callback schedules after it runs.</summary>
        CallbackSleep,
    }

    /// <summary>One beat of a native body list.</summary>
    /// <param name="Address">The beat's native address; a callback's sleep stands at the instruction after the callback.</param>
    /// <param name="Kind">What the beat is.</param>
    /// <param name="SleepUpdates">For a callback's sleep, its length.</param>
    private sealed record BodyBeat(ushort Address, BodyBeatKind Kind, int SleepUpdates = 0);

    /// <summary>One control instruction of a native body list.</summary>
    /// <param name="Kind">What it does.</param>
    /// <param name="AfterBeat">The number of the list's beats before it.</param>
    /// <param name="Target">The list it continues at, where it names one.</param>
    /// <param name="Count">The timer it loads, where it loads one.</param>
    private sealed record BodyControl(NativeEnemyControlKind Kind, int AfterBeat, ushort? Target, int? Count);

    /// <summary>One native body list: its beats and its control instructions.</summary>
    private sealed record BodyList(BodyBeat[] Beats, BodyControl[] Controls);

    /// <summary>One run of a native list an animation plays, as a typed phase plays a run of its steps.</summary>
    private sealed record NativePhase(ushort List, int FirstBeat, int BeatCount, int Plays, AnimationPhaseCounting Counting);

    /// <summary>Every body animation matches the native lists it plays, and every native list is played by some animation.</summary>
    internal static void VerifyTorizoAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertTrue(BodyLists.Zip(BodyLists.Skip(1), (first, second) => first < second).All(ascends => ascends), "native Torizo body lists ascend");
        AssertEqual(Enum.GetValues<TorizoAnimation>().Length, AnimationEntries.Count, "every Torizo body animation starts at a native list");
        AssertTrue(AnimationEntries.Values.All(IsBodyList), "every Torizo body animation starts at a native list's entry");
        AssertEqual(AnimationEntries.Count, AnimationEntries.Values.Distinct().Count(), "no two Torizo body animations start at one native list");
        Dictionary<ushort, BodyList> lists = BodyLists.ToDictionary(entry => entry, entry => ReadBodyList(cartridge, entry));
        var played = new HashSet<ushort>();
        int steps = 0;
        foreach (TorizoAnimation animation in Enum.GetValues<TorizoAnimation>())
            steps += CompareAnimation(cartridge, lists, animation, played);
        AssertTrue(played.SetEquals(BodyLists), $"every native Torizo body list is played by some animation; unplayed: {string.Join(", ", BodyLists.Except(played).Select(list => $"${list:X4}"))}");
        VerifyResumedSleeps(cartridge, lists);
        var poses = new HashSet<ushort>(TorizoAnimations.Segments.Frames.Select(TorizoFrameDefinitionsConstants.Spritemap));
        AssertEqual(TorizoFrameDefinitionsConstants.PoseCount, poses.Count, "every Torizo pose shows a distinct spritemap");
        AssertTrue(poses.SetEquals(TorizoAnimationsConstants.Spritemaps(TorizoAnimationsConstants.AllAnimations)), "the body animations show every Torizo pose");
        AssertTrue(poses.SetEquals(TorizoCollisionDefinitions.FramePointers), "every Torizo pose has its physical frame");
        Console.WriteLine($"  Torizo animations: {AnimationEntries.Count} animations ({steps} steps, {poses.Count} poses) play all {BodyLists.Length} bank-$AA lists as the cartridge does.");
    }

    /// <summary>
    /// Compares <paramref name="animation"/> with the native lists it plays: its phases, each a run of its own steps in step
    /// order, with the runs and plays of the lists' control flow from its entry, each step with the native beat it plays, and
    /// its end. Returns its step count.
    /// </summary>
    private static int CompareAnimation(IImportCartridgeSource cartridge, Dictionary<ushort, BodyList> lists, TorizoAnimation animation,
        HashSet<ushort> played)
    {
        (List<NativePhase> native, AnimationEnd end) = Linearize(lists, Entry(animation));
        Animation<TorizoAnimation> typed = TorizoAnimations.StartingWith(animation);
        IReadOnlyList<AnimationBeat<EnemyFrameId, TorizoCue>> steps = TorizoAnimations.Segments.Beats(animation);
        AssertEqual(native.Count, typed.Phases.Count, $"{animation} plays its native lists' runs");
        int offset = 0;
        for (int index = 0; index < native.Count; index++)
        {
            AnimationPhase<TorizoAnimation> phase = typed.Phases[index];
            NativePhase run = native[index];
            string at = $"Torizo {animation} phase {index}";
            AssertEqual(animation, phase.Segment, $"{at} plays its own steps");
            AssertEqual(offset, phase.FirstBeat, $"{at} follows the phase before it in its steps");
            AssertEqual(run.BeatCount, phase.BeatCount, $"{at}'s run length");
            AssertEqual(run.Plays, phase.Plays, $"{at}'s plays");
            AssertEqual(run.Counting, phase.Counting, $"{at}'s counting");
            for (int beat = 0; beat < run.BeatCount; beat++)
                CompareBeat(cartridge, run, lists[run.List].Beats[run.FirstBeat + beat], steps[offset + beat], $"{at} step {offset + beat}");
            played.Add(run.List);
            offset += run.BeatCount;
        }
        AssertEqual(steps.Count, offset, $"every step of {animation} is played by one of its phases");
        AssertEqual(end, typed.End, $"{animation}'s end follows its native lists");
        return steps.Count;
    }

    /// <summary>
    /// Compares one step with the native beat it plays: a frame's duration and spritemap, a wait's length, a tile
    /// transfer's look and descriptor, an instruction's cue and operands, a callback's sleep.
    /// </summary>
    private static void CompareBeat(IImportCartridgeSource cartridge, NativePhase run, BodyBeat beat, AnimationBeat<EnemyFrameId, TorizoCue> step, string at)
    {
        at = $"{at} at $AA:{beat.Address:X4}";
        int address = (BodyBankNumber << 16) | beat.Address;
        ushort word = cartridge.ReadCartridgeWord(address);
        ushort operand = cartridge.ReadCartridgeWord(address + 2);
        switch (beat.Kind)
        {
            case BodyBeatKind.TileTransfer:
            {
                var copy = AssertIs<TorizoCue.ShowArtwork>(AssertIs<AnimationBeat<EnemyFrameId, TorizoCue>.Event>(step, $"{at}: tile transfer").Raised, $"{at}: artwork look cue");
                TorizoLookUpload transfer = TorizoLookUploadDefinitions.Uploads[copy.Look];
                AssertTrue(TorizoLookUploadDefinitions.Sites.ToArray().Any(site => site.Instruction == beat.Address && site.Look == copy.Look), $"{at}: tile transfer descriptor");
                AssertEqual(transfer.ByteCount, cartridge.ReadCartridgeWord(address + 2), $"{at}: tile transfer size");
                AssertEqual(transfer.SourceAddress, cartridge.ReadCartridgeLongFixedBank(address + 4), $"{at}: tile transfer source");
                AssertEqual(ObjCharacterNativeWords.Of(transfer.FirstCharacter), cartridge.ReadCartridgeWord(address + 7), $"{at}: tile transfer destination");
                return;
            }
            case BodyBeatKind.Frame:
            {
                var show = AssertIs<AnimationBeat<EnemyFrameId, TorizoCue>.Frame>(step, $"{at}: frame");
                AssertEqual(word, show.Duration, $"{at}: duration");
                AssertEqual(operand, TorizoFrameDefinitionsConstants.Spritemap(show.Shown), $"{at}: spritemap");
                return;
            }
            case BodyBeatKind.Wait:
                AssertEqual(operand, AssertIs<AnimationBeat<EnemyFrameId, TorizoCue>.Pause>(step, $"{at}: wait").Duration, $"{at}: wait length");
                return;
            case BodyBeatKind.CallbackSleep:
                AssertEqual(beat.SleepUpdates, AssertIs<AnimationBeat<EnemyFrameId, TorizoCue>.Pause>(step, $"{at}: the callback's sleep").Duration,
                    $"{at}: the callback's sleep length");
                return;
            case BodyBeatKind.Instruction:
                CompareInstruction(cartridge, run, address, AssertIs<AnimationBeat<EnemyFrameId, TorizoCue>.Event>(step, $"{at}: instruction").Raised, at);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BodyBeatKind)} {beat.Kind}.");
        }
    }

    /// <summary>
    /// Compares a Torizo instruction with its cue: the cue it must become and each operand word with the value, phase, frame
    /// behaviour or animation the cue carries; a replay's operand is the entry of the list its run begins at.
    /// </summary>
    private static void CompareInstruction(IImportCartridgeSource cartridge, NativePhase run, int address, TorizoCue cue, string at)
    {
        ushort word = cartridge.ReadCartridgeWord(address);
        AssertTrue(BodyInstructions.TryGetValue(word, out TorizoNativeListAddressesBodyInstruction instruction), $"{at}: native instruction ${word:X4} is known");
        instruction.Cue.Check(cue, at);
        for (int operandIndex = 0; operandIndex < instruction.Operands.Length; operandIndex++)
        {
            ushort value = cartridge.ReadCartridgeWord(address + 2 + 2 * operandIndex);
            TorizoOperand role = instruction.Operands[operandIndex];
            if (role == TorizoOperand.ReplayedRun)
                AssertEqual(0, run.FirstBeat, $"{at}: a replayed run begins its list");
            ushort typed = role switch
            {
                TorizoOperand.Value => checked((ushort)(cue switch
                {
                    // The cartridge's operand is the step's byte offset into its word table.
                    TorizoCue.Posture posture => posture.Step * sizeof(ushort),
                    TorizoCue.Walk walk => walk.Step * sizeof(ushort),
                    // A swipe's operand is its placement's byte offset; a low-health explosion's selects the gut's or the
                    // face's rows, six bytes apart.
                    TorizoCue.ParameterizedSpawn { Kind: TorizoParameterizedSpawnKind.SpawnExplosiveSwipe } swipe => swipe.Parameter * sizeof(ushort),
                    TorizoCue.LowHealthExplosions explosions => (explosions.Site == BombTorizoExplosionSite.Gut ? 0 : 1) * 6,
                    TorizoCue.ParameterizedSpawn spawn => spawn.Parameter,
                    _ => throw new InvalidOperationException($"{at}: {cue} carries no value."),
                })),
                TorizoOperand.Function => TorizoNativeWords.Phases.Word(AssertIs<TorizoCue.SetPhase>(cue, $"{at}: cue").Phase),
                TorizoOperand.FrameBehavior => TorizoNativeWords.FrameBehaviors.Word(AssertIs<TorizoCue.SetFrameBehavior>(cue, $"{at}: cue").FrameBehavior),
                TorizoOperand.Target => Entry(cue switch
                {
                    TorizoCue.Branch branch => branch.Target,
                    TorizoCue.Choice choice => choice.Target,
                    _ => throw new InvalidOperationException($"{at}: {cue} carries no target."),
                }),
                TorizoOperand.Alternate => Entry(AssertIs<TorizoCue.Choice>(cue, $"{at}: cue").Alternate),
                TorizoOperand.ReplayedRun => run.List,
                _ => throw new InvalidOperationException($"Undefined {nameof(TorizoOperand)} {role}."),
            };
            AssertEqual(value, typed, $"{at}: {role} operand");
        }
    }

    /// <summary>
    /// Reads the native body list at <paramref name="entry"/>: each frame, wait, tile transfer and Torizo instruction (with
    /// the fixed sleep two callbacks schedule, a beat at the instruction after the callback), and each control instruction
    /// with the beat it follows. A return always continues elsewhere, so the list ends there.
    /// </summary>
    private static BodyList ReadBodyList(IImportCartridgeSource cartridge, ushort entry)
    {
        var beats = new List<BodyBeat>();
        var controls = new List<BodyControl>();
        ushort cursor = entry;
        while (true)
        {
            string at = $"Torizo list ${entry:X4} at $AA:{cursor:X4}";
            if (cursor != entry && IsBodyList(cursor))
            {
                controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, beats.Count, cursor, null));
                break;
            }
            int address = (BodyBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if (ClassifyBodyWord(word) == BodyWordKind.TileTransfer)
            {
                beats.Add(new(cursor, BodyBeatKind.TileTransfer));
                cursor = unchecked((ushort)(cursor + TileTransferBytes));
                continue;
            }
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                    beats.Add(new(cursor, BodyBeatKind.Frame));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    controls.Add(new(NativeEnemyControlKind.Sleep, beats.Count, null, null));
                    break;
                case NativeEnemyListDefinitions.WordKind.Stop:
                    controls.Add(new(NativeEnemyControlKind.Stop, beats.Count, null, null));
                    break;
                case NativeEnemyListDefinitions.WordKind.GoTo:
                    AssertTrue(IsBodyList(operand), $"{at}: ${operand:X4} is a native body list's entry");
                    controls.Add(new(NativeEnemyControlKind.Goto, beats.Count, operand, null));
                    break;
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                    controls.Add(new(NativeEnemyControlKind.SetCounter, beats.Count, null, operand));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                    AssertEqual(entry, operand, $"{at}: a counted repeat replays its own list");
                    controls.Add(new(NativeEnemyControlKind.Repeat, beats.Count, entry, null));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Wait:
                    beats.Add(new(cursor, BodyBeatKind.Wait));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Callback when OmittedBodyInstructions.Contains(word):
                    // A routine that returned at once has no event: read past it without a beat (#1870).
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    AssertTrue(BodyInstructions.TryGetValue(word, out TorizoNativeListAddressesBodyInstruction instruction), $"{at}: native instruction ${word:X4} is known");
                    beats.Add(new(cursor, BodyBeatKind.Instruction));
                    cursor = unchecked((ushort)(cursor + 2 + 2 * instruction.Operands.Length));
                    // Two callbacks sleep after they run; every other callback runs straight on.
                    if (instruction.Cue.Matches(new TorizoCue.LowHealthExplosions(BombTorizoExplosionSite.Gut)))
                        beats.Add(new(cursor, BodyBeatKind.CallbackSleep, LowHealthExplosionSleep));
                    else if (instruction.Cue.Matches(new TorizoCue.Signal(TorizoCueKind.SpawnDeathExplosion)))
                        beats.Add(new(cursor, BodyBeatKind.CallbackSleep, DeathExplosionSleep));
                    // A return always continues elsewhere and saves no return point: the list ends there.
                    if (Returns(instruction))
                    {
                        controls.Add(new(NativeEnemyControlKind.Sleep, beats.Count, null, null));
                        break;
                    }
                    continue;
                }
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
            }
            break;
        }
        return new BodyList([.. beats], [.. controls]);
    }

    /// <summary>Whether <paramref name="instruction"/> returns: to a saved place, or to the animation an interruption saved.</summary>
    private static bool Returns(TorizoNativeListAddressesBodyInstruction instruction) =>
        instruction.Cue.Matches(new TorizoCue.Signal(TorizoCueKind.Return)) || instruction.Cue.Matches(new TorizoCue.Signal(TorizoCueKind.ResumeInterrupted));

    /// <summary>
    /// The runs and plays the native lists play from <paramref name="start"/>, by the typed animations' rules, and how the
    /// play then ends: a counter load makes the next list's counted run play that many times, and a goto or a run on into a
    /// list already entered loops from the phase it entered there.
    /// </summary>
    private static (List<NativePhase> Phases, AnimationEnd End) Linearize(Dictionary<ushort, BodyList> lists, ushort start)
    {
        var phases = new List<NativePhase>();
        var entered = new Dictionary<ushort, int> { [start] = 0 };
        ushort list = start;
        int runStart = 0, plays = 1;
        int? pending = null;
        // A run entered after a counter load counts the owner's counter even when the load is one and it plays once.
        bool counted = false;
        for (int guard = 0; guard < 100000; guard++)
        {
            foreach (BodyControl control in lists[list].Controls)
            {
                if (control.AfterBeat > runStart)
                    phases.Add(new(list, runStart, control.AfterBeat - runStart, plays,
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
                        continue;
                    case NativeEnemyControlKind.Stop:
                        return (phases, AnimationEnd.Stops);
                    case NativeEnemyControlKind.Sleep:
                        return (phases, AnimationEnd.Holds);
                    case NativeEnemyControlKind.Goto:
                    case NativeEnemyControlKind.RunOnIntoNext:
                        ushort target = control.Target!.Value;
                        if (entered.TryGetValue(target, out int loop))
                            return (phases, AnimationEnd.LoopsFrom(loop));
                        entered[target] = phases.Count;
                        list = target;
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
        throw new InvalidDataException($"The native Torizo lists from ${start:X4} never end or loop.");
    }

    /// <summary>
    /// The AI resumes a sleeping list by stepping its pointer past the sleep; <see cref="TorizoAnimations.AfterSleep"/>
    /// must name the animation that starts there.
    /// </summary>
    private static void VerifyResumedSleeps(IImportCartridgeSource cartridge, Dictionary<ushort, BodyList> lists)
    {
        int resumed = 0;
        foreach ((TorizoAnimation animation, ushort entry) in AnimationEntries)
        {
            if (lists[entry].Controls.LastOrDefault() is not { Kind: NativeEnemyControlKind.Sleep })
                continue;
            ushort? sleep = SleepAddress(cartridge, entry);
            if (sleep is not { } address || AnimationAt(unchecked((ushort)(address + 2))) is not { } next)
                continue;
            AssertEqual(next, TorizoAnimations.AfterSleep(animation), $"Torizo {animation} resumes past its sleep into {next}");
            resumed++;
        }
        AssertEqual(3, resumed, "Torizo resumes three sleeping animations");
    }

    /// <summary>The native sleep that ends the list at <paramref name="entry"/>, if it ends in one.</summary>
    private static ushort? SleepAddress(IImportCartridgeSource cartridge, ushort entry)
    {
        ushort cursor = entry;
        while (true)
        {
            if (cursor != entry && IsBodyList(cursor))
                return null;
            int address = (BodyBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            if (ClassifyBodyWord(word) == BodyWordKind.TileTransfer)
            {
                cursor = unchecked((ushort)(cursor + TileTransferBytes));
                continue;
            }
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                case NativeEnemyListDefinitions.WordKind.Wait:
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Callback when OmittedBodyInstructions.Contains(word):
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    TorizoNativeListAddressesBodyInstruction instruction = BodyInstructions[word];
                    // A return ends its list without a sleep.
                    if (Returns(instruction))
                        return null;
                    cursor = unchecked((ushort)(cursor + 2 + 2 * instruction.Operands.Length));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    return cursor;
                case NativeEnemyListDefinitions.WordKind.GoTo:
                case NativeEnemyListDefinitions.WordKind.Stop:
                    return null;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
            }
        }
    }
}

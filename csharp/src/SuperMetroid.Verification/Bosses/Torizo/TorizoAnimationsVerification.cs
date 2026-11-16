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

    /// <summary>Bomb Torizo's swipe placement X table, which a swipe instruction's operand indexes by byte.</summary>
    private const int SwipeXTable = 0x86a738;

    /// <summary>Bomb Torizo's swipe placement Y table, beside the X table.</summary>
    private const int SwipeYTable = 0x86a74e;

    /// <summary>The standing-up posture X table a posture instruction's operand indexes by byte (#1891).</summary>
    private const int StandingXTable = 0xaac3ee;

    /// <summary>The standing-up posture Y table; an operand indexes its eight words wrapped to sixteen bytes.</summary>
    private const int StandingYTable = 0xaac40e;

    /// <summary>The sitting-down posture X table.</summary>
    private const int SittingXTable = 0xaac440;

    /// <summary>The sitting-down posture Y table; an operand indexes its eight words wrapped to sixteen bytes.</summary>
    private const int SittingYTable = 0xaac460;

    /// <summary>Bomb Torizo's walking velocity table.</summary>
    private const int BombWalkTable = 0xaac4bd;

    /// <summary>The faceless Bomb Torizo's walking velocity table.</summary>
    private const int FacelessWalkTable = 0xaac532;

    /// <summary>Golden Torizo's walking velocity table.</summary>
    private const int GoldenWalkTable = 0xaad59a;

    /// <summary>The bytes a common $814B tile transfer occupies: its opcode word and seven-byte descriptor.</summary>
    private const int TileTransferBytes = 9;

    /// <summary>What one beat of a native body list is.</summary>
    private enum BodyBeatKind
    {
        /// <summary>A frame shown for a duration.</summary>
        Frame,
        /// <summary>A wait on the shown frame.</summary>
        Wait,
        /// <summary>A run of common tile transfers with nothing between them: one artwork look (#1942).</summary>
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
    /// <param name="Transfers">For a run of tile transfers, how many it holds.</param>
    private sealed record BodyBeat(ushort Address, BodyBeatKind Kind, int SleepUpdates = 0, int Transfers = 1);

    /// <summary>One control instruction of a native body list.</summary>
    /// <param name="Kind">What it does.</param>
    /// <param name="AfterBeat">The number of the list's beats before it.</param>
    /// <param name="Target">The list it continues at, where it names one.</param>
    /// <param name="Count">The timer it loads, where it loads one.</param>
    private sealed record BodyControl(NativeEnemyControlKind Kind, int AfterBeat, ushort? Target, int? Count);

    /// <summary>One native body list: its beats and its control instructions.</summary>
    private sealed record BodyList(BodyBeat[] Beats, BodyControl[] Controls);

    /// <summary>One run of a native list an animation plays, as a typed phase plays a run of its steps.</summary>
    private sealed record NativePhase(ushort List, int FirstBeat, int BeatCount, int Plays);

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
        VerifyContinuations(cartridge, lists);
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
        // The posture the animation's last step cue left, which its decisions' targets follow (#2175).
        TorizoPostureFlags? posture = null;
        for (int index = 0; index < native.Count; index++)
        {
            AnimationPhase<TorizoAnimation> phase = typed.Phases[index];
            NativePhase run = native[index];
            string at = $"Torizo {animation} phase {index}";
            AssertEqual(animation, phase.Segment, $"{at} plays its own steps");
            AssertEqual(offset, phase.FirstBeat, $"{at} follows the phase before it in its steps");
            AssertEqual(run.BeatCount, phase.BeatCount, $"{at}'s run length");
            AssertEqual(run.Plays, phase.Plays, $"{at}'s plays");
            for (int beat = 0; beat < run.BeatCount; beat++)
            {
                AnimationBeat<EnemyFrameId, TorizoCue> step = steps[offset + beat];
                if (step is AnimationBeat<EnemyFrameId, TorizoCue>.Event { Raised: TorizoCue.Stepped stepped })
                    posture = TorizoFacing.AfterStep(stepped.Direction, stepped.Foot);
                CompareBeat(cartridge, run, lists[run.List].Beats[run.FirstBeat + beat], step, posture, $"{at} step {offset + beat}");
            }
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
    /// <param name="cartridge">The retail cartridge.</param>
    /// <param name="run">The native run the step's phase plays.</param>
    /// <param name="beat">The native beat the step plays.</param>
    /// <param name="step">The typed step.</param>
    /// <param name="posture">The posture the animation's last step cue before this step left, if any.</param>
    /// <param name="at">Where the step is, for failure messages.</param>
    private static void CompareBeat(IImportCartridgeSource cartridge, NativePhase run, BodyBeat beat, AnimationBeat<EnemyFrameId, TorizoCue> step,
        TorizoPostureFlags? posture, string at)
    {
        at = $"{at} at $AA:{beat.Address:X4}";
        int address = (BodyBankNumber << 16) | beat.Address;
        ushort word = cartridge.ReadCartridgeWord(address);
        ushort operand = cartridge.ReadCartridgeWord(address + 2);
        switch (beat.Kind)
        {
            case BodyBeatKind.TileTransfer:
            {
                // The transfers of one run land in one update: they are one look's copies, in order (#1942).
                var copy = AssertIs<TorizoCue.ShowArtwork>(AssertIs<AnimationBeat<EnemyFrameId, TorizoCue>.Event>(step, $"{at}: tile transfer").Raised, $"{at}: artwork look cue");
                TorizoLookUpload[] transfers = TorizoLookUploadDefinitions.Uploads[copy.Look];
                AssertEqual(beat.Transfers, transfers.Length, $"{at}: the look's copies");
                for (int index = 0; index < transfers.Length; index++)
                {
                    int transferAddress = address + index * TileTransferBytes;
                    ushort instruction = unchecked((ushort)(beat.Address + index * TileTransferBytes));
                    TorizoLookUpload transfer = transfers[index];
                    AssertTrue(TorizoLookUploadDefinitions.Sites.ToArray().Any(site => site.Instruction == instruction && site.Look == copy.Look), $"{at}: tile transfer {index} descriptor");
                    AssertEqual(transfer.ByteCount, cartridge.ReadCartridgeWord(transferAddress + 2), $"{at}: tile transfer {index} size");
                    AssertEqual(transfer.SourceAddress, cartridge.ReadCartridgeLongFixedBank(transferAddress + 4), $"{at}: tile transfer {index} source");
                    AssertEqual(ObjCharacterNativeWords.Of(transfer.FirstCharacter), cartridge.ReadCartridgeWord(transferAddress + 7), $"{at}: tile transfer {index} destination");
                }
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
                CompareInstruction(cartridge, run, address, AssertIs<AnimationBeat<EnemyFrameId, TorizoCue>.Event>(step, $"{at}: instruction").Raised, posture, at);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BodyBeatKind)} {beat.Kind}.");
        }
    }

    /// <summary>
    /// Compares a Torizo instruction with its cue: the cue it must become and each operand word with the value, phase, body
    /// motion or animation the cue carries; a replay's operand is the entry of the list its run begins at, and a decision's
    /// the animation its owner continues in for the posture the walk's last step left (#2175).
    /// </summary>
    private static void CompareInstruction(IImportCartridgeSource cartridge, NativePhase run, int address, TorizoCue cue,
        TorizoPostureFlags? stepPosture, string at)
    {
        ushort word = cartridge.ReadCartridgeWord(address);
        AssertTrue(BodyInstructions.TryGetValue(word, out TorizoNativeListAddressesBodyInstruction instruction), $"{at}: native instruction ${word:X4} is known");
        instruction.Cue.Check(cue, at);
        for (int operandIndex = 0; operandIndex < instruction.Operands.Length; operandIndex++)
        {
            ushort value = cartridge.ReadCartridgeWord(address + 2 + 2 * operandIndex);
            TorizoOperand role = instruction.Operands[operandIndex];
            if (role == TorizoOperand.Unread)
            {
                AssertTrue(value <= 1, $"{at}: the unread operand is the authored zero or one");
                continue;
            }
            if (role == TorizoOperand.SwipePlacement)
            {
                Offset16<RoomPixels> native = new(unchecked((short)cartridge.ReadCartridgeWord(SwipeXTable + value)),
                    unchecked((short)cartridge.ReadCartridgeWord(SwipeYTable + value)));
                AssertEqual(native, AssertIs<TorizoCue.Swipe>(cue, $"{at}: cue").Hand, $"{at}: swipe hand placement");
                continue;
            }
            if (role == TorizoOperand.Value && cue is TorizoCue.Posture posture)
            {
                // The cartridge's operand is the byte offset of the cue's entry in its posture tables (#1891).
                bool standing = posture.Kind == TorizoPostureKind.StandUp;
                Offset16<RoomPixels> native = new(
                    unchecked((short)cartridge.ReadCartridgeWord((standing ? StandingXTable : SittingXTable) + value)),
                    unchecked((short)cartridge.ReadCartridgeWord((standing ? StandingYTable : SittingYTable) + (value & 0xf))));
                AssertEqual(native, posture.Displacement, $"{at}: posture displacement");
                continue;
            }
            if (role == TorizoOperand.Value && cue is TorizoCue.Walk walk)
            {
                // The cartridge's operand is the byte offset of the cue's entry in its walking table (#1891).
                int table = walk.Kind switch
                {
                    TorizoWalkKind.WalkBombTorizo => BombWalkTable,
                    TorizoWalkKind.WalkFacelessBombTorizo => FacelessWalkTable,
                    TorizoWalkKind.WalkGoldenTorizo => GoldenWalkTable,
                    _ => throw new InvalidOperationException($"Undefined {nameof(TorizoWalkKind)} {walk.Kind}."),
                };
                AssertEqual(unchecked((short)cartridge.ReadCartridgeWord(table + value)), walk.Velocity, $"{at}: walking velocity");
                continue;
            }
            if (role == TorizoOperand.Function)
            {
                // Bomb and Golden Torizo's falling routines both name the falling phase (#1931).
                AssertEqual(TorizoNativeWords.Phases.Decode(value, "Torizo phase routine"),
                    AssertIs<TorizoCue.SetPhase>(cue, $"{at}: cue").Phase, $"{at}: phase");
                continue;
            }
            if (role == TorizoOperand.Motion)
            {
                // Bomb and Golden Torizo's walking and standing routines start the same motions (#1931).
                AssertEqual(TorizoNativeWords.MotionCues.Decode(value, "Torizo body-motion routine"),
                    AssertIs<TorizoCue.Signal>(cue, $"{at}: cue").Kind, $"{at}: body motion");
                continue;
            }
            if (role == TorizoOperand.ReplayedRun)
                AssertEqual(0, run.FirstBeat, $"{at}: a replayed run begins its list");
            ushort typed = role switch
            {
                TorizoOperand.Value => checked((ushort)(cue switch
                {
                    // A low-health explosion's operand selects the gut's or the face's rows, six bytes apart.
                    TorizoCue.LowHealthExplosions explosions => (explosions.Site == BombTorizoExplosionSite.Gut ? 0 : 1) * 6,
                    _ => throw new InvalidOperationException($"{at}: {cue} carries no value."),
                })),
                TorizoOperand.Target => Entry(cue switch
                {
                    TorizoCue.Branch branch => TorizoBranches.Target(branch.Kind,
                        stepPosture ?? throw new InvalidDataException($"{at}: a decision follows no step of its walk.")),
                    TorizoCue.Interrupt interrupt => interrupt.Animation,
                    TorizoCue.Choice choice => choice.Target,
                    TorizoCue.LandIn landIn => landIn.Landing,
                    _ => throw new InvalidOperationException($"{at}: {cue} carries no target."),
                }),
                TorizoOperand.Alternate => Entry(AssertIs<TorizoCue.Choice>(cue, $"{at}: cue").Alternate),
                TorizoOperand.ReplayedRun => run.List,
                TorizoOperand.SwipePlacement or TorizoOperand.Unread or TorizoOperand.Function or TorizoOperand.Motion =>
                    throw new InvalidOperationException($"{at}: the {role} operand is compared before the word table."),
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
                // A transfer straight after another, with no beat or control between, joins its run: one look (#1942).
                if (beats.Count > 0 && beats[^1] is { Kind: BodyBeatKind.TileTransfer } run &&
                    unchecked((ushort)(run.Address + run.Transfers * TileTransferBytes)) == cursor &&
                    (controls.Count == 0 || controls[^1].AfterBeat < beats.Count))
                    beats[^1] = run with { Transfers = run.Transfers + 1 };
                else
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
                    if (Returns(word))
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

    /// <summary>Whether the native instruction <paramref name="word"/> returns: to a place a call saved, or to the animation an interruption saved.</summary>
    private static bool Returns(ushort word) => word is ReturnInstruction or ResumeInterruptedInstruction;

    /// <summary>
    /// The native calls that interrupted the walk, keeping its place to return to (#2028): Bomb Torizo's swipe and Golden
    /// Torizo's stagger, eye beam and eggs. Each animation they call ends by resuming the walk.
    /// </summary>
    private static readonly HashSet<ushort> WalkInterruptions = [0xc567, 0xd49b, 0xd445, 0xd474];

    /// <summary>
    /// The native calls that chose an attack on landing, returning to the landing's list past the call (#2028). Each attack
    /// they call ends by walking on in the animation that starts where that return continued.
    /// </summary>
    private static readonly HashSet<ushort> AttackChoicesOnLanding = [0xc5a4, 0xd526];

    /// <summary>
    /// The cartridge's calls returned to the place they saved; each typed animation instead names where it continues
    /// (#2028). Every animation a walk interruption calls must resume the walk; every attack a landing chooses must walk on
    /// in the animation that starts where the cartridge's return continued, past any goto; and every named continuation
    /// must be one a native call establishes, but the face blow-up's turn, which the AI's own code chose.
    /// </summary>
    private static void VerifyContinuations(IImportCartridgeSource cartridge, Dictionary<ushort, BodyList> lists)
    {
        var resumers = new HashSet<TorizoAnimation>();
        var walksOn = new Dictionary<TorizoAnimation, TorizoAnimation>();
        int calls = 0;
        foreach (BodyBeat beat in lists.Values.SelectMany(list => list.Beats).Where(beat => beat.Kind == BodyBeatKind.Instruction))
        {
            int address = (BodyBankNumber << 16) | beat.Address;
            ushort word = cartridge.ReadCartridgeWord(address);
            bool interrupts = WalkInterruptions.Contains(word);
            if (!interrupts && !AttackChoicesOnLanding.Contains(word))
                continue;
            calls++;
            int operands = BodyInstructions[word].Operands.Length;
            // A walk interruption returns into the middle of the walk's list, which the kept playback stands in.
            ushort? returnPoint = interrupts ? null : PastGoTos(cartridge, unchecked((ushort)(beat.Address + 2 + 2 * operands)));
            for (int operand = 0; operand < operands; operand++)
            {
                ushort target = cartridge.ReadCartridgeWord(address + 2 + 2 * operand);
                TorizoAnimation called = AnimationAt(target)
                    ?? throw new InvalidDataException($"Torizo call at $AA:{beat.Address:X4} calls ${target:X4}, where no animation starts.");
                string at = $"Torizo {called}, called at $AA:{beat.Address:X4}";
                TorizoCue end = EndCue(called);
                if (interrupts)
                {
                    AssertEqual<TorizoCue>(new TorizoCue.Signal(TorizoCueKind.ResumeWalk), end, $"{at}: resumes the walk it interrupted");
                    resumers.Add(called);
                    continue;
                }
                var walkOn = AssertIs<TorizoCue.WalkOn>(end, $"{at}: walks on");
                AssertEqual(returnPoint, Entry(walkOn.Walking), $"{at}: walks on where the cartridge's return continued, ${returnPoint:X4}");
                walksOn[called] = walkOn.Walking;
            }
        }
        foreach (TorizoAnimation animation in Enum.GetValues<TorizoAnimation>())
        {
            IReadOnlyList<AnimationBeat<EnemyFrameId, TorizoCue>> steps = TorizoAnimations.Segments.Beats(animation);
            if (steps[^1] is not AnimationBeat<EnemyFrameId, TorizoCue>.Event { Raised: var cue })
                continue;
            switch (cue)
            {
                case TorizoCue.WalkOn:
                    AssertTrue(walksOn.ContainsKey(animation), $"Torizo {animation} walks on as some native attack choice returned");
                    break;
                case TorizoCue.Signal { Kind: TorizoCueKind.ResumeWalk }:
                    AssertTrue(resumers.Contains(animation), $"Torizo {animation} resumes a walk some native call interrupted");
                    break;
                case TorizoCue.Turn turn:
                    AssertEqual(TorizoAnimation.BombTorizoFaceBlowingUp, animation, "only the face blow-up ends in a turn");
                    AssertEqual(TorizoAnimation.FacelessTurningLeft, turn.WhenFacingRight, "the face blow-up turns a right-facing Torizo left");
                    AssertEqual(TorizoAnimation.FacelessTurningRight, turn.WhenFacingLeft, "the face blow-up turns a left-facing Torizo right");
                    break;
                default:
                    // Every other cue ends an animation that holds, stops or loops on in its own steps.
                    break;
            }
        }
        Console.WriteLine($"  Torizo continuations: {calls} native calls; {resumers.Count} animations resume the walk, {walksOn.Count} walk on where the cartridge returned.");
    }

    /// <summary>The cue <paramref name="animation"/>'s steps end with: where an attack, a stagger or a blow-up continues.</summary>
    private static TorizoCue EndCue(TorizoAnimation animation) =>
        AssertIs<AnimationBeat<EnemyFrameId, TorizoCue>.Event>(TorizoAnimations.Segments.Beats(animation)[^1], $"Torizo {animation} ends in a cue").Raised;

    /// <summary>The native list a return to <paramref name="address"/> continues in, following any gotos there.</summary>
    private static ushort PastGoTos(IImportCartridgeSource cartridge, ushort address)
    {
        while (!IsBodyList(address))
        {
            ushort word = cartridge.ReadCartridgeWord((BodyBankNumber << 16) | address);
            if (NativeEnemyListDefinitions.Classify(word) != NativeEnemyListDefinitions.WordKind.GoTo)
                throw new InvalidDataException($"The Torizo return point $AA:{address:X4} is neither a list's entry nor a goto.");
            address = cartridge.ReadCartridgeWord(((BodyBankNumber << 16) | address) + 2);
        }
        return address;
    }

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
        for (int guard = 0; guard < 100000; guard++)
        {
            foreach (BodyControl control in lists[list].Controls)
            {
                if (control.AfterBeat > runStart)
                    phases.Add(new(list, runStart, control.AfterBeat - runStart, plays));
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
                        ushort target = control.Target!.Value;
                        if (entered.TryGetValue(target, out int loop))
                            return (phases, AnimationEnd.LoopsFrom(loop));
                        entered[target] = phases.Count;
                        list = target;
                        runStart = 0;
                        plays = pending ?? 1;
                        pending = null;
                        break;
                    case NativeEnemyControlKind.LoopStart:
                        throw new InvalidDataException("The Torizo list reader reads no goto back into a list's own beats (#2144).");
                    default:
                        throw new InvalidDataException($"Undefined native control {control.Kind}.");
                }
                break;
            }
        }
        throw new InvalidDataException($"The native Torizo lists from ${start:X4} never end or loop.");
    }

    /// <summary>
    /// The cartridge's AI resumes a sleeping list by stepping its pointer past the sleep; the animation each wake starts by
    /// name, the awakening of the dormant statue, must be the one that starts there.
    /// </summary>
    private static void VerifyResumedSleeps(IImportCartridgeSource cartridge, Dictionary<ushort, BodyList> lists)
    {
        var wakes = new Dictionary<TorizoAnimation, TorizoAnimation>
        {
            [TorizoAnimation.BombTorizoDormant] = TorizoAnimation.BombTorizoAwakening,
            [TorizoAnimation.GoldenTorizoDormant] = TorizoAnimation.GoldenTorizoAwakening,
        };
        int resumed = 0;
        foreach ((TorizoAnimation animation, ushort entry) in AnimationEntries)
        {
            if (lists[entry].Controls.LastOrDefault() is not { Kind: NativeEnemyControlKind.Sleep })
                continue;
            ushort? sleep = SleepAddress(cartridge, entry);
            if (sleep is not { } address || AnimationAt(unchecked((ushort)(address + 2))) is not { } next)
                continue;
            AssertEqual(next, wakes[animation], $"Torizo {animation} resumes past its sleep into {next}");
            resumed++;
        }
        AssertEqual(2, resumed, "Torizo resumes its two sleeping animations");
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
                    if (Returns(word))
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

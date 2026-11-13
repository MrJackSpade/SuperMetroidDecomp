using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Frontend.Intro.CinematicSpriteNativeListAddresses;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// #1290, #1600: every typed cinematic sprite segment and animation against the retail bank-$8B lists. Each segment must
/// match its native list beat by beat (durations, poses, pre-instructions and owner cues), each animation the lists'
/// counters, branches, runs on and ends, and each animation must then play through the real
/// <see cref="IntroDiscoverySprite"/> exactly as an independent reader of the native list does, update by update.
/// </summary>
internal static class CinematicSpriteAnimationsVerification
{
    /// <summary>Updates each program is played for, enough for every list's loops and deletions.</summary>
    private const int PlaybackUpdates = 1200;

    /// <summary>The shared cinematic sprite instructions, by what they do.</summary>
    private enum SharedWord
    {
        /// <summary>The deletion.</summary>
        Delete,
        /// <summary>The sleep.</summary>
        Sleep,
        /// <summary>The pre-instruction install and its routine operand.</summary>
        SetFrameBehavior,
        /// <summary>The goto and its target operand.</summary>
        GoTo,
        /// <summary>The timer countdown and its target operand.</summary>
        RepeatWhileCounting,
        /// <summary>The timer load and its count operand.</summary>
        SetCounter,
    }

    /// <summary>The shared instructions' native words.</summary>
    private static readonly Dictionary<ushort, SharedWord> SharedWords = new()
    {
        [(ushort)CinematicSpriteInstruction.Delete] = SharedWord.Delete,
        [(ushort)CinematicSpriteInstruction.Sleep] = SharedWord.Sleep,
        [(ushort)CinematicSpriteInstruction.SetFrameBehavior] = SharedWord.SetFrameBehavior,
        [(ushort)CinematicSpriteInstruction.Goto] = SharedWord.GoTo,
        [(ushort)CinematicSpriteInstruction.DecrementTimerAndGoto] = SharedWord.RepeatWhileCounting,
        [(ushort)CinematicSpriteInstruction.SetTimer] = SharedWord.SetCounter,
    };

    /// <summary>
    /// Every segment matches its native list, every animation the lists' control flow and the lists' layout, and every
    /// animation plays as its list does.
    /// </summary>
    internal static void VerifyCinematicSpriteAnimations(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<CinematicSpriteSegment>().Length, Entries.Length, "every cinematic sprite segment has a native entry");
        for (int index = 0; index < Entries.Length; index++)
            AssertEqual(Enum.GetValues<CinematicSpriteSegment>()[index], Entries[index].Program, "the cinematic sprite segments are declared in authored order");
        AssertTrue(Entries.Zip(Entries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends), "native cinematic sprite entries ascend");

        Dictionary<CinematicSpriteSegment, NativeEnemyList<CinematicSpriteSegment>> lists =
            Entries.ToDictionary(item => item.Program, item => ReadList(cartridge, item.Program, item.Entry));
        NativeEnemyAnimationOracle.CompareAnimations(lists, CinematicSpriteAnimations.StartingAt);
        HashSet<CinematicSpriteSegment> runOn = NativeEnemyAnimationOracle.RunsOnIntoNextAfterLastBeat(lists);
        AssertTrue(runOn.SetEquals(CinematicSpriteAnimations.RunsOnInto.Keys),
            "the lists that run on into the next authored list, directly after their last beat, are the typed run-ons");
        foreach (CinematicSpriteSegment segment in runOn)
        {
            AssertEqual(lists[segment].Controls.Single(control => control.Kind == NativeEnemyControlKind.RunOnIntoNext).Target,
                (CinematicSpriteSegment?)CinematicSpriteAnimations.RunsOnInto[segment], $"{segment} runs on into the list authored next");
        }
        int beats = lists.Values.Sum(list => list.BeatAddresses.Length), updates = 0;
        foreach (CinematicSpriteNativeListAddressesEntry item in Entries.Where(item => CinematicSpriteAnimations.StartingAt.ContainsKey(item.Program)))
            updates += ComparePlayback(cartridge, item.Program, item.Entry);
        Console.WriteLine($"  cinematic sprite animations: {Entries.Length} segments ({beats} beats, {updates} played updates) match bank $8B.");
    }

    /// <summary>The program whose native list starts at <paramref name="address"/>, if one does.</summary>
    private static CinematicSpriteSegment? ProgramAt(ushort address) =>
        Entries.Any(item => item.Entry == address) ? Entries.Single(item => item.Entry == address).Program : null;

    /// <summary>Reads one native list word.</summary>
    private static ushort Word(IImportCartridgeSource cartridge, ushort address) =>
        cartridge.ReadCartridgeWord((int)new SnesAddress(Bank, address));

    /// <summary>
    /// Reads one segment's native list, comparing its beats with the segment's: each beat's native address, and each
    /// control instruction with its address and the beat it follows.
    /// </summary>
    private static NativeEnemyList<CinematicSpriteSegment> ReadList(IImportCartridgeSource cartridge, CinematicSpriteSegment program, ushort entry)
    {
        IReadOnlyList<AnimationBeat<CinematicSpritePose, CinematicSpriteCue>> beats = CinematicSpriteAnimations.Segments.Beats(program);
        var addresses = new List<ushort>();
        var controls = new List<NativeEnemyControl<CinematicSpriteSegment>>();
        ushort cursor = entry;
        AnimationBeat<CinematicSpritePose, CinematicSpriteCue> Next(string what)
        {
            AssertTrue(addresses.Count < beats.Count, $"{program} has a beat for native {what} at $8B:{cursor:X4}");
            addresses.Add(cursor);
            return beats[addresses.Count - 1];
        }
        while (true)
        {
            string at = $"{program} at $8B:{cursor:X4}";
            if (cursor != entry && ProgramAt(cursor) is { } following)
            {
                controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, addresses.Count, following, null, cursor));
                break;
            }
            ushort word = Word(cartridge, cursor);
            ushort operand = Word(cartridge, unchecked((ushort)(cursor + 2)));
            if ((word & CinematicCodePointers.InstructionCommandBit) == 0)
            {
                var frame = AssertIs<AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Frame>(Next("frame"), $"{at}: frame");
                AssertEqual(word, frame.Duration, $"{at}: duration");
                AssertEqual(operand, CinematicSpritePoseSpritemaps.Of(frame.Shown), $"{at}: spritemap");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            if (OwnerInstructions.TryGetValue(word, out CinematicSpriteCueKind kind))
            {
                var raised = AssertIs<AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Event>(Next("owner instruction"), $"{at}: owner instruction");
                AssertEqual(new CinematicSpriteCue(kind), raised.Raised, $"{at}: owner cue");
                cursor = unchecked((ushort)(cursor + 2));
                continue;
            }
            AssertTrue(SharedWords.TryGetValue(word, out SharedWord shared), $"{at}: native word ${word:X4} is known");
            switch (shared)
            {
                case SharedWord.Delete:
                    controls.Add(new(NativeEnemyControlKind.Stop, addresses.Count, null, null, cursor));
                    break;
                case SharedWord.Sleep:
                    controls.Add(new(NativeEnemyControlKind.Sleep, addresses.Count, null, null, cursor));
                    break;
                case SharedWord.GoTo:
                    controls.Add(new(NativeEnemyControlKind.Goto, addresses.Count, SegmentAt(operand, at), null, cursor));
                    break;
                case SharedWord.SetFrameBehavior:
                    var install = AssertIs<AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Event>(Next(shared.ToString()), $"{at}: pre-instruction install");
                    AssertEqual(CinematicSpriteCueKind.SetFrameBehavior, install.Raised.Kind, $"{at}: pre-instruction cue");
                    AssertEqual(operand, install.Raised.FrameBehavior.NativeAddress, $"{at}: pre-instruction");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case SharedWord.SetCounter:
                    controls.Add(new(NativeEnemyControlKind.SetCounter, addresses.Count, null, operand, cursor));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case SharedWord.RepeatWhileCounting:
                    CinematicSpriteSegment repeated = SegmentAt(operand, at);
                    AssertEqual(program, repeated, $"{at}: a counted repeat replays its own list");
                    controls.Add(new(NativeEnemyControlKind.Repeat, addresses.Count, repeated, null, cursor));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(SharedWord)} {shared}.");
            }
            break;
        }
        AssertEqual(beats.Count, addresses.Count, $"{program}: its native list holds each of its beats");
        return new NativeEnemyList<CinematicSpriteSegment>([.. addresses], [.. controls]);
    }

    /// <summary>The segment whose native list starts at <paramref name="address"/>, failing when none does.</summary>
    private static CinematicSpriteSegment SegmentAt(ushort address, string at) =>
        ProgramAt(address) ?? throw new InvalidDataException($"{at}: ${address:X4} is no segment's native entry.");

    /// <summary>
    /// Plays the animation of <paramref name="program"/> through the real actor beside an independent reader of its native list, comparing
    /// liveness, the shown spritemap, the pre-instruction, the loop timer and each update's owner cues; returns the updates.
    /// </summary>
    private static int ComparePlayback(IImportCartridgeSource cartridge, CinematicSpriteSegment program, ushort entry)
    {
        var actor = new IntroDiscoverySprite(default, IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors), program);
        var native = new NativeReader(cartridge, entry);
        var cues = new List<CinematicSpriteCueKind>();
        for (int update = 0; update < PlaybackUpdates; update++)
        {
            cues.Clear();
            actor.Step(cues.Add);
            native.Step();
            string at = $"{program} update {update}";
            AssertEqual(native.Active, actor.IsActive, $"{at}: liveness");
            if (!native.Active)
                return update + 1;
            AssertEqual(native.Spritemap, CinematicSpritePoseSpritemaps.Of(actor.Pose), $"{at}: spritemap");
            AssertEqual(native.FrameBehavior, actor.FrameBehaviorPointer, $"{at}: pre-instruction");
            AssertEqual(native.Timer.RawValue, actor.LoopCountdown.RemainingFrames, $"{at}: loop timer");
            AssertTrue(native.Cues.SequenceEqual(cues), $"{at}: owner cues");
        }
        return PlaybackUpdates;
    }

    /// <summary>An independent reader of a native bank-$8B list with the generic cinematic sprite handler's timer gate.</summary>
    private sealed class NativeReader(IImportCartridgeSource cartridge, ushort cursor)
    {
        /// <summary>The next native word.</summary>
        private ushort cursor = cursor;
        /// <summary>Updates before the next word is read.</summary>
        private WrappingCountdown frameTimer = WrappingCountdown.Start(1);
        /// <summary>Whether the list has deleted the actor.</summary>
        internal bool Active { get; private set; } = true;
        /// <summary>The latest frame's spritemap.</summary>
        internal ushort Spritemap { get; private set; }
        /// <summary>The installed pre-instruction.</summary>
        internal ushort FrameBehavior { get; private set; }
        /// <summary>The goto timer word.</summary>
        internal WrappingCountdown Timer { get; private set; }
        /// <summary>The owner instructions the latest update ran.</summary>
        internal List<CinematicSpriteCueKind> Cues { get; } = [];

        /// <summary>Runs one generic handler call.</summary>
        internal void Step()
        {
            Cues.Clear();
            if (!Active)
                return;
            frameTimer = frameTimer.Decrement().Remaining;
            if (!frameTimer.IsZero)
                return;
            for (int command = 0; command < 32; command++)
            {
                ushort word = Word(cartridge, cursor);
                ushort operand = Word(cartridge, unchecked((ushort)(cursor + 2)));
                if ((word & CinematicCodePointers.InstructionCommandBit) == 0)
                {
                    frameTimer = WrappingCountdown.Start(word);
                    Spritemap = operand;
                    cursor = unchecked((ushort)(cursor + 4));
                    return;
                }
                if (OwnerInstructions.TryGetValue(word, out CinematicSpriteCueKind kind))
                {
                    Cues.Add(kind);
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                }
                switch (SharedWords[word])
                {
                    case SharedWord.Delete:
                        Active = false;
                        Spritemap = 0;
                        return;
                    case SharedWord.Sleep:
                        return;
                    case SharedWord.SetFrameBehavior:
                        FrameBehavior = operand;
                        cursor = unchecked((ushort)(cursor + 4));
                        continue;
                    case SharedWord.GoTo:
                        cursor = operand;
                        continue;
                    case SharedWord.SetCounter:
                        Timer = WrappingCountdown.Start(operand);
                        cursor = unchecked((ushort)(cursor + 4));
                        continue;
                    case SharedWord.RepeatWhileCounting:
                        Timer = Timer.Decrement().Remaining;
                        cursor = !Timer.IsZero ? operand : unchecked((ushort)(cursor + 4));
                        continue;
                    default:
                        throw new InvalidOperationException($"Undefined {nameof(SharedWord)} {SharedWords[word]}.");
                }
            }
            throw new InvalidDataException("A native cinematic sprite list ran thirty-two commands without a frame.");
        }
    }
}

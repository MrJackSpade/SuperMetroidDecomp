using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Animals.EscapeDachoraNativeListAddresses;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>
/// #1290, #1600, #2061: the escaping Dachora's typed animations, each segment matched to its native bank-$B3 list, each
/// animation to the lists' control flow from where it starts, and each branch cue's continuation to the native place its
/// target names.
/// </summary>
internal static class EscapeDachoraAnimationsVerification
{
    /// <summary>
    /// The laps the Dachora's spawn starts, each at the native entry that loads its count (#2061): the low tide's and the
    /// high tide's, which the high-tide branches continue in.
    /// </summary>
    private static IReadOnlyList<NativeCountedEntry<EscapeDachoraSegment>> CountedLaps { get; } =
    [
        new NativeCountedEntry<EscapeDachoraSegment>("the low-tide laps", EscapeDachoraAnimations.RunningAroundLowTide, LowTideLapsStart),
        new NativeCountedEntry<EscapeDachoraSegment>("the high-tide laps", EscapeDachoraAnimations.RunningAroundHighTide, HighTideLapsStart),
    ];

    /// <summary>
    /// How the Dachora's lists are read: each owner instruction becomes its cue, and a lap end's native target is read back
    /// below against where its owner takes the play (#2167).
    /// </summary>
    private static NativeEnemyCueRules<EscapeDachoraCue> Rules(List<(ushort Target, AnimationContinuation<EscapeDachoraSegment> Then, string At)> branches) => new(
        word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
        (word, operands, cue, at) =>
        {
            var instruction = Instructions[word];
            AssertEqual(instruction.Operands.Length, operands.Length, $"{at}: operands");
            if (instruction.Move is { } move)
            {
                AssertEqual<EscapeDachoraCue>(move, cue, $"{at}: cue");
                return;
            }
            AssertEqual(instruction.Branch, (Type?)cue.GetType(), $"{at}: cue");
            AnimationContinuation<EscapeDachoraSegment> then = cue switch
            {
                EscapeDachoraCue.LowTideLapEnds lowTide => EscapeAnimalsBehavior.RisingTideTakesOver(lowTide.Lap),
                EscapeDachoraCue.LapEnds lap => EscapeAnimalsBehavior.EscapeTakesOver(lap.Lap),
                _ => throw new InvalidOperationException($"{at}: {cue} ends no lap."),
            };
            for (int index = 0; index < operands.Length; index++)
            {
                if (instruction.Operands[index] != 'T')
                    throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}.");
                branches.Add((operands[index], then, at));
            }
        });

    /// <summary>
    /// Every segment matches its native list, every animation its lists' control flow, and every branch cue's continuation
    /// stands where its native target points: the start of the run for the escape, or the end of a counted lap of the high
    /// tide, counting on the plays left (#2061).
    /// </summary>
    internal static void VerifyEscapeDachoraAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var branches = new List<(ushort Target, AnimationContinuation<EscapeDachoraSegment> Then, string At)>();
        int beats = NativeEnemyAnimationOracle.CompareAll(cartridge, Bank, Entries,
            EscapeDachoraAnimations.Segments, EscapeDachoraAnimationsConstants.StartingAt, EscapeDachoraFrameToolingDefinitions.Spritemap,
            Rules(branches), countedEntries: CountedLaps);
        Dictionary<EscapeDachoraSegment, NativeEnemyList<EscapeDachoraSegment>> lists = NativeEnemyAnimationOracle.Lists(cartridge, Bank, Entries,
            EscapeDachoraAnimations.Segments, EscapeDachoraFrameToolingDefinitions.Spritemap, Rules([]));
        IReadOnlyList<NativeCountedStart<EscapeDachoraSegment>> counted = NativeEnemyAnimationOracle.CountedStarts(cartridge, Bank, Entries, CountedLaps, Rules([]));
        AssertEqual(6, branches.Count, "every branch cue of the laps is read");
        foreach ((ushort target, AnimationContinuation<EscapeDachoraSegment> then, string at) in branches)
        {
            AssertEqual(target, NativeEnemyAnimationOracle.PointerOf(lists, Entries, EscapeDachoraAnimationsConstants.StartingAt,
                new AnimationPlayback<EscapeDachoraSegment>(then.Animation, then.Place), counted), $"{at}: the branch continues where its native target points");
            bool countsDown = !then.Place.HasNotEntered
                && then.Animation.Phases[then.Place.Phase] is { IsRepeated: true } phase && then.Place.Beat == phase.BeatCount;
            AssertEqual(countsDown, then.CountsOn, $"{at}: a branch into a lap's count-down counts on the plays left, and only it does");
        }
        Console.WriteLine($"  escape Dachora animations: {Entries.Length} segments ({beats} beats), {EscapeDachoraAnimationsConstants.StartingAt.Count + CountedLaps.Count} animations and {branches.Count} branches match bank $B3.");
    }

    /// <summary>
    /// #2006: the acid rising during the low-tide laps branches into the high-tide laps counting on the laps the low-tide
    /// run had left, as the cartridge's count-down tail counts down the loop counter the low-tide laps loaded: rising
    /// during lap <c>k</c> of five leaves five minus <c>k</c> more high-tide laps to the left before the laps to the right.
    /// </summary>
    internal static void VerifyEscapeDachoraHighTideCountsOnLaps()
    {
        for (int risesDuringLap = 1; risesDuringLap <= 5; risesDuringLap++)
        {
            Animation<EscapeDachoraSegment> animation = EscapeDachoraAnimations.RunningAroundLowTide;
            AnimationPlace place = AnimationPlace.StartOfAnimation;
            int lowTideLaps = 0;
            int highTideLeftFrames = 0;
            bool reachedRightLaps = false;
            for (int step = 0; step < 1000 && !reachedRightLaps; step++)
            {
                AnimationShown<EscapeDachoraSegment, EnemyFrameId> shown = EscapeDachoraAnimations.Segments.PlayToNextFrame(ref animation, ref place,
                    cue =>
                    {
                        if (cue is not EscapeDachoraCue.LowTideLapEnds lowTide)
                            return null;
                        lowTideLaps++;
                        return lowTideLaps == risesDuringLap ? EscapeAnimalsBehavior.RisingTideTakesOver(lowTide.Lap) : null;
                    });
                AnimationShown<EscapeDachoraSegment, EnemyFrameId>.Frame frame = AssertIs<AnimationShown<EscapeDachoraSegment, EnemyFrameId>.Frame>(shown,
                    $"rising during lap {risesDuringLap}, step {step} shows a frame");
                if (frame.Segment == EscapeDachoraSegment.RunningAroundHighTideLeft)
                    highTideLeftFrames++;
                reachedRightLaps = frame.Segment == EscapeDachoraSegment.RunningAroundHighTideRight;
            }
            AssertTrue(reachedRightLaps, $"rising during lap {risesDuringLap} reaches the high-tide laps to the right");
            AssertEqual(risesDuringLap, lowTideLaps, $"rising during lap {risesDuringLap} leaves the low-tide laps then");
            int lapFrames = EscapeDachoraAnimations.Segments.Beats(EscapeDachoraSegment.RunningAroundHighTideLeft)
                .Count(beat => beat is AnimationBeat<EnemyFrameId, EscapeDachoraCue>.Frame);
            AssertEqual(5 - risesDuringLap, highTideLeftFrames / lapFrames, $"rising during lap {risesDuringLap} counts on the laps left");
            AssertEqual(0, highTideLeftFrames % lapFrames, $"rising during lap {risesDuringLap} runs whole high-tide laps");
        }
        Console.WriteLine("  escape Dachora: a rising tide's high-tide laps count on the laps the low-tide run had left.");
    }
}

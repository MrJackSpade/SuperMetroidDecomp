using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Animals.EscapeDachoraNativeListAddresses;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>
/// #1290, #1600: the escaping Dachora's typed animations, each segment matched to its native bank-$B3 list, each
/// animation to the lists' control flow, and each branch into a lap's count-down to the end of a counted lap.
/// </summary>
internal static class EscapeDachoraAnimationsVerification
{
    /// <summary>Every segment matches its native list, every animation its lists' control flow, and every branch its target.</summary>
    internal static void VerifyEscapeDachoraAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            EscapeDachoraAnimations.Segments, EscapeDachoraAnimations.StartingAt, EscapeDachoraFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<EscapeDachoraCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    for (int index = 0; index < operands.Length; index++)
                        AssertEqual(operands[index], instruction.Operands[index] switch
                    {
                        'T' => NativeEnemyAnimationOracle.EntryOf(Entries, cue.Target),
                        _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                    }, $"{at}: operand {index}");
                }),
            EscapeDachoraAnimations.BranchTargets);
        foreach (EscapeDachoraSegment target in EscapeDachoraAnimations.Segments.Beats(EscapeDachoraSegment.RunningAroundLowTideLeft)
            .Concat(EscapeDachoraAnimations.Segments.Beats(EscapeDachoraSegment.RunningAroundLowTideRight))
            .Concat(EscapeDachoraAnimations.Segments.Beats(EscapeDachoraSegment.RunningAroundHighTideLeft))
            .Concat(EscapeDachoraAnimations.Segments.Beats(EscapeDachoraSegment.RunningAroundHighTideRight))
            .OfType<AnimationBeat<EnemyFrameId, EscapeDachoraCue>.Event>()
            .Where(raised => raised.Raised.Kind is EscapeDachoraCueKind.GoToIfAcidHigh or EscapeDachoraCueKind.GoToIfCrittersEscaped)
            .Select(raised => raised.Raised.Target))
        {
            AssertTrue(EscapeDachoraAnimations.BranchTargets.ContainsKey(target), $"branch target {target} continues the play");
            if (EscapeDachoraAnimations.StartingAt.TryGetValue(target, out var started))
                AssertEqual(new AnimationContinuation<EscapeDachoraSegment>(started, AnimationPlace.StartOfAnimation), EscapeDachoraAnimations.BranchTargets[target],
                    $"a branch to {target} starts its animation");
        }
        Console.WriteLine($"  escape Dachora animations: {Entries.Length} segments ({beats} beats) and {EscapeDachoraAnimations.StartingAt.Count} animations match bank $B3.");
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
            Animation<EscapeDachoraSegment> animation = EscapeDachoraAnimations.StartingWith(EscapeDachoraSegment.RunningAroundLowTide);
            AnimationPlace place = AnimationPlace.StartOfAnimation;
            int lowTideLaps = 0;
            int highTideLeftFrames = 0;
            bool reachedRightLaps = false;
            for (int step = 0; step < 1000 && !reachedRightLaps; step++)
            {
                AnimationShown<EscapeDachoraSegment, EnemyFrameId> shown = EscapeDachoraAnimations.Segments.PlayToNextFrame(ref animation, ref place,
                    cue =>
                    {
                        if (cue.Kind != EscapeDachoraCueKind.GoToIfAcidHigh)
                            return null;
                        lowTideLaps++;
                        return lowTideLaps == risesDuringLap ? EscapeDachoraAnimations.BranchTargets[cue.Target] : null;
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

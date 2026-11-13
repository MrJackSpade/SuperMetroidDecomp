using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Animals.EscapeDachoraNativeListAddresses;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Programs;

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
        foreach (EscapeDachoraProgram target in EscapeDachoraAnimations.Segments.Beats(EscapeDachoraProgram.RunningAroundLowTideLeft)
            .Concat(EscapeDachoraAnimations.Segments.Beats(EscapeDachoraProgram.RunningAroundLowTideRight))
            .Concat(EscapeDachoraAnimations.Segments.Beats(EscapeDachoraProgram.RunningAroundHighTideLeft))
            .Concat(EscapeDachoraAnimations.Segments.Beats(EscapeDachoraProgram.RunningAroundHighTideRight))
            .OfType<AnimationBeat<EscapeDachoraFrame, EscapeDachoraCue>.Event>()
            .Where(raised => raised.Raised.Kind is EscapeDachoraCueKind.GoToIfAcidHigh or EscapeDachoraCueKind.GoToIfCrittersEscaped)
            .Select(raised => raised.Raised.Target))
        {
            AssertTrue(EscapeDachoraAnimations.BranchTargets.ContainsKey(target), $"branch target {target} continues the play");
            if (EscapeDachoraAnimations.StartingAt.TryGetValue(target, out var started))
                AssertEqual(new AnimationContinuation<EscapeDachoraProgram>(started, AnimationPlace.StartOfAnimation), EscapeDachoraAnimations.BranchTargets[target],
                    $"a branch to {target} starts its animation");
        }
        Console.WriteLine($"  escape Dachora animations: {Entries.Length} segments ({beats} beats) and {EscapeDachoraAnimations.StartingAt.Count} animations match bank $B3.");
    }
}

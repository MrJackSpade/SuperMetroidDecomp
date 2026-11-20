using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.MamaTurtles.MamaTurtleNativeListAddresses;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Enemies.MamaTurtles.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.MamaTurtles;

/// <summary>#641, #1600: the turtles' typed animations, each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class MamaTurtleAnimationsVerification
{
    /// <summary>
    /// Every segment of the Mama Turtle's and the baby turtles' animations matches its native list, and every animation its
    /// lists' control flow (#2342: each owner has its own segments and cues).
    /// </summary>
    internal static void VerifyMamaTurtleAnimations(CartridgeImportAddressSpace rom)
    {
        int mamaBeats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            MamaTurtleAnimations.Segments, MamaTurtleAnimations.StartingAt, MamaTurtleFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<MamaTurtleCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Cue, cue, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                },
                static _ => false));
        int babyBeats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, BabyEntries,
            BabyTurtleAnimations.Segments, BabyTurtleAnimations.StartingAt, MamaTurtleFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<BabyTurtleCue>(
                word => BabyInstructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = BabyInstructions[word];
                    AssertEqual(instruction.Cue, cue, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                },
                cue => cue.Kind == BabyTurtleCueKind.LoopOrTurnAround),
            // A crawl strides on with its legs trailing only after its counted strides (#2439).
            onlyRunOnInto: new HashSet<BabyTurtleSegment> { BabyTurtleSegment.CrawlingLeftLegsTrailing, BabyTurtleSegment.CrawlingRightLegsTrailing });
        Console.WriteLine($"  Mama Turtle animations: {Entries.Length} segments ({mamaBeats} beats) and {MamaTurtleAnimations.StartingAt.Count} animations, " +
            $"baby turtle animations: {BabyEntries.Length} segments ({babyBeats} beats) and {BabyTurtleAnimations.StartingAt.Count} animations match bank $A2.");
    }
}

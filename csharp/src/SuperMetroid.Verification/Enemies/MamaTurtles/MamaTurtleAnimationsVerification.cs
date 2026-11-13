using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.MamaTurtles.MamaTurtleNativeListAddresses;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Enemies.MamaTurtles.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.MamaTurtles;

/// <summary>#641, #1600: the turtles' typed animations, each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class MamaTurtleAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyMamaTurtleAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            MamaTurtleAnimations.Segments, MamaTurtleAnimations.StartingAt, MamaTurtleFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<MamaTurtleCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                },
                cue => cue.Kind is MamaTurtleCueKind.LoopOrTurnAround));
        Console.WriteLine($"  Mama Turtle animations: {Entries.Length} segments ({beats} beats) and {MamaTurtleAnimations.StartingAt.Count} animations match bank $A2.");
    }
}

using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Choot.ChootNativeListAddresses;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Choot.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Choot;

/// <summary>#641, #1600: Choot's typed animations, each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class ChootAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyChootAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            ChootAnimations.Segments, ChootAnimations.StartingAt, ChootFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<ChootCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Choot animations: {Entries.Length} segments ({beats} beats) and {ChootAnimations.StartingAt.Count} animations match bank $A2.");
    }
}

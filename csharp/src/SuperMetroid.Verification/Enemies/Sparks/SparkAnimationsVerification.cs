using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Sparks.SparkNativeListAddresses;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.Sparks.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Sparks;

/// <summary>#641: the Wrecked Ship Spark's typed animations (#1600), each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class SparkAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifySparkAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            SparkAnimations.Segments, SparkAnimations.StartingAt, SparkFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<SparkCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Spark animations: {Entries.Length} segments ({beats} beats) and {SparkAnimations.StartingAt.Count} animations match bank $A8.");
    }
}

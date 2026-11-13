using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Platforms.PlatformNativeListAddresses;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Platforms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Platforms;

/// <summary>#641: the Tripper's and Kamer's typed animations (#1600), each segment matched to its native bank-$A3 list and each animation to the lists' control flow.</summary>
internal static class PlatformAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyPlatformAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            PlatformAnimations.Segments, PlatformAnimations.StartingAt, PlatformFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<PlatformCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Tripper or Kamer animations: {Entries.Length} segments ({beats} beats) and {PlatformAnimations.StartingAt.Count} animations match bank $A3.");
    }
}

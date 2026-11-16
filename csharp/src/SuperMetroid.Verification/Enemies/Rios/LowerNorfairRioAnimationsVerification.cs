using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Rios.LowerNorfairRioNativeListAddresses;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>#641: the Lower Norfair Rio's typed animations (#1600), each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class LowerNorfairRioAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyLowerNorfairRioAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            LowerNorfairRioAnimations.Segments, LowerNorfairRioAnimations.StartingAt, LowerNorfairRioFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<LowerNorfairRioCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Lower Norfair Rio animations: {Entries.Length} segments ({beats} beats) and {LowerNorfairRioAnimations.StartingAt.Count} animations match bank $A2.");
    }
}

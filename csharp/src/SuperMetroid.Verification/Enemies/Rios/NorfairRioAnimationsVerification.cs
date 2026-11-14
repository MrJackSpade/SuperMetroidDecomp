using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Rios.NorfairRioNativeListAddresses;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>#641: the Norfair Rio's typed animations (#1600), each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class NorfairRioAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyNorfairRioAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            NorfairRioAnimations.Segments, NorfairRioAnimations.StartingAt, NorfairRioFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NorfairRioCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Cue, cue, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Norfair Rio animations: {Entries.Length} segments ({beats} beats) and {NorfairRioAnimations.StartingAt.Count} animations match bank $A2.");
    }
}

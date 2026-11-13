using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.KiHunters.KiHunterNativeListAddresses;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>#641, #1600: the Ki-Hunter's typed animations, each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class KiHunterAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyKiHunterAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            KiHunterAnimations.Segments, KiHunterAnimations.StartingAt, KiHunterFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<KiHunterCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Ki-Hunter animations: {Entries.Length} segments ({beats} beats) and {KiHunterAnimations.StartingAt.Count} animations match bank $A8.");
    }
}

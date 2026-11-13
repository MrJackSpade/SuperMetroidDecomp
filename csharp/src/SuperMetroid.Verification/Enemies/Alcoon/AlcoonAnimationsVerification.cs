using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Alcoon.AlcoonNativeListAddresses;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Alcoon;

/// <summary>#641, #1600: the Alcoon's typed animations, each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class AlcoonAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyAlcoonAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            AlcoonAnimations.Segments, AlcoonAnimations.StartingAt, AlcoonFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<AlcoonCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Alcoon animations: {Entries.Length} segments ({beats} beats) and {AlcoonAnimations.StartingAt.Count} animations match bank $A8.");
    }
}

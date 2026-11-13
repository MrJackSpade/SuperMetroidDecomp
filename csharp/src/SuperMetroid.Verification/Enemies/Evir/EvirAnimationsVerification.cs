using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Evir.EvirNativeListAddresses;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.Evir.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Evir;

/// <summary>#641, #1600: the Evir's typed animations, each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class EvirAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyEvirAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            EvirAnimations.Segments, EvirAnimations.StartingAt, EvirFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<EvirCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Evir animations: {Entries.Length} segments ({beats} beats) and {EvirAnimations.StartingAt.Count} animations match bank $A8.");
    }
}

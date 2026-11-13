using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Beetom.BeetomNativeListAddresses;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Beetom.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Beetom;

/// <summary>#641, #1600: the Beetom's typed animations, each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class BeetomAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyBeetomAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            BeetomAnimations.Segments, BeetomAnimations.StartingAt, BeetomFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<BeetomCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Beetom animations: {Entries.Length} segments ({beats} beats) and {BeetomAnimations.StartingAt.Count} animations match bank $A8.");
    }
}

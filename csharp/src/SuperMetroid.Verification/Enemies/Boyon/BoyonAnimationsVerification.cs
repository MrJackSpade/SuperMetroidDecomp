using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Boyon.BoyonNativeListAddresses;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Boyon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Boyon;

/// <summary>#641, #1600: Boyon's typed animations, each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class BoyonAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyBoyonAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            BoyonAnimations.Segments, BoyonAnimations.StartingAt, BoyonFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<BoyonCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : OmittedInstructions.Contains(word) ? 0 : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                },
                Omitted: OmittedInstructions.Contains));
        Console.WriteLine($"  Boyon animations: {Entries.Length} segments ({beats} beats) and {BoyonAnimations.StartingAt.Count} animations match bank $A2.");
    }
}

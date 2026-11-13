using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Skree.SkreeMetareeNativeListAddresses;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Skree.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>
/// #641, #1600: the Skree's and Metaree's typed animations, each segment matched to its native bank-$A3 list and each
/// animation to the lists' control flow.
/// </summary>
internal static class SkreeMetareeAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifySkreeMetareeAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            SkreeMetareeAnimations.Segments, SkreeMetareeAnimations.StartingAt, SkreeMetareeFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<SkreeMetareeCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Skree or Metaree animations: {Entries.Length} segments ({beats} beats) and {SkreeMetareeAnimations.StartingAt.Count} animations match bank $A3.");
    }
}

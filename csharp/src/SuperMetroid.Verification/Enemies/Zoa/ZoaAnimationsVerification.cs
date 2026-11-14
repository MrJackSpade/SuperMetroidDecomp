using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Zoa.ZoaNativeListAddresses;
using SuperMetroid.Core.Enemies.Zoa;
using SuperMetroid.Core.Enemies.Zoa.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Zoa;

/// <summary>#641: Zoa's typed animations (#1600), each segment matched to its native bank-$A3 list and each animation to the lists' control flow.</summary>
internal static class ZoaAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyZoaAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            ZoaAnimations.Segments, ZoaAnimations.StartingAt, ZoaFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<ZoaCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Cue, cue, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Zoa animations: {Entries.Length} segments ({beats} beats) and {ZoaAnimations.StartingAt.Count} animations match bank $A3.");
    }
}

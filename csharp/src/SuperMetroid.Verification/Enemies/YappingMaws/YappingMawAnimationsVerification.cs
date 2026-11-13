using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.YappingMaws.YappingMawNativeListAddresses;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>#641: the Yapping Maw's typed animations (#1600), each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class YappingMawAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyYappingMawAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            YappingMawAnimations.Segments, YappingMawAnimations.StartingAt, YappingMawFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<YappingMawCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Yapping Maw animations: {Entries.Length} segments ({beats} beats) and {YappingMawAnimations.StartingAt.Count} animations match bank $A8.");
    }
}

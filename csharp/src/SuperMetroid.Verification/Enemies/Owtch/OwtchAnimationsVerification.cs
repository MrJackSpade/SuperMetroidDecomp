using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Owtch.OwtchNativeListAddresses;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.Owtch.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Owtch;

/// <summary>#641: Owtch's typed animations (#1600), each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class OwtchAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyOwtchAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            OwtchAnimations.Segments, OwtchAnimations.StartingAt, OwtchFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<OwtchCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Owtch animations: {Entries.Length} segments ({beats} beats) and {OwtchAnimations.StartingAt.Count} animations match bank $A2.");
    }
}

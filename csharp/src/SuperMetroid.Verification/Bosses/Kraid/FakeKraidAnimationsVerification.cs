using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Bosses.Kraid.FakeKraidNativeListAddresses;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>#641: Mini-Kraid's typed programs, each matched step by step to its native bank-$A6 list.</summary>
internal static class FakeKraidAnimationsVerification
{
    /// <summary>Every segment matches its native list and every animation the lists' control flow.</summary>
    internal static void VerifyFakeKraidAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, FakeKraidAnimations.Segments, FakeKraidAnimations.StartingAt,
            FakeKraidFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<FakeKraidCue>(word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null, (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            }));
        Console.WriteLine($"  Mini-Kraid animations: {Entries.Length} segments ({beats} beats) match bank $A6.");
    }
}

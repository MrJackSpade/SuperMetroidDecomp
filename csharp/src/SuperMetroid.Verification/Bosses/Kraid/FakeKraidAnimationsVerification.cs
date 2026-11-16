using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Bosses.Kraid.FakeKraidNativeListAddresses;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>#641: Fake Kraid's typed programs, each matched step by step to its native bank-$A6 list.</summary>
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
                AssertEqual(instruction.Cue, cue, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            },
            // #2214: the choice always continues at the step or spit it picks, so the step loop each choice list runs on
            // into never plays.
            EndsList: cue => cue is FakeKraidCue.Signal { Kind: FakeKraidCueKind.ChooseAction }),
            onlyRunOnInto: new HashSet<FakeKraidSegment> { FakeKraidSegment.ChooseAction });
        Console.WriteLine($"  Fake Kraid animations: {Entries.Length} native lists ({beats} beats) match bank $A6.");
    }
}

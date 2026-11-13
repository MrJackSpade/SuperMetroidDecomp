using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Bosses.MotherBrain.RinkaNativeListAddresses;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>#641, #1600: the Rinkas' typed animations, each segment matched beat by beat to its native bank-$A2 list.</summary>
internal static class RinkaAnimationsVerification
{
    /// <summary>Every segment matches its native list and every animation the lists' control flow.</summary>
    internal static void VerifyRinkaAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, RinkaAnimations.Segments,
            RinkaAnimations.StartingAt, RinkaFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<RinkaCue>(word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Rinka animations: {Entries.Length} segments ({beats} beats) match bank $A2.");
    }
}

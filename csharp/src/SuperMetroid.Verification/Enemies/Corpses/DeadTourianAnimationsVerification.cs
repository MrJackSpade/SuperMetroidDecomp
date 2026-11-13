using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Corpses.DeadTourianNativeListAddresses;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>#641, #1600: the dead Sidehopper's and Tourian corpses' typed animations, each segment matched to its native bank-$A9 list and each animation to the lists' control flow.</summary>
internal static class DeadTourianAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyDeadTourianAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            DeadTourianAnimations.Segments, DeadTourianAnimations.StartingAt, DeadTourianFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<DeadTourianCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  dead Tourian enemy animations: {Entries.Length} segments ({beats} beats) and {DeadTourianAnimations.StartingAt.Count} animations match bank $A9.");
    }
}

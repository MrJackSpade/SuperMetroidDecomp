using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Dragon.DragonNativeListAddresses;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Dragon;

/// <summary>#641, #1600: Dragon's typed animations, each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class DragonAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyDragonAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            DragonAnimations.Segments, DragonAnimations.StartingAt, DragonFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<DragonCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Dragon animations: {Entries.Length} segments ({beats} beats) and {DragonAnimations.StartingAt.Count} animations match bank $A2.");
    }
}

using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.FuneNamihe.FuneNamiheNativeListAddresses;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.FuneNamihe;

/// <summary>#641, #1600: the Fune's and Namihe's typed animations, each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class FuneNamiheAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyFuneNamiheAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            FuneNamiheAnimations.Segments, FuneNamiheAnimations.StartingAt, FuneNamiheFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<FuneNamiheCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Fune or Namihe animations: {Entries.Length} segments ({beats} beats) and {FuneNamiheAnimations.StartingAt.Count} animations match bank $A8.");
    }
}

using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Waver.WaverNativeListAddresses;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.Waver.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Waver;

/// <summary>#641: Waver's typed animations (#1600), each segment matched to its native bank-$A3 list and each animation to the lists' control flow.</summary>
internal static class WaverAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyWaverAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            WaverAnimations.Segments, WaverAnimations.StartingAt, WaverFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<WaverCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Waver animations: {Entries.Length} segments ({beats} beats) and {WaverAnimations.StartingAt.Count} animations match bank $A3.");
    }
}

using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.MaridiaSnails.OumNativeListAddresses;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.MaridiaSnails.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.MaridiaSnails;

/// <summary>
/// #641, #1600: Oum's typed animations, each segment matched to its native bank-$A2 list and each animation to the
/// lists' control flow.
/// </summary>
internal static class OumAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyOumAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            OumAnimations.Segments, OumAnimations.StartingAt, OumFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<OumCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Oum animations: {Entries.Length} segments ({beats} beats) and {OumAnimations.StartingAt.Count} animations match bank $A2.");
    }
}

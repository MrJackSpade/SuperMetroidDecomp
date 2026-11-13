using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Shaktool.ShaktoolNativeListAddresses;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Shaktool.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>#641: the Shaktool's typed animations (#1600), each segment matched to its native bank-$AA list and each animation to the lists' control flow.</summary>
internal static class ShaktoolAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyShaktoolAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            ShaktoolAnimations.Segments, ShaktoolAnimations.StartingAt, ShaktoolFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<ShaktoolCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Shaktool animations: {Entries.Length} segments ({beats} beats) and {ShaktoolAnimations.StartingAt.Count} animations match bank $AA.");
    }
}

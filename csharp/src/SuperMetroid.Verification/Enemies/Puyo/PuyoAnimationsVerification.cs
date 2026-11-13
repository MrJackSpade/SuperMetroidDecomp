using SuperMetroid.Core.Animation;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Puyo.PuyoNativeListAddresses;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Enemies.Puyo.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Puyo;

/// <summary>#641: Puyo's typed animations (#1600), each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class PuyoAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyPuyoAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            PuyoAnimations.Segments, PuyoAnimations.StartingAt, PuyoFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Puyo animations: {Entries.Length} segments ({beats} beats) and {PuyoAnimations.StartingAt.Count} animations match bank $A2.");
    }
}

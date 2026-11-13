using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Kzan.KzanNativeListAddresses;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Enemies.Kzan.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Verification.Enemies.Kzan;

/// <summary>#641, #1600: Kzan's typed animations, each segment matched to its native bank-$A6 list and each animation to the lists' control flow.</summary>
internal static class KzanAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyKzanAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            KzanAnimations.Segments, KzanAnimations.StartingAt, KzanFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Kzan animations: {Entries.Length} segments ({beats} beats) and {KzanAnimations.StartingAt.Count} animations match bank $A6.");
    }
}

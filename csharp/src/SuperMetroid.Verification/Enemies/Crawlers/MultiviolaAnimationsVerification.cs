using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Crawlers.MultiviolaNativeListAddresses;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>#641, #1600: Multiviola's typed animations, each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class MultiviolaAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyMultiviolaAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            MultiviolaAnimations.Segments, MultiviolaAnimations.StartingAt, MultiviolaFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Multiviola animations: {Entries.Length} segments ({beats} beats) and {MultiviolaAnimations.StartingAt.Count} animations match bank $A2.");
    }
}

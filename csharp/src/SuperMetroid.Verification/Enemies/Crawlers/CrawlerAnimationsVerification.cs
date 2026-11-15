using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Crawlers.CrawlerNativeListAddresses;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>#641, #1600: the crawlers' typed animations, each segment matched to its native bank-$A3 list and each animation to the lists' control flow.</summary>
internal static class CrawlerAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyCrawlerAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            CrawlerAnimations.Segments, CrawlerAnimations.StartingAt, CrawlerFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => OmittedInstructions.Contains(word) ? 1 : null,
                (word, _, cue, at) => throw new InvalidDataException($"{at}: native instruction ${word:X4} became {cue}, but crawler animations raise no cue."),
                Omitted: OmittedInstructions.Contains));
        Console.WriteLine($"  crawler animations: {Entries.Length} segments ({beats} beats) and {CrawlerAnimations.StartingAt.Count} animations match bank $A3.");
    }
}

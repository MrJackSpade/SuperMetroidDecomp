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
    /// <summary>How the crawlers' lists are read: their only owner instructions install movement routines, which the port omits.</summary>
    internal static NativeEnemyCueRules<NoAnimationCue> Rules { get; } = new(
        word => OmittedInstructions.Contains(word) ? 1 : null,
        (word, _, cue, at) => throw new InvalidDataException($"{at}: native instruction ${word:X4} became {cue}, but crawler animations raise no cue."),
        Omitted: (word, _) => OmittedInstructions.Contains(word));

    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyCrawlerAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            CrawlerAnimations.Segments, CrawlerAnimations.StartingAt, CrawlerFrameToolingDefinitions.Spritemap, Rules);
        Console.WriteLine($"  crawler animations: {Entries.Length} segments ({beats} beats) and {CrawlerAnimations.StartingAt.Count} animations match bank $A3.");
    }
}

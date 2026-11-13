using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Crawlers.CrawlerNativeListAddresses;
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
            new NativeEnemyCueRules<CrawlerCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    for (int index = 0; index < operands.Length; index++)
                        AssertEqual(operands[index], instruction.Operands[index] switch
                    {
                        'E' => CrawlersNativeWords.CrawlerActions.Word(cue.Action),
                        _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                    }, $"{at}: operand {index}");
                }));
        Console.WriteLine($"  crawler animations: {Entries.Length} segments ({beats} beats) and {CrawlerAnimations.StartingAt.Count} animations match bank $A3.");
    }
}

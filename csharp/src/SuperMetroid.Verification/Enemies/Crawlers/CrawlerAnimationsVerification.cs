using static SuperMetroid.Verification.Enemies.Crawlers.CrawlerNativeListAddresses;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>#641: the crawlers' typed programs, each matched step by step to its native bank-$A3 list.</summary>
internal static class CrawlerAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyCrawlerAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, CrawlerAnimations.Programs,
            CrawlerFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'E' => CrawlersNativeWords.CrawlerEnemyFunctions.Word(cue.Function),
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            });
        Console.WriteLine($"  crawler programs: {Entries.Length} programs ({steps} steps) match bank $A3.");
    }
}

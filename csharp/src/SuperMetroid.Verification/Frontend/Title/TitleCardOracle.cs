using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Frontend.Title;

/// <summary>Walks each title card's native bank-$8B list beside its typed steps.</summary>
internal static class TitleCardOracle
{
    /// <summary>The four title cards, in authored order.</summary>
    internal static TitleTextSequenceDefinition[] Cards { get; } =
    [
        TitleSequenceRomData.TextSequences.Year, TitleSequenceRomData.TextSequences.Nintendo,
        TitleSequenceRomData.TextSequences.Presents, TitleSequenceRomData.TextSequences.MetroidThree,
    ];

    /// <summary>
    /// Requires every card's typed steps to match its native list word for word: each timed frame's duration, a blank
    /// typed frame on a blank native entry, each named composition on the spritemap the extraction assigns it, then
    /// the scene trigger and deletion. Also requires the extraction to give all 31 compositions distinct spritemaps,
    /// with the logo on the title's logo pointer. Returns the number of steps compared.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the title cards' native lists")]
    internal static int CompareAll(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        IReadOnlyDictionary<TitleSprite, ushort> extracted = TitleSpriteExtractor.MapSprites(rom);
        int compared = 0;
        foreach (TitleTextSequenceDefinition card in Cards)
        {
            int address = card.InstructionAddress;
            foreach (TitleCardStep step in TitleCards.Steps(card.Phase))
            {
                ushort word = cartridge.ReadCartridgeWord(address);
                string at = $"{card.Phase} at ${address:X6}";
                if (step.Command is { } command)
                {
                    AssertEqual((ushort)command, word, $"{at}: command");
                    address += TitleSequenceRomDataTextSequencesConstants.InstructionWordByteCount;
                }
                else
                {
                    AssertEqual(step.Duration, word, $"{at}: duration");
                    ushort nativeSpritemap = cartridge.ReadCartridgeWord(address + sizeof(ushort));
                    if (step.Sprite is { } sprite)
                        AssertEqual(nativeSpritemap, extracted[sprite], $"{at}: {sprite} is extracted from the listed spritemap");
                    else
                        AssertEqual(FrontendSourceDefinitions.TitleBlankSpritemap, nativeSpritemap, $"{at}: blank typed frame is a blank native entry");
                    address += TitleSequenceRomData.TextSequences.TimedEntryByteCount;
                }
                compared++;
            }
        }
        AssertEqual(cartridge.ReadCartridgeWord((int)FrontendSourceDefinitions.TitleLogoSpritemapOperand),
            extracted[TitleSprite.SuperMetroidLogo], "title logo is extracted from the logo pointer");
        AssertEqual(TitleSpriteDefinitions.All.Count, extracted.Count, "extraction maps every title composition");
        AssertEqual(extracted.Count, extracted.Values.Distinct().Count(), "every title composition has its own spritemap");
        return compared;
    }
}

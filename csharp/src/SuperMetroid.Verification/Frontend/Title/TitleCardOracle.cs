using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Title;

namespace SuperMetroid.Verification.Frontend.Title;

/// <summary>Walks each title card's native bank-$8B list beside its typed steps.</summary>
internal static class TitleCardOracle
{
    /// <summary>The four title cards, in authored order.</summary>
    internal static TitleTextSequenceDefinition[] Cards { get; } =
    [
        TitleSequenceDefinitions.TextSequences.Year, TitleSequenceDefinitions.TextSequences.Nintendo,
        TitleSequenceDefinitions.TextSequences.Presents, TitleSequenceDefinitions.TextSequences.MetroidThree,
    ];

    /// <summary>
    /// Requires every card's typed steps to match its native list word for word: each timed frame's duration, a blank
    /// typed frame on a blank native entry, each named composition on the spritemap the extraction assigns it, then
    /// the scene trigger and deletion. Also requires the extraction to give the two compositions and the 29 typing steps distinct spritemaps,
    /// with the logo on the title's logo pointer. Returns the number of steps compared.
    /// </summary>
    internal static int CompareAll(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        TitleSpritemaps extracted = TitleSpriteExtractor.MapSprites(rom);
        int compared = 0;
        foreach (TitleTextSequenceDefinition card in Cards)
        {
            int address = TitleSourceDefinitions.TextLists[card.Phase];
            foreach (NativeEntry step in Entries(card.Phase))
            {
                ushort word = cartridge.ReadCartridgeWord(address);
                string at = $"{card.Phase} at ${address:X6}";
                if (step.Cue is { } command)
                {
                    AssertEqual(TitleNativeWords.Instructions.Word(command), word, $"{at}: command");
                    address += TitleNativeWords.InstructionWordByteCount;
                }
                else
                {
                    AssertEqual(step.Duration, word, $"{at}: duration");
                    ushort nativeSpritemap = cartridge.ReadCartridgeWord(address + sizeof(ushort));
                    if (step.Typed is { } typed)
                        AssertEqual(nativeSpritemap, extracted.Cards[typed], $"{at}: {typed} is extracted from the listed spritemap");
                    else
                        AssertEqual(FrontendSourceDefinitions.TitleBlankSpritemap, nativeSpritemap, $"{at}: blank typed frame is a blank native entry");
                    address += TitleSourceDefinitions.TimedEntryByteCount;
                }
                compared++;
            }
        }
        AssertEqual(cartridge.ReadCartridgeWord((int)FrontendSourceDefinitions.TitleLogoSpritemapOperand),
            extracted.Sprites[TitleSprite.SuperMetroidLogo], "title logo is extracted from the logo pointer");
        AssertEqual(TitleGraphicsFormat.Sprites.Count, extracted.Sprites.Count, "extraction maps every title composition");
        AssertEqual(TitleGraphicsFormat.Typings.Count, extracted.Cards.Count, "extraction maps every card typing step");
        AssertTrue(extracted.AllDistinct, "every title composition and typing step has its own spritemap");
        return compared;
    }

    /// <summary>
    /// The native list entries the card <paramref name="phase"/> shows types as: each beat of its typing, a frame or the
    /// blank opening as a timed entry and each cue as a command.
    /// </summary>
    internal static IEnumerable<NativeEntry> Entries(TitleSequencePhase phase) => TitleCards.Segments.Beats(phase).Select(beat => beat switch
    {
        AnimationBeat<TitleCardTyping, TitleCardCue>.Frame frame => new NativeEntry(frame.Duration, frame.Shown, null),
        AnimationBeat<TitleCardTyping, TitleCardCue>.Pause pause => new NativeEntry(pause.Duration, null, null),
        AnimationBeat<TitleCardTyping, TitleCardCue>.Event cue => new NativeEntry(0, null, cue.Raised),
        _ => throw new InvalidDataException($"Title card {phase} plays a beat no native list entry shows: {beat}."),
    });

    /// <summary>One native title-list entry a card's typing stands for: a timed frame or a command.</summary>
    /// <param name="Duration">The timed entry's updates; zero for a command.</param>
    /// <param name="Typed">The card's text as far as the timed entry shows it typed, or null for a blank entry or a command.</param>
    /// <param name="Cue">The command's cue, or null for a timed entry.</param>
    internal readonly record struct NativeEntry(int Duration, TitleCardTyping? Typed, TitleCardCue? Cue);
}

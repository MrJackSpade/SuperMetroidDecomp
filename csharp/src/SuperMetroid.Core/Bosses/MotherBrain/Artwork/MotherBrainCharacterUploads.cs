using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// What each page Mother Brain's encounter shows covers of the sprite characters its frames draw over (#1392, #1406): the
/// page's artwork and the characters it shows in, for her frames' pictures to name. The encounter selects pages; this is
/// the one place a page is placed among the characters.
/// </summary>
internal static class MotherBrainCharacterUploads
{
    /// <summary>The bytes one sprite character holds.</summary>
    private const int CharacterBytes = 32;

    /// <summary>Every page's placement, in the order <see cref="MotherBrainSpritePage.All"/> lists the pages, which the catalogue names them by.</summary>
    internal static IReadOnlyList<RuntimeCharacterUpload> All { get; } = [.. MotherBrainSpritePage.All.Select(Of)];

    /// <summary>The artwork <paramref name="page"/> shows and the characters it shows in.</summary>
    /// <exception cref="InvalidOperationException">The page's sheet is not one of Mother Brain's.</exception>
    internal static RuntimeCharacterUpload Of(MotherBrainSpritePage page) => page.Sheet switch
    {
        MotherBrainSpritePageSheet.Legs => Special(0, page.Page),
        MotherBrainSpritePageSheet.BabyMetroid => Special(1, page.Page),
        MotherBrainSpritePageSheet.Attack => Special(2, page.Page),
        MotherBrainSpritePageSheet.ExplodedDoor => Special(3, page.Page),
        MotherBrainSpritePageSheet.Corpse => new(new MotherBrainCorpseCharacters(page.Page),
            MotherBrainCorpseArtworkDefinitions.PageFirstCharacter(page.Page) * CharacterBytes, MotherBrainCorpseArtworkDefinitions.PageByteCount),
        MotherBrainSpritePageSheet.EscapeTimer => EscapeTimer(Enum.GetValues<EscapeTimerTilePage>()[page.Page]),
        MotherBrainSpritePageSheet.EscapeText => EscapeText(page.Page),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainSpritePageSheet)} {(int)page.Sheet}."),
    };

    /// <summary>The characters corpse row <paramref name="row"/>'s rotting part shows in.</summary>
    internal static CharacterRegion RotRegion(MotherBrainCorpseRotRow row) => new(row.FirstCharacter * CharacterBytes, row.ByteCount);

    /// <summary>Page <paramref name="page"/> of special sheet <paramref name="sheet"/>, by its place in <see cref="MotherBrainSpecialSpriteArtworkDefinitions.All"/>.</summary>
    private static RuntimeCharacterUpload Special(int sheet, int page)
    {
        MotherBrainSpecialSpriteSheetDefinition definition = MotherBrainSpecialSpriteArtworkDefinitions.All[sheet];
        return new(new MotherBrainSpecialSpriteCharacters(sheet, page, MotherBrainSpecialSpriteSheetDefinition.PageByteCount),
            definition.PageFirstCharacter(page) * CharacterBytes, MotherBrainSpecialSpriteSheetDefinition.PageByteCount);
    }

    /// <summary>The escape timer's digit page <paramref name="page"/>, as its installed asset.</summary>
    [PpuLayoutBoundary("codec: the escape timer pages' native VRAM destinations")]
    private static RuntimeCharacterUpload EscapeTimer(EscapeTimerTilePage page) =>
        RuntimeCharacterUpload.To(VramDmaDestination.FromNative(EscapeTimerTilePages.DestinationWordOf(page)),
            EscapeTimerTilePages.ByteCountOf(page), new AssetObjCharacters(EscapeTimerTilePages.AssetOf(page)));

    /// <summary>The escape message's letter page <paramref name="page"/>.</summary>
    [PpuLayoutBoundary("codec: the escape text pages' native VRAM destinations")]
    private static RuntimeCharacterUpload EscapeText(int page) =>
        RuntimeCharacterUpload.To(VramDmaDestination.FromNative(EscapeTextArtworkDefinitions.PageDestination(page)),
            EscapeTextArtworkDefinitions.PageByteCount(page), new MotherBrainEscapeTextCharacters(page));
}

using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// What each page Mother Brain's encounter shows covers of the sprite characters its frames draw over (#1392, #1406): the
/// page's artwork and the characters it shows in, for her frames' pictures to name. The encounter selects pages; this is
/// the one place a page is placed among the characters.
/// </summary>
internal static class MotherBrainCharacterSources
{
    /// <summary>Every page's artwork, in the order <see cref="MotherBrainSpritePage.All"/> lists the pages, which the catalogue names them by.</summary>
    internal static IReadOnlyList<ObjCharacterSource> All { get; } = [.. MotherBrainSpritePage.All.Select(Of)];

    /// <summary>The artwork <paramref name="page"/> shows.</summary>
    /// <exception cref="InvalidOperationException">The page's sheet is not one of Mother Brain's.</exception>
    internal static ObjCharacterSource Of(MotherBrainSpritePage page) => page.Sheet switch
    {
        MotherBrainSpritePageSheet.Legs => Special(0, page.Page),
        MotherBrainSpritePageSheet.BabyMetroid => Special(1, page.Page),
        MotherBrainSpritePageSheet.Attack => Special(2, page.Page),
        MotherBrainSpritePageSheet.ExplodedDoor => Special(3, page.Page),
        MotherBrainSpritePageSheet.Corpse => new MotherBrainCorpseCharacters(page.Page),
        MotherBrainSpritePageSheet.EscapeTimer => new AssetObjCharacters(EscapeTimerTilePages.AssetOf(Enum.GetValues<EscapeTimerTilePage>()[page.Page])),
        MotherBrainSpritePageSheet.EscapeText => new MotherBrainEscapeTextCharacters(page.Page),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainSpritePageSheet)} {(int)page.Sheet}."),
    };

    /// <summary>Page <paramref name="page"/> of special sheet <paramref name="sheet"/>, by its place in <see cref="MotherBrainSpecialSpriteArtworkDefinitions.All"/>.</summary>
    private static MotherBrainSpecialSpriteCharacters Special(int sheet, int page) =>
        new(sheet, page, MotherBrainSpecialSpriteSheetDefinition.PageByteCount);
}

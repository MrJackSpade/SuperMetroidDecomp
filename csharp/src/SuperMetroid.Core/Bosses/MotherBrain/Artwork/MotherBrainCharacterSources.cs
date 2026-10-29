using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The look each page Mother Brain's encounter shows is (#1392, #1406, #1463), for her frames' pictures to name. The
/// encounter selects pages; where each page's look lands is the import pipeline's knowledge.
/// </summary>
internal static class MotherBrainCharacterSources
{
    /// <summary>Every page's artwork, in the order <see cref="MotherBrainSpritePage.All"/> lists the pages, which the catalogue names them by.</summary>
    internal static IReadOnlyList<SpriteArtworkSource> All { get; } = [.. MotherBrainSpritePage.All.Select(Of)];

    /// <summary>The artwork <paramref name="page"/> shows.</summary>
    /// <exception cref="InvalidOperationException">The page's sheet is not one of Mother Brain's.</exception>
    internal static SpriteArtworkSource Of(MotherBrainSpritePage page) => page.Sheet switch
    {
        MotherBrainSpritePageSheet.Legs => Special(0, page.Page),
        MotherBrainSpritePageSheet.BabyMetroid => Special(1, page.Page),
        MotherBrainSpritePageSheet.Attack => Special(2, page.Page),
        MotherBrainSpritePageSheet.ExplodedDoor => Special(3, page.Page),
        MotherBrainSpritePageSheet.Corpse => new MotherBrainCorpseCharacters(page.Page),
        MotherBrainSpritePageSheet.EscapeTimer => new InstalledArtworkCharacters(EscapeTimerTilePages.AssetOf(Enum.GetValues<EscapeTimerTilePage>()[page.Page])),
        MotherBrainSpritePageSheet.EscapeText => new MotherBrainEscapeTextCharacters(page.Page),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainSpritePageSheet)} {page.Sheet}."),
    };

    /// <summary>Page <paramref name="page"/> of special sheet <paramref name="sheet"/>, by its place in <see cref="MotherBrainSpecialSpriteArtworkDefinitions.All"/>.</summary>
    private static MotherBrainSpecialSpriteCharacters Special(int sheet, int page) => new(sheet, page);
}

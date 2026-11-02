using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// Every page Mother Brain's encounter can show and the artwork each shows, which the installed catalogue pairs with the
/// characters' places; the encounter itself only selects one page at a time.
/// </summary>
internal static class MotherBrainPageCharacters
{
    /// <summary>Every page of every sheet, sheet by sheet in their declared order and page by page within each.</summary>
    internal static IEnumerable<MotherBrainSpritePage> Pages =>
        Enum.GetValues<MotherBrainSpritePageSheet>().SelectMany(sheet =>
            Enumerable.Range(0, MotherBrainSpritePage.PageCount(sheet)).Select(page => MotherBrainSpritePage.Of(sheet, page)));

    /// <summary>Every page's artwork, in the order <see cref="Pages"/> lists the pages, which the catalogue names them by.</summary>
    internal static IReadOnlyList<SpriteArtworkSource> All { get; } = [.. Pages.Select(MotherBrainCharacterSources.Of)];
}

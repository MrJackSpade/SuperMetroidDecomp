using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// One page of a sheet Mother Brain's encounter shows its sprites (#1406). The encounter shows a page on the update its
/// sequence reaches it, and from then on the frames drawn over that page's characters picture what the page holds.
/// </summary>
public readonly record struct MotherBrainSpritePage
{
    /// <summary>The page <paramref name="page"/> of <paramref name="sheet"/>.</summary>
    private MotherBrainSpritePage(MotherBrainSpritePageSheet sheet, int page)
    {
        Sheet = sheet;
        Page = page;
    }

    /// <summary>The sheet the page belongs to.</summary>
    public MotherBrainSpritePageSheet Sheet { get; }

    /// <summary>The page's place in its sheet, from zero.</summary>
    public int Page { get; }

    /// <summary>Page <paramref name="page"/> of <paramref name="sheet"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet has no such page.</exception>
    public static MotherBrainSpritePage Of(MotherBrainSpritePageSheet sheet, int page) =>
        (uint)page < (uint)PageCount(sheet)
            ? new(sheet, page)
            : throw new ArgumentOutOfRangeException(nameof(page), page, $"Mother Brain's {sheet} sheet has {PageCount(sheet)} pages.");

    /// <summary>How many pages <paramref name="sheet"/> has.</summary>
    /// <exception cref="InvalidOperationException">The sheet is not one of Mother Brain's.</exception>
    public static int PageCount(MotherBrainSpritePageSheet sheet) => sheet switch
    {
        MotherBrainSpritePageSheet.Legs => MotherBrainSpecialSpriteArtworkDefinitions.Legs.PageCount,
        MotherBrainSpritePageSheet.BabyMetroid => MotherBrainSpecialSpriteArtworkDefinitions.BabyMetroid.PageCount,
        MotherBrainSpritePageSheet.Attack => MotherBrainSpecialSpriteArtworkDefinitions.Attack.PageCount,
        MotherBrainSpritePageSheet.ExplodedDoor => MotherBrainSpecialSpriteArtworkDefinitions.ExplodedDoor.PageCount,
        MotherBrainSpritePageSheet.Corpse => MotherBrainCorpseArtworkDefinitions.RowCount,
        MotherBrainSpritePageSheet.EscapeTimer => EscapeTextArtworkDefinitions.TimerPageCount,
        MotherBrainSpritePageSheet.EscapeText => EscapeTextArtworkDefinitions.PageCount,
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainSpritePageSheet)} {sheet}."),
    };

    /// <summary>Every page of every sheet, sheet by sheet in their declared order and page by page within each.</summary>
    public static IEnumerable<MotherBrainSpritePage> All =>
        Enum.GetValues<MotherBrainSpritePageSheet>().SelectMany(sheet => Enumerable.Range(0, PageCount(sheet)).Select(page => Of(sheet, page)));

    /// <summary>Names the page for diagnostics.</summary>
    public override string ToString() => $"Mother Brain {Sheet} page {Page}";
}

using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Visual identity of the Japanese text's next-page arrow in the intro narration (#1655).</summary>
internal static class IntroNextPageArrowSpriteDefinitions
{
    /// <summary>
    /// The arrow's three two-part poses: every pose the arrow's actor (definition $8B:CE73, lists $8B:CC0F to $8B:CC2B)
    /// shows. Its loop shows $8C:8CCF, $8C:8CDB, $8C:8CE7 and $8C:8CDB again; its delayed list shows nothing first.
    /// </summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("next-page-arrow-1", CinematicSpritePose.NextPageArrow1, 2),
        new("next-page-arrow-2", CinematicSpritePose.NextPageArrow2, 2),
        new("next-page-arrow-3", CinematicSpritePose.NextPageArrow3, 2),
    ];
}

using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Visual identity of the Japanese text's next-page arrow in the intro narration (#1655).</summary>
internal static partial class IntroNextPageArrowSpriteDefinitions
{
    /// <summary>
    /// The arrow's three two-part poses: every pose the arrow's actor (definition $8B:CE73, lists $8B:CC0F to $8B:CC2B)
    /// shows. Its loop shows $8C:8CCF, $8C:8CDB, $8C:8CE7 and $8C:8CDB again; its delayed list shows nothing first.
    /// </summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("next-page-arrow-bright", CinematicSpritePose.NextPageArrowBright, 2),
        new("next-page-arrow-fading", CinematicSpritePose.NextPageArrowFading, 2),
        new("next-page-arrow-faint", CinematicSpritePose.NextPageArrowFaint, 2),
    ];
}

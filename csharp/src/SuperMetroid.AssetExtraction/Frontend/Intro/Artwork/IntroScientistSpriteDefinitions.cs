using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Visual compositions selected by the scientist delivery and examination lists.</summary>
internal static class IntroScientistSpriteDefinitions
{
    /// <summary>
    /// Three six-part delivered poses and three one-part examined poses: every pose the two scientist scenes' babies show
    /// (#1654). The delivered baby's lists, from $8B:CB9F to $8B:CBCD, show the first three; the examined baby's, from
    /// $8B:CBCD to $8B:CBFB, the last three. The text caret's block ($8C:8D68) is the caret's own pose, and the Japanese
    /// text's next-page arrow ($8C:8CCF to $8C:8CE7) is no pose of theirs.
    /// </summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("delivered-baby-1", CinematicSpritePose.DeliveredBaby1, 6),
        new("delivered-baby-2", CinematicSpritePose.DeliveredBaby2, 6),
        new("delivered-baby-3", CinematicSpritePose.DeliveredBaby3, 6),
        new("examined-baby-1", CinematicSpritePose.ExaminedBaby1, 1),
        new("examined-baby-2", CinematicSpritePose.ExaminedBaby2, 1),
        new("examined-baby-3", CinematicSpritePose.ExaminedBaby3, 1),
    ];
}

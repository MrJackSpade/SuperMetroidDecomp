using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The six ending cloud compositions, in the order of the bank-$8B cloud instruction lists.</summary>
public static class EndingCloudSpriteDefinitions
{
    /// <summary>
    /// The cloud compositions in the order the ending actors use them: the upper pattern and its edge, the lower edge and
    /// its pattern (four sixteen-part horizontal clouds), then the right and left 32-part side clouds.
    /// </summary>
    public static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("top-cloud-pattern", CinematicSpritePose.TopCloudPattern, 16),
        new("top-cloud-edge", CinematicSpritePose.TopCloudEdge, 16),
        new("bottom-cloud-edge", CinematicSpritePose.BottomCloudEdge, 16),
        new("bottom-cloud-pattern", CinematicSpritePose.BottomCloudPattern, 16),
        new("right-side-cloud", CinematicSpritePose.RightSideCloud, 32),
        new("left-side-cloud", CinematicSpritePose.LeftSideCloud, 32),
    ];
}

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
        new("scene-b-upper-a", new(159), 16),
        new("scene-b-upper-b", new(160), 16),
        new("scene-b-lower-a", new(161), 16),
        new("scene-b-lower-b", new(162), 16),
        new("scene-a-right", new(163), 32),
        new("scene-a-left", new(164), 32),
    ];
}

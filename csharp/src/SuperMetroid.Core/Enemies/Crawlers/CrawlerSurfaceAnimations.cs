namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Chooses a crawler's animation for the surface it clings to.</summary>
internal static class CrawlerSurfaceAnimations
{
    /// <summary>The animation <paramref name="family"/> shows on the surface <paramref name="orientation"/> names.</summary>
    /// <exception cref="InvalidDataException"><paramref name="family"/> has no authored animations.</exception>
    internal static CrawlerSegment AnimationFor(
        CrawlerAnimationFamily family,
        CrawlerSurfaceOrientation orientation) => ForOrientation(Family(family), orientation);

    /// <summary>One family's four surface animations.</summary>
    private static CrawlerAnimationDefinition Family(CrawlerAnimationFamily family) => CrawlerAnimationDefinitions.Families[family];

    /// <summary>Selects an animation by one of the four physical surface orientations.</summary>
    private static CrawlerSegment ForOrientation(CrawlerAnimationDefinition definition, CrawlerSurfaceOrientation orientation) =>
        orientation switch
        {
            CrawlerSurfaceOrientation.OnLeftWall => definition.OnLeftWall,
            CrawlerSurfaceOrientation.OnRightWall => definition.OnRightWall,
            CrawlerSurfaceOrientation.OnCeiling => definition.OnCeiling,
            CrawlerSurfaceOrientation.OnFloor => definition.OnFloor,
            _ => throw new InvalidDataException(
                $"Crawler surface orientation {orientation} is not one of the four surfaces."),
        };
}

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Chooses a crawler's animation for the surface it clings to.</summary>
internal static class CrawlerSurfaceAnimations
{
    /// <summary>Returns one family-specific initial animation.</summary>
    internal static CrawlerProgram InitialProgram(
        CrawlerAnimationFamily family,
        CrawlerSurfaceOrientation orientation) => ForOrientation(Family(family), orientation);

    /// <summary>Returns one shared surface animation using the native even species offset.</summary>
    internal static CrawlerProgram SurfaceProgram(
        int speciesByteOffset,
        CrawlerSurfaceOrientation orientation)
    {
        if ((speciesByteOffset & 1) != 0
            || !CrawlerAnimationDefinitions.SurfaceFamilies.TryGetValue(speciesByteOffset, out CrawlerAnimationFamily family))
        {
            throw new InvalidDataException(
                $"Crawler instruction-table offset ${speciesByteOffset:X4} is invalid.");
        }
        return ForOrientation(Family(family), orientation);
    }

    /// <summary>One family's four surface animations.</summary>
    private static CrawlerAnimationDefinition Family(CrawlerAnimationFamily family) =>
        CrawlerAnimationDefinitions.Families.TryGetValue(family, out CrawlerAnimationDefinition definition)
            ? definition
            : throw new InvalidDataException(
                $"Crawler animation family {family} exceeds five authored tables.");

    /// <summary>Selects an animation by one of the four physical surface orientations.</summary>
    private static CrawlerProgram ForOrientation(CrawlerAnimationDefinition definition, CrawlerSurfaceOrientation orientation) =>
        orientation switch
        {
            CrawlerSurfaceOrientation.UpsideRight => definition.UpsideRight,
            CrawlerSurfaceOrientation.UpsideLeft => definition.UpsideLeft,
            CrawlerSurfaceOrientation.UpsideDown => definition.UpsideDown,
            CrawlerSurfaceOrientation.UpsideUp => definition.UpsideUp,
            _ => throw new InvalidDataException(
                $"Crawler surface orientation {orientation} exceeds four authored values."),
        };
}

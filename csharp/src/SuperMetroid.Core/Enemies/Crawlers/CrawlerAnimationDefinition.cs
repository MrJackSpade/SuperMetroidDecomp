namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Four surface-oriented animations for one crawler family or species.</summary>
internal readonly record struct CrawlerAnimationDefinition(
    CrawlerProgram UpsideRight,
    CrawlerProgram UpsideLeft,
    CrawlerProgram UpsideDown,
    CrawlerProgram UpsideUp)
{
    /// <summary>Selects an animation by one of the four physical surface orientations.</summary>
    internal CrawlerProgram ForOrientation(CrawlerSurfaceOrientation orientation) => orientation switch
    {
        CrawlerSurfaceOrientation.UpsideRight => UpsideRight,
        CrawlerSurfaceOrientation.UpsideLeft => UpsideLeft,
        CrawlerSurfaceOrientation.UpsideDown => UpsideDown,
        CrawlerSurfaceOrientation.UpsideUp => UpsideUp,
        _ => throw new InvalidDataException(
            $"Crawler surface orientation {orientation} exceeds four authored values."),
    };
}

namespace SuperMetroid.Core.Game;

/// <summary>Four surface-oriented instruction lists for one crawler family or species.</summary>
internal readonly record struct CrawlerAnimationDefinition(
    ushort UpsideRight,
    ushort UpsideLeft,
    ushort UpsideDown,
    ushort UpsideUp)
{
    /// <summary>Selects a native instruction-list pointer by one of the four physical surface orientations.</summary>
    internal ushort ForOrientation(CrawlerSurfaceOrientation orientation) => orientation switch
    {
        CrawlerSurfaceOrientation.UpsideRight => UpsideRight,
        CrawlerSurfaceOrientation.UpsideLeft => UpsideLeft,
        CrawlerSurfaceOrientation.UpsideDown => UpsideDown,
        CrawlerSurfaceOrientation.UpsideUp => UpsideUp,
        _ => throw new InvalidDataException(
            $"Crawler surface orientation {(ushort)orientation} exceeds four authored values."),
    };
}

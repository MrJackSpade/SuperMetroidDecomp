namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Initial and shared surface animation selectors for crawler enemies.</summary>
internal static class CrawlerAnimationDefinitions
{
    /// <summary>The four surface orientations in the order the two-bit orientation index counts them.</summary>
    internal static readonly IdentitySequence<CrawlerSurfaceOrientation> Orientations = new(
        CrawlerSurfaceOrientation.UpsideRight, CrawlerSurfaceOrientation.UpsideLeft,
        CrawlerSurfaceOrientation.UpsideDown, CrawlerSurfaceOrientation.UpsideUp);

    /// <summary>
    /// The four-word initial tables at <c>$A3:E2CC</c>, <c>$B667</c>, <c>$96DB</c>,
    /// <c>$992B</c>, and <c>$E03B</c>, indexed by <see cref="CrawlerAnimationFamily"/>.
    /// </summary>
    private static CrawlerAnimationDefinition Family(CrawlerAnimationFamily family) => family switch
    {
        CrawlerAnimationFamily.Shared => new(
            CrawlerProgram.SharedUpsideRight,
            CrawlerProgram.SharedUpsideLeft,
            CrawlerProgram.SharedUpsideDown,
            CrawlerProgram.SharedUpsideUp),
        CrawlerAnimationFamily.Viola => new(
            CrawlerProgram.ViolaUpsideRight,
            CrawlerProgram.ViolaUpsideLeft,
            CrawlerProgram.ViolaUpsideDown,
            CrawlerProgram.ViolaUpsideUp),
        CrawlerAnimationFamily.Sciser => new(
            CrawlerProgram.SciserUpsideRight,
            CrawlerProgram.SciserUpsideLeft,
            CrawlerProgram.SciserUpsideDown,
            CrawlerProgram.SciserUpsideUp),
        CrawlerAnimationFamily.Zero => new(
            CrawlerProgram.ZeroUpsideRight,
            CrawlerProgram.ZeroUpsideLeft,
            CrawlerProgram.ZeroUpsideDown,
            CrawlerProgram.ZeroUpsideUp),
        CrawlerAnimationFamily.HZoomer => new(
            CrawlerProgram.HZoomerUpsideRight,
            CrawlerProgram.HZoomerUpsideLeft,
            CrawlerProgram.HZoomerUpsideDown,
            CrawlerProgram.HZoomerUpsideUp),
        _ => throw new InvalidDataException(
            $"Crawler animation family {family} exceeds five authored tables."),
    };
    /// <summary>Returns one family-specific initial animation.</summary>
    internal static CrawlerProgram InitialProgram(
        CrawlerAnimationFamily family,
        CrawlerSurfaceOrientation orientation) => Family(family).ForOrientation(orientation);
    /// <summary>Returns one shared surface animation using the native even species offset.</summary>
    internal static CrawlerProgram SurfaceProgram(
        int speciesByteOffset,
        CrawlerSurfaceOrientation orientation)
    {
        if ((speciesByteOffset & 1) != 0 || speciesByteOffset > 10)
        {
            throw new InvalidDataException(
                $"Crawler instruction-table offset ${speciesByteOffset:X4} is invalid.");
        }

        // $A3:E630-$E65F: the first three species share the common animation;
        // the remaining species select Viola, Sciser and Zero respectively.
        CrawlerAnimationFamily family = speciesByteOffset switch
        {
            <= 4 => CrawlerAnimationFamily.Shared,
            6 => CrawlerAnimationFamily.Viola,
            8 => CrawlerAnimationFamily.Sciser,
            _ => CrawlerAnimationFamily.Zero,
        };
        return Family(family).ForOrientation(orientation);
    }
}

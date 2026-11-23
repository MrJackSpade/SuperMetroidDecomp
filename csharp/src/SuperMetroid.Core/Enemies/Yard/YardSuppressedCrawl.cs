namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// How a Yard crawling along a floor or ceiling goes on while its turns are suppressed (#2572): it looks no way ahead and,
/// instead of turning, continues at a crawl along a wall.
/// </summary>
/// <param name="WhenSurfaceEnds">The crawl along a wall it continues at when the surface it crawls along ends.</param>
/// <param name="WhenSurfaceMet">The crawl along a wall it continues at when it meets a surface ahead.</param>
internal readonly partial record struct YardSuppressedCrawl(YardSegment WhenSurfaceEnds, YardSegment WhenSurfaceMet);

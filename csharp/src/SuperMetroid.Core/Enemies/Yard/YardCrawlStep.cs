namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// How one crawling Yard's step goes on (#2572): how far ahead it probes its surface, and the animation it starts when the
/// surface ends or when it meets one ahead. An ordinary turn's lookahead and outside and inside turns give it, or, while
/// the Yard's turns are suppressed, its suppressed crawl's two crawls along a wall, probed from where the Yard stands.
/// </summary>
/// <param name="Lookahead">The room-pixel offset along the direction of motion its surface probe is taken at.</param>
/// <param name="WhenSurfaceEnds">The animation it starts when its surface ends.</param>
/// <param name="WhenSurfaceMet">The animation it starts when it meets a surface ahead.</param>
internal readonly partial record struct YardCrawlStep(
    Offset16<RoomPixels> Lookahead,
    YardSegment WhenSurfaceEnds,
    YardSegment WhenSurfaceMet);

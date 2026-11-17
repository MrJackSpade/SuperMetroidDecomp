namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>
/// Owtch's left and right shell-cycling animations (#641): each facing's entry cue, which the animation a spawn, branch or
/// wake starts there plays once, and the crawl it then loops (#2276), in authored order. Their steps are
/// <see cref="OwtchAnimations"/>.
/// </summary>
internal enum OwtchSegment
{
    /// <summary>The moving left animation's entry: the cue that sets the walking direction left.</summary>
    MovingLeft,
    /// <summary>The left crawl's three feet poses, looped.</summary>
    CrawlingLeft,
    /// <summary>The moving right animation's entry: the cue that sets the walking direction right.</summary>
    MovingRight,
    /// <summary>The right crawl's three feet poses, looped.</summary>
    CrawlingRight,
}

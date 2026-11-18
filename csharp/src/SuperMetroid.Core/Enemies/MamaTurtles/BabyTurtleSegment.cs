namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The baby turtles' crawl, hiding, shell and spin animations (#641, #2342): one segment per animation a spawn, branch or wake enters, in authored order. Their steps are
/// <see cref="BabyTurtleAnimations"/>.
/// </summary>
internal enum BabyTurtleSegment
{
    /// <summary>A baby turtle crawling left, its legs tucking, reaching, planting and pushing: a stride, taken twice.</summary>
    CrawlingLeft,
    /// <summary>
    /// A baby turtle crawling left on with its legs trailing and lifting, before it crawls on or turns around (#2439).
    /// </summary>
    CrawlingLeftLegsTrailing,
    /// <summary>A baby turtle's shell spinning a quarter turn at a time with its sound, repeating.</summary>
    Spinning,
    /// <summary>
    /// A baby turtle facing left pulling in its legs and hiding in its shell before it comes out again.
    /// </summary>
    HidingLeft,
    /// <summary>
    /// A baby turtle's legs coming out of its shell and resting, before and after it has left the shell; they face left
    /// whichever way it crawls (#2337).
    /// </summary>
    LeaveShell,
    /// <summary>A baby turtle crawling right, its legs tucking, reaching, planting and pushing: a stride, taken twice.</summary>
    CrawlingRight,
    /// <summary>
    /// A baby turtle crawling right on with its legs trailing and lifting, before it crawls on or turns around (#2439).
    /// </summary>
    CrawlingRightLegsTrailing,
    /// <summary>
    /// A baby turtle facing right pulling in its legs and hiding in its shell before it comes out again.
    /// </summary>
    HidingRight,
}

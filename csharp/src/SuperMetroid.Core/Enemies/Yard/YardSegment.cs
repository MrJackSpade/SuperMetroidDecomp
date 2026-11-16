namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// The Yard's crawl, turn, hiding and airborne animations (#641): one segment per animation a spawn, branch or wake enters,
/// and the closed shell, hidden shell and tumble those animations play on into (#2276), in authored order.
/// Their steps are <see cref="YardAnimations"/>.
/// </summary>
internal enum YardSegment
{
    /// <summary>The outside turn upside right moving up animation.</summary>
    OutsideTurnUpsideRightMovingUp,
    /// <summary>The crawling upside up moving left animation.</summary>
    CrawlingUpsideUpMovingLeft,
    /// <summary>The outside turn upside up moving left animation.</summary>
    OutsideTurnUpsideUpMovingLeft,
    /// <summary>The crawling upside left moving down animation.</summary>
    CrawlingUpsideLeftMovingDown,
    /// <summary>The outside turn upside left moving down animation.</summary>
    OutsideTurnUpsideLeftMovingDown,
    /// <summary>The crawling upside down moving right animation.</summary>
    CrawlingUpsideDownMovingRight,
    /// <summary>The outside turn upside down moving right animation.</summary>
    OutsideTurnUpsideDownMovingRight,
    /// <summary>The crawling upside right moving up animation.</summary>
    CrawlingUpsideRightMovingUp,
    /// <summary>The outside turn upside left moving up animation.</summary>
    OutsideTurnUpsideLeftMovingUp,
    /// <summary>The crawling upside up moving right animation.</summary>
    CrawlingUpsideUpMovingRight,
    /// <summary>The outside turn upside up moving right animation.</summary>
    OutsideTurnUpsideUpMovingRight,
    /// <summary>The crawling upside right moving down animation.</summary>
    CrawlingUpsideRightMovingDown,
    /// <summary>The outside turn upside right moving down animation.</summary>
    OutsideTurnUpsideRightMovingDown,
    /// <summary>The crawling upside down moving left animation.</summary>
    CrawlingUpsideDownMovingLeft,
    /// <summary>The outside turn upside down moving left animation.</summary>
    OutsideTurnUpsideDownMovingLeft,
    /// <summary>The crawling upside left moving up animation.</summary>
    CrawlingUpsideLeftMovingUp,
    /// <summary>The inside turn upside up moving left animation.</summary>
    InsideTurnUpsideUpMovingLeft,
    /// <summary>The inside turn upside right moving up animation.</summary>
    InsideTurnUpsideRightMovingUp,
    /// <summary>The inside turn upside down moving right animation.</summary>
    InsideTurnUpsideDownMovingRight,
    /// <summary>The inside turn upside left moving down animation.</summary>
    InsideTurnUpsideLeftMovingDown,
    /// <summary>The inside turn upside up moving right animation.</summary>
    InsideTurnUpsideUpMovingRight,
    /// <summary>The inside turn upside left moving up animation.</summary>
    InsideTurnUpsideLeftMovingUp,
    /// <summary>The inside turn upside down moving left animation.</summary>
    InsideTurnUpsideDownMovingLeft,
    /// <summary>The inside turn upside right moving down animation.</summary>
    InsideTurnUpsideRightMovingDown,
    /// <summary>The hiding upside up moving left animation.</summary>
    HidingUpsideUpMovingLeft,
    /// <summary>The closed shell of a hiding Yard upside up moving left, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideUpMovingLeft,
    /// <summary>The hidden upside up moving left animation.</summary>
    HiddenUpsideUpMovingLeft,
    /// <summary>The hiding upside down moving left animation.</summary>
    HidingUpsideDownMovingLeft,
    /// <summary>The closed shell of a hiding Yard upside down moving left, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideDownMovingLeft,
    /// <summary>The hidden shell of a Yard upside down moving left before the Yard crawls on (#2276).</summary>
    HiddenUpsideDownMovingLeft,
    /// <summary>The hiding upside down moving right animation.</summary>
    HidingUpsideDownMovingRight,
    /// <summary>The closed shell of a hiding Yard upside down moving right, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideDownMovingRight,
    /// <summary>The hidden shell of a Yard upside down moving right before the Yard crawls on (#2276).</summary>
    HiddenUpsideDownMovingRight,
    /// <summary>The hiding upside up moving right animation.</summary>
    HidingUpsideUpMovingRight,
    /// <summary>The closed shell of a hiding Yard upside up moving right, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideUpMovingRight,
    /// <summary>The hidden upside up moving right animation.</summary>
    HiddenUpsideUpMovingRight,
    /// <summary>The hiding upside right moving up animation.</summary>
    HidingUpsideRightMovingUp,
    /// <summary>The closed shell of a hiding Yard upside right moving up, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideRightMovingUp,
    /// <summary>The hidden shell of a Yard upside right moving up before the Yard crawls on (#2276).</summary>
    HiddenUpsideRightMovingUp,
    /// <summary>The hiding upside left moving up animation.</summary>
    HidingUpsideLeftMovingUp,
    /// <summary>The closed shell of a hiding Yard upside left moving up, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideLeftMovingUp,
    /// <summary>The hidden shell of a Yard upside left moving up before the Yard crawls on (#2276).</summary>
    HiddenUpsideLeftMovingUp,
    /// <summary>The hiding upside left moving down animation.</summary>
    HidingUpsideLeftMovingDown,
    /// <summary>The closed shell of a hiding Yard upside left moving down, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideLeftMovingDown,
    /// <summary>The hidden shell of a Yard upside left moving down before the Yard crawls on (#2276).</summary>
    HiddenUpsideLeftMovingDown,
    /// <summary>The hiding upside right moving down animation.</summary>
    HidingUpsideRightMovingDown,
    /// <summary>The closed shell of a hiding Yard upside right moving down, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideRightMovingDown,
    /// <summary>The hidden shell of a Yard upside right moving down before the Yard crawls on (#2276).</summary>
    HiddenUpsideRightMovingDown,
    /// <summary>The airborne facing left animation.</summary>
    AirborneFacingLeft,
    /// <summary>The airborne shell facing left tumbling, looped (#2276).</summary>
    AirborneFacingLeftTumbling,
    /// <summary>The airborne facing right animation.</summary>
    AirborneFacingRight,
    /// <summary>The airborne shell facing right tumbling, looped (#2276).</summary>
    AirborneFacingRightTumbling,
}

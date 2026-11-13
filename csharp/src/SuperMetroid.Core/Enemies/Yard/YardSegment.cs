namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// The Yard's crawl, turn, hiding and airborne animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
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
    /// <summary>The hidden upside up moving left animation.</summary>
    HiddenUpsideUpMovingLeft,
    /// <summary>The hiding upside down moving left animation.</summary>
    HidingUpsideDownMovingLeft,
    /// <summary>The hiding upside down moving right animation.</summary>
    HidingUpsideDownMovingRight,
    /// <summary>The hiding upside up moving right animation.</summary>
    HidingUpsideUpMovingRight,
    /// <summary>The hidden upside up moving right animation.</summary>
    HiddenUpsideUpMovingRight,
    /// <summary>The hiding upside right moving up animation.</summary>
    HidingUpsideRightMovingUp,
    /// <summary>The hiding upside left moving up animation.</summary>
    HidingUpsideLeftMovingUp,
    /// <summary>The hiding upside left moving down animation.</summary>
    HidingUpsideLeftMovingDown,
    /// <summary>The hiding upside right moving down animation.</summary>
    HidingUpsideRightMovingDown,
    /// <summary>The airborne facing left animation.</summary>
    AirborneFacingLeft,
    /// <summary>The airborne facing left loop animation.</summary>
    AirborneFacingLeftLoop,
    /// <summary>The airborne facing right animation.</summary>
    AirborneFacingRight,
    /// <summary>The airborne facing right loop animation.</summary>
    AirborneFacingRightLoop,
}

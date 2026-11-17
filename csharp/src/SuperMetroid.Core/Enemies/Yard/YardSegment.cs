namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// The Yard's crawl, turn, hiding and airborne animations (#641): one segment per animation a spawn, branch or wake enters,
/// and the closed shell, hidden shell and tumble those animations play on into (#2276), in authored order.
/// Their steps are <see cref="YardAnimations"/>.
/// </summary>
internal enum YardSegment
{
    /// <summary>
    /// Rounding an outside corner from crawling up with its back to the right, a quarter turn in three steps, then
    /// crawling left with its back up.
    /// </summary>
    OutsideTurnUpsideRightMovingUp,
    /// <summary>
    /// Crawling left with its back up, its legs tucking, spreading and stepping, repeating; it can hide from here.
    /// </summary>
    CrawlingUpsideUpMovingLeft,
    /// <summary>
    /// Rounding an outside corner from crawling left with its back up, a quarter turn in three steps, then crawling
    /// down with its back to the left.
    /// </summary>
    OutsideTurnUpsideUpMovingLeft,
    /// <summary>
    /// Crawling down with its back to the left, its legs tucking, spreading and stepping, repeating; it can hide from
    /// here.
    /// </summary>
    CrawlingUpsideLeftMovingDown,
    /// <summary>
    /// Rounding an outside corner from crawling down with its back to the left, a quarter turn in three steps, then
    /// crawling right with upside down.
    /// </summary>
    OutsideTurnUpsideLeftMovingDown,
    /// <summary>
    /// Crawling right with upside down, its legs tucking, spreading and stepping, repeating; it can hide from here.
    /// </summary>
    CrawlingUpsideDownMovingRight,
    /// <summary>
    /// Rounding an outside corner from crawling right with upside down, a quarter turn in three steps, then crawling up
    /// with its back to the right.
    /// </summary>
    OutsideTurnUpsideDownMovingRight,
    /// <summary>
    /// Crawling up with its back to the right, its legs tucking, spreading and stepping, repeating; it can hide from
    /// here.
    /// </summary>
    CrawlingUpsideRightMovingUp,
    /// <summary>
    /// Rounding an outside corner from crawling up with its back to the left, a quarter turn in three steps, then
    /// crawling right with its back up.
    /// </summary>
    OutsideTurnUpsideLeftMovingUp,
    /// <summary>
    /// Crawling right with its back up, its legs tucking, spreading and stepping, repeating; it can hide from here.
    /// </summary>
    CrawlingUpsideUpMovingRight,
    /// <summary>
    /// Rounding an outside corner from crawling right with its back up, a quarter turn in three steps, then crawling
    /// down with its back to the right.
    /// </summary>
    OutsideTurnUpsideUpMovingRight,
    /// <summary>
    /// Crawling down with its back to the right, its legs tucking, spreading and stepping, repeating; it can hide from
    /// here.
    /// </summary>
    CrawlingUpsideRightMovingDown,
    /// <summary>
    /// Rounding an outside corner from crawling down with its back to the right, a quarter turn in three steps, then
    /// crawling left with upside down.
    /// </summary>
    OutsideTurnUpsideRightMovingDown,
    /// <summary>
    /// Crawling left with upside down, its legs tucking, spreading and stepping, repeating; it can hide from here.
    /// </summary>
    CrawlingUpsideDownMovingLeft,
    /// <summary>
    /// Rounding an outside corner from crawling left with upside down, a quarter turn in three steps, then crawling up
    /// with its back to the left.
    /// </summary>
    OutsideTurnUpsideDownMovingLeft,
    /// <summary>
    /// Crawling up with its back to the left, its legs tucking, spreading and stepping, repeating; it can hide from
    /// here.
    /// </summary>
    CrawlingUpsideLeftMovingUp,
    /// <summary>
    /// Turning into an inside corner from crawling left with its back up, a quarter turn in three steps, then crawling
    /// up with its back to the right.
    /// </summary>
    InsideTurnUpsideUpMovingLeft,
    /// <summary>
    /// Turning into an inside corner from crawling up with its back to the right, a quarter turn in three steps, then
    /// crawling right with upside down.
    /// </summary>
    InsideTurnUpsideRightMovingUp,
    /// <summary>
    /// Turning into an inside corner from crawling right with upside down, a quarter turn in three steps, then crawling
    /// down with its back to the left.
    /// </summary>
    InsideTurnUpsideDownMovingRight,
    /// <summary>
    /// Turning into an inside corner from crawling down with its back to the left, a quarter turn in three steps, then
    /// crawling left with its back up.
    /// </summary>
    InsideTurnUpsideLeftMovingDown,
    /// <summary>
    /// Turning into an inside corner from crawling right with its back up, a quarter turn in three steps, then crawling
    /// up with its back to the left.
    /// </summary>
    InsideTurnUpsideUpMovingRight,
    /// <summary>
    /// Turning into an inside corner from crawling up with its back to the left, a quarter turn in three steps, then
    /// crawling left with upside down.
    /// </summary>
    InsideTurnUpsideLeftMovingUp,
    /// <summary>
    /// Turning into an inside corner from crawling left with upside down, a quarter turn in three steps, then crawling
    /// down with its back to the right.
    /// </summary>
    InsideTurnUpsideDownMovingLeft,
    /// <summary>
    /// Turning into an inside corner from crawling down with its back to the right, a quarter turn in three steps, then
    /// crawling right with its back up.
    /// </summary>
    InsideTurnUpsideRightMovingDown,
    /// <summary>
    /// Tucking its legs in to hide while crawling left with its back up, then shut in its shell before it crawls on.
    /// </summary>
    HidingUpsideUpMovingLeft,
    /// <summary>The closed shell of a hiding Yard upside up moving left, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideUpMovingLeft,
    /// <summary>
    /// Shut in its shell with its back up facing left, then its legs showing again before it crawls left.
    /// </summary>
    HiddenUpsideUpMovingLeft,
    /// <summary>
    /// Tucking its legs in to hide while crawling left with upside down, then shut in its shell before it crawls on.
    /// </summary>
    HidingUpsideDownMovingLeft,
    /// <summary>The closed shell of a hiding Yard upside down moving left, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideDownMovingLeft,
    /// <summary>The hidden shell of a Yard upside down moving left before the Yard crawls on (#2276).</summary>
    HiddenUpsideDownMovingLeft,
    /// <summary>
    /// Tucking its legs in to hide while crawling right with upside down, then shut in its shell before it crawls on.
    /// </summary>
    HidingUpsideDownMovingRight,
    /// <summary>The closed shell of a hiding Yard upside down moving right, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideDownMovingRight,
    /// <summary>The hidden shell of a Yard upside down moving right before the Yard crawls on (#2276).</summary>
    HiddenUpsideDownMovingRight,
    /// <summary>
    /// Tucking its legs in to hide while crawling right with its back up, then shut in its shell before it crawls on.
    /// </summary>
    HidingUpsideUpMovingRight,
    /// <summary>The closed shell of a hiding Yard upside up moving right, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideUpMovingRight,
    /// <summary>
    /// Shut in its shell with its back up facing right, then its legs showing again before it crawls right.
    /// </summary>
    HiddenUpsideUpMovingRight,
    /// <summary>
    /// Tucking its legs in to hide while crawling up with its back to the right, then shut in its shell before it
    /// crawls on.
    /// </summary>
    HidingUpsideRightMovingUp,
    /// <summary>The closed shell of a hiding Yard upside right moving up, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideRightMovingUp,
    /// <summary>The hidden shell of a Yard upside right moving up before the Yard crawls on (#2276).</summary>
    HiddenUpsideRightMovingUp,
    /// <summary>
    /// Tucking its legs in to hide while crawling up with its back to the left, then shut in its shell before it crawls
    /// on.
    /// </summary>
    HidingUpsideLeftMovingUp,
    /// <summary>The closed shell of a hiding Yard upside left moving up, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideLeftMovingUp,
    /// <summary>The hidden shell of a Yard upside left moving up before the Yard crawls on (#2276).</summary>
    HiddenUpsideLeftMovingUp,
    /// <summary>
    /// Tucking its legs in to hide while crawling down with its back to the left, then shut in its shell before it
    /// crawls on.
    /// </summary>
    HidingUpsideLeftMovingDown,
    /// <summary>The closed shell of a hiding Yard upside left moving down, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideLeftMovingDown,
    /// <summary>The hidden shell of a Yard upside left moving down before the Yard crawls on (#2276).</summary>
    HiddenUpsideLeftMovingDown,
    /// <summary>
    /// Tucking its legs in to hide while crawling down with its back to the right, then shut in its shell before it
    /// crawls on.
    /// </summary>
    HidingUpsideRightMovingDown,
    /// <summary>The closed shell of a hiding Yard upside right moving down, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedUpsideRightMovingDown,
    /// <summary>The hidden shell of a Yard upside right moving down before the Yard crawls on (#2276).</summary>
    HiddenUpsideRightMovingDown,
    /// <summary>Shut in its shell as it flies off facing left, then tumbling.</summary>
    AirborneFacingLeft,
    /// <summary>The airborne shell facing left tumbling, looped (#2276).</summary>
    AirborneFacingLeftTumbling,
    /// <summary>Shut in its shell as it flies off facing right, then tumbling.</summary>
    AirborneFacingRight,
    /// <summary>The airborne shell facing right tumbling, looped (#2276).</summary>
    AirborneFacingRightTumbling,
}

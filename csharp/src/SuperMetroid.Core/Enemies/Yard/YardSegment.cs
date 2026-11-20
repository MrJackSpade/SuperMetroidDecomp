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
    OutsideTurnOnLeftWallMovingUp,
    /// <summary>
    /// Crawling left with its back up, its legs tucking, spreading and stepping, repeating; it can hide from here.
    /// </summary>
    CrawlingOnFloorMovingLeft,
    /// <summary>
    /// Rounding an outside corner from crawling left with its back up, a quarter turn in three steps, then crawling
    /// down with its back to the left.
    /// </summary>
    OutsideTurnOnFloorMovingLeft,
    /// <summary>
    /// Crawling down with its back to the left, its legs tucking, spreading and stepping, repeating; it can hide from
    /// here.
    /// </summary>
    CrawlingOnRightWallMovingDown,
    /// <summary>
    /// Rounding an outside corner from crawling down with its back to the left, a quarter turn in three steps, then
    /// crawling right with upside down.
    /// </summary>
    OutsideTurnOnRightWallMovingDown,
    /// <summary>
    /// Crawling right with upside down, its legs tucking, spreading and stepping, repeating; it can hide from here.
    /// </summary>
    CrawlingOnCeilingMovingRight,
    /// <summary>
    /// Rounding an outside corner from crawling right with upside down, a quarter turn in three steps, then crawling up
    /// with its back to the right.
    /// </summary>
    OutsideTurnOnCeilingMovingRight,
    /// <summary>
    /// Crawling up with its back to the right, its legs tucking, spreading and stepping, repeating; it can hide from
    /// here.
    /// </summary>
    CrawlingOnLeftWallMovingUp,
    /// <summary>
    /// Rounding an outside corner from crawling up with its back to the left, a quarter turn in three steps, then
    /// crawling right with its back up.
    /// </summary>
    OutsideTurnOnRightWallMovingUp,
    /// <summary>
    /// Crawling right with its back up, its legs tucking, spreading and stepping, repeating; it can hide from here.
    /// </summary>
    CrawlingOnFloorMovingRight,
    /// <summary>
    /// Rounding an outside corner from crawling right with its back up, a quarter turn in three steps, then crawling
    /// down with its back to the right.
    /// </summary>
    OutsideTurnOnFloorMovingRight,
    /// <summary>
    /// Crawling down with its back to the right, its legs tucking, spreading and stepping, repeating; it can hide from
    /// here.
    /// </summary>
    CrawlingOnLeftWallMovingDown,
    /// <summary>
    /// Rounding an outside corner from crawling down with its back to the right, a quarter turn in three steps, then
    /// crawling left with upside down.
    /// </summary>
    OutsideTurnOnLeftWallMovingDown,
    /// <summary>
    /// Crawling left with upside down, its legs tucking, spreading and stepping, repeating; it can hide from here.
    /// </summary>
    CrawlingOnCeilingMovingLeft,
    /// <summary>
    /// Rounding an outside corner from crawling left with upside down, a quarter turn in three steps, then crawling up
    /// with its back to the left.
    /// </summary>
    OutsideTurnOnCeilingMovingLeft,
    /// <summary>
    /// Crawling up with its back to the left, its legs tucking, spreading and stepping, repeating; it can hide from
    /// here.
    /// </summary>
    CrawlingOnRightWallMovingUp,
    /// <summary>
    /// Turning into an inside corner from crawling left with its back up, a quarter turn in three steps, then crawling
    /// up with its back to the right.
    /// </summary>
    InsideTurnOnFloorMovingLeft,
    /// <summary>
    /// Turning into an inside corner from crawling up with its back to the right, a quarter turn in three steps, then
    /// crawling right with upside down.
    /// </summary>
    InsideTurnOnLeftWallMovingUp,
    /// <summary>
    /// Turning into an inside corner from crawling right with upside down, a quarter turn in three steps, then crawling
    /// down with its back to the left.
    /// </summary>
    InsideTurnOnCeilingMovingRight,
    /// <summary>
    /// Turning into an inside corner from crawling down with its back to the left, a quarter turn in three steps, then
    /// crawling left with its back up.
    /// </summary>
    InsideTurnOnRightWallMovingDown,
    /// <summary>
    /// Turning into an inside corner from crawling right with its back up, a quarter turn in three steps, then crawling
    /// up with its back to the left.
    /// </summary>
    InsideTurnOnFloorMovingRight,
    /// <summary>
    /// Turning into an inside corner from crawling up with its back to the left, a quarter turn in three steps, then
    /// crawling left with upside down.
    /// </summary>
    InsideTurnOnRightWallMovingUp,
    /// <summary>
    /// Turning into an inside corner from crawling left with upside down, a quarter turn in three steps, then crawling
    /// down with its back to the right.
    /// </summary>
    InsideTurnOnCeilingMovingLeft,
    /// <summary>
    /// Turning into an inside corner from crawling down with its back to the right, a quarter turn in three steps, then
    /// crawling right with its back up.
    /// </summary>
    InsideTurnOnLeftWallMovingDown,
    /// <summary>
    /// Tucking its legs in to hide while crawling left with its back up, then shut in its shell before it crawls on.
    /// </summary>
    HidingOnFloorMovingLeft,
    /// <summary>The closed shell of a hiding Yard upside up moving left, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedOnFloorMovingLeft,
    /// <summary>
    /// Shut in its shell with its back up facing left, then its legs showing again before it crawls left.
    /// </summary>
    HiddenOnFloorMovingLeft,
    /// <summary>
    /// Tucking its legs in to hide while crawling left with upside down, then shut in its shell before it crawls on.
    /// </summary>
    HidingOnCeilingMovingLeft,
    /// <summary>The closed shell of a hiding Yard upside down moving left, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedOnCeilingMovingLeft,
    /// <summary>The hidden shell of a Yard upside down moving left before the Yard crawls on (#2276).</summary>
    HiddenOnCeilingMovingLeft,
    /// <summary>
    /// Tucking its legs in to hide while crawling right with upside down, then shut in its shell before it crawls on.
    /// </summary>
    HidingOnCeilingMovingRight,
    /// <summary>The closed shell of a hiding Yard upside down moving right, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedOnCeilingMovingRight,
    /// <summary>The hidden shell of a Yard upside down moving right before the Yard crawls on (#2276).</summary>
    HiddenOnCeilingMovingRight,
    /// <summary>
    /// Tucking its legs in to hide while crawling right with its back up, then shut in its shell before it crawls on.
    /// </summary>
    HidingOnFloorMovingRight,
    /// <summary>The closed shell of a hiding Yard upside up moving right, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedOnFloorMovingRight,
    /// <summary>
    /// Shut in its shell with its back up facing right, then its legs showing again before it crawls right.
    /// </summary>
    HiddenOnFloorMovingRight,
    /// <summary>
    /// Tucking its legs in to hide while crawling up with its back to the right, then shut in its shell before it
    /// crawls on.
    /// </summary>
    HidingOnLeftWallMovingUp,
    /// <summary>The closed shell of a hiding Yard upside right moving up, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedOnLeftWallMovingUp,
    /// <summary>The hidden shell of a Yard upside right moving up before the Yard crawls on (#2276).</summary>
    HiddenOnLeftWallMovingUp,
    /// <summary>
    /// Tucking its legs in to hide while crawling up with its back to the left, then shut in its shell before it crawls
    /// on.
    /// </summary>
    HidingOnRightWallMovingUp,
    /// <summary>The closed shell of a hiding Yard upside left moving up, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedOnRightWallMovingUp,
    /// <summary>The hidden shell of a Yard upside left moving up before the Yard crawls on (#2276).</summary>
    HiddenOnRightWallMovingUp,
    /// <summary>
    /// Tucking its legs in to hide while crawling down with its back to the left, then shut in its shell before it
    /// crawls on.
    /// </summary>
    HidingOnRightWallMovingDown,
    /// <summary>The closed shell of a hiding Yard upside left moving down, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedOnRightWallMovingDown,
    /// <summary>The hidden shell of a Yard upside left moving down before the Yard crawls on (#2276).</summary>
    HiddenOnRightWallMovingDown,
    /// <summary>
    /// Tucking its legs in to hide while crawling down with its back to the right, then shut in its shell before it
    /// crawls on.
    /// </summary>
    HidingOnLeftWallMovingDown,
    /// <summary>The closed shell of a hiding Yard upside right moving down, shown again however often its update ends (#2180, #2276).</summary>
    ShellClosedOnLeftWallMovingDown,
    /// <summary>The hidden shell of a Yard upside right moving down before the Yard crawls on (#2276).</summary>
    HiddenOnLeftWallMovingDown,
    /// <summary>Shut in its shell as it flies off facing left, then tumbling.</summary>
    AirborneFacingLeft,
    /// <summary>The airborne shell facing left tumbling, looped (#2276).</summary>
    AirborneFacingLeftTumbling,
    /// <summary>Shut in its shell as it flies off facing right, then tumbling.</summary>
    AirborneFacingRight,
    /// <summary>The airborne shell facing right tumbling, looped (#2276).</summary>
    AirborneFacingRightTumbling,
}

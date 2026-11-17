namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The Mama Turtle's and baby turtles' crawl, spin, shell and dormant animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="MamaTurtleAnimations"/>.
/// </summary>
internal enum MamaTurtleSegment
{
    /// <summary>
    /// A baby turtle crawling left, its legs tucking, reaching, planting and pushing, before it crawls on or turns
    /// around.
    /// </summary>
    BabyCrawlingLeft,
    /// <summary>A baby turtle's shell spinning a quarter turn at a time with its sound, repeating.</summary>
    BabySpinning,
    /// <summary>
    /// The Mama Turtle spinning in her shell with its sound, facing left, the viewer and right, repeating.
    /// </summary>
    MamaSpinning,
    /// <summary>
    /// The Mama Turtle facing left withdrawing into her shell and rising to hover, held in her shell.
    /// </summary>
    MamaEnterShellLeft,
    /// <summary>
    /// A baby turtle facing left pulling in its legs and hiding in its shell before it comes out again.
    /// </summary>
    BabyHidingLeft,
    /// <summary>The dormant mama's animation.</summary>
    MamaAsleep,
    /// <summary>The Mama Turtle facing left coming out of her shell and standing, before she withdraws again.</summary>
    MamaLeaveShellLeft,
    /// <summary>
    /// A baby turtle's legs coming out of its shell and resting, before and after it has left the shell.
    /// </summary>
    BabyLeaveShellLeft,
    /// <summary>
    /// A baby turtle crawling right, its legs tucking, reaching, planting and pushing, before it crawls on or turns
    /// around.
    /// </summary>
    BabyCrawlingRight,
    /// <summary>
    /// The Mama Turtle facing right withdrawing into her shell and rising to hover, held in her shell.
    /// </summary>
    MamaEnterShellRight,
    /// <summary>
    /// A baby turtle facing right pulling in its legs and hiding in its shell before it comes out again.
    /// </summary>
    BabyHidingRight,
    /// <summary>
    /// The Mama Turtle facing right coming out of her shell and standing, before she withdraws again.
    /// </summary>
    MamaLeaveShellRight,
    /// <summary>
    /// A baby turtle's legs coming out of its shell and resting, facing left, before and after it has left the shell.
    /// </summary>
    BabyLeaveShellRight,
}

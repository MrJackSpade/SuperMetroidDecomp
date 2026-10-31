namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The Mama Turtle's and baby turtles' crawl, spin, shell and sleep animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="MamaTurtleAnimations"/>.
/// </summary>
internal enum MamaTurtleProgram
{
    /// <summary>The baby crawling left animation.</summary>
    BabyCrawlingLeft,
    /// <summary>The baby spinning animation.</summary>
    BabySpinning,
    /// <summary>The mama spinning animation.</summary>
    MamaSpinning,
    /// <summary>The mama enter shell left animation.</summary>
    MamaEnterShellLeft,
    /// <summary>The baby hiding left animation.</summary>
    BabyHidingLeft,
    /// <summary>The mama asleep animation.</summary>
    MamaAsleep,
    /// <summary>The mama leave shell left animation.</summary>
    MamaLeaveShellLeft,
    /// <summary>The baby leave shell left animation.</summary>
    BabyLeaveShellLeft,
    /// <summary>The baby crawling right animation.</summary>
    BabyCrawlingRight,
    /// <summary>The mama enter shell right animation.</summary>
    MamaEnterShellRight,
    /// <summary>The baby hiding right animation.</summary>
    BabyHidingRight,
    /// <summary>The mama leave shell right animation.</summary>
    MamaLeaveShellRight,
    /// <summary>The baby leave shell right animation.</summary>
    BabyLeaveShellRight,
}

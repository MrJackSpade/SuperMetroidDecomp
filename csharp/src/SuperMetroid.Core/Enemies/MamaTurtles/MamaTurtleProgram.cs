namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The Mama Turtle's and baby turtles' crawl, spin, shell and sleep animations as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="MamaTurtleAnimations"/>.
/// </summary>
internal enum MamaTurtleProgram
{
    /// <summary><c>InstList_BabyTurtle_CrawlingLeft</c> at $A2:8B80.</summary>
    BabyCrawlingLeft,
    /// <summary><c>InstList_BabyTurtle_Spinning</c> at $A2:8BD2.</summary>
    BabySpinning,
    /// <summary><c>InstList_MamaTurtle_Spinning</c> at $A2:8C02.</summary>
    MamaSpinning,
    /// <summary><c>InstList_MamaTurtle_FacingLeft_EnterShell</c> at $A2:8C1C.</summary>
    MamaEnterShellLeft,
    /// <summary><c>InstList_BabyTurtle_FacingLeft_Hiding</c> at $A2:8C30.</summary>
    BabyHidingLeft,
    /// <summary><c>InstList_MamaTurtle_Asleep</c> at $A2:8C44.</summary>
    MamaAsleep,
    /// <summary><c>InstList_MamaTurtle_FacingLeft_LeaveShell</c> at $A2:8C4A.</summary>
    MamaLeaveShellLeft,
    /// <summary><c>InstList_BabyTurtle_FacingLeft_LeaveShell</c> at $A2:8C62.</summary>
    BabyLeaveShellLeft,
    /// <summary><c>InstList_BabyTurtle_CrawlingRight</c> at $A2:8C72.</summary>
    BabyCrawlingRight,
    /// <summary><c>InstList_MamaTurtle_FacingRight_EnterShell</c> at $A2:8D00.</summary>
    MamaEnterShellRight,
    /// <summary><c>InstList_BabyTurtle_FacingRight_Hiding</c> at $A2:8D14.</summary>
    BabyHidingRight,
    /// <summary><c>InstList_MamaTurtle_FacingRight_LeaveShell</c> at $A2:8D28.</summary>
    MamaLeaveShellRight,
    /// <summary><c>InstList_BabyTurtle_FacingRight_LeaveShell</c> at $A2:8D40.</summary>
    BabyLeaveShellRight,
}

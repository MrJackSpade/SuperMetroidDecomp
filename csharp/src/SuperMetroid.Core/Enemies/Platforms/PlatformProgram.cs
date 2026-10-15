namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The Tripper's and Kamer's vertically moving and still platform loops as typed programs (#641), one per bank-$A3 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="PlatformAnimations"/>.
/// </summary>
internal enum PlatformProgram
{
    /// <summary><c>InstList_Kamer2_VerticallyMoving_Left_0</c> at $A3:9BBB.</summary>
    KamerMovingLeft,
    /// <summary>The loop at $A3:9BBD.</summary>
    KamerMovingLeftLoop,
    /// <summary><c>InstList_Kamer2_VerticallyMoving_Right_0</c> at $A3:9BD1.</summary>
    KamerMovingRight,
    /// <summary>The loop at $A3:9BD3.</summary>
    KamerMovingRightLoop,
    /// <summary><c>InstList_Kamer2_VerticallyStill_Left_0</c> at $A3:9BE7.</summary>
    KamerStillLeft,
    /// <summary>The loop at $A3:9BE9.</summary>
    KamerStillLeftLoop,
    /// <summary><c>InstList_Kamer2_VerticallyStill_Right_0</c> at $A3:9BFD.</summary>
    KamerStillRight,
    /// <summary>The loop at $A3:9BFF.</summary>
    KamerStillRightLoop,
    /// <summary><c>InstList_Tripper_VerticallyMoving_Left_0</c> at $A3:9C13.</summary>
    TripperMovingLeft,
    /// <summary>The loop at $A3:9C15.</summary>
    TripperMovingLeftLoop,
    /// <summary><c>InstList_Tripper_VerticallyMoving_Right_0</c> at $A3:9C29.</summary>
    TripperMovingRight,
    /// <summary>The loop at $A3:9C2B.</summary>
    TripperMovingRightLoop,
    /// <summary><c>InstList_Tripper_VerticallyStill_Right_0</c> at $A3:9C3F.</summary>
    TripperStillMovingLeft,
    /// <summary>The loop at $A3:9C41.</summary>
    TripperStillMovingLeftLoop,
    /// <summary><c>InstList_Tripper_VerticallyStill_Left_0</c> at $A3:9C55.</summary>
    TripperStillMovingRight,
    /// <summary>The loop at $A3:9C57.</summary>
    TripperStillMovingRightLoop,
}

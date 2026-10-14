namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres doors' and walls' typed programs (#641), one per bank-$A6 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="CeresDoorAnimations"/>.
/// </summary>
internal enum CeresDoorProgram
{
    /// <summary><c>InstList_CeresDoor_RidleysRoom_FacingRight_0</c> at $A6:F53A.</summary>
    RidleyRoomFacingRight,
    /// <summary><c>InstList_CeresDoor_RidleysRoom_FacingRight_1</c> at $A6:F55E.</summary>
    RidleyRoomFacingRightWait,
    /// <summary><c>InstList_CeresDoor_Normal_FacingRight</c> at $A6:F56C.</summary>
    NormalFacingRight,
    /// <summary><c>InstList_CeresDoor_Open_FacingRight_0</c> at $A6:F578.</summary>
    OpenFacingRight,
    /// <summary><c>InstList_CeresDoor_Open_FacingRight_1</c> at $A6:F584.</summary>
    CloseFacingRight,
    /// <summary><c>InstList_CeresDoor_Closed_FacingRight_0</c> at $A6:F598.</summary>
    ClosedFacingRight,
    /// <summary><c>InstList_CeresDoor_Closed_FacingRight_1</c> at $A6:F59C.</summary>
    ClosedFacingRightWait,
    /// <summary><c>InstList_CeresDoor_Normal_FacingLeft_0</c> at $A6:F5BE.</summary>
    NormalFacingLeft,
    /// <summary>The native list at $A6:F5CA.</summary>
    OpenFacingLeft,
    /// <summary>The native list at $A6:F5D6.</summary>
    CloseFacingLeft,
    /// <summary>The native list at $A6:F5EA.</summary>
    ClosedFacingLeft,
    /// <summary>The native list at $A6:F5EE.</summary>
    ClosedFacingLeftWait,
    /// <summary><c>InstList_CeresDoor_RotatingElevRoom_PreExploDoorOverlay_0</c> at $A6:F610.</summary>
    RotatingElevatorPreExplosionOverlay,
    /// <summary><c>InstList_CeresDoor_RotatingElevRoom_PreExploDoorOverlay_1</c> at $A6:F612.</summary>
    RotatingElevatorPreExplosionOverlayLoop,
    /// <summary><c>InstList_CeresDoor_RotatingElevatorRoom_InvisibleWall_0</c> at $A6:F61A.</summary>
    RotatingElevatorInvisibleWall,
    /// <summary><c>InstList_CeresDoor_RotatingElevatorRoom_InvisibleWall_1</c> at $A6:F622.</summary>
    RotatingElevatorInvisibleWallLoop,
    /// <summary><c>InstList_CeresDoor_RidleyEscapeMode7LeftWall_0</c> at $A6:F62A.</summary>
    RidleyEscapeMode7LeftWall,
    /// <summary><c>InstList_CeresDoor_RidleyEscapeMode7LeftWall_1</c> at $A6:F62C.</summary>
    RidleyEscapeMode7LeftWallLoop,
    /// <summary><c>InstList_CeresDoor_RidleyEscapeMode7RightWall_0</c> at $A6:F634.</summary>
    RidleyEscapeMode7RightWall,
    /// <summary><c>InstList_CeresDoor_RidleyEscapeMode7RightWall_1</c> at $A6:F636.</summary>
    RidleyEscapeMode7RightWallLoop,
}

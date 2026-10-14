namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The Work Robot's unpowered poses and its walk, wall, recoil, laser and ledge animations as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="WorkRobotAnimations"/>.
/// </summary>
internal enum WorkRobotProgram
{
    /// <summary><c>InstList_RobotNoPower_Neutral</c> at $A8:C6D3.</summary>
    NoPowerNeutral,
    /// <summary><c>InstList_RobotNoPower_LeaningLeft</c> at $A8:C6D9.</summary>
    NoPowerLeaningLeft,
    /// <summary><c>InstList_RobotNoPower_LeaningRIght</c> at $A8:C6DF.</summary>
    NoPowerLeaningRight,
    /// <summary><c>InstList_Robot_Initial</c> at $A8:C6E5.</summary>
    Initial,
    /// <summary><c>InstList_Robot_FacingLeft_WalkingForwards_0</c> at $A8:C6E9.</summary>
    FacingLeftWalkingForwards,
    /// <summary><c>InstList_Robot_FacingLeft_WalkingForwards_1</c> at $A8:C6ED.</summary>
    FacingLeftWalkingForwardsLoop,
    /// <summary><c>InstList_Robot_FacingLeft_HitWallMovingForwards</c> at $A8:C73F.</summary>
    FacingLeftHitWallMovingForwards,
    /// <summary><c>InstList_Robot_FacingLeft_Shot_SamusIsAhead</c> at $A8:C7BB.</summary>
    FacingLeftShotSamusAhead,
    /// <summary><c>InstList_Robot_FacingLeft_Shot_SamusIsBehind</c> at $A8:C833.</summary>
    FacingLeftShotSamusBehind,
    /// <summary><c>InstList_Robot_FacingLeft_ShotLaserDownLeft</c> at $A8:C8B1.</summary>
    FacingLeftShotLaserDownLeft,
    /// <summary><c>InstList_Robot_FacingLeft_ShotLaserLeft</c> at $A8:C8BD.</summary>
    FacingLeftShotLaserLeft,
    /// <summary><c>InstList_Robot_FacingLeft_ShotLaserUpLeft</c> at $A8:C8D1.</summary>
    FacingLeftShotLaserUpLeft,
    /// <summary><c>InstList_Robot_FacingLeft_LaserShotRecoil</c> at $A8:C8E9.</summary>
    FacingLeftLaserShotRecoil,
    /// <summary><c>InstList_Robot_ApproachingAFallToTheRight</c> at $A8:C91B.</summary>
    ApproachingFallRight,
    /// <summary><c>InstList_Robot_FacingRight_WalkingForwards_0</c> at $A8:C92D.</summary>
    FacingRightWalkingForwards,
    /// <summary><c>InstList_Robot_FacingRight_WalkingForwards_1</c> at $A8:C931.</summary>
    FacingRightWalkingForwardsLoop,
    /// <summary><c>InstList_Robot_FacingRight_HitWallMovingForwards</c> at $A8:C985.</summary>
    FacingRightHitWallMovingForwards,
    /// <summary><c>InstList_Robot_FacingRight_Shot_SamusIsAhead</c> at $A8:CA01.</summary>
    FacingRightShotSamusAhead,
    /// <summary><c>InstList_Robot_FacingRight_Shot_SamusIsBehind</c> at $A8:CA7D.</summary>
    FacingRightShotSamusBehind,
    /// <summary><c>InstList_Robot_FacingRight_ShotLaserDownRight</c> at $A8:CAFD.</summary>
    FacingRightShotLaserDownRight,
    /// <summary><c>InstList_Robot_FacingRight_ShotLaserRight</c> at $A8:CB09.</summary>
    FacingRightShotLaserRight,
    /// <summary><c>InstList_Robot_FacingRight_ShotLaserUpRight</c> at $A8:CB1D.</summary>
    FacingRightShotLaserUpRight,
    /// <summary><c>InstList_Robot_FacingRight_LaserShotRecoil</c> at $A8:CB35.</summary>
    FacingRightLaserShotRecoil,
    /// <summary><c>InstList_Robot_ApproachingAFallToTheLeft</c> at $A8:CB65.</summary>
    ApproachingFallLeft,
}

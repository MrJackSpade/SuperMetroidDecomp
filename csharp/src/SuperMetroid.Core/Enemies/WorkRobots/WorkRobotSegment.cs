namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The Work Robot's unpowered poses and its walk, wall, recoil, laser and ledge animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="WorkRobotAnimations"/>.
/// </summary>
internal enum WorkRobotSegment
{
    /// <summary>The no power neutral animation.</summary>
    NoPowerNeutral,
    /// <summary>The no power leaning left animation.</summary>
    NoPowerLeaningLeft,
    /// <summary>The no power leaning right animation.</summary>
    NoPowerLeaningRight,
    /// <summary>The initial animation.</summary>
    Initial,
    /// <summary>The facing left walking forwards animation.</summary>
    FacingLeftWalkingForwards,
    /// <summary>The facing left walking forwards loop animation.</summary>
    FacingLeftWalkingForwardsLoop,
    /// <summary>The facing left hit wall moving forwards animation.</summary>
    FacingLeftHitWallMovingForwards,
    /// <summary>The facing left shot Samus ahead animation.</summary>
    FacingLeftShotSamusAhead,
    /// <summary>The facing left shot Samus behind animation.</summary>
    FacingLeftShotSamusBehind,
    /// <summary>The facing left shot laser down left animation.</summary>
    FacingLeftShotLaserDownLeft,
    /// <summary>The facing left shot laser left animation.</summary>
    FacingLeftShotLaserLeft,
    /// <summary>The facing left shot laser up left animation.</summary>
    FacingLeftShotLaserUpLeft,
    /// <summary>The facing left laser shot recoil animation.</summary>
    FacingLeftLaserShotRecoil,
    /// <summary>The approaching fall right animation.</summary>
    ApproachingFallRight,
    /// <summary>The facing right walking forwards animation.</summary>
    FacingRightWalkingForwards,
    /// <summary>The facing right walking forwards loop animation.</summary>
    FacingRightWalkingForwardsLoop,
    /// <summary>The facing right hit wall moving forwards animation.</summary>
    FacingRightHitWallMovingForwards,
    /// <summary>The facing right shot Samus ahead animation.</summary>
    FacingRightShotSamusAhead,
    /// <summary>The facing right shot Samus behind animation.</summary>
    FacingRightShotSamusBehind,
    /// <summary>The facing right shot laser down right animation.</summary>
    FacingRightShotLaserDownRight,
    /// <summary>The facing right shot laser right animation.</summary>
    FacingRightShotLaserRight,
    /// <summary>The facing right shot laser up right animation.</summary>
    FacingRightShotLaserUpRight,
    /// <summary>The facing right laser shot recoil animation.</summary>
    FacingRightLaserShotRecoil,
    /// <summary>The approaching fall left animation.</summary>
    ApproachingFallLeft,
}

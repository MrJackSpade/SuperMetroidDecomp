namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The Work Robot's unpowered poses and its walk, wall, recoil, laser and ledge animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="WorkRobotAnimations"/>.
/// </summary>
internal enum WorkRobotSegment
{
    /// <summary>Unpowered and upright, held.</summary>
    NoPowerNeutral,
    /// <summary>Unpowered and tilted left, held.</summary>
    NoPowerLeaningLeft,
    /// <summary>Unpowered and tilted right, held.</summary>
    NoPowerLeaningRight,
    /// <summary>Standing facing left with its near foot easing up for a long moment, then walking left.</summary>
    StandingFacingLeft,
    /// <summary>Facing left with its near foot easing up as it sets off, then striding left.</summary>
    FacingLeftWalkingForwards,
    /// <summary>The stride walking forwards facing left, looped (#2276).</summary>
    FacingLeftWalkingStride,
    /// <summary>
    /// Facing left after meeting a wall, backing away two slow strides with their footsteps, watching for ledges, then
    /// walking right.
    /// </summary>
    FacingLeftHitWallMovingForwards,
    /// <summary>
    /// Facing left, shot with Samus ahead, backing away two quick strides with their footsteps, then walking right.
    /// </summary>
    FacingLeftShotSamusAhead,
    /// <summary>
    /// Facing left, shot with Samus behind, pressing on two quick strides with their footsteps, then walking left
    /// again.
    /// </summary>
    FacingLeftShotSamusBehind,
    /// <summary>
    /// Facing left, leaning back as it fires its laser down and left, then recoiling and walking left again.
    /// </summary>
    FacingLeftShotLaserDownLeft,
    /// <summary>
    /// Facing left, leaning back as it fires its laser left and stepping back with a footstep, then recoiling and
    /// walking left again.
    /// </summary>
    FacingLeftShotLaserLeft,
    /// <summary>
    /// Facing left, rising and leaning back as it fires its laser up and left, stepping back with a footstep, then
    /// recoiling and walking left again.
    /// </summary>
    FacingLeftShotLaserUpLeft,
    /// <summary>
    /// Facing left, pushed back a stride by its laser with a footstep, watching for ledges, then standing upright while
    /// its laser cools.
    /// </summary>
    FacingLeftLaserShotRecoil,
    /// <summary>
    /// Facing right at a drop, leaning back for a long moment, then upright while its laser cools, before walking left.
    /// </summary>
    ApproachingFallRight,
    /// <summary>Facing right leaning back as it sets off, then striding right.</summary>
    FacingRightWalkingForwards,
    /// <summary>The stride walking forwards facing right, looped (#2276).</summary>
    FacingRightWalkingStride,
    /// <summary>
    /// Facing right after meeting a wall, backing away two slow strides with their footsteps, watching for ledges, then
    /// walking left.
    /// </summary>
    FacingRightHitWallMovingForwards,
    /// <summary>
    /// Facing right, shot with Samus ahead, backing away two quick strides with their footsteps, then walking left.
    /// </summary>
    FacingRightShotSamusAhead,
    /// <summary>
    /// Facing right, shot with Samus behind, pressing on two quick strides with their footsteps, then walking right
    /// again.
    /// </summary>
    FacingRightShotSamusBehind,
    /// <summary>
    /// Facing right, leaning back as it fires its laser down and right, then recoiling and walking right again.
    /// </summary>
    FacingRightShotLaserDownRight,
    /// <summary>
    /// Facing right, leaning back as it fires its laser right and stepping back with a footstep, then recoiling and
    /// walking right again.
    /// </summary>
    FacingRightShotLaserRight,
    /// <summary>
    /// Facing right, rising and leaning back as it fires its laser up and right, stepping back with a footstep, then
    /// recoiling and walking right again.
    /// </summary>
    FacingRightShotLaserUpRight,
    /// <summary>
    /// Facing right, pushed back a stride by its laser with a footstep, watching for ledges, then standing upright a
    /// long moment.
    /// </summary>
    FacingRightLaserShotRecoil,
    /// <summary>
    /// Facing left at a drop, leaning back for a long moment, then upright while its laser cools, before walking right.
    /// </summary>
    ApproachingFallLeft,
}

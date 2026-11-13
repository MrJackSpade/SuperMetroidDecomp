namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres doors' and walls' typed animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="CeresDoorAnimations"/>.
/// </summary>
internal enum CeresDoorSegment
{
    /// <summary>The Ridley room facing right animation.</summary>
    RidleyRoomFacingRight,
    /// <summary>The Ridley room facing right wait animation.</summary>
    RidleyRoomFacingRightWait,
    /// <summary>The normal facing right animation.</summary>
    NormalFacingRight,
    /// <summary>The open facing right animation.</summary>
    OpenFacingRight,
    /// <summary>The close facing right animation.</summary>
    CloseFacingRight,
    /// <summary>The closed facing right animation.</summary>
    ClosedFacingRight,
    /// <summary>The closed facing right wait animation.</summary>
    ClosedFacingRightWait,
    /// <summary>The normal facing left animation.</summary>
    NormalFacingLeft,
    /// <summary>The open facing left animation.</summary>
    OpenFacingLeft,
    /// <summary>The close facing left animation.</summary>
    CloseFacingLeft,
    /// <summary>The closed facing left animation.</summary>
    ClosedFacingLeft,
    /// <summary>The closed facing left wait animation.</summary>
    ClosedFacingLeftWait,
    /// <summary>The rotating elevator pre explosion overlay animation.</summary>
    RotatingElevatorPreExplosionOverlay,
    /// <summary>The rotating elevator pre explosion overlay loop animation.</summary>
    RotatingElevatorPreExplosionOverlayLoop,
    /// <summary>The rotating elevator invisible wall animation.</summary>
    RotatingElevatorInvisibleWall,
    /// <summary>The rotating elevator invisible wall loop animation.</summary>
    RotatingElevatorInvisibleWallLoop,
    /// <summary>The Ridley escape floor left wall animation.</summary>
    RidleyEscapeFloorLeftWall,
    /// <summary>The Ridley escape floor left wall loop animation.</summary>
    RidleyEscapeFloorLeftWallLoop,
    /// <summary>The Ridley escape floor right wall animation.</summary>
    RidleyEscapeFloorRightWall,
    /// <summary>The Ridley escape floor right wall loop animation.</summary>
    RidleyEscapeFloorRightWallLoop,
}

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The seven mutually exclusive meanings of Ceres-door population parameter one.</summary>
internal enum CeresDoorVariant : ushort
{
    /// <summary>Ordinary Ceres door facing right during escape.</summary>
    NormalFacingRight = 0,
    /// <summary>Ordinary Ceres door facing left during escape.</summary>
    NormalFacingLeft = 1,
    /// <summary>Pre-explosion door overlay in the rotating elevator room.</summary>
    RotatingElevatorPreExplosionOverlay = 2,
    /// <summary>Right-facing door in Ceres Ridley's room.</summary>
    RidleyRoomFacingRight = 3,
    /// <summary>Invisible wall associated with the rotating elevator room.</summary>
    RotatingElevatorInvisibleWall = 4,
    /// <summary>Left wall beside the rotating floor of Ceres Ridley's escape scene.</summary>
    RidleyEscapeFloorLeftWall = 5,
    /// <summary>Right wall beside the rotating floor of Ceres Ridley's escape scene.</summary>
    RidleyEscapeFloorRightWall = 6,
}

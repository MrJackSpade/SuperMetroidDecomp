namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The seven mutually exclusive meanings of Ceres-door population parameter one.</summary>
internal enum CeresDoorVariant : ushort
{
    /// <summary>Ordinary Ceres door facing right during escape.</summary>
    NormalFacingRight,
    /// <summary>Ordinary Ceres door facing left during escape.</summary>
    NormalFacingLeft,
    /// <summary>Pre-explosion door overlay in the rotating elevator room.</summary>
    RotatingElevatorPreExplosionOverlay,
    /// <summary>Right-facing door in Ceres Ridley's room.</summary>
    RidleyRoomFacingRight,
    /// <summary>Invisible wall associated with the rotating elevator room.</summary>
    RotatingElevatorInvisibleWall,
    /// <summary>Left wall beside the rotating floor of Ceres Ridley's escape scene.</summary>
    RidleyEscapeFloorLeftWall,
    /// <summary>Right wall beside the rotating floor of Ceres Ridley's escape scene.</summary>
    RidleyEscapeFloorRightWall,
}

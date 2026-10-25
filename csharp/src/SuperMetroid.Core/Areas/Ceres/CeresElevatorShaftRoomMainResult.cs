using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Debugger-visible result of one Ceres elevator shaft room-main call.</summary>
[IndependentFlag("independent facts of one room-main call: whether the Mode 7 matrix changed and whether departure was requested")]
public readonly record struct CeresElevatorShaftRoomMainResult(
    TurningFloorTransform Transform,
    bool MatrixChanged,
    bool DepartureRequestedThisFrame);

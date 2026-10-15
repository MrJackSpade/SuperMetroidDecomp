using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Debugger-visible result of one Ceres elevator shaft room-main call.</summary>
public readonly record struct CeresElevatorShaftRoomMainResult(
    SamusMode7Transform Transform,
    bool MatrixChanged,
    bool DepartureRequestedThisFrame);

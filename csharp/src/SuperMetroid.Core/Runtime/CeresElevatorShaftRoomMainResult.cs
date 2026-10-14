using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Runtime;

/// <summary>Debugger-visible result of one Ceres elevator shaft room-main call.</summary>
public readonly record struct CeresElevatorShaftRoomMainResult(
    SamusMode7Transform Transform,
    bool MatrixChanged,
    bool DepartureRequestedThisFrame);

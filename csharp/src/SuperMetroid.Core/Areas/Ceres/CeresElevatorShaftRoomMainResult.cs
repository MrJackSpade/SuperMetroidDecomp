using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>What one update of the Ceres elevator shaft room did: the turning floor's tilt and whether it changed, and whether departure began.</summary>
[IndependentFlag("independent facts of one room-main call: whether the shaft tilt changed and whether departure was requested")]
public readonly record struct CeresElevatorShaftRoomMainResult(
    TurningFloorTransform Transform,
    bool TiltChanged,
    bool DepartureRequestedThisFrame);

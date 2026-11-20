using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// What one update of the Ceres elevator shaft room did: the turning floor's tilt and whether it changed, and whether the
/// elevator took Samus, which the update acts on by moving the game to the Ceres departure (#2545).
/// </summary>
[IndependentFlag("independent facts of one room-main update: whether the shaft tilt changed and whether the elevator took Samus")]
public readonly record struct CeresElevatorShaftRoomMainResult(
    TurningFloorTransform Transform,
    bool TiltChanged,
    SuperMetroid.Core.World.RoomMainOutcome Outcome);

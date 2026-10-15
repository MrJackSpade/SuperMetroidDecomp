using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>State branches used by horizontal camera target selection at <c>$90:95B7</c>.</summary>
public readonly record struct HorizontalCameraContext(
    ushort KnockbackDirection,
    SamusMovementType MovementType,
    ushort XAccelerationMode,
    byte PoseXDirection,
    CameraDistanceMode CameraDistanceIndex);

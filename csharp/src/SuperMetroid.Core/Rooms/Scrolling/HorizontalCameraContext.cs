using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// What horizontal camera target selection at <c>$90:95B7</c> asks of Samus: whether her motion runs
/// against her facing (knockback, moonwalking, or a turn that keeps the old momentum), whether she faces
/// left, and how far ahead of her the camera sits.
/// </summary>
/// <param name="KnockedBack">Whether a knockback is carrying Samus, which reverses the facing target.</param>
/// <param name="MovementType">Samus's movement type; moonwalking reverses the facing target.</param>
/// <param name="TurningAgainstMomentum">
/// Whether a turn is still carrying Samus opposite her new facing, which reverses the facing target.
/// </param>
/// <param name="FacesLeft">
/// Whether Samus's pose faces left. Every other pose facing, front-view included, selects the right-facing target.
/// </param>
/// <param name="CameraDistanceIndex">How far ahead of Samus the camera target sits.</param>
[IndependentFlag("independent facts about Samus the camera target reads")]
public readonly record struct HorizontalCameraContext(
    bool KnockedBack,
    SamusMovementType MovementType,
    bool TurningAgainstMomentum,
    bool FacesLeft,
    CameraDistanceMode CameraDistanceIndex)
{
    /// <summary>
    /// Decodes the runtime's native knockback, acceleration-mode and facing words into the camera context: a nonzero
    /// knockback direction is a knockback, acceleration mode one is the turn that keeps the old momentum, and pose
    /// facing four is left.
    /// </summary>
    /// <param name="KnockedBack">Whether a knockback is pending or running.</param>
    /// <param name="MovementType">Samus's movement type.</param>
    /// <param name="XAccelerationMode">Samus's horizontal acceleration mode.</param>
    /// <param name="PoseXDirection">Samus's pose-definition facing byte.</param>
    /// <param name="CameraDistanceIndex">How far ahead of Samus the camera target sits.</param>
    public HorizontalCameraContext(
        bool KnockedBack,
        SamusMovementType MovementType,
        SamusAccelerationMode XAccelerationMode,
        byte PoseXDirection,
        CameraDistanceMode CameraDistanceIndex)
        : this(
            KnockedBack,
            MovementType,
            TurningAgainstMomentum: XAccelerationMode == SamusAccelerationMode.Turning,
            FacesLeft: PoseXDirection == LeftFacingPose,
            CameraDistanceIndex)
    {
    }

    /// <summary>The pose facing byte of a left-facing pose.</summary>
    private const byte LeftFacingPose = 4;
}
